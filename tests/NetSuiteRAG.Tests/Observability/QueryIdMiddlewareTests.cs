using Microsoft.AspNetCore.Http;
using NetSuiteRAG.Shared.Middleware;
using Xunit;

namespace NetSuiteRAG.Tests.Observability;

public class QueryIdMiddlewareTests
{
    [Fact]
    public async Task QueryIdMiddleware_ShouldGenerateQueryId_WhenNotProvided()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new QueryIdMiddleware(async (ctx) =>
        {
            await Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-Query-Id"));
        Assert.NotEmpty(context.Response.Headers["X-Query-Id"].ToString());
    }

    [Fact]
    public async Task QueryIdMiddleware_ShouldUseProvidedQueryId_WhenPresent()
    {
        // Arrange
        var providedQueryId = "test-query-id-123";
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Query-Id"] = providedQueryId;

        var middleware = new QueryIdMiddleware(async (ctx) =>
        {
            await Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(providedQueryId, context.Response.Headers["X-Query-Id"].ToString());
    }

    [Fact]
    public async Task QueryIdMiddleware_ShouldAddQueryIdToHttpContextItems()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var middleware = new QueryIdMiddleware(async (ctx) =>
        {
            await Task.CompletedTask;
        });

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Items.ContainsKey("QueryId"));
        Assert.NotNull(context.Items["QueryId"]);
        Assert.IsType<string>(context.Items["QueryId"]);
    }
}
