using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;

namespace AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents
{
    public interface IHumanFeedbackAgent
    {
        Task<bool> NeedsReviewAsync(Submission submission);
        Task ApplyCorrectionAsync(Guid submissionId, string field, string correctedValue);
    }
}
