using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents
{
    public class IngestionAgent : IIngestionAgent
    {
        private readonly ILogger<IngestionAgent> _logger;
        public IngestionAgent(ILogger<IngestionAgent> logger)
        {
            _logger = logger;
        }

        public async Task<Submission> IngestAsync(IFormFile file)
        {
            _logger.LogInformation("Ingesting file: {FileName}", file.FileName);
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var submission = new Submission
            {
                FileName = file.FileName,
                FileContent = ms.ToArray()
            };
            InMemoryStore.Submissions.Add(submission);
            InMemoryStore.Logs.Add(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                User = "system",
                Action = "Ingest",
                Details = $"File {file.FileName} ingested."
            });
            return submission;
        }
    }
}
