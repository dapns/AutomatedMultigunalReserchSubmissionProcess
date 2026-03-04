using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents
{
    public class PreProcessAgent : IPreProcessAgent
    {
        private readonly ILogger<PreProcessAgent> _logger;
        private readonly IConfiguration _config;
        // Optionally inject Tesseract engine if OCR enabled

        public PreProcessAgent(ILogger<PreProcessAgent> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<Submission> PreProcessAsync(Submission submission)
        {
            _logger.LogInformation("Pre-processing submission {Id}", submission.Id);

            // 1. Detect language (mock: use a simple library or SK prompt)
            submission.OriginalLanguage = await DetectLanguageAsync(submission.FileContent);

            // 2. Extract text (if PDF/image use OCR, else read directly)
            submission.ExtractedText = await ExtractTextAsync(submission.FileContent, submission.FileName);

            // 3. Validate file type (mocked)
            // ...

            return submission;
        }

        private Task<string> DetectLanguageAsync(byte[] content)
        {
            // In a real app, you'd call a language detection model.
            // For demo, return "en" always.
            return Task.FromResult("en");
        }

        private async Task<string> ExtractTextAsync(byte[] content, string fileName)
        {
            // Simplified: if OCR enabled and image, mock OCR.
            // Otherwise, read as text.
            var extension = Path.GetExtension(fileName).ToLower();
            if (extension == ".pdf" || extension == ".png" || extension == ".jpg")
            {
                // Mock OCR result
                return "This is a sample extracted text from a scanned document. Title: AI in Healthcare. Authors: John Doe, Jane Smith.";
            }
            else
            {
                // Assume plain text or docx (simplified)
                return System.Text.Encoding.UTF8.GetString(content);
            }
        }
    }
}
