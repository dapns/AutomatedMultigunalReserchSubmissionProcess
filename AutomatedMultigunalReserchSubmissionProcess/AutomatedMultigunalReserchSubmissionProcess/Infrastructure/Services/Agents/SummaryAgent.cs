using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using Microsoft.SemanticKernel;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents
{
    public class SummaryAgent : ISummaryAgent
    {
        private readonly Kernel _kernel;
        private readonly ILogger<SummaryAgent> _logger;

        public SummaryAgent(Kernel kernel, ILogger<SummaryAgent> logger)
        {
            _kernel = kernel;
            _logger = logger;
        }

        public async Task<Summary> GenerateSummaryAsync(Submission submission)
        {
            var prompt = @"
                You are a research assistant. Write a concise summary (max 250 words) of the following research paper submission. 
                Highlight key findings, major validation issues, and missing sections. Use plain language.

                Title: {{$title}}
                Authors: {{$authors}}
                Abstract: {{$abstract}}
                Validation Errors: {{$errors}}
                Toxicity: {{$toxicity}} Illicit: {{$illicit}}

                Text: {{$text}}";

            var args = new KernelArguments
            {
                ["title"] = submission.ExtractedInfo?.Title ?? "N/A",
                ["authors"] = submission.ExtractedInfo?.Authors != null ? string.Join(", ", submission.ExtractedInfo.Authors) : "N/A",
                ["abstract"] = submission.ExtractedInfo?.Abstract ?? "N/A",
                ["errors"] = submission.ValidationResult?.Errors != null ? string.Join("; ", submission.ValidationResult.Errors) : "None",
                ["toxicity"] = submission.ValidationResult?.HasToxicity == true ? "Yes" : "No",
                ["illicit"] = submission.ValidationResult?.HasIllicitContent == true ? "Yes" : "No",
                ["text"] = submission.EnglishTranslation ?? submission.ExtractedText
            };

            var result = await _kernel.InvokePromptAsync(prompt, args);
            return new Summary { Content = result.ToString(), GeneratedAt = DateTime.UtcNow };
        }
    }
}
