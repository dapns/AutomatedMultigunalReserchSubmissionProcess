using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Memory;

#pragma warning disable SKEXP0001

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents
{

    public class RAGAgent : IRAGAgent
    {
        private readonly ISemanticTextMemory _memory;
        private readonly ILogger<RAGAgent> _logger;
        private const string CollectionName = "research-submissions";

        public RAGAgent(ISemanticTextMemory memory, ILogger<RAGAgent> logger)
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

            await _memory.SaveInformationAsync(CollectionName, text, submission.Id.ToString());
            _logger.LogInformation("Indexed submission {Id}", submission.Id);
        }

        public async Task<IEnumerable<string>> SearchAsync(string query, int topN = 5)
        {
            var texts = new List<string>();
            await foreach (var result in _memory.SearchAsync(CollectionName, query, topN, 0.5))
            {
                if (result?.Metadata?.Text is string text && !string.IsNullOrEmpty(text))
                {
                    texts.Add(text);
                }
            }
            return texts;
        }
    }
}

#pragma warning restore SKEXP0001

