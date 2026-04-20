using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineSurvey.Api.Middlewares;
using OnlineSurvey.Domain.Exceptions;

#pragma warning disable CA1873 // Moq Verify expressions are always evaluated

namespace OnlineSurvey.Api.Tests.Middlewares;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;

    public ExceptionHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        
        await middleware.InvokeAsync(context);

        
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenDomainException_ShouldReturn400()
    {
        
        var context = CreateHttpContext();
        var exceptionMessage = "Domain validation failed";
        RequestDelegate next = _ => throw new DomainException(exceptionMessage);

        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        
        await middleware.InvokeAsync(context);

        
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/problem+json");

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Bad Request");
        responseBody.Should().Contain(exceptionMessage);
    }

    [Fact]
    public async Task InvokeAsync_WhenArgumentException_ShouldReturn400()
    {
        
        var context = CreateHttpContext();
        var exceptionMessage = "Invalid argument";
        RequestDelegate next = _ => throw new ArgumentException(exceptionMessage);

        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        
        await middleware.InvokeAsync(context);

        
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Bad Request");
    }

    [Fact]
    public async Task InvokeAsync_WhenKeyNotFoundException_ShouldReturn404()
    {
        
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new KeyNotFoundException("Resource not found");

        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        
        await middleware.InvokeAsync(context);

        
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Not Found");
        responseBody.Should().Contain("Resource not found.");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_ShouldReturn500()
    {
        
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new InvalidOperationException("Something went wrong");

        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        
        await middleware.InvokeAsync(context);

        
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Internal Server Error");
        responseBody.Should().Contain("An unexpected error occurred.");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnhandledException_ShouldLogError()
    {
        
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Critical error");
        RequestDelegate next = _ => throw exception;

        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        
        await middleware.InvokeAsync(context);

        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenDomainException_ShouldLogWarning()
    {
        
        var context = CreateHttpContext();
        _loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        RequestDelegate next = _ => throw new DomainException("Validation error");

        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        
        await middleware.InvokeAsync(context);

        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ResponseBody_ShouldContainTraceId()
    {
        
        var context = CreateHttpContext();
        context.TraceIdentifier = "test-trace-123";
        RequestDelegate next = _ => throw new DomainException("Test error");

        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        
        await middleware.InvokeAsync(context);

        
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("test-trace-123");
    }

    [Fact]
    public async Task InvokeAsync_ResponseBody_ShouldContainProblemType()
    {
        
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new DomainException("Test error");

        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        
        await middleware.InvokeAsync(context);

        
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("https://tools.ietf.org/html/rfc7231");
    }

    [Fact]
    public async Task InvokeAsync_ResponseBody_ShouldBeCamelCase()
    {
        
        var context = CreateHttpContext();
        RequestDelegate next = _ => throw new DomainException("Test error");

        var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

        
        await middleware.InvokeAsync(context);

        
        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("\"traceId\"");
        responseBody.Should().Contain("\"status\"");
        responseBody.Should().NotContain("\"TraceId\"");
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
