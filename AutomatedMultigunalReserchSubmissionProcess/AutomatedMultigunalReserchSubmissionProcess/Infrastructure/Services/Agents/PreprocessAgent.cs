using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using DocumentFormat.OpenXml.Packaging;
using LanguageDetection;
using System.Text;
using Tesseract;
using UglyToad.PdfPig;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents
{
    public class PreProcessAgent : IPreProcessAgent
    {
        private readonly ILogger<PreProcessAgent> _logger;
        private readonly IConfiguration _config;
        private readonly LanguageDetector _languageDetector;
        private readonly string? _tesseractDataPath;

        public PreProcessAgent(ILogger<PreProcessAgent> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            // Initialize language detector (LanguageDetection library)
            _languageDetector = new LanguageDetector();
            _languageDetector.AddAllLanguages(); // Adds all built‑in profiles

            // Read OCR settings
            var ocrEnabled = _config.GetValue<bool>("Ocr:Enabled");
            _tesseractDataPath = ocrEnabled ? _config["Ocr:TesseractDataPath"] : null;
        }

        public async Task<Submission> PreProcessAsync(Submission submission)
        {
            _logger.LogInformation("Pre‑processing submission {Id}: {FileName}", submission.Id, submission.FileName);

            try
            {
                // 1. Detect language from the raw content (best effort)
                submission.OriginalLanguage = await DetectLanguageAsync(submission.FileContent, submission.FileName);

                // 2. Extract text (OCR if needed)
                submission.ExtractedText = await ExtractTextAsync(submission.FileContent, submission.FileName);

                _logger.LogInformation("Pre‑processing completed for {Id}. Language: {Lang}, Text length: {Len}",
                    submission.Id, submission.OriginalLanguage, submission.ExtractedText?.Length ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during pre‑processing of submission {Id}", submission.Id);
                throw; // Rethrow – the orchestrator should handle it
            }

            return submission;
        }

        /// <summary>Detects the language of the document content.</summary>
        private async Task<string> DetectLanguageAsync(byte[] content, string fileName)
        {
            // For detection we need some sample text. First try to extract a small portion.
            string sample = await ExtractTextSampleAsync(content, fileName, maxLength: 500);
            if (string.IsNullOrWhiteSpace(sample))
            {
                _logger.LogWarning("Could not extract text sample for language detection, defaulting to 'en'");
                return "en";
            }

            try
            {
                // LanguageDetector is not thread‑safe; we wrap it.
                string? lang = await Task.Run(() => _languageDetector.Detect(sample));
                if (!string.IsNullOrEmpty(lang))
                {
                    _logger.LogDebug("Detected language '{Lang}' from sample", lang);
                    return lang;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Language detection failed, falling back to 'en'");
            }

            return "en";
        }

        /// <summary>Extracts a short text sample for language detection.</summary>
        private async Task<string> ExtractTextSampleAsync(byte[] content, string fileName, int maxLength)
        {
            // Use the same extraction logic but stop early
            string fullText = await ExtractTextAsync(content, fileName, maxLength);
            return fullText;
        }

        /// <summary>Extracts all text from the file (PDF, DOCX, or image).</summary>
        private async Task<string> ExtractTextAsync(byte[] content, string fileName, int? maxLength = null)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            string text = extension switch
            {
                ".pdf" => ExtractTextFromPdf(content),
                ".docx" => ExtractTextFromDocx(content),
                ".jpg" or ".jpeg" or ".png" or ".bmp" or ".tiff" => await ExtractTextFromImageAsync(content),
                _ => Encoding.UTF8.GetString(content) // fallback: treat as plain text
            };

            if (maxLength.HasValue && text.Length > maxLength.Value)
                text = text.Substring(0, maxLength.Value);

            return text;
        }
        private string ExtractTextFromPdf(byte[] pdfBytes)
        {
            using var stream = new MemoryStream(pdfBytes);
            using var pdf = PdfDocument.Open(stream);
            var sb = new StringBuilder();

            foreach (var page in pdf.GetPages())
            {
                sb.AppendLine(page.Text);
            }

            return sb.ToString();
        }

        /// <summary>Extract text from a DOCX file using Open XML SDK.</summary>
        private string ExtractTextFromDocx(byte[] docxBytes)
        {
            using var stream = new MemoryStream(docxBytes);
            using var wordDoc = WordprocessingDocument.Open(stream, false);
            var body = wordDoc.MainDocumentPart?.Document.Body;
            if (body == null) return string.Empty;

            return body.InnerText; // Simple extraction – may include control characters but works for demo
        }

        /// <summary>Extract text from an image using Tesseract OCR.</summary>
        private async Task<string> ExtractTextFromImageAsync(byte[] imageBytes)
        {
            if (!_config.GetValue<bool>("Ocr:Enabled") || string.IsNullOrEmpty(_tesseractDataPath))
            {
                _logger.LogWarning("OCR is disabled or data path not configured. Returning empty string.");
                return string.Empty;
            }

            return await Task.Run(() =>
            {
                using var engine = new TesseractEngine(_tesseractDataPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromMemory(imageBytes);
                using var page = engine.Process(img);
                return page.GetText();
            });
        }
    }
}