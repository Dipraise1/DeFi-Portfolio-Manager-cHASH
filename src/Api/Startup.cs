using System;
using System.Collections.Generic;
using System.Net.Http;
using DefiPortfolioManager.Api.Middleware;
using DefiPortfolioManager.Core.Interfaces;
using DefiPortfolioManager.Core.Settings;
using DefiPortfolioManager.Infrastructure.Services;
using DefiPortfolioManager.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

namespace DefiPortfolioManager.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure app settings from config
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DeFi Portfolio Manager API", Version = "v1" });
            });

            // CORS policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Register HttpClient
            services.AddHttpClient();

            // Register Redis (if enabled)
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var settings = provider.GetRequiredService<IOptions<AppSettings>>().Value;
                var logger = provider.GetRequiredService<ILogger<Startup>>();
                
                // Only create Redis connection if configured to use it
                if (settings.Cache.UseRedisCache)
                {
                    logger.LogInformation("Connecting to Redis at {connectionString}", settings.Redis.ConnectionString);
                    try
                    {
                        return ConnectionMultiplexer.Connect(settings.Redis.ConnectionString);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to connect to Redis, falling back to in-memory cache");
                        return null;
                    }
                }
                
                logger.LogInformation("Redis cache disabled, using in-memory cache");
                return null;
            });

            // Register cache factory and service
            services.AddSingleton<CacheFactory>();
            services.AddSingleton<ICacheService>(provider =>
            {
                var factory = provider.GetRequiredService<CacheFactory>();
                return factory.CreateCacheService();
            });

            // Register application services
            services.AddSingleton<IPriceService>(provider => 
            {
                var settings = provider.GetRequiredService<IOptions<AppSettings>>();
                var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
                
                // Configure the HTTP client with settings
                httpClient.BaseAddress = new Uri(settings.Value.ExternalApis.CoinGeckoBaseUrl);
                
                return new CoinGeckoPriceService(
                    httpClient,
                    provider.GetRequiredService<ICacheService>(),
                    settings);
            });
                
            // Register blockchain services
            services.AddSingleton<IBlockchainService>(provider =>
            {
                return new EthereumBlockchainService(
                    provider.GetRequiredService<IPriceService>());
            });
            
            // Register yield service
            services.AddSingleton<IYieldService>(provider =>
            {
                return new DeFiYieldService(
                    provider.GetRequiredService<IEnumerable<IBlockchainService>>(),
                    provider.GetRequiredService<IPriceService>());
            });
            
            // Register portfolio service
            services.AddSingleton<IPortfolioService>(provider =>
            {
                var settings = provider.GetRequiredService<IOptions<AppSettings>>().Value;
                return new PortfolioService(
                    provider.GetRequiredService<IEnumerable<IBlockchainService>>(),
                    provider.GetRequiredService<IPriceService>(),
                    provider.GetRequiredService<IYieldService>(),
                    provider.GetRequiredService<ICacheService>());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation("Starting application in {environment} environment", env.EnvironmentName);
            
            if (env.IsDevelopment())
            {
                logger.LogInformation("Configuring for development environment");
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DeFi Portfolio Manager API v1"));
            }
            else
            {
                logger.LogInformation("Configuring for production environment");
                // In production, use our custom error handler
                app.UseGlobalErrorHandling();
            }

            // Add performance tracking middleware
            app.UseApiPerformanceTracking();

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseCors("AllowAll");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
            logger.LogInformation("Application startup complete");
        }
    }
}