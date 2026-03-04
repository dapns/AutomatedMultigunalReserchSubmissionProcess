using AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents
{
    public class HumanFeedbackAgent : IHumanFeedbackAgent
    {
        private readonly ILogger<HumanFeedbackAgent> _logger;

        public HumanFeedbackAgent(ILogger<HumanFeedbackAgent> logger)
        {
            _logger = logger;
        }

        public Task<bool> NeedsReviewAsync(Submission submission)
        {
            // If validation errors or toxicity, or if deviation <25% (we'll simulate)
            if (submission.ValidationResult == null) return Task.FromResult(false);

            // Simulate a confidence check: if any error or toxicity, review needed.
            bool review = !submission.ValidationResult.IsValid
                          || submission.ValidationResult.HasToxicity
                          || submission.ValidationResult.HasIllicitContent;

            // Also if deviation <25% (mocked: always false for demo)
            return Task.FromResult(review);
        }

        public Task ApplyCorrectionAsync(Guid submissionId, string field, string correctedValue)
        {
            var submission = InMemoryStore.Submissions.FirstOrDefault(s => s.Id == submissionId);
            if (submission == null) return Task.CompletedTask;

            var original = field switch
            {
                "Title" => submission.ExtractedInfo?.Title,
                "Abstract" => submission.ExtractedInfo?.Abstract,
                // etc.
                _ => null
            };

            // Apply correction (simplified)
            switch (field)
            {
                case "Title":
                    if (submission.ExtractedInfo != null) submission.ExtractedInfo.Title = correctedValue;
                    break;
                case "Abstract":
                    if (submission.ExtractedInfo != null) submission.ExtractedInfo.Abstract = correctedValue;
                    break;
                    // ... handle other fields
            }

            submission.HumanFeedback.Add(new FeedbackEntry
            {
                SubmissionId = submissionId,
                Field = field,
                OriginalValue = original,
                CorrectedValue = correctedValue,
                Timestamp = DateTime.UtcNow
            });

            InMemoryStore.Logs.Add(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                User = "admin",
                Action = "Correction",
                Details = $"Field '{field}' corrected for submission {submissionId}"
            });

            return Task.CompletedTask;
        }
    }
}
