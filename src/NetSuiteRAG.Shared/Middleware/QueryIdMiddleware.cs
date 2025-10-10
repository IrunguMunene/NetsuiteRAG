using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace NetSuiteRAG.Shared.Middleware;

/// <summary>
/// Middleware that ensures every request has a queryId for correlation across logs and traces
/// </summary>
public class QueryIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string QueryIdHeader = "X-Query-Id";

    public QueryIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or create queryId
        var queryId = context.Request.Headers[QueryIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");

        // Add to response headers
        context.Response.Headers[QueryIdHeader] = queryId;

        // Add to HttpContext items for easy access
        context.Items["QueryId"] = queryId;

        // Push to Serilog LogContext for automatic inclusion in all logs
        using (LogContext.PushProperty("QueryId", queryId))
        {
            await _next(context);
        }
    }
}
