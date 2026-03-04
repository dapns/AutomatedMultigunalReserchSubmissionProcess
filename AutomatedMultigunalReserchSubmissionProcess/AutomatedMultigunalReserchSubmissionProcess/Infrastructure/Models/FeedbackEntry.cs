namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models
{
    public class FeedbackEntry
    {
        public Guid SubmissionId { get; set; }
        public string Field { get; set; }            
        public string OriginalValue { get; set; }
        public string CorrectedValue { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
