using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using Microsoft.SemanticKernel;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents
{
    public class QnAAgent : IQnAAgent
    {
        private readonly Kernel _kernel;
        private readonly IRAGAgent _ragAgent;
        private readonly ILogger<QnAAgent> _logger;

        public QnAAgent(Kernel kernel, IRAGAgent ragAgent, ILogger<QnAAgent> logger)
        {
            _kernel = kernel;
            _ragAgent = ragAgent;
            _logger = logger;
        }

        public async Task<string> AskAsync(string question, string? submissionId = null)
        {
            // Retrieve relevant context
            var context = submissionId != null
                ? await RetrieveFromSpecificSubmission(submissionId, question)
                : await _ragAgent.SearchAsync(question);

            var contextText = string.Join("\n---\n", context);

            var prompt = @"
                    You are a helpful research assistant. Answer the question based on the provided context from research paper submissions. 
                    If the answer cannot be found in the context, say 'I cannot find that information in the submissions.'

                    Context:
                    {{$context}}

                    Question: {{$question}}
                    Answer:";

            var result = await _kernel.InvokePromptAsync(prompt, new KernelArguments
            {
                ["context"] = contextText,
                ["question"] = question
            });

            return result.ToString();
        }

        private async Task<IEnumerable<string>> RetrieveFromSpecificSubmission(string submissionId, string question)
        {
            // In a real app, you might filter by metadata. Here we just search all and then filter by id.
            // For demo, we'll return the indexed text of that submission.
            var submission = InMemoryStore.Submissions.FirstOrDefault(s => s.Id.ToString() == submissionId);
            if (submission == null) return Enumerable.Empty<string>();

            var text = $@"
                        Title: {submission.ExtractedInfo?.Title}
                        Authors: {string.Join(", ", submission.ExtractedInfo?.Authors ?? new())}
                        Abstract: {submission.ExtractedInfo?.Abstract}
                        Keywords: {string.Join(", ", submission.ExtractedInfo?.Keywords ?? new())}
                        Full Text: {submission.EnglishTranslation ?? submission.ExtractedText}";
            return new[] { text };
        }
    }
}
