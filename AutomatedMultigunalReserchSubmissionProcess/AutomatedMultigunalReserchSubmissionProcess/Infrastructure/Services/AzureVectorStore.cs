using AutomatedMultigunalReserchSubmissionProcess.Core.IServices;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services.Agents;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services
{
    public class AzureVectorStore : IVectorStore
    {
        private readonly SearchClient _searchClient;
        private const string VectorFieldName = "embedding";

        public AzureVectorStore(SearchClient searchClient)
        {
            _searchClient = searchClient;
        }

        public async Task UpsertAsync(string collectionName, VectorRecord record)
        {
            var doc = new
            {
                id = record.Id,
                text = record.Metadata?["text"],
                submissionId = record.Metadata?["submissionId"],
                chunkIndex = record.Metadata?["chunkIndex"],
                title = record.Metadata?["title"],
                embedding = record.Vector.ToArray() // must match Azure Cognitive Search vector type
            };


            await _searchClient.MergeOrUploadDocumentsAsync(new[] { doc });
        }

        public async Task<IEnumerable<VectorRecord>> SearchAsync(string collectionName, IList<float> queryVector, int topN)
        {
            var vectorQuery = new VectorizedQuery(queryVector.ToArray())
            {
                KNearestNeighborsCount = topN,
                Fields = { "embedding" }
            };

            var options = new SearchOptions
            {
                Size = topN,
                VectorSearch = new VectorSearchOptions
                {
                    Queries = { vectorQuery }
                }
            };

            var response = await _searchClient.SearchAsync<SearchDocument>("*", options);

            var results = new List<VectorRecord>();

            await foreach (var result in response.Value.GetResultsAsync())
            {
                var doc = result.Document;

                var record = new VectorRecord
                {
                    Id = doc["id"]?.ToString() ?? "",
                    Metadata = new Dictionary<string, object>
                    {
                        ["text"] = doc["text"],
                        ["submissionId"] = doc["submissionId"],
                        ["chunkIndex"] = doc["chunkIndex"],
                        ["title"] = doc["title"]
                    }
                };

                results.Add(record);
            }

            return results;
        }
    }
}