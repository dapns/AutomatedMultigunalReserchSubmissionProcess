using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using Microsoft.SemanticKernel;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents
{
    public class ValidationAgent : IValidationAgent
    {
        private readonly Kernel _kernel;
        private readonly ILogger<ValidationAgent> _logger;

        public ValidationAgent(Kernel kernel, ILogger<ValidationAgent> logger)
        {
            _kernel = kernel;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateAsync(ExtractedInfo info, string fullText)
        {
            var result = new ValidationResult();
            var errors = new List<string>();

            // Use native function from plugin
            var pageCheck = await _kernel.InvokeAsync("Validation", "check_page_count", new() { ["pageCount"] = info.PageCount });
            if (!pageCheck.ToString().Contains("OK"))
                errors.Add(pageCheck.ToString());

            var sections = string.Join(", ", info.Sections);
            var sectionsCheck = await _kernel.InvokeAsync("Validation", "check_sections_present", new() { ["sections"] = sections });
            if (!sectionsCheck.ToString().Contains("OK"))
                errors.Add(sectionsCheck.ToString());

            // Toxicity and illicit content check using a prompt
            var safetyPrompt = @"
                    Analyze the following text for toxicity or illicit content. 
                    If you find any toxic or illicit content, respond with 'TOXIC' or 'ILLICIT' and explain. 
                    Otherwise respond with 'SAFE'.

                    Text: {{$text}}";

            var safetyResult = await _kernel.InvokePromptAsync(safetyPrompt, new() { ["text"] = fullText });
            var safety = safetyResult.ToString();
            if (safety.Contains("TOXIC")) result.HasToxicity = true;
            if (safety.Contains("ILLICIT")) result.HasIllicitContent = true;

            result.Errors = errors;
            result.IsValid = !errors.Any() && !result.HasToxicity && !result.HasIllicitContent;

            return result;
        }
    }
}
