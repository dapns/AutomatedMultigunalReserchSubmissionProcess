using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;

namespace AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents
{
    public interface IValidationAgent
    {
        Task<ValidationResult> ValidateAsync(ExtractedInfo info, string fullText);
    }
}
