using System.Net.Http.Json;
using System.Text.Json;
using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Models;
using NetSuiteRAG.Shared.Monitoring;

namespace NetSuiteRAG.Api.Services.Implementations;

/// <summary>
/// Service for generating embeddings using Ollama.
/// </summary>
public class OllamaEmbeddingService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    MetricsCollector metrics,
    ILogger<OllamaEmbeddingService> logger) : IOllamaEmbeddingService
{
    private const string DefaultModel = "nomic-embed-text";
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 1000;

    /// <summary>
    /// Generates an embedding vector for the given text.
    /// </summary>
    public async Task<Result<float[]>> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result<float[]>.Failure("Text cannot be empty");
        }

        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("ollama.embedding.requests");

        try
        {
            var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
            var model = configuration["Ollama:EmbeddingModel"] ?? DefaultModel;

            var httpClient = httpClientFactory.CreateClient();
            var request = new OllamaEmbeddingRequest
            {
                Model = model,
                Prompt = text
            };

            var response = await httpClient.PostAsJsonAsync(
                $"{baseUrl}/api/embeddings",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                metrics.IncrementCounter("ollama.embedding.errors");
                logger.LogError("Ollama embedding failed: {StatusCode} - {Error}",
                    response.StatusCode, error);
                return Result<float[]>.Failure(
                    $"Ollama API error: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(
                cancellationToken: cancellationToken);

            if (result?.Embedding == null || result.Embedding.Length == 0)
            {
                metrics.IncrementCounter("ollama.embedding.errors");
                return Result<float[]>.Failure("Empty embedding returned from Ollama");
            }

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("ollama.embedding", elapsedMs);

            logger.LogDebug("Generated embedding for text (length: {Length}, dimensions: {Dimensions})",
                text.Length, result.Embedding.Length);

            return Result<float[]>.Success(result.Embedding);
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("ollama.embedding.errors");
            logger.LogError(ex, "Error generating embedding");
            return Result<float[]>.Failure($"Embedding generation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates embedding vectors for multiple texts in batch.
    /// </summary>
    public async Task<Result<List<float[]>>> GenerateBatchEmbeddingsAsync(
        List<string> texts,
        CancellationToken cancellationToken = default)
    {
        if (texts == null || texts.Count == 0)
        {
            return Result<List<float[]>>.Failure("Text list cannot be empty");
        }

        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("ollama.embedding.batch-requests");

        try
        {
            var embeddings = new List<float[]>();

            // Process texts sequentially with retry logic
            foreach (var text in texts)
            {
                var result = await GenerateEmbeddingWithRetryAsync(text, cancellationToken);

                if (!result.IsSuccess)
                {
                    metrics.IncrementCounter("ollama.embedding.batch-errors");
                    return Result<List<float[]>>.Failure(
                        $"Failed to generate embedding: {result.Error}");
                }

                embeddings.Add(result.Value!);
            }

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("ollama.embedding.batch", elapsedMs);

            logger.LogInformation("Generated {Count} embeddings in {ElapsedMs}ms",
                texts.Count, elapsedMs);

            return Result<List<float[]>>.Success(embeddings);
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("ollama.embedding.batch-errors");
            logger.LogError(ex, "Error generating batch embeddings");
            return Result<List<float[]>>.Failure($"Batch embedding failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Tests connection to Ollama service.
    /// </summary>
    public async Task<Result<bool>> TestConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
            var httpClient = httpClientFactory.CreateClient();

            var response = await httpClient.GetAsync($"{baseUrl}/api/tags", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Ollama connection test failed: {StatusCode}",
                    response.StatusCode);
                return Result<bool>.Failure(
                    $"Ollama not available: {response.StatusCode}");
            }

            logger.LogInformation("Ollama connection test successful");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error testing Ollama connection");
            return Result<bool>.Failure($"Connection test failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates an embedding with retry logic.
    /// </summary>
    private async Task<Result<float[]>> GenerateEmbeddingWithRetryAsync(
        string text,
        CancellationToken cancellationToken)
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            var result = await GenerateEmbeddingAsync(text, cancellationToken);

            if (result.IsSuccess)
            {
                return result;
            }

            if (attempt < MaxRetries)
            {
                logger.LogWarning(
                    "Embedding generation failed (attempt {Attempt}/{MaxRetries}), retrying...",
                    attempt, MaxRetries);

                await Task.Delay(RetryDelayMs * attempt, cancellationToken);
            }
        }

        return Result<float[]>.Failure($"Failed after {MaxRetries} attempts");
    }
}
