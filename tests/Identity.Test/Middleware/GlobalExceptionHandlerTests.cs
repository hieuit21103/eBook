using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Middleware;
using NSubstitute;
using Xunit;

namespace Identity.Test.Middleware;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task Invoke_WhenNoException_ShouldCallNext()
    {
        // Arrange
        var next = Substitute.For<RequestDelegate>();
        var middleware = new GlobalExceptionHandler(next);
        var context = new DefaultHttpContext();

        // Act
        await middleware.Invoke(context);

        // Assert
        await next.Received(1).Invoke(context);
        context.Response.StatusCode.Should().Be(200); // Default
    }

    [Theory]
    [MemberData(nameof(GetExceptions))]
    public async Task Invoke_WhenExceptionOccurs_ShouldReturnExpectedStatusCode(Exception exception, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var next = Substitute.For<RequestDelegate>();
        next.When(x => x.Invoke(Arg.Any<HttpContext>())).Do(x => throw exception);
        
        var middleware = new GlobalExceptionHandler(next);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Response.StatusCode.Should().Be((int)expectedStatusCode);
        context.Response.ContentType.Should().Be("application/json");
        
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(context.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        
        responseBody.Should().Contain("\"Success\":false");
        responseBody.Should().Contain(exception.Message);
    }

    public static IEnumerable<object[]> GetExceptions()
    {
        yield return new object[] { new KeyNotFoundException("Not found"), HttpStatusCode.NotFound };
        yield return new object[] { new UnauthorizedAccessException("Forbidden"), HttpStatusCode.Forbidden };
        yield return new object[] { new InvalidOperationException("Invalid op"), HttpStatusCode.BadRequest };
        yield return new object[] { new ArgumentException("Bad arg"), HttpStatusCode.BadRequest };
        yield return new object[] { new AuthenticationFailureException("Auth failed"), HttpStatusCode.Unauthorized };
        yield return new object[] { new Exception("Internal error"), HttpStatusCode.InternalServerError };
    }
}
