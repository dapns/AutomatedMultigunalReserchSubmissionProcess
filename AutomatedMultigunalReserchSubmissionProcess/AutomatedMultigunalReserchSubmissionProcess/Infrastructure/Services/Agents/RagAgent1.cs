using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using Microsoft.KernelMemory;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents
{
    public class RagAgent1 : IRAGAgent
    {
        private readonly IKernelMemory _memory;
        private readonly ILogger<RagAgent1> _logger;
        private const string IndexName = "research-submissions";

        public RagAgent1(IKernelMemory memory, ILogger<RagAgent1> logger)
        {
            _memory = memory;
            _logger = logger;
        }

        public async Task IndexSubmissionAsync(Submission submission)
        {
            var text = $@"
                Title: {submission.ExtractedInfo?.Title}
                Authors: {string.Join(", ", submission.ExtractedInfo?.Authors ?? new())}
                Abstract: {submission.ExtractedInfo?.Abstract}
                Keywords: {string.Join(", ", submission.ExtractedInfo?.Keywords ?? new())}
                Full Text: {submission.EnglishTranslation ?? submission.ExtractedText}";

            // ImportTextAsync automatically handles chunking and embedding generation
            await _memory.ImportTextAsync(
                text: text,
                documentId: submission.Id.ToString(),
                index: IndexName,
                tags: new TagCollection
                {
                    { "submissionId", submission.Id.ToString() },
                    { "title", submission.ExtractedInfo?.Title ?? "" }
                });

            _logger.LogInformation(
                "Indexed submission {Id} into Azure AI Search",
                submission.Id);
        }

        public async Task<IEnumerable<string>> SearchAsync(string query, int topN = 5)
        {
            // Execute the vector search against Azure AI Search
            var searchResult = await _memory.SearchAsync(
                query: query,
                index: IndexName,
                limit: topN
            );

            // Extract the text chunks from the search results
            var texts = searchResult.Results
                .SelectMany(result => result.Partitions)
                .Select(partition => partition.Text)
                .ToList();

            return texts;
        }
    }
}


