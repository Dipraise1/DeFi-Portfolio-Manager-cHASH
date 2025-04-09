using Microsoft.AspNetCore.Builder;

namespace DefiPortfolioManager.Api.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseApiPerformanceTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiPerformanceMiddleware>();
        }
        
        public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
} 