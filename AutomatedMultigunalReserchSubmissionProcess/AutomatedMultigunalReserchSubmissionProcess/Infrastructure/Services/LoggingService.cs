using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services
{
    public class LoggingService
    {
        public void Log(string user, string action, string details)
        {
            InMemoryStore.Logs.Add(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                User = user,
                Action = action,
                Details = details
            });
        }

        public IEnumerable<LogEntry> GetLogs() => InMemoryStore.Logs;
    }
}
