using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using Microsoft.SemanticKernel;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents
{
    public class TranslationAgent : ITranslationAgent
    {
        private readonly Kernel _kernel;
        private readonly ILogger<TranslationAgent> _logger;

        public TranslationAgent(Kernel kernel, ILogger<TranslationAgent> logger)
        {
            _kernel = kernel;
            _logger = logger;
        }

        public async Task<Submission> TranslateAsync(Submission submission)
        {
            if (submission.OriginalLanguage == "en")
            {
                submission.EnglishTranslation = submission.ExtractedText;
                return submission;
            }

            _logger.LogInformation("Translating from {lang} to English", submission.OriginalLanguage);

            // Use Semantic Kernel to translate
            var prompt = @"
Translate the following text from {{$sourceLang}} to English. 
Return only the translated text.

Text: {{$text}}";

            var result = await _kernel.InvokePromptAsync(prompt, new KernelArguments
            {
                ["sourceLang"] = submission.OriginalLanguage,
                ["text"] = submission.ExtractedText
            });

            submission.EnglishTranslation = result.ToString();
            return submission;
        }
    }
}
