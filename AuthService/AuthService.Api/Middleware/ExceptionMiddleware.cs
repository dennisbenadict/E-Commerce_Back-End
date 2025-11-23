using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

//public class ExceptionMiddleware : IMiddleware
//{
//    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
//    {
//        try
//        {
//            await next(context);
//        }
//        catch (Exception ex)
//        {
//            var statusCode = GetStatusCode(ex);

//            var response = new
//            {
//                success = false,
//                message = ex.Message,
//                error = ex.GetType().Name,
//                statusCode = statusCode
//            };

//            context.Response.StatusCode = statusCode;
//            context.Response.ContentType = "application/json";

//            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
//        }
//    }

//    private int GetStatusCode(Exception ex)
//    {
//        return ex switch
//        {
//            ArgumentNullException => (int)HttpStatusCode.BadRequest,
//            ArgumentException => (int)HttpStatusCode.BadRequest,
//            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
//            InvalidOperationException => (int)HttpStatusCode.Conflict,
//            KeyNotFoundException => (int)HttpStatusCode.NotFound,
//            _ => (int)HttpStatusCode.InternalServerError
//        };
//    }
//}

public class ExceptionMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("The response has already started, cannot write exception response.");
                return;
            }

            var statusCode = GetStatusCode(ex);

            var response = new
            {
                success = false,
                message = GetSafeMessage(ex),
                error = ex.GetType().Name,
                statusCode = statusCode
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }

    private static string GetSafeMessage(Exception ex)
    {
        return ex switch
        {
            ArgumentException => ex.Message,
            KeyNotFoundException => "Resource not found.",
            UnauthorizedAccessException => "Unauthorized.",
            InvalidOperationException => "Operation not allowed.",
            _ => "An unexpected error occurred."
        };
    }

    private static int GetStatusCode(Exception ex)
    {
        return ex switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            InvalidOperationException => (int)HttpStatusCode.Conflict,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
}

