using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BusBuddy.Services
{
    /// <summary>
    /// Highly optimized XAI Service for Bus Buddy with advanced performance features:
    /// - Token efficiency and budget management
    /// - Response caching with smart invalidation
    /// - Request batching and parallel processing
    /// - Connection pooling and retry policies
    /// - Performance monitoring and metrics
    /// - Response streaming for large requests
    /// </summary>
    public class OptimizedXAIService : IDisposable
    {
        #region Private Fields

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<OptimizedXAIService> _logger;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly TokenBudgetManager _budgetManager;
        private readonly ConcurrentQueue<(string Prompt, TaskCompletionSource<string> Tcs)> _requestQueue;
        private readonly SemaphoreSlim _batchSemaphore;
        private readonly SemaphoreSlim _concurrencySemaphore;
        private readonly System.Threading.Timer _batchTimer;
        private readonly int _retryPolicy = 3; // Simple retry count for requests

        // Performance Monitoring
        private readonly Meter _meter;
        private readonly Histogram<double> _requestDuration;
        private readonly Counter<long> _tokenUsage;
        private readonly Counter<long> _requestCount;
        private readonly Counter<long> _errorCount;
        private readonly Counter<long> _cacheHits;

        // Configuration Constants
        private const int MAX_BATCH_SIZE = 10;
        private const int BATCH_DELAY_MS = 1000;
        private const int MAX_CONCURRENT_REQUESTS = 5;
        private const string API_ENDPOINT = "https://api.x.ai/v1/chat/completions";
        private const string CACHE_PREFIX = "xai_response_";
        private readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(30);

        #endregion

        #region Constructor

        public OptimizedXAIService(IConfiguration configuration, IMemoryCache cache, ILogger<OptimizedXAIService> logger)
        {
            _cache = cache;
            _logger = logger;

            // Load configuration
            var xaiConfig = configuration.GetSection("XAI");
            _apiKey = xaiConfig["ApiKey"] ?? throw new ArgumentException("XAI ApiKey not configured");
            _model = xaiConfig["DefaultModel"] ?? "grok-3-latest";

            var maxTokensPerDay = xaiConfig.GetValue<int>("MaxTokensPerDay", 100000);
            _budgetManager = new TokenBudgetManager(maxTokensPerDay, TimeSpan.FromDays(1));

            // Initialize queues and semaphores
            _requestQueue = new ConcurrentQueue<(string, TaskCompletionSource<string>)>();
            _batchSemaphore = new SemaphoreSlim(1, 1);
            _concurrencySemaphore = new SemaphoreSlim(MAX_CONCURRENT_REQUESTS, MAX_CONCURRENT_REQUESTS);

            // Configure HttpClient with connection pooling
            var handler = new HttpClientHandler
            {
                MaxConnectionsPerServer = 100
            };
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            // Note: Retry policy implementation would require Polly package
            // For now, implement basic retry in the request methods

            // Initialize performance monitoring
            _meter = new Meter("BusBuddy.XAIService");
            _requestDuration = _meter.CreateHistogram<double>("request_duration_seconds", "s", "Duration of API requests");
            _tokenUsage = _meter.CreateCounter<long>("token_usage", "tokens", "Tokens used per request");
            _requestCount = _meter.CreateCounter<long>("request_total", "requests", "Total number of requests");
            _errorCount = _meter.CreateCounter<long>("error_total", "errors", "Total number of failed requests");
            _cacheHits = _meter.CreateCounter<long>("cache_hits", "hits", "Cache hit count");

            // Start batch processing timer
            _batchTimer = new System.Threading.Timer(async _ => await ProcessBatchAsync(), null, TimeSpan.FromMilliseconds(BATCH_DELAY_MS), TimeSpan.FromMilliseconds(BATCH_DELAY_MS));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Process a single request with optimization features
        /// </summary>
        public async Task<string> ProcessRequestAsync(string prompt, string? context = null)
        {
            _requestCount.Add(1);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var optimizedPrompt = OptimizePrompt(prompt, context);
                int estimatedTokens = EstimateTokens(optimizedPrompt);

                if (!_budgetManager.CanProcess(estimatedTokens))
                {
                    _errorCount.Add(1);
                    throw new InvalidOperationException("Token budget exceeded for the current period");
                }

                var response = await GetCachedOrExecuteAsync(optimizedPrompt);

                _requestDuration.Record(stopwatch.Elapsed.TotalSeconds);
                return response;
            }
            catch (Exception ex)
            {
                _errorCount.Add(1);
                _logger.LogError(ex, "Error processing XAI request");
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        /// <summary>
        /// Process multiple requests in parallel with batching
        /// </summary>
        public async Task<IEnumerable<string>> ProcessBatchRequestsAsync(IEnumerable<string> prompts)
        {
            var tasks = prompts.Select(async prompt =>
            {
                await _concurrencySemaphore.WaitAsync();
                try
                {
                    return await ProcessRequestAsync(prompt);
                }
                finally
                {
                    _concurrencySemaphore.Release();
                }
            });

            return await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Stream large responses for better user experience
        /// </summary>
        public async IAsyncEnumerable<string> StreamResponseAsync(string prompt)
        {
            var optimizedPrompt = OptimizePrompt(prompt);
            int estimatedTokens = EstimateTokens(optimizedPrompt);

            if (!_budgetManager.CanProcess(estimatedTokens))
            {
                throw new InvalidOperationException("Token budget exceeded for the current period");
            }

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are an AI assistant for Bus Buddy transportation system." },
                    new { role = "user", content = optimizedPrompt }
                },
                stream = true,
                temperature = 0.3,
                max_tokens = 2000
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(API_ENDPOINT, content);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            int totalTokens = 0;
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrEmpty(line) || !line.StartsWith("data: ")) continue;

                var data = line.Substring(6); // Remove "data: " prefix
                if (data == "[DONE]") break;

                // Parse JSON outside try-catch to allow yield
                StreamResponse? chunk = null;
                try
                {
                    chunk = JsonSerializer.Deserialize<StreamResponse>(data);
                }
                catch (JsonException)
                {
                    // Skip malformed JSON chunks
                    continue;
                }

                // Yield outside try-catch
                if (chunk?.Choices?.Length > 0 && chunk.Choices[0].Delta?.Content is string deltaContent && !string.IsNullOrEmpty(deltaContent))
                {
                    totalTokens += EstimateTokens(deltaContent);
                    yield return deltaContent;
                }
                continue;
            }

            // Record final token usage
            _budgetManager.RecordUsage(totalTokens);
            _tokenUsage.Add(totalTokens);
        }

        /// <summary>
        /// Get current performance metrics
        /// </summary>
        public PerformanceMetrics GetPerformanceMetrics()
        {
            // Note: .NET System.Diagnostics.Metrics doesn't provide direct access to current values
            // This is a temporary implementation - proper metrics collection would need a separate tracking system
            return new PerformanceMetrics
            {
                TotalRequests = 0, // TODO: Implement proper counter tracking
                TotalErrors = 0, // TODO: Implement proper counter tracking  
                TotalTokensUsed = 0, // TODO: Implement proper counter tracking
                CacheHitCount = 0, // TODO: Implement proper counter tracking
                RemainingTokenBudget = _budgetManager.GetRemainingBudget(),
                AverageResponseTime = 0 // TODO: Implement proper histogram tracking
            };
        }

        #endregion

        #region Private Optimization Methods

        /// <summary>
        /// Optimize prompts for token efficiency
        /// </summary>
        private string OptimizePrompt(string userInput, string? context = null)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return string.Empty;

            // Remove unnecessary whitespace and normalize
            var optimized = userInput.Trim()
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("  ", " ");

            // Use shorthand for common Bus Buddy queries
            optimized = optimized
                .Replace("What time does the next bus arrive?", "Next bus time?")
                .Replace("Where is the nearest bus stop?", "Nearest stop?")
                .Replace("How long until the bus arrives?", "Bus ETA?")
                .Replace("What is the bus schedule?", "Schedule?");

            // Add context only if relevant and not redundant
            if (!string.IsNullOrEmpty(context) && !optimized.Contains(context, StringComparison.OrdinalIgnoreCase))
            {
                optimized = $"{context}: {optimized}";
            }

            // Add Bus Buddy context prefix only if necessary
            if (!optimized.Contains("bus", StringComparison.OrdinalIgnoreCase))
            {
                optimized = $"BusBuddy: {optimized}";
            }

            return optimized;
        }

        /// <summary>
        /// Estimate token count for budget management
        /// </summary>
        private int EstimateTokens(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // Rough estimation: ~4 characters per token for English text
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        /// <summary>
        /// Get cached response or execute API request
        /// </summary>
        private async Task<string> GetCachedOrExecuteAsync(string prompt)
        {
            var cacheKey = ComputeCacheKey(prompt);

            if (_cache.TryGetValue(cacheKey, out string? cachedResponse) && cachedResponse != null)
            {
                _cacheHits.Add(1);
                _logger.LogDebug("Cache hit for prompt hash: {CacheKey}", cacheKey);
                return cachedResponse;
            }

            var response = await ExecuteApiRequestAsync(prompt);

            // Cache the response with TTL
            _cache.Set(cacheKey, response, CACHE_DURATION);
            _logger.LogDebug("Cached response for prompt hash: {CacheKey}", cacheKey);

            return response;
        }

        /// <summary>
        /// Execute the actual API request with retry policy
        /// </summary>
        private async Task<string> ExecuteApiRequestAsync(string prompt)
        {
            Exception? lastException = null;

            for (int attempt = 0; attempt < _retryPolicy; attempt++)
            {
                try
                {
                    var requestBody = new
                    {
                        model = _model,
                        messages = new[]
                        {
                            new { role = "system", content = "You are an AI assistant for Bus Buddy, a school transportation management system. Provide concise, actionable responses." },
                            new { role = "user", content = prompt }
                        },
                        temperature = 0.3,
                        max_tokens = 1000
                    };

                    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(API_ENDPOINT, content);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonSerializer.Deserialize<XAIResponse>(responseContent);

                    if (jsonResponse?.Choices?.Length > 0)
                    {
                        var responseText = jsonResponse.Choices[0].Message?.Content ?? string.Empty;

                        // Record actual token usage if available
                        if (jsonResponse.Usage != null)
                        {
                            _budgetManager.RecordUsage(jsonResponse.Usage.TotalTokens);
                            _tokenUsage.Add(jsonResponse.Usage.TotalTokens);
                        }
                        else
                        {
                            // Fallback to estimation
                            var estimatedTokens = EstimateTokens(prompt + responseText);
                            _budgetManager.RecordUsage(estimatedTokens);
                            _tokenUsage.Add(estimatedTokens);
                        }

                        return responseText;
                    }

                    throw new InvalidOperationException("No valid response received from XAI API");
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "API request attempt {Attempt} failed", attempt + 1);

                    if (attempt < _retryPolicy - 1)
                    {
                        await Task.Delay(1000 * (attempt + 1)); // Exponential backoff
                    }
                }
            }

            throw new InvalidOperationException($"All retry attempts failed. Last error: {lastException?.Message}", lastException);
        }

        /// <summary>
        /// Process batched requests for efficiency
        /// </summary>
        private async Task ProcessBatchAsync()
        {
            if (!await _batchSemaphore.WaitAsync(100))
                return;

            try
            {
                var batch = new List<(string Prompt, TaskCompletionSource<string> Tcs)>();

                while (_requestQueue.TryDequeue(out var request) && batch.Count < MAX_BATCH_SIZE)
                {
                    batch.Add(request);
                }

                if (batch.Count == 0) return;

                // Process batch requests in parallel with concurrency limits
                var tasks = batch.Select(async item =>
                {
                    await _concurrencySemaphore.WaitAsync();
                    try
                    {
                        var result = await GetCachedOrExecuteAsync(item.Prompt);
                        item.Tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        item.Tcs.SetException(ex);
                    }
                    finally
                    {
                        _concurrencySemaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
            }
            finally
            {
                _batchSemaphore.Release();
            }
        }

        /// <summary>
        /// Compute cache key from prompt
        /// </summary>
        private static string ComputeCacheKey(string prompt)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(prompt));
            return Convert.ToBase64String(hash);
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            _httpClient?.Dispose();
            _batchSemaphore?.Dispose();
            _concurrencySemaphore?.Dispose();
            _batchTimer?.Dispose();
            _meter?.Dispose();
        }

        #endregion
    }

    #region Supporting Classes

    /// <summary>
    /// Token budget manager for cost control
    /// </summary>
    public class TokenBudgetManager
    {
        private readonly int _maxTokensPerPeriod;
        private readonly TimeSpan _period;
        private int _usedTokens;
        private DateTime _periodStart;
        private readonly object _lock = new();

        public TokenBudgetManager(int maxTokensPerPeriod, TimeSpan period)
        {
            _maxTokensPerPeriod = maxTokensPerPeriod;
            _period = period;
            _periodStart = DateTime.UtcNow;
            _usedTokens = 0;
        }

        public bool CanProcess(int estimatedTokens)
        {
            lock (_lock)
            {
                ResetPeriodIfNeeded();
                return _usedTokens + estimatedTokens <= _maxTokensPerPeriod;
            }
        }

        public void RecordUsage(int actualTokens)
        {
            lock (_lock)
            {
                ResetPeriodIfNeeded();
                _usedTokens += actualTokens;
            }
        }

        public int GetRemainingBudget()
        {
            lock (_lock)
            {
                ResetPeriodIfNeeded();
                return Math.Max(0, _maxTokensPerPeriod - _usedTokens);
            }
        }

        private void ResetPeriodIfNeeded()
        {
            if (DateTime.UtcNow - _periodStart > _period)
            {
                _usedTokens = 0;
                _periodStart = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Performance metrics for monitoring
    /// </summary>
    public class PerformanceMetrics
    {
        public long TotalRequests { get; set; }
        public long TotalErrors { get; set; }
        public long TotalTokensUsed { get; set; }
        public long CacheHitCount { get; set; }
        public int RemainingTokenBudget { get; set; }
        public double AverageResponseTime { get; set; }

        public double ErrorRate => TotalRequests > 0 ? (double)TotalErrors / TotalRequests : 0;
        public double CacheHitRate => TotalRequests > 0 ? (double)CacheHitCount / TotalRequests : 0;
    }

    /// <summary>
    /// XAI API response models
    /// </summary>
    public class XAIResponse
    {
        public Choice[] Choices { get; set; } = Array.Empty<Choice>();
        public Usage? Usage { get; set; }
    }

    public class Choice
    {
        public Message? Message { get; set; }
        public Delta? Delta { get; set; }
    }

    public class Message
    {
        public string Content { get; set; } = string.Empty;
    }

    public class Delta
    {
        public string Content { get; set; } = string.Empty;
    }

    public class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    public class StreamResponse
    {
        public Choice[] Choices { get; set; } = Array.Empty<Choice>();
    }

    #endregion
}
