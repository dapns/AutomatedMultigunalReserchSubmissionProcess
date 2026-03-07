using AutomatedMultigunalReserchSubmissionProcess.Core.IServices;
using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using OpenAI.Embeddings;   // Required for Embedding generation

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents
{
    public class RagAgent : IRAGAgent
    {
        private readonly IVectorStore _vectorStore;
        private readonly EmbeddingClient _embeddingClient;   // New client for embeddings
        private readonly ILogger<RagAgent> _logger;

        private const string CollectionName = "research-submissions";

        public RagAgent(
            IVectorStore vectorStore,
            EmbeddingClient embeddingClient,   // Inject EmbeddingClient directly
            ILogger<RagAgent> logger)
        {
            _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
            _embeddingClient = embeddingClient ?? throw new ArgumentNullException(nameof(embeddingClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task IndexSubmissionAsync(Submission submission)
        {
            if (submission == null) throw new ArgumentNullException(nameof(submission));

            try
            {
                var text = $@"
                    Title: {submission.ExtractedInfo?.Title ?? "N/A"}
                    Authors: {string.Join(", ", submission.ExtractedInfo?.Authors ?? new List<string>())}
                    Abstract: {submission.ExtractedInfo?.Abstract ?? "N/A"}
                    Keywords: {string.Join(", ", submission.ExtractedInfo?.Keywords ?? new List<string>())}
                    Full Text: {submission.EnglishTranslation ?? submission.ExtractedText ?? "N/A"}";

                var chunks = TextChunker.Chunk(text, chunkSize: 800, overlap: 100).ToList();
                if (!chunks.Any())
                {
                    _logger.LogWarning("No text chunks generated for submission {Id}", submission.Id);
                    return;
                }

                int chunkIndex = 0;
                foreach (var chunk in chunks)
                {
                    try
                    {
                        var embedding = await GenerateEmbeddingAsync(chunk);
                        // ReadOnlyMemory<float> is a struct and cannot be null; check Length instead.
                        if (embedding.Length == 0)
                        {
                            _logger.LogWarning("Empty embedding for chunk {ChunkIndex} of submission {Id}", chunkIndex, submission.Id);
                            chunkIndex++;
                            continue;
                        }

                        var record = new VectorRecord
                        {
                            Id = $"{submission.Id}-{chunkIndex}",
                            // ReadOnlyMemory<float> does not implement IEnumerable<float>, convert to array first.
                            Vector = embedding.ToArray().ToList(),
                            Metadata = new Dictionary<string, object>
                            {
                                ["text"] = chunk,
                                ["submissionId"] = submission.Id.ToString(),
                                ["chunkIndex"] = chunkIndex,
                                ["title"] = submission.ExtractedInfo?.Title ?? string.Empty,
                                ["fileName"] = submission.FileName,
                                ["timestamp"] = DateTime.UtcNow.ToString("o")
                            }
                        };

                        await _vectorStore.UpsertAsync(CollectionName, record);
                        _logger.LogDebug("Indexed chunk {ChunkIndex} for submission {Id}", chunkIndex, submission.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to index chunk {ChunkIndex} for submission {Id}", chunkIndex, submission.Id);
                    }
                    chunkIndex++;
                }

                _logger.LogInformation("Successfully indexed submission {Id} with {Chunks} chunks", submission.Id, chunkIndex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index submission {Id}", submission?.Id);
                throw;
            }
        }

        public async Task<IEnumerable<string>> SearchAsync(string query, int topN = 5)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Empty query provided for search");
                return Array.Empty<string>();
            }

            try
            {
                var queryEmbedding = await GenerateEmbeddingAsync(query);
                // ReadOnlyMemory<float> is a struct and cannot be null; check Length instead.
                if (queryEmbedding.Length == 0)
                {
                    _logger.LogWarning("Failed to generate embedding for query");
                    return Array.Empty<string>();
                }

                // Convert ReadOnlyMemory<float> to List<float> via array
                var results = await _vectorStore.SearchAsync(CollectionName, queryEmbedding.ToArray().ToList(), topN);

                var texts = new List<string>();
                foreach (var record in results)
                {
                    if (record?.Metadata != null &&
                        record.Metadata.TryGetValue("text", out var textObj) &&
                        textObj is string text &&
                        !string.IsNullOrWhiteSpace(text))
                    {
                        texts.Add(text);
                    }
                }

                _logger.LogInformation("Found {Count} results for query", texts.Count);
                return texts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during search for query: {Query}", query);
                return Array.Empty<string>();
            }
        }

        private async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                _logger.LogWarning("Empty input for embedding generation");
                return ReadOnlyMemory<float>.Empty;
            }

            try
            {
                // New SDK: EmbeddingClient.GenerateEmbeddingAsync returns OpenAI.Embedding
                var response = await _embeddingClient.GenerateEmbeddingAsync(input);
                var embedding = response.Value;
                _logger.LogDebug("Generated embedding of size {Size} for input length {Length}",
                    embedding.ToFloats().Length, input.Length);

                return embedding.ToFloats();   // Returns ReadOnlyMemory<float>
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate embedding for input length {Length}", input?.Length ?? 0);
                return ReadOnlyMemory<float>.Empty;
            }
        }

        public static class TextChunker
        {
            public static IEnumerable<string> Chunk(string text, int chunkSize = 800, int overlap = 100)
            {
                if (string.IsNullOrWhiteSpace(text))
                    yield break;

                text = text.Trim();
                int start = 0;
                int textLength = text.Length;

                while (start < textLength)
                {
                    int length = Math.Min(chunkSize, textLength - start);

                    // Try to break at sentence boundary
                    if (start + length < textLength)
                    {
                        int lookAhead = Math.Min(50, textLength - (start + length));
                        string nextPart = text.Substring(start + length, lookAhead);
                        int sentenceEnd = nextPart.IndexOfAny(new[] { '.', '!', '?', '\n' });

                        if (sentenceEnd > 0 && sentenceEnd < lookAhead - 1)
                        {
                            length += sentenceEnd + 1;
                        }
                    }

                    yield return text.Substring(start, Math.Min(length, textLength - start));
                    start += chunkSize - overlap;

                    if (start >= textLength)
                        break;
                }
            }
        }
    }
}