using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;

namespace AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents
{
    public interface IPreProcessAgent
    {
        Task<Submission> PreProcessAsync(Submission submission);
    }
}
