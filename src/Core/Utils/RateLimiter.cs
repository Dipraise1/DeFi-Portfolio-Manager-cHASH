using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DefiPortfolioManager.Core.Utils
{
    /// <summary>
    /// Implements rate limiting for API calls to prevent hitting rate limits
    /// </summary>
    public class RateLimiter
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _apiSemaphores = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastCallTime = new();
        private readonly ConcurrentDictionary<string, TimeSpan> _minInterval = new();

        /// <summary>
        /// Configures rate limiting for a specific API
        /// </summary>
        public void Configure(string apiName, int maxRequestsPerSecond)
        {
            // Create or get semaphore for the API
            var semaphore = _apiSemaphores.GetOrAdd(apiName, _ => new SemaphoreSlim(1, 1));
            
            // Calculate minimum interval between calls based on max requests per second
            var interval = TimeSpan.FromMilliseconds(1000.0 / maxRequestsPerSecond);
            _minInterval[apiName] = interval;
        }

        /// <summary>
        /// Executes an API call with rate limiting
        /// </summary>
        public async Task<T> ExecuteAsync<T>(string apiName, Func<Task<T>> apiCall)
        {
            // Get or create the semaphore
            var semaphore = _apiSemaphores.GetOrAdd(apiName, _ => new SemaphoreSlim(1, 1));
            
            try
            {
                // Wait for semaphore
                await semaphore.WaitAsync();
                
                // Check if we need to delay before making the call
                if (_lastCallTime.TryGetValue(apiName, out var lastCall))
                {
                    var minInterval = _minInterval.GetOrAdd(apiName, _ => TimeSpan.FromMilliseconds(500));
                    var timeSinceLastCall = DateTime.UtcNow - lastCall;
                    
                    if (timeSinceLastCall < minInterval)
                    {
                        var delayTime = minInterval - timeSinceLastCall;
                        await Task.Delay(delayTime);
                    }
                }
                
                // Record the call time
                _lastCallTime[apiName] = DateTime.UtcNow;
                
                // Make the API call
                return await apiCall();
            }
            finally
            {
                // Release the semaphore
                semaphore.Release();
            }
        }
    }
} 