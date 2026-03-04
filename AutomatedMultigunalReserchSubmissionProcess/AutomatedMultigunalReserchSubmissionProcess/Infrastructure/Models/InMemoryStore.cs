namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models
{
    public class InMemoryStore
    {
        public static List<Submission> Submissions { get; } = new();
        public static List<LogEntry> Logs { get; } = new();
    }
}
