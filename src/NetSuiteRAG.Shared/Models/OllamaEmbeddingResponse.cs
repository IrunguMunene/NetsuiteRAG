namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Response model from Ollama embedding API.
/// </summary>
public record OllamaEmbeddingResponse
{
    /// <summary>
    /// Gets the embedding vector.
    /// </summary>
    public required float[] Embedding { get; init; }
}
