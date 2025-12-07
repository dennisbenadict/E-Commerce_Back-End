using UserService.Api.Middlewares;

namespace UserService.Api.Extensions;

public static class MiddlewareExtensions
{
	public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
	{
		return app.UseMiddleware<ErrorHandlingMiddleware>();
	}
}
