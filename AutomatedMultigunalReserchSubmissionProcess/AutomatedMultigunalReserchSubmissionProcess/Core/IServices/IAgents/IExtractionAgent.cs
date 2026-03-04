using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;

namespace AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents
{
    public interface IExtractionAgent
    {
        Task<ExtractedInfo> ExtractAsync(string englishText);
    }
}
