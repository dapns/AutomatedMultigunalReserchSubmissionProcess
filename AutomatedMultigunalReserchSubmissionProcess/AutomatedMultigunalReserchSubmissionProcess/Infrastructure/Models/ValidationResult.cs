namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool HasToxicity { get; set; }
        public bool HasIllicitContent { get; set; }
    }
}
