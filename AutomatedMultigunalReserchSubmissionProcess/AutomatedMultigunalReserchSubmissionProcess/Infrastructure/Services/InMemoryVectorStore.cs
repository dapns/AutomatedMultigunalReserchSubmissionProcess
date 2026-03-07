using AutomatedMultigunalReserchSubmissionProcess.Core.IServices;
using AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Models;
using System.Collections.Concurrent;

namespace AutomatedMultigunalReserchSubmissionProcess.Infrastructure.Services
{
    public class InMemoryVectorStore : IVectorStore
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, VectorRecord>> _collections
            = new();

        public Task UpsertAsync(string collectionName, VectorRecord record)
        {
            if (string.IsNullOrWhiteSpace(collectionName)) throw new ArgumentException("collectionName required", nameof(collectionName));
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (string.IsNullOrWhiteSpace(record.Id)) throw new ArgumentException("record.Id required", nameof(record));

            var col = _collections.GetOrAdd(collectionName, _ => new ConcurrentDictionary<string, VectorRecord>());
            col[record.Id] = record;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<VectorRecord>> SearchAsync(string collectionName, IList<float> queryVector, int topN)
        {
            if (string.IsNullOrWhiteSpace(collectionName)) throw new ArgumentException("collectionName required", nameof(collectionName));
            if (queryVector == null) throw new ArgumentNullException(nameof(queryVector));
            if (!_collections.TryGetValue(collectionName, out var col) || col.Count == 0)
            {
                return Task.FromResult(Enumerable.Empty<VectorRecord>());
            }

            var results = col.Values
                .Where(r => r.Vector != null && r.Vector.Count == queryVector.Count)
                .Select(r => (Record: r, Score: CosineSimilarity(queryVector, r.Vector!)))
                .OrderByDescending(x => x.Score)
                .Take(Math.Max(0, topN))
                .Select(x => x.Record)
                .ToList()
                .AsEnumerable();

            return Task.FromResult(results);
        }

        private static double CosineSimilarity(IList<float> a, IList<float> b)
        {
            if (a == null || b == null) return 0.0;
            if (a.Count != b.Count) return 0.0;

            double dot = 0, na = 0, nb = 0;
            for (int i = 0; i < a.Count; i++)
            {
                var va = a[i];
                var vb = b[i];
                dot += va * vb;
                na += va * va;
                nb += vb * vb;
            }

            var denom = Math.Sqrt(na) * Math.Sqrt(nb);
            return denom == 0 ? 0.0 : dot / denom;
        }
    }
}
