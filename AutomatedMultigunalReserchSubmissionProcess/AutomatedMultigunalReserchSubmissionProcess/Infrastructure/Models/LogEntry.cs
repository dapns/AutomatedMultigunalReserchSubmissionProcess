namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string User { get; set; }            
        public string Action { get; set; }
        public string Details { get; set; }
    }
}
