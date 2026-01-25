using BlogApp1.Shared;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Text.Json;
using Qdrant.Client;
using Qdrant.Client.Grpc;
namespace BlogApp1.Server.Services
{
    

    public class RetrievalService
    {
        private readonly QdrantClient _qdrant;
        private const string COLLECTION = "rag_blog_chunks";

        public RetrievalService(QdrantClient qdrant)
        {
            _qdrant = qdrant;
        }

        public async Task<List<RagChunkResult>> RetrieveAsync(
            string query,
            float[] queryVector)
        {
            // 1️⃣ Dense ANN
            var dense = await DenseSearch(queryVector, 20);

            // 2️⃣ Re-ranking
            var reranked = await Rerank(query, dense);

            // 3️⃣ Sparse BM25
            var sparse = await SparseSearch(query);

            // 4️⃣ Merge dense + sparse
            var merged = MergeResults(reranked, sparse);

            // 5️⃣ Final re-rank after merge
            var finalRanked = await Rerank(query, merged);

            return finalRanked.Take(5).ToList();
        }

        // ---------------- METHODS ----------------

        // Dense ANN
        private async Task<List<RagChunkResult>>
            DenseSearch(float[] vector, int limit)
        {
            var res = await _qdrant.SearchAsync(
                collectionName: COLLECTION,
                vector: vector,
                filter: null,
                limit: (ulong)limit
            );

            return res.Select(p => new RagChunkResult
            {
                Text = p.Payload["text"].StringValue,
                Score = p.Score
            }).ToList();
        }

        // 🔥 Re-ranking (simple cross scoring for now)
        private async Task<List<RagChunkResult>>
            Rerank(string query,
                   List<RagChunkResult> chunks)
        {
            // temporary logic:
            // boost score if query words appear
            await Task.Delay(1);

            foreach (var c in chunks)
            {
                if (c.Text
                    .ToLower()
                    .Contains(query.ToLower()))
                {
                    c.Score += 0.2f;
                }
            }

            return chunks
                .OrderByDescending(x => x.Score)
                .ToList();
        }

        // Sparse retrieval (BM25 placeholder)
        private async Task<List<RagChunkResult>>
            SparseSearch(string query)
        {
            // Later integrate Elastic/Lucene
            await Task.Delay(1);

            return new List<RagChunkResult>();
        }

        // Merge dense + sparse
        private List<RagChunkResult>
            MergeResults(
                List<RagChunkResult> dense,
                List<RagChunkResult> sparse)
        {
            return dense
                .Concat(sparse)
                .GroupBy(x => x.Text)
                .Select(g => g.First())
                .ToList();
        }
    }

}
