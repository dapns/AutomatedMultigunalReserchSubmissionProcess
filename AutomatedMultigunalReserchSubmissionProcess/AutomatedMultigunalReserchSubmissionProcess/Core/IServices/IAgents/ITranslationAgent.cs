using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;

namespace AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents
{
    public interface ITranslationAgent
    {
        Task<Submission> TranslateAsync(Submission submission);
    }
}
