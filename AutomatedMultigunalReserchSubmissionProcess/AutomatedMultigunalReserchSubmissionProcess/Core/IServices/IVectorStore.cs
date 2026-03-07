using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;

namespace AutomatedMultigunalReserchSubmissionProcess.Core.IServices
{
    public interface IVectorStore
    {
        Task UpsertAsync(string collectionName, VectorRecord record);
        Task<IEnumerable<VectorRecord>> SearchAsync(string collectionName, IList<float> vector, int topN);
    }
}
