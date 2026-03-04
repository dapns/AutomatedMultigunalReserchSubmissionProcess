using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using Microsoft.SemanticKernel.Memory;

namespace AutomatedMultigunalReserchSubmissionProcess.Core.IServices.IAgents
{
    public interface IRAGAgent
    {
        Task IndexSubmissionAsync(Submission submission);
        Task<IEnumerable<string>> SearchAsync(string query, int topN = 5);
    }

}
