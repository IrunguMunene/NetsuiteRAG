namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Request model for Ollama embedding API.
/// </summary>
public record OllamaEmbeddingRequest
{
    /// <summary>
    /// Gets the model name to use for embeddings.
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Gets the prompt text to generate embeddings for.
    /// </summary>
    public required string Prompt { get; init; }
}
