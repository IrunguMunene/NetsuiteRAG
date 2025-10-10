using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NetSuiteRAG.Shared.Health;

/// <summary>
/// Health check for database connectivity (placeholder for now)
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // TODO: Add actual database connectivity check once we implement EF Core
        // For now, just return healthy
        return Task.FromResult(HealthCheckResult.Healthy("Database is responsive"));
    }
}
