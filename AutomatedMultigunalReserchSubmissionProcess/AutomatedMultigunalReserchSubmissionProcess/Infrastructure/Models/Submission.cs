using System.ComponentModel.DataAnnotations;
using static Microsoft.KernelMemory.Constants.CustomContext;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models
{
    public class Submission
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }          // Original file (PDF, DOCX, image)
        public string OriginalLanguage { get; set; }
        public string ExtractedText { get; set; }        // After OCR/text extraction
        public string EnglishTranslation { get; set; }   // After translation
        public ExtractedInfo ExtractedInfo { get; set; }
        public ValidationResult ValidationResult { get; set; }
        public Summary Summary { get; set; }
        public bool NeedsHumanReview { get; set; }
        public List<FeedbackEntry> HumanFeedback { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
