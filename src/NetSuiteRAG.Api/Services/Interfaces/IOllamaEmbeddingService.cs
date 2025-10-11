using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Services.Interfaces;

/// <summary>
/// Service for generating embeddings using Ollama.
/// </summary>
public interface IOllamaEmbeddingService
{
    /// <summary>
    /// Generates an embedding vector for the given text.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the embedding vector (768 dimensions).</returns>
    Task<Result<float[]>> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embedding vectors for multiple texts in batch.
    /// </summary>
    /// <param name="texts">The texts to embed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing list of embedding vectors.</returns>
    Task<Result<List<float[]>>> GenerateBatchEmbeddingsAsync(
        List<string> texts,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests connection to Ollama service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating connection status.</returns>
    Task<Result<bool>> TestConnectionAsync(
        CancellationToken cancellationToken = default);
}
