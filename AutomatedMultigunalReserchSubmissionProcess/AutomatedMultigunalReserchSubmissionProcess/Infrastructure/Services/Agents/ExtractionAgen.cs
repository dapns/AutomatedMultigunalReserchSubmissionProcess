using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using Microsoft.SemanticKernel;
using System.Text.Json;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents
{
    public class ExtractionAgent : IExtractionAgent
    {
        private readonly Kernel _kernel;
        private readonly ILogger<ExtractionAgent> _logger;

        public ExtractionAgent(Kernel kernel, ILogger<ExtractionAgent> logger)
        {
            _kernel = kernel;
            _logger = logger;
        }

        public async Task<ExtractedInfo> ExtractAsync(string englishText)
        {
            _logger.LogInformation("Extracting structured information");

            var prompt = @"
Extract the following fields from the research paper text below. 
Return a JSON object with keys: title, authors (array), affiliations (array), abstract, keywords (array), sections (array), references (array), pageCount (number). 
If a field is missing, use empty string or empty array.

Text: {{$text}}";

            var result = await _kernel.InvokePromptAsync(prompt, new KernelArguments
            {
                ["text"] = englishText
            });

            // Parse JSON into ExtractedInfo
            var json = result.ToString();
            var extracted = JsonSerializer.Deserialize<ExtractedInfo>(json) ?? new ExtractedInfo();

            // Mock plagiarism check (low score)
            extracted.PlagiarismScore = 0.05;

            return extracted;
        }
    }
}
