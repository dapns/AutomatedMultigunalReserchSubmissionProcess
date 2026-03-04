using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;

namespace AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents
{
    public interface ISummaryAgent
    {
        Task<Summary> GenerateSummaryAsync(Submission submission);
    }
}
