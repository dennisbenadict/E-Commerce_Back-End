using System.Net;
using System.Text.Json;

namespace UserService.Api.Middlewares;

public class ErrorHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ErrorHandlingMiddleware> _logger;

	public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unhandled exception occurred");

			await HandleExceptionAsync(context, ex);
		}
	}

	private static Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";

		var status = exception switch
		{
			ArgumentException => HttpStatusCode.BadRequest,
			UnauthorizedAccessException => HttpStatusCode.Unauthorized,
			KeyNotFoundException => HttpStatusCode.NotFound,
			_ => HttpStatusCode.InternalServerError
		};

		context.Response.StatusCode = (int)status;

		var response = new
		{
			message = exception.Message,
			error = exception.GetType().Name,
			statusCode = context.Response.StatusCode
		};

		var json = JsonSerializer.Serialize(response);

		return context.Response.WriteAsync(json);
	}
}
