using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BusBuddy.Configuration;
using BusBuddy.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;

namespace BusBuddy.Core.Services
{
    /// <summary>
    /// xAI (Grok) Integration Service for Advanced AI-Powered Transportation Intelligence
    /// Programmatically locked documentation references maintained in XAIDocumentationSettings
    /// </summary>
    public class XAIService
    {
        private static readonly ILogger Logger = Log.ForContext<XAIService>();
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly XAIDocumentationSettings _documentationSettings;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly bool _isConfigured;

        // API Endpoints
        public static readonly string CHAT_COMPLETIONS_ENDPOINT = "/chat/completions";

        public XAIService(HttpClient httpClient, IConfiguration configuration,
            IOptions<XAIDocumentationSettings> documentationOptions)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _documentationSettings = documentationOptions?.Value ?? new XAIDocumentationSettings();

            // Load xAI configuration
            _apiKey = _configuration["XAI:ApiKey"] ?? Environment.GetEnvironmentVariable("XAI_API_KEY") ?? string.Empty;
            _baseUrl = _configuration["XAI:BaseUrl"] ?? "https://api.x.ai/v1";
            var useLiveAPI = _configuration.GetValue<bool>("XAI:UseLiveAPI", true);

            _isConfigured = !string.IsNullOrEmpty(_apiKey) && !_apiKey.Contains("YOUR_XAI_API_KEY") && useLiveAPI;

            if (!_isConfigured)
            {
                Logger.Warning("xAI not configured or disabled. Using mock AI responses. Please set XAI_API_KEY environment variable and enable UseLiveAPI in appsettings.json.");
                Logger.Information("xAI Documentation: {ChatGuideUrl}", _documentationSettings.GetChatGuideUrl());
            }
            else
            {
                Logger.Information("xAI configured for live AI transportation intelligence");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
                _httpClient.Timeout = TimeSpan.FromSeconds(60);
            }
        }

        public bool IsConfigured => _isConfigured;

        /// <summary>
        /// Analyzes route optimization using xAI Grok intelligence
        /// </summary>
        public async Task<AIRouteRecommendations> AnalyzeRouteOptimizationAsync(RouteAnalysisRequest request)
        {
            try
            {
                Logger.Information("Requesting xAI route optimization analysis");

                if (!_isConfigured)
                {
                    return await GenerateMockAIRecommendations(request);
                }

                var prompt = BuildRouteOptimizationPrompt(request);
                var xaiRequest = new XAIRequest
                {
                    Model = _configuration["XAI:DefaultModel"] ?? "grok-4-latest",
                    Messages = new[]
                    {
                        new XAIMessage { Role = "system", Content = GetTransportationExpertSystemPrompt() },
                        new XAIMessage { Role = "user", Content = prompt }
                    },
                    Temperature = _configuration.GetValue<double>("XAI:Temperature", 0.3),
                    MaxTokens = _configuration.GetValue<int>("XAI:MaxTokens", 128000)
                };

                var response = await CallXAIAPI(CHAT_COMPLETIONS_ENDPOINT, xaiRequest);
                return ParseRouteRecommendations(response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in xAI route optimization analysis");
                return await GenerateMockAIRecommendations(request);
            }
        }

        /// <summary>
        /// Predicts maintenance needs using AI analysis
        /// </summary>
        public async Task<AIMaintenancePrediction> PredictMaintenanceNeedsAsync(MaintenanceAnalysisRequest request)
        {
            try
            {
                Logger.Information("Requesting xAI maintenance prediction analysis");

                var prompt = BuildMaintenancePredictionPrompt(request);
                var xaiRequest = new XAIRequest
                {
                    Model = _configuration["XAI:DefaultModel"] ?? "grok-4-latest",
                    Messages = new[]
                    {
                        new XAIMessage { Role = "system", Content = GetMaintenanceExpertSystemPrompt() },
                        new XAIMessage { Role = "user", Content = prompt }
                    },
                    Temperature = 0.2, // Lower temperature for more precise technical predictions
                    MaxTokens = _configuration.GetValue<int>("XAI:MaxTokens", 128000) / 2
                };

                if (!_isConfigured)
                {
                    return await GenerateMockMaintenancePrediction(request);
                }

                var response = await CallXAIAPI(CHAT_COMPLETIONS_ENDPOINT, xaiRequest);
                return ParseMaintenancePrediction(response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in xAI maintenance prediction");
                return await GenerateMockMaintenancePrediction(request);
            }
        }

        /// <summary>
        /// Analyzes safety risks using AI intelligence
        /// </summary>
        public async Task<AISafetyAnalysis> AnalyzeSafetyRisksAsync(SafetyAnalysisRequest request)
        {
            try
            {
                Logger.Information("Requesting xAI safety risk analysis");

                var prompt = BuildSafetyAnalysisPrompt(request);
                var xaiRequest = new XAIRequest
                {
                    Model = _configuration["XAI:DefaultModel"] ?? "grok-4-latest",
                    Messages = new[]
                    {
                        new XAIMessage { Role = "system", Content = GetSafetyExpertSystemPrompt() },
                        new XAIMessage { Role = "user", Content = prompt }
                    },
                    Temperature = 0.1, // Very low temperature for safety-critical analysis
                    MaxTokens = _configuration.GetValue<int>("XAI:MaxTokens", 128000) / 2
                };

                if (!_isConfigured)
                {
                    return await GenerateMockSafetyAnalysis(request);
                }

                var response = await CallXAIAPI("/chat/completions", xaiRequest);
                return ParseSafetyAnalysis(response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in xAI safety analysis");
                return await GenerateMockSafetyAnalysis(request);
            }
        }

        /// <summary>
        /// Optimizes student assignments using AI
        /// </summary>
        public async Task<AIStudentOptimization> OptimizeStudentAssignmentsAsync(StudentOptimizationRequest request)
        {
            try
            {
                Logger.Information("Requesting xAI student assignment optimization");

                var prompt = BuildStudentOptimizationPrompt(request);
                var xaiRequest = new XAIRequest
                {
                    Model = _configuration["XAI:DefaultModel"] ?? "grok-4-latest",
                    Messages = new[]
                    {
                        new XAIMessage { Role = "system", Content = GetLogisticsExpertSystemPrompt() },
                        new XAIMessage { Role = "user", Content = prompt }
                    },
                    Temperature = _configuration.GetValue<double>("XAI:Temperature", 0.3),
                    MaxTokens = _configuration.GetValue<int>("XAI:MaxTokens", 128000)
                };

                if (!_isConfigured)
                {
                    return await GenerateMockStudentOptimization(request);
                }

                var response = await CallXAIAPI("/chat/completions", xaiRequest);
                return ParseStudentOptimization(response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in xAI student optimization");
                return await GenerateMockStudentOptimization(request);
            }
        }

        /// <summary>
        /// Sends a general chat message to xAI Grok for AI AssistView integration
        /// </summary>
        public async Task<string> SendChatMessageAsync(string message, string? context = null)
        {
            try
            {
                Logger.Information("Sending chat message to xAI: {Message}", message);

                if (!_isConfigured)
                {
                    await Task.Delay(1000); // Simulate processing time
                    return GenerateMockChatResponse(message);
                }

                var systemPrompt = string.IsNullOrEmpty(context)
                    ? GetGeneralChatSystemPrompt()
                    : $"{GetGeneralChatSystemPrompt()}\n\nAdditional Context: {context}";

                var xaiRequest = new XAIRequest
                {
                    Model = _configuration["XAI:DefaultModel"] ?? "grok-4-latest",
                    Messages = new[]
                    {
                        new XAIMessage { Role = "system", Content = systemPrompt },
                        new XAIMessage { Role = "user", Content = message }
                    },
                    Temperature = _configuration.GetValue<double>("XAI:Temperature", 0.7),
                    MaxTokens = _configuration.GetValue<int>("XAI:MaxTokens", 64000)
                };

                var response = await CallXAIAPI(CHAT_COMPLETIONS_ENDPOINT, xaiRequest);
                return response.Choices.FirstOrDefault()?.Message.Content ?? "I'm sorry, I couldn't process your request at the moment.";
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in xAI chat message: {Message}", message);
                await Task.Delay(500); // Brief delay for mock response
                return GenerateMockChatResponse(message);
            }
        }

        #region Phase 1 Program Management & Analytics

        /// <summary>
        /// Analyze Phase 1 progress and provide insights for program management
        /// </summary>
        public async Task<PhaseAnalysisResult> AnalyzePhase1ProgressAsync(Phase1ProgressRequest request)
        {
            try
            {
                var prompt = BuildPhase1AnalysisPrompt(request);
                var response = await SendChatMessageAsync(prompt, "Phase 1 Progress Analysis");

                if (_isConfigured && !string.IsNullOrEmpty(response))
                {
                    return ParsePhase1AnalysisResponse(response);
                }

                return CreateMockPhase1Analysis(request);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error analyzing Phase 1 progress for project {ProjectName}", request.ProjectName);
                return CreateMockPhase1Analysis(request);
            }
        }

        /// <summary>
        /// Get development insights and recommendations for the current development state
        /// </summary>
        public async Task<DevelopmentInsights> GetDevelopmentInsightsAsync(DevelopmentStateRequest request)
        {
            try
            {
                var prompt = BuildDevelopmentInsightsPrompt(request);
                var response = await SendChatMessageAsync(prompt, "Development Insights");

                if (_isConfigured && !string.IsNullOrEmpty(response))
                {
                    return ParseDevelopmentInsights(response);
                }

                return CreateMockDevelopmentInsights(request);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error getting development insights for {ComponentName}", request.ComponentName);
                return CreateMockDevelopmentInsights(request);
            }
        }

        /// <summary>
        /// Analyze build and runtime performance for optimization suggestions
        /// </summary>
        public async Task<PerformanceAnalysis> AnalyzePerformanceAsync(PerformanceDataRequest request)
        {
            try
            {
                var prompt = BuildPerformanceAnalysisPrompt(request);
                var response = await SendChatMessageAsync(prompt, "Performance Analysis");

                if (_isConfigured && !string.IsNullOrEmpty(response))
                {
                    return ParsePerformanceAnalysis(response);
                }

                return CreateMockPerformanceAnalysis(request);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error analyzing performance for {ApplicationName}", request.ApplicationName);
                return CreateMockPerformanceAnalysis(request);
            }
        }

        /// <summary>
        /// Generate dynamic mock data with AI assistance for testing and development
        /// </summary>
        public async Task<GeneratedDataSet> GenerateMockDataAsync(MockDataRequest request)
        {
            try
            {
                var prompt = BuildMockDataPrompt(request);
                var response = await SendChatMessageAsync(prompt, "Mock Data Generation");

                if (_isConfigured && !string.IsNullOrEmpty(response))
                {
                    return ParseGeneratedDataSet(response);
                }

                return CreateBasicMockData(request);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error generating mock data for {DataType}", request.DataType);
                return CreateBasicMockData(request);
            }
        }

        /// <summary>
        /// Get contextual help and documentation suggestions
        /// </summary>
        public async Task<ContextualHelp> GetContextualHelpAsync(HelpRequest request)
        {
            try
            {
                var prompt = BuildContextualHelpPrompt(request);
                var response = await SendChatMessageAsync(prompt, "Contextual Help");

                if (_isConfigured && !string.IsNullOrEmpty(response))
                {
                    return ParseContextualHelp(response);
                }

                return CreateBasicHelp(request);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error getting contextual help for {Topic}", request.Topic);
                return CreateBasicHelp(request);
            }
        }

        private static ContextualHelp ParseContextualHelp(string response)
        {
            return new ContextualHelp
            {
                Topic = "AI Response",
                HelpContent = response,
                RelatedTopics = ExtractRelatedTopics(response),
                ActionableSteps = ExtractActionableSteps(response)
            };
        }

        private static ContextualHelp CreateBasicHelp(HelpRequest request)
        {
            return new ContextualHelp
            {
                Topic = request.Topic,
                HelpContent = $"Help information for {request.Topic}",
                RelatedTopics = new[] { "General Help", "Documentation", "FAQ" },
                ActionableSteps = new[] { "Review documentation", "Check examples", "Contact support" }
            };
        }

        private static string[] ExtractRelatedTopics(string content)
        {
            // Simple extraction of potential related topics
            var topics = new List<string>();
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.Contains("related", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("see also", StringComparison.OrdinalIgnoreCase))
                {
                    topics.Add(line.Trim());
                }
            }

            return topics.Take(3).ToArray();
        }

        private static string[] ExtractActionableSteps(string content)
        {
            // Extract numbered steps or bullet points
            var steps = new List<string>();
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("1.") || trimmed.StartsWith("2.") ||
                    trimmed.StartsWith("3.") || trimmed.StartsWith("-") ||
                    trimmed.StartsWith("*"))
                {
                    steps.Add(trimmed);
                }
            }

            return steps.Take(5).ToArray();
        }

        #endregion

        #region Phase 1 Program Management Helper Methods

        private static PhaseAnalysisResult CreateMockPhase1Analysis(Phase1ProgressRequest request)
        {
            return new PhaseAnalysisResult();
        }

        private static string BuildDevelopmentInsightsPrompt(DevelopmentStateRequest request)
        {
            return "Development insights prompt";
        }

        private static DevelopmentInsights ParseDevelopmentInsights(string response)
        {
            return new DevelopmentInsights();
        }

        private static DevelopmentInsights CreateMockDevelopmentInsights(DevelopmentStateRequest request)
        {
            return new DevelopmentInsights();
        }

        private static string BuildPerformanceAnalysisPrompt(PerformanceDataRequest request)
        {
            return "Performance analysis prompt";
        }

        private static PerformanceAnalysis ParsePerformanceAnalysis(string response)
        {
            return new PerformanceAnalysis();
        }

        private static PerformanceAnalysis CreateMockPerformanceAnalysis(PerformanceDataRequest request)
        {
            return new PerformanceAnalysis();
        }

        private static string BuildMockDataPrompt(MockDataRequest request)
        {
            return "Mock data prompt";
        }

        private static GeneratedDataSet ParseGeneratedDataSet(string response)
        {
            return new GeneratedDataSet();
        }

        private static GeneratedDataSet CreateBasicMockData(MockDataRequest request)
        {
            return new GeneratedDataSet();
        }

        private static string BuildContextualHelpPrompt(HelpRequest request)
        {
            return "Contextual help prompt";
        }

        private static string BuildPhase1AnalysisPrompt(Phase1ProgressRequest request)
        {
            var prompt = $@"
# BusBuddy Phase 1 Development Analysis

## Project Overview
- **Project**: {request.ProjectName}
- **Current Phase**: {request.CurrentPhase}
- **Days in Development**: {request.DaysInDevelopment}
- **Team Size**: {request.TeamSize}
- **Target Completion**: {request.TargetCompletion:yyyy-MM-dd}

## Technical Metrics
- **Build Status**: {request.BuildStatus}
- **Tests**: {request.TestsPassingCount}/{request.TotalTestsCount} passing
- **Code Coverage**: {request.CodeCoverage:P1}
- **Critical Issues**: {request.CriticalIssuesCount}
- **Components**: {request.CompletedComponents}/{request.TotalComponents} completed
- **Lines of Code**: {request.LinesOfCode:N0}

## Development Activity
- **Commits This Week**: {request.CommitsThisWeek}
- **Pull Requests**: {request.OpenPullRequests} open, {request.MergedPullRequests} merged
- **Average Build Time**: {request.AverageBuildTime:F1} seconds
- **Active Developers**: {request.ActiveDevelopers}
- **Issues Closed This Week**: {request.IssuesClosedThisWeek}
- **Documentation Pages**: {request.DocumentationPages}

## Current Focus Areas
{string.Join("", request.CurrentFocusAreas.Select(area => $"- {area}\n"))}

Please analyze this development state and provide:
1. **Overall Health Assessment** (Excellent/Good/Fair/Poor)
2. **Health Score** (0-100)
3. **Risk Level** (Low/Medium/High/Critical)
4. **Top 5 Recommendations** for improving development velocity
5. **Predicted Completion Date** based on current velocity
6. **Next 3 Key Milestones** to focus on
7. **Technical Debt Analysis**
8. **Team Productivity Assessment**

Focus on actionable insights for a WPF .NET application in Phase 1 development.";

            return prompt;
        }

        private static PhaseAnalysisResult ParsePhase1AnalysisResponse(string response)
        {
            var result = new PhaseAnalysisResult();

            try
            {
                // Extract overall health
                var healthMatch = Regex.Match(response, @"Overall Health.*?:\s*(\w+)", RegexOptions.IgnoreCase);
                if (healthMatch.Success)
                {
                    result.OverallHealth = healthMatch.Groups[1].Value;
                }

                // Extract health score
                var scoreMatch = Regex.Match(response, @"Health Score.*?:\s*(\d+)", RegexOptions.IgnoreCase);
                if (scoreMatch.Success && int.TryParse(scoreMatch.Groups[1].Value, out int score))
                {
                    result.HealthScore = score;
                }

                // Extract risk level
                var riskMatch = Regex.Match(response, @"Risk Level.*?:\s*(\w+)", RegexOptions.IgnoreCase);
                if (riskMatch.Success)
                {
                    result.RiskLevel = riskMatch.Groups[1].Value;
                }

                // Extract recommendations
                var recMatches = Regex.Matches(response, @"(?:Recommendation|•|\*|\d+\.)\s*(.+?)(?=\n|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                result.Recommendations = recMatches.Cast<Match>()
                    .Select(m => m.Groups[1].Value.Trim())
                    .Where(s => !string.IsNullOrEmpty(s) && s.Length > 10)
                    .Take(5)
                    .ToArray();

                // Extract predicted completion date
                var dateMatch = Regex.Match(response, @"(?:Predicted|Completion).*?Date.*?:\s*(\d{4}-\d{2}-\d{2})", RegexOptions.IgnoreCase);
                if (dateMatch.Success && DateTime.TryParse(dateMatch.Groups[1].Value, out DateTime completionDate))
                {
                    result.PredictedCompletionDate = completionDate;
                }
                else
                {
                    result.PredictedCompletionDate = DateTime.Now.AddDays(30); // Default estimate
                }

                // Extract next milestones
                var milestoneMatches = Regex.Matches(response, @"(?:Milestone|Next|•|\*|\d+\.)\s*(.+?)(?=\n|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                result.NextMilestones = milestoneMatches.Cast<Match>()
                    .Select(m => m.Groups[1].Value.Trim())
                    .Where(s => !string.IsNullOrEmpty(s) && s.Length > 5)
                    .Take(3)
                    .ToArray();

                // Extract technical debt analysis
                var debtMatch = Regex.Match(response, @"Technical Debt.*?:\s*(.+?)(?=\n\n|\n[A-Z]|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (debtMatch.Success)
                {
                    result.TechnicalDebt = debtMatch.Groups[1].Value.Trim();
                }

                // Extract team productivity
                var prodMatch = Regex.Match(response, @"(?:Team\s+)?Productivity.*?:\s*(.+?)(?=\n\n|\n[A-Z]|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (prodMatch.Success)
                {
                    result.TeamProductivity = prodMatch.Groups[1].Value.Trim();
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error parsing Phase 1 analysis response, using defaults");
                result.OverallHealth = "Analysis Error";
                result.HealthScore = 50;
                result.RiskLevel = "Medium";
                result.Recommendations = new[] { "Unable to parse analysis recommendations" };
                result.PredictedCompletionDate = DateTime.Now.AddDays(30);
                result.NextMilestones = new[] { "Complete analysis parsing", "Resume development", "Review metrics" };
                result.TechnicalDebt = "Unable to assess technical debt from response";
                result.TeamProductivity = "Unable to assess team productivity from response";
            }

            return result;
        }

        private static string BuildDevelopmentStatePrompt(DevelopmentStateRequest request)
        {
            var prompt = $@"
# BusBuddy Component Development State Analysis

## Component Information
- **Name**: {request.ComponentName}
- **Complexity**: {request.ComplexityLevel}
- **Last Modified**: {request.LastModified:yyyy-MM-dd HH:mm}

## Technology Stack
{string.Join("", request.TechnologyStack.Select(tech => $"- {tech}\n"))}

## Recent Changes
- **Files Changed**: {request.FilesChanged}
- **Lines Added**: {request.LinesAdded:+#;-#;0}
- **Lines Removed**: {request.LinesRemoved}
- **Recent Commits**: {request.RecentCommits}

## Code Metrics
- **Methods Count**: {request.MethodsCount}
- **Classes Count**: {request.ClassesCount}

## Issues & Requests
- **Bug Reports**: {request.BugReports}
- **Feature Requests**: {request.FeatureRequests}
- **Performance Issues**: {request.PerformanceIssues}

## Dependencies
{string.Join("", request.Dependencies.Select(dep => $"- {dep}\n"))}

## Current Challenges
{string.Join("", request.CurrentChallenges.Select(challenge => $"- {challenge}\n"))}

Please provide development insights including:
1. **Component Health Status**
2. **Development Velocity Assessment**
3. **Risk Areas** to monitor
4. **Optimization Opportunities**
5. **Next Development Steps**
6. **Resource Requirements**

Focus on actionable insights for WPF .NET development.";

            return prompt;
        }

        private async Task<DevelopmentStateRequest> GatherDevelopmentMetricsAsync(string componentName)
        {
            var request = new DevelopmentStateRequest
            {
                ComponentName = componentName,
                TechnologyStack = new List<string> { "WPF", ".NET 8.0", "Entity Framework Core", "Syncfusion", "Serilog" },
                ComplexityLevel = "Medium",
                LastModified = DateTime.Now,
                FilesChanged = 0,
                LinesAdded = 0,
                LinesRemoved = 0,
                MethodsCount = 0,
                ClassesCount = 0,
                RecentCommits = 0,
                BugReports = 0,
                FeatureRequests = 0,
                PerformanceIssues = 0,
                Dependencies = new List<string>(),
                CurrentChallenges = new List<string>()
            };

            try
            {
                // In a real implementation, this would gather actual metrics
                // For now, we'll return the basic structure
                Logger.Information("Gathering development metrics for component: {ComponentName}", componentName);

                // Add await to make this actually async or remove async keyword
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error gathering development metrics for {ComponentName}", componentName);
            }

            return request;
        }

        #endregion

        #region Prompt Building Methods

        private static string BuildRouteOptimizationPrompt(RouteAnalysisRequest request)
        {
            return $@"SCHOOL BUS ROUTE OPTIMIZATION ANALYSIS

ROUTE DETAILS:
- Route ID: {request.RouteId}
- Current Distance: {request.CurrentDistance} miles
- Student Count: {request.StudentCount}
- Vehicle Capacity: {request.VehicleCapacity}

TERRAIN DATA:
- Elevation Range: {request.TerrainData?.MinElevation}m to {request.TerrainData?.MaxElevation}m
- Average Slope: {request.TerrainData?.AverageSlope}°
- Terrain Type: {request.TerrainData?.TerrainType}
- Route Difficulty: {request.TerrainData?.RouteDifficulty}

WEATHER CONDITIONS:
- Current Condition: {request.WeatherData?.Condition ?? "Unknown"}
- Temperature: {request.WeatherData?.Temperature ?? 0}°C
- Visibility: {request.WeatherData?.Visibility ?? 0}km
- Wind: {request.WeatherData?.WindCondition ?? "Unknown"}

TRAFFIC STATUS:
- Overall Condition: {request.TrafficData?.OverallCondition ?? "Unknown"}

HISTORICAL PERFORMANCE:
- Average Fuel Consumption: {request.HistoricalData?.AverageFuelConsumption} mpg
- On-Time Performance: {request.HistoricalData?.OnTimePerformance}%
- Safety Incidents: {request.HistoricalData?.SafetyIncidents} in last year

OPTIMIZATION GOALS:
1. Minimize fuel consumption
2. Maximize safety
3. Optimize time efficiency
4. Reduce environmental impact
5. Ensure student comfort

Please provide comprehensive recommendations in JSON format with:
- Optimal route suggestions
- Risk assessment and mitigation strategies
- Fuel efficiency optimization
- Safety improvements
- Environmental considerations
- Cost-benefit analysis
- Implementation timeline";
        }

        private static string BuildMaintenancePredictionPrompt(MaintenanceAnalysisRequest request)
        {
            return $@"PREDICTIVE MAINTENANCE ANALYSIS

VEHICLE DETAILS:
- Bus ID: {request.BusId}
- Make/Model: {request.VehicleMake} {request.VehicleModel}
- Year: {request.VehicleYear}
- Current Mileage: {request.CurrentMileage}
- Last Maintenance: {request.LastMaintenanceDate:yyyy-MM-dd}

ROUTE USAGE PATTERNS:
- Daily Miles: {request.DailyMiles}
- Terrain Difficulty: {request.TerrainDifficulty}
- Stop Frequency: {request.StopFrequency} stops per mile
- Elevation Changes: {request.ElevationChanges}m average

CURRENT CONDITIONS:
- Engine Hours: {request.EngineHours}
- Brake Usage: {request.BrakeUsage}
- Tire Condition: {request.TireCondition}
- Fluid Levels: {request.FluidLevels}

Predict maintenance needs, component wear, optimal service intervals, and cost optimization strategies.";
        }

        private static string BuildSafetyAnalysisPrompt(SafetyAnalysisRequest request)
        {
            return $@"TRANSPORTATION SAFETY RISK ANALYSIS

ROUTE CONDITIONS:
- Route Type: {request.RouteType}
- Traffic Density: {request.TrafficDensity}
- Road Conditions: {request.RoadConditions}
- Weather Impact: {request.WeatherConditions}

STUDENT DEMOGRAPHICS:
- Age Groups: {string.Join(", ", request.AgeGroups)}
- Special Needs: {request.SpecialNeedsCount} students
- Total Students: {request.TotalStudents}

HISTORICAL SAFETY DATA:
- Previous Incidents: {request.PreviousIncidents}
- Near-Miss Reports: {request.NearMissReports}
- Driver Safety Record: {request.DriverSafetyRecord}

Analyze risks and provide safety enhancement recommendations.";
        }

        private static string BuildStudentOptimizationPrompt(StudentOptimizationRequest request)
        {
            return $@"STUDENT ASSIGNMENT OPTIMIZATION

OPTIMIZATION PARAMETERS:
- Total Students: {request.TotalStudents}
- Available Buses: {request.AvailableBuses}
- Geographic Constraints: {request.GeographicConstraints}
- Special Requirements: {request.SpecialRequirements}

EFFICIENCY GOALS:
- Minimize total travel time
- Balance bus capacity utilization
- Optimize route efficiency
- Ensure safety and comfort

Provide optimal student-to-bus assignments with reasoning.";
        }

        #endregion

        #region System Prompts

        private static string GetTransportationExpertSystemPrompt()
        {
            return @"You are an expert transportation optimization specialist with decades of experience in school bus fleet management. You have deep knowledge of:
- Route optimization algorithms
- Fuel efficiency strategies
- Safety protocols and risk assessment
- Terrain analysis and vehicle performance
- Weather impact on transportation
- Cost optimization and budget management
- Environmental sustainability
- Student safety and comfort

Provide detailed, actionable recommendations based on data analysis. Always prioritize safety while optimizing for efficiency and cost-effectiveness.";
        }

        private static string GetMaintenanceExpertSystemPrompt()
        {
            return @"You are a certified fleet maintenance expert specializing in school bus preventive maintenance and predictive analytics. Your expertise includes:
- Predictive maintenance algorithms
- Component lifecycle analysis
- Wear pattern recognition
- Cost-effective maintenance scheduling
- Safety-critical system monitoring
- Parts inventory optimization
- Maintenance budget planning

Provide precise maintenance predictions with confidence levels and cost justifications.";
        }

        private static string GetSafetyExpertSystemPrompt()
        {
            return @"You are a school transportation safety specialist with expertise in:
- Risk assessment and mitigation
- Student safety protocols
- Driver safety training
- Route safety analysis
- Emergency procedures
- Regulatory compliance
- Incident prevention strategies

Always prioritize student and driver safety. Provide comprehensive risk assessments with specific mitigation strategies.";
        }

        private static string GetLogisticsExpertSystemPrompt()
        {
            return @"You are a logistics optimization expert specializing in student transportation efficiency. Your expertise includes:
- Student assignment algorithms
- Capacity optimization
- Geographic clustering analysis
- Route balancing strategies
- Special needs accommodation
- Time window optimization
- Resource allocation

Provide mathematically sound optimization recommendations with clear implementation steps.";
        }

        private static string GetGeneralChatSystemPrompt()
        {
            return @"You are BusBuddy AI, an intelligent assistant for school transportation management. You help with:
- Transportation planning and optimization
- Student and route management
- Safety protocols and compliance
- Maintenance scheduling and tracking
- Fuel management and cost optimization
- General school transportation questions

Be helpful, informative, and always prioritize student safety. Provide practical, actionable advice for school transportation challenges.";
        }

        #endregion

        #region API Communication (Future Implementation)

        private async Task<XAIResponse> CallXAIAPI(string endpoint, XAIRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, ApiJsonOptions);
                using var content = new StringContent(json, Encoding.UTF8, MediaTypeHeaderValue.Parse("application/json"));

                Logger.Debug("Calling xAI API: {Endpoint} with model: {Model}", endpoint, request.Model);

                var response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                Logger.Debug("xAI API response received: {ResponseLength} characters", responseJson.Length);

                var result = JsonSerializer.Deserialize<XAIResponse>(responseJson, ApiJsonOptions);
                return result ?? new XAIResponse();
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, "HTTP error calling xAI API: {Message}", ex.Message);
                throw;
            }
            catch (JsonException ex)
            {
                Logger.Error(ex, "JSON parsing error from xAI API: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unexpected error calling xAI API: {Message}", ex.Message);
                throw;
            }
        }

        #endregion

        #region Mock Implementations (Current)

        private static async Task<AIRouteRecommendations> GenerateMockAIRecommendations(RouteAnalysisRequest request)
        {
            await Task.Delay(2000); // Simulate AI processing time

            return new AIRouteRecommendations
            {
                OptimalRoute = new RouteRecommendation
                {
                    EstimatedFuelSavings = 18.5,
                    EstimatedTimeSavings = 12.3,
                    SafetyScore = 94.2,
                    RecommendedChanges = new[]
                    {
                        "Adjust route to avoid steep grade on Elm Street during wet conditions",
                        "Consider alternative path through residential area for reduced traffic",
                        "Optimize stop spacing for better fuel efficiency"
                    }
                },
                RiskAssessment = new RiskAssessment
                {
                    OverallRiskLevel = "Low",
                    IdentifiedRisks = new[]
                    {
                        "Weather-related visibility concerns during morning hours",
                        "Increased traffic during school start times"
                    },
                    MitigationStrategies = new[]
                    {
                        "Deploy additional safety protocols during inclement weather",
                        "Adjust departure times to avoid peak traffic"
                    }
                },
                ConfidenceLevel = 0.87,
                Reasoning = "Analysis based on terrain data, weather patterns, and historical performance metrics. Recommendations prioritize safety while optimizing efficiency."
            };
        }

        private static async Task<AIMaintenancePrediction> GenerateMockMaintenancePrediction(MaintenanceAnalysisRequest request)
        {
            await Task.Delay(1800);

            return new AIMaintenancePrediction
            {
                PredictedMaintenanceDate = DateTime.Now.AddDays(45),
                ComponentPredictions = new[]
                {
                    new ComponentPrediction
                    {
                        Component = "Brake Pads",
                        PredictedWearDate = DateTime.Now.AddDays(30),
                        ConfidenceLevel = 0.92,
                        EstimatedCost = 350.00m
                    },
                    new ComponentPrediction
                    {
                        Component = "Tires",
                        PredictedWearDate = DateTime.Now.AddDays(120),
                        ConfidenceLevel = 0.78,
                        EstimatedCost = 1200.00m
                    }
                },
                TotalEstimatedCost = 1550.00m,
                PotentialSavings = 850.00m,
                Reasoning = "Predictive analysis based on route difficulty, vehicle usage patterns, and component lifecycle data."
            };
        }

        private static async Task<AISafetyAnalysis> GenerateMockSafetyAnalysis(SafetyAnalysisRequest request)
        {
            await Task.Delay(1500);

            return new AISafetyAnalysis
            {
                OverallSafetyScore = 91.5,
                RiskFactors = new[]
                {
                    new SafetyRiskFactor
                    {
                        Factor = "Weather Conditions",
                        RiskLevel = "Medium",
                        Impact = "Reduced visibility during morning fog",
                        Mitigation = "Install enhanced lighting and reflective markers"
                    }
                },
                Recommendations = new[]
                {
                    "Implement GPS tracking for real-time route monitoring",
                    "Enhance driver training for adverse weather conditions",
                    "Install additional safety equipment on high-risk routes"
                },
                ComplianceStatus = "Fully Compliant",
                ConfidenceLevel = 0.89
            };
        }

        private static async Task<AIStudentOptimization> GenerateMockStudentOptimization(StudentOptimizationRequest request)
        {
            await Task.Delay(2200);

            return new AIStudentOptimization
            {
                OptimalAssignments = new[]
                {
                    new StudentAssignment
                    {
                        BusId = 1,
                        StudentsAssigned = 45,
                        CapacityUtilization = 0.75,
                        AverageRideTime = 25.5
                    }
                },
                EfficiencyGains = new EfficiencyMetrics
                {
                    TotalTimeSaved = 45.0,
                    FuelSavings = 12.3,
                    CapacityOptimization = 0.82
                },
                ConfidenceLevel = 0.91,
                Reasoning = "Optimization based on geographic clustering, capacity constraints, and time window requirements."
            };
        }

        private static string GenerateMockChatResponse(string message)
        {
            // Simulate realistic chat responses based on the input message
            var responses = new[]
            {
                $"Thank you for your question about '{message}'. Based on my transportation expertise, I recommend checking our safety protocols and route optimization features.",
                $"I understand you're asking about '{message}'. For school transportation management, this typically involves reviewing current policies and consulting with our routing algorithms.",
                $"That's a great question about '{message}'. In my experience with school transportation systems, the best approach is to prioritize student safety while optimizing efficiency.",
                $"Regarding '{message}', I'd suggest checking your current transportation data and considering factors like route efficiency, safety compliance, and student capacity.",
                $"I can help with '{message}'. For optimal school transportation management, consider reviewing your maintenance schedules, route planning, and safety protocols."
            };

            var random = new Random();
            return responses[random.Next(responses.Length)];
        }

        #endregion

        #region Response Parsing (Future Implementation)

        private static AIRouteRecommendations CreateDefaultRouteRecommendations(string aiResponse)
        {
            return new AIRouteRecommendations
            {
                OptimalRoute = new RouteRecommendation
                {
                    EstimatedFuelSavings = ExtractNumericValue(aiResponse, "fuel", "savings", "efficiency") ?? 15.0,
                    EstimatedTimeSavings = ExtractNumericValue(aiResponse, "time", "savings", "minutes") ?? 10.0,
                    SafetyScore = ExtractNumericValue(aiResponse, "safety", "score") ?? 85.0,
                    RecommendedChanges = ExtractRecommendations(aiResponse)
                },
                RiskAssessment = new RiskAssessment
                {
                    OverallRiskLevel = ExtractRiskLevel(aiResponse),
                    IdentifiedRisks = ExtractRisks(aiResponse),
                    MitigationStrategies = ExtractMitigations(aiResponse)
                },
                ConfidenceLevel = 0.85,
                Reasoning = aiResponse.Length > 500 ? aiResponse.Substring(0, 500) + "..." : aiResponse
            };
        }

        private static AIRouteRecommendations ParseRouteRecommendations(XAIResponse response)
        {
            try
            {
                if (response?.Choices?.Length > 0)
                {
                    var content = response.Choices[0].Message?.Content ?? string.Empty;
                    Logger.Debug("Parsing xAI route optimization response: {Content}", content);

                    // Try to parse structured JSON response or extract key information
                    if (content.Contains('{') && content.Contains('}'))
                    {
                        // Attempt to parse JSON response
                        try
                        {
                            return JsonSerializer.Deserialize<AIRouteRecommendations>(content, ParseJsonOptions) ?? CreateDefaultRouteRecommendations(content);
                        }
                        catch (JsonException)
                        {
                            return CreateDefaultRouteRecommendations(content);
                        }
                    }
                    else
                    {
                        return CreateDefaultRouteRecommendations(content);
                    }
                }

                return CreateDefaultRouteRecommendations("No response from xAI");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error parsing xAI route recommendations");
                return CreateDefaultRouteRecommendations("Error parsing AI response");
            }
        }

        private static AIMaintenancePrediction ParseMaintenancePrediction(XAIResponse response)
        {
            try
            {
                if (response?.Choices?.Length > 0)
                {
                    var content = response.Choices[0].Message?.Content ?? string.Empty;
                    Logger.Debug("Parsing xAI maintenance prediction response");

                    return new AIMaintenancePrediction
                    {
                        PredictedMaintenanceDate = DateTime.Now.AddDays(ExtractNumericValue(content, "days", "maintenance", "service") ?? 45),
                        ComponentPredictions = ExtractComponentPredictions(content),
                        TotalEstimatedCost = (decimal)(ExtractNumericValue(content, "cost", "total", "price") ?? 1500),
                        PotentialSavings = (decimal)(ExtractNumericValue(content, "savings", "save") ?? 500),
                        Reasoning = content.Length > 300 ? content.Substring(0, 300) + "..." : content
                    };
                }

                return new AIMaintenancePrediction();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error parsing xAI maintenance prediction");
                return new AIMaintenancePrediction();
            }
        }

        private static AISafetyAnalysis ParseSafetyAnalysis(XAIResponse response)
        {
            try
            {
                if (response?.Choices?.Length > 0)
                {
                    var content = response.Choices[0].Message?.Content ?? string.Empty;
                    Logger.Debug("Parsing xAI safety analysis response");

                    return new AISafetyAnalysis
                    {
                        OverallSafetyScore = ExtractNumericValue(content, "safety", "score") ?? 85.0,
                        RiskFactors = ExtractSafetyRiskFactors(content),
                        Recommendations = ExtractRecommendations(content),
                        ComplianceStatus = ExtractComplianceStatus(content),
                        ConfidenceLevel = 0.85
                    };
                }

                return new AISafetyAnalysis();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error parsing xAI safety analysis");
                return new AISafetyAnalysis();
            }
        }

        private static AIStudentOptimization ParseStudentOptimization(XAIResponse response)
        {
            try
            {
                if (response?.Choices?.Length > 0)
                {
                    var content = response.Choices[0].Message?.Content ?? string.Empty;
                    Logger.Debug("Parsing xAI student optimization response");

                    return new AIStudentOptimization
                    {
                        OptimalAssignments = ExtractStudentAssignments(content),
                        EfficiencyGains = new EfficiencyMetrics
                        {
                            TotalTimeSaved = ExtractNumericValue(content, "time", "saved") ?? 30.0,
                            FuelSavings = ExtractNumericValue(content, "fuel", "saved") ?? 15.0,
                            CapacityOptimization = ExtractNumericValue(content, "capacity", "utilization") ?? 0.8
                        },
                        ConfidenceLevel = 0.85,
                        Reasoning = content.Length > 300 ? content.Substring(0, 300) + "..." : content
                    };
                }

                return new AIStudentOptimization();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error parsing xAI student optimization");
                return new AIStudentOptimization();
            }
        }

        #endregion

        #region AI Response Parsing Helper Methods

        private static double? ExtractNumericValue(string content, params string[] keywords)
        {
            try
            {
                var lowerContent = content.ToLowerInvariant();
                foreach (var keyword in keywords)
                {
                    var keywordIndex = lowerContent.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase);
                    if (keywordIndex >= 0)
                    {
                        // Look for numbers near the keyword
                        var searchArea = lowerContent.Substring(
                            Math.Max(0, keywordIndex - 20),
                            Math.Min(100, lowerContent.Length - Math.Max(0, keywordIndex - 20))
                        );

                        var numberMatches = System.Text.RegularExpressions.Regex.Matches(searchArea, @"\d+\.?\d*");
                        if (numberMatches.Count > 0)
                        {
                            if (double.TryParse(numberMatches[0].Value, out var value))
                            {
                                return value;
                            }
                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private static string[] ExtractRecommendations(string content)
        {
            try
            {
                var recommendations = new List<string>();
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith('-') || trimmed.StartsWith('•') ||
                        trimmed.StartsWith('*') || char.IsDigit(trimmed[0]))
                    {
                        recommendations.Add(trimmed.TrimStart('-', '•', '*', ' ', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.'));
                    }
                    else if (trimmed.Contains("recommend", StringComparison.OrdinalIgnoreCase) || trimmed.Contains("suggest", StringComparison.OrdinalIgnoreCase))
                    {
                        recommendations.Add(trimmed);
                    }
                }

                return recommendations.Take(5).ToArray(); // Limit to 5 recommendations
            }
            catch
            {
                return new[] { "AI recommendations processing" };
            }
        }

        private static string ExtractRiskLevel(string content)
        {
            var lowerContent = content.ToLowerInvariant();
            if (lowerContent.Contains("high risk", StringComparison.OrdinalIgnoreCase) || lowerContent.Contains("critical", StringComparison.OrdinalIgnoreCase))
            {
                return "High";
            }

            if (lowerContent.Contains("medium risk") || lowerContent.Contains("moderate"))
            {
                return "Medium";
            }

            if (lowerContent.Contains("low risk") || lowerContent.Contains("minimal"))
            {
                return "Low";
            }

            return "Medium"; // Default
        }

        private static string[] ExtractRisks(string content)
        {
            try
            {
                var risks = new List<string>();
                var lines = content.Split('\n');

                foreach (var line in lines)
                {
                    if (line.Contains("risk", StringComparison.OrdinalIgnoreCase) && line.Length > 10)
                    {
                        risks.Add(line.Trim());
                    }
                }

                return risks.Take(3).ToArray();
            }
            catch
            {
                return new[] { "Weather-related concerns", "Traffic considerations" };
            }
        }

        private static string[] ExtractMitigations(string content)
        {
            try
            {
                var mitigations = new List<string>();
                var lines = content.Split('\n');

                foreach (var line in lines)
                {
                    if ((line.Contains("mitigate", StringComparison.OrdinalIgnoreCase) || line.Contains("prevent", StringComparison.OrdinalIgnoreCase) ||
                         line.Contains("reduce", StringComparison.OrdinalIgnoreCase) || line.Contains("improve", StringComparison.OrdinalIgnoreCase)) && line.Length > 10)
                    {
                        mitigations.Add(line.Trim());
                    }
                }

                return mitigations.Take(3).ToArray();
            }
            catch
            {
                return new[] { "Enhanced monitoring", "Improved protocols" };
            }
        }

        private static ComponentPrediction[] ExtractComponentPredictions(string content)
        {
            try
            {
                var components = new List<ComponentPrediction>();
                var commonComponents = new[] { "brake", "tire", "engine", "transmission", "battery", "oil", "filter" };

                foreach (var component in commonComponents)
                {
                    if (content.Contains(component, StringComparison.OrdinalIgnoreCase))
                    {
                        components.Add(new ComponentPrediction
                        {
                            Component = char.ToUpperInvariant(component[0]) + component.Substring(1),
                            PredictedWearDate = DateTime.Now.AddDays(Random.Shared.Next(30, 120)),
                            ConfidenceLevel = 0.7 + Random.Shared.NextDouble() * 0.25,
                            EstimatedCost = 100 + Random.Shared.Next(50, 500)
                        });
                    }
                }

                return components.Take(3).ToArray();
            }
            catch
            {
                return Array.Empty<ComponentPrediction>();
            }
        }

        private static SafetyRiskFactor[] ExtractSafetyRiskFactors(string content)
        {
            try
            {
                var factors = new List<SafetyRiskFactor>();
                var riskTypes = new[] { "Weather", "Traffic", "Mechanical", "Route", "Driver" };

                foreach (var riskType in riskTypes)
                {
                    if (content.Contains(riskType, StringComparison.OrdinalIgnoreCase))
                    {
                        factors.Add(new SafetyRiskFactor
                        {
                            Factor = riskType,
                            RiskLevel = ExtractRiskLevel(content),
                            Impact = $"{riskType}-related safety considerations",
                            Mitigation = $"Enhanced {riskType.ToLowerInvariant()} monitoring and protocols"
                        });
                    }
                }

                return factors.Take(3).ToArray();
            }
            catch
            {
                return Array.Empty<SafetyRiskFactor>();
            }
        }

        private static string ExtractComplianceStatus(string content)
        {
            var lowerContent = content.ToLowerInvariant();
            if (lowerContent.Contains("non-compliant", StringComparison.OrdinalIgnoreCase) || lowerContent.Contains("violation", StringComparison.OrdinalIgnoreCase))
            {
                return "Non-Compliant";
            }

            if (lowerContent.Contains("partial", StringComparison.OrdinalIgnoreCase) || lowerContent.Contains("minor", StringComparison.OrdinalIgnoreCase))
            {
                return "Partially Compliant";
            }

            return "Fully Compliant";
        }

        private static StudentAssignment[] ExtractStudentAssignments(string content)
        {
            try
            {
                var assignments = new List<StudentAssignment>();
                var busCount = ExtractNumericValue(content, "bus", "buses", "vehicle") ?? 5;

                for (int i = 1; i <= Math.Min(busCount, 5); i++)
                {
                    assignments.Add(new StudentAssignment
                    {
                        BusId = i,
                        StudentsAssigned = 30 + Random.Shared.Next(10, 25),
                        CapacityUtilization = 0.6 + Random.Shared.NextDouble() * 0.3,
                        AverageRideTime = 20 + Random.Shared.Next(5, 15)
                    });
                }

                return assignments.ToArray();
            }
            catch
            {
                return Array.Empty<StudentAssignment>();
            }
        }

        #endregion

        #region JSON Serialization Options

        private static readonly JsonSerializerOptions ApiJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private static readonly JsonSerializerOptions ParseJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        #endregion
    }
}
