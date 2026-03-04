using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;

namespace AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents
{
    public interface IIngestionAgent
    {
        Task<Submission> IngestAsync(IFormFile file);
    }
}
