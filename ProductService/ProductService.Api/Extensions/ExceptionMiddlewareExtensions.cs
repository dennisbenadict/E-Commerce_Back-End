using ProductService.Api.Middlewares;

namespace ProductService.Api.Extensions;

public static class ExceptionMiddlewareExtensions
{
	public static IApplicationBuilder UseGlobalException(this IApplicationBuilder app)
	{
		return app.UseMiddleware<ErrorHandlingMiddleware>();
	}
}
