using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DefiPortfolioManager.Api.Middleware
{
    public class ApiPerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiPerformanceMiddleware> _logger;
        
        public ApiPerformanceMiddleware(RequestDelegate next, ILogger<ApiPerformanceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            
            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                var elapsedMs = sw.ElapsedMilliseconds;
                var statusCode = context.Response.StatusCode;
                var method = context.Request.Method;
                var path = context.Request.Path;
                
                var logLevel = elapsedMs switch
                {
                    < 500 => LogLevel.Debug,      // Fast response (< 500ms)
                    < 1000 => LogLevel.Information, // Normal response (< 1s)
                    < 3000 => LogLevel.Warning,     // Slow response (< 3s)
                    _ => LogLevel.Error            // Very slow response (>= 3s)
                };
                
                _logger.Log(
                    logLevel,
                    "API {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    method,
                    path,
                    statusCode,
                    elapsedMs);
                
                // Add performance tracking headers if in development or staging
                if (context.Response.HasStarted == false) 
                {
                    context.Response.Headers.Add("X-Response-Time-ms", elapsedMs.ToString());
                }
            }
        }
    }
} 