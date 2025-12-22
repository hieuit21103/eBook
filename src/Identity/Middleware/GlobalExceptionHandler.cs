using System.Net;
using System.Text.Json;
using Application.DTOs;

namespace Middleware;
public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandler(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            switch (error)
            {
                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                case InvalidOperationException:
                case ArgumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            List<string> errors = new List<string> { error?.Message ?? "Unknown error" };
            var result = JsonSerializer.Serialize(
                new 
                {
                    Success = false,
                    Message = "An error occurred while processing your request.",
                    Errors = errors
                }
            );
            await response.WriteAsync(result);
        }
    }
}