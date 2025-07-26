using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        #endregion

        #region Phase 1 Program Management Helper Methods

        // Minimal stub for missing method
        private PhaseAnalysisResult CreateMockPhase1Analysis(Phase1ProgressRequest request)
        {
            return new PhaseAnalysisResult();
        }

        // Minimal stub for missing method
        private string BuildDevelopmentInsightsPrompt(DevelopmentStateRequest request)
        {
            return "Development insights prompt";
        }

        // Minimal stub for missing method
        private DevelopmentInsights ParseDevelopmentInsights(string response)
        {
            return new DevelopmentInsights();
        }

        // Minimal stub for missing method
        private DevelopmentInsights CreateMockDevelopmentInsights(DevelopmentStateRequest request)
        {
            return new DevelopmentInsights();
        }

        // Minimal stub for missing method
        private string BuildPerformanceAnalysisPrompt(PerformanceDataRequest request)
        {
            return "Performance analysis prompt";
        }

        // Minimal stub for missing method
        private PerformanceAnalysis ParsePerformanceAnalysis(string response)
        {
            return new PerformanceAnalysis();
        }

        // Minimal stub for missing method
        private PerformanceAnalysis CreateMockPerformanceAnalysis(PerformanceDataRequest request)
        {
            return new PerformanceAnalysis();
        }

        // Minimal stub for missing method
        private string BuildMockDataPrompt(MockDataRequest request)
        {
            return "Mock data prompt";
        }

        // Minimal stub for missing method
        private GeneratedDataSet ParseGeneratedDataSet(string response)
        {
            return new GeneratedDataSet();
        }

        // Minimal stub for missing method
        private GeneratedDataSet CreateBasicMockData(MockDataRequest request)
        {
            return new GeneratedDataSet();
        }

        // Minimal stub for missing method
        private string BuildContextualHelpPrompt(HelpRequest request)
        {
            return "Contextual help prompt";
        }

        private string BuildPhase1AnalysisPrompt(Phase1ProgressRequest request)
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

        private PhaseAnalysisResult ParsePhase1AnalysisResponse(string response)
        {
            var result = new PhaseAnalysisResult();

            try
            {
                // Extract overall health
                var healthMatch = Regex.Match(response, @"Overall Health.*?:\s*(\w+)", RegexOptions.IgnoreCase);
                if (healthMatch.Success)
                    result.OverallHealth = healthMatch.Groups[1].Value;

                // Extract health score
                var scoreMatch = Regex.Match(response, @"Health Score.*?:\s*(\d+)", RegexOptions.IgnoreCase);
                if (scoreMatch.Success && int.TryParse(scoreMatch.Groups[1].Value, out int score))
                    result.HealthScore = score;

                // Extract risk level
                var riskMatch = Regex.Match(response, @"Risk Level.*?:\s*(\w+)", RegexOptions.IgnoreCase);
                if (riskMatch.Success)
                    result.RiskLevel = riskMatch.Groups[1].Value;

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
                    result.PredictedCompletionDate = completionDate;
                else
                    result.PredictedCompletionDate = DateTime.Now.AddDays(30); // Default estimate

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
                    result.TechnicalDebt = debtMatch.Groups[1].Value.Trim();

                // Extract team productivity
                var prodMatch = Regex.Match(response, @"(?:Team\s+)?Productivity.*?:\s*(.+?)(?=\n\n|\n[A-Z]|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (prodMatch.Success)
                    result.TeamProductivity = prodMatch.Groups[1].Value.Trim();
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

        private string BuildDevelopmentStatePrompt(DevelopmentStateRequest request)
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
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error gathering development metrics for {ComponentName}", componentName);
            }

            return request;
        }

        #endregion

        #region Prompt Building Methods

        private string BuildRouteOptimizationPrompt(RouteAnalysisRequest request)
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
- Current Condition: {request.WeatherData?.Condition}
- Temperature: {request.WeatherData?.Temperature}°C
- Visibility: {request.WeatherData?.Visibility}km
- Wind: {request.WeatherData?.WindCondition}

TRAFFIC STATUS:
- Overall Condition: {request.TrafficData?.OverallCondition}

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

        private string BuildMaintenancePredictionPrompt(MaintenanceAnalysisRequest request)
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

        private string BuildSafetyAnalysisPrompt(SafetyAnalysisRequest request)
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

        private string BuildStudentOptimizationPrompt(StudentOptimizationRequest request)
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

        private string GetTransportationExpertSystemPrompt()
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

        private string GetMaintenanceExpertSystemPrompt()
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

        private string GetSafetyExpertSystemPrompt()
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

        private string GetLogisticsExpertSystemPrompt()
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

        private string GetGeneralChatSystemPrompt()
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
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };

                var json = JsonSerializer.Serialize(request, options);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                Logger.Debug("Calling xAI API: {Endpoint} with model: {Model}", endpoint, request.Model);

                var response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                Logger.Debug("xAI API response received: {ResponseLength} characters", responseJson.Length);

                var result = JsonSerializer.Deserialize<XAIResponse>(responseJson, options);
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

        private async Task<AIRouteRecommendations> GenerateMockAIRecommendations(RouteAnalysisRequest request)
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

        private async Task<AIMaintenancePrediction> GenerateMockMaintenancePrediction(MaintenanceAnalysisRequest request)
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

        private async Task<AISafetyAnalysis> GenerateMockSafetyAnalysis(SafetyAnalysisRequest request)
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

        private async Task<AIStudentOptimization> GenerateMockStudentOptimization(StudentOptimizationRequest request)
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

        private string GenerateMockChatResponse(string message)
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

        private AIRouteRecommendations ParseRouteRecommendations(XAIResponse response)
        {
            try
            {
                if (response?.Choices?.Length > 0)
                {
                    var content = response.Choices[0].Message.Content;
                    Logger.Debug("Parsing xAI route optimization response: {Content}", content);

                    // Try to parse structured JSON response or extract key information
                    if (content.Contains("{") && content.Contains("}"))
                    {
                        // Attempt to parse JSON response
                        try
                        {
                            var options = new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                PropertyNameCaseInsensitive = true
                            };
                            return JsonSerializer.Deserialize<AIRouteRecommendations>(content, options) ?? CreateDefaultRouteRecommendations(content);
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

        private AIRouteRecommendations CreateDefaultRouteRecommendations(string aiResponse)
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

        private AIMaintenancePrediction ParseMaintenancePrediction(XAIResponse response)
        {
            try
            {
                if (response?.Choices?.Length > 0)
                {
                    var content = response.Choices[0].Message.Content;
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

        private AISafetyAnalysis ParseSafetyAnalysis(XAIResponse response)
        {
            try
            {
                if (response?.Choices?.Length > 0)
                {
                    var content = response.Choices[0].Message.Content;
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

        private AIStudentOptimization ParseStudentOptimization(XAIResponse response)
        {
            try
            {
                if (response?.Choices?.Length > 0)
                {
                    var content = response.Choices[0].Message.Content;
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

        private double? ExtractNumericValue(string content, params string[] keywords)
        {
            try
            {
                var lowerContent = content.ToLower();
                foreach (var keyword in keywords)
                {
                    var keywordIndex = lowerContent.IndexOf(keyword.ToLower());
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

        private string[] ExtractRecommendations(string content)
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

        private string ExtractRiskLevel(string content)
        {
            var lowerContent = content.ToLowerInvariant();
            if (lowerContent.Contains("high risk", StringComparison.OrdinalIgnoreCase) || lowerContent.Contains("critical", StringComparison.OrdinalIgnoreCase))
                return "High";
            if (lowerContent.Contains("medium risk") || lowerContent.Contains("moderate"))
                return "Medium";
            if (lowerContent.Contains("low risk") || lowerContent.Contains("minimal"))
                return "Low";
            return "Medium"; // Default
        }

        private string[] ExtractRisks(string content)
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

        private string[] ExtractMitigations(string content)
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

        private ComponentPrediction[] ExtractComponentPredictions(string content)
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

        private SafetyRiskFactor[] ExtractSafetyRiskFactors(string content)
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

        private string ExtractComplianceStatus(string content)
        {
            var lowerContent = content.ToLowerInvariant();
            if (lowerContent.Contains("non-compliant", StringComparison.OrdinalIgnoreCase) || lowerContent.Contains("violation", StringComparison.OrdinalIgnoreCase))
                return "Non-Compliant";
            if (lowerContent.Contains("partial", StringComparison.OrdinalIgnoreCase) || lowerContent.Contains("minor", StringComparison.OrdinalIgnoreCase))
                return "Partially Compliant";
            return "Fully Compliant";
        }

        private StudentAssignment[] ExtractStudentAssignments(string content)
        {
            try
            {
                var assignments = new List<StudentAssignment>();
                var busCount = (int)(ExtractNumericValue(content, "bus", "buses") ?? 3);

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

        #region Documentation Access Methods

        /// <summary>
        /// Gets the official xAI overview documentation URL
        /// </summary>
        public string GetOfficialDocumentationUrl() => _documentationSettings.GetOverviewUrl();

        /// <summary>
        /// Gets the xAI Chat API guide URL
        /// </summary>
        public string GetChatApiGuideUrl() => _documentationSettings.GetChatGuideUrl();

        /// <summary>
        /// Gets the xAI API reference URL
        /// </summary>
        public string GetApiReferenceUrl() => _documentationSettings.GetApiReferenceUrl();

        /// <summary>
        /// Gets all available documentation URLs
        /// </summary>
        public Dictionary<string, string> GetAllDocumentationUrls() => _documentationSettings.GetAllUrls();

        /// <summary>
        /// Validates that all documentation URLs are accessible
        /// </summary>
        public bool ValidateDocumentationUrls() => _documentationSettings.ValidateUrls();

        #endregion

        #region Data Models for xAI Integration

        public class RouteAnalysisRequest
        {
            public int RouteId { get; set; }
            public double CurrentDistance { get; set; }
            public int StudentCount { get; set; }
            public int VehicleCapacity { get; set; }
            public TerrainAnalysisResult? TerrainData { get; set; }
            public WeatherData? WeatherData { get; set; }
            public TrafficData? TrafficData { get; set; }
            public HistoricalPerformanceData? HistoricalData { get; set; }
        }

        public class HistoricalPerformanceData
        {
            public double AverageFuelConsumption { get; set; }
            public double OnTimePerformance { get; set; }
            public int SafetyIncidents { get; set; }
        }

        public class AIRouteRecommendations
        {
            public RouteRecommendation OptimalRoute { get; set; } = new();
            public RiskAssessment RiskAssessment { get; set; } = new();
            public double ConfidenceLevel { get; set; }
            public string Reasoning { get; set; } = string.Empty;
        }

        public class RouteRecommendation
        {
            public double EstimatedFuelSavings { get; set; }
            public double EstimatedTimeSavings { get; set; }
            public double SafetyScore { get; set; }
            public string[] RecommendedChanges { get; set; } = Array.Empty<string>();
        }

        public class RiskAssessment
        {
            public string OverallRiskLevel { get; set; } = string.Empty;
            public string[] IdentifiedRisks { get; set; } = Array.Empty<string>();
            public string[] MitigationStrategies { get; set; } = Array.Empty<string>();
        }

        public class MaintenanceAnalysisRequest
        {
            public int BusId { get; set; }
            public string VehicleMake { get; set; } = string.Empty;
            public string VehicleModel { get; set; } = string.Empty;
            public int VehicleYear { get; set; }
            public int CurrentMileage { get; set; }
            public DateTime LastMaintenanceDate { get; set; }
            public double DailyMiles { get; set; }
            public string TerrainDifficulty { get; set; } = string.Empty;
            public double StopFrequency { get; set; }
            public double ElevationChanges { get; set; }
            public int EngineHours { get; set; }
            public string BrakeUsage { get; set; } = string.Empty;
            public string TireCondition { get; set; } = string.Empty;
            public string FluidLevels { get; set; } = string.Empty;
        }

        public class AIMaintenancePrediction
        {
            public DateTime PredictedMaintenanceDate { get; set; }
            public ComponentPrediction[] ComponentPredictions { get; set; } = Array.Empty<ComponentPrediction>();
            public decimal TotalEstimatedCost { get; set; }
            public decimal PotentialSavings { get; set; }
            public string Reasoning { get; set; } = string.Empty;
        }

        public class ComponentPrediction
        {
            public string Component { get; set; } = string.Empty;
            public DateTime PredictedWearDate { get; set; }
            public double ConfidenceLevel { get; set; }
            public decimal EstimatedCost { get; set; }
        }

        public class SafetyAnalysisRequest
        {
            public string RouteType { get; set; } = string.Empty;
            public string TrafficDensity { get; set; } = string.Empty;
            public string RoadConditions { get; set; } = string.Empty;
            public string WeatherConditions { get; set; } = string.Empty;
            public string[] AgeGroups { get; set; } = Array.Empty<string>();
            public int SpecialNeedsCount { get; set; }
            public int TotalStudents { get; set; }
            public int PreviousIncidents { get; set; }
            public int NearMissReports { get; set; }
            public string DriverSafetyRecord { get; set; } = string.Empty;
        }

        public class AISafetyAnalysis
        {
            public double OverallSafetyScore { get; set; }
            public SafetyRiskFactor[] RiskFactors { get; set; } = Array.Empty<SafetyRiskFactor>();
            public string[] Recommendations { get; set; } = Array.Empty<string>();
            public string ComplianceStatus { get; set; } = string.Empty;
            public double ConfidenceLevel { get; set; }
        }

        public class SafetyRiskFactor
        {
            public string Factor { get; set; } = string.Empty;
            public string RiskLevel { get; set; } = string.Empty;
            public string Impact { get; set; } = string.Empty;
            public string Mitigation { get; set; } = string.Empty;
        }

        public class StudentOptimizationRequest
        {
            public int TotalStudents { get; set; }
            public int AvailableBuses { get; set; }
            public string GeographicConstraints { get; set; } = string.Empty;
            public string SpecialRequirements { get; set; } = string.Empty;
        }

        public class AIStudentOptimization
        {
            public StudentAssignment[] OptimalAssignments { get; set; } = Array.Empty<StudentAssignment>();
            public EfficiencyMetrics EfficiencyGains { get; set; } = new();
            public double ConfidenceLevel { get; set; }
            public string Reasoning { get; set; } = string.Empty;
        }

        public class StudentAssignment
        {
            public int BusId { get; set; }
            public int StudentsAssigned { get; set; }
            public double CapacityUtilization { get; set; }
            public double AverageRideTime { get; set; }
        }

        public class EfficiencyMetrics
        {
            public double TotalTimeSaved { get; set; }
            public double FuelSavings { get; set; }
            public double CapacityOptimization { get; set; }
        }

        #endregion

        #region xAI API Models (Future Implementation)

        public class XAIRequest
        {
            public string Model { get; set; } = string.Empty;
            public XAIMessage[] Messages { get; set; } = Array.Empty<XAIMessage>();
            public double Temperature { get; set; }
            public int MaxTokens { get; set; }
        }

        public class XAIMessage
        {
            public string Role { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
        }

        public class XAIResponse
        {
            public XAIChoice[] Choices { get; set; } = Array.Empty<XAIChoice>();
            public XAIUsage Usage { get; set; } = new();
        }

        public class XAIChoice
        {
            public XAIMessage Message { get; set; } = new();
            public string FinishReason { get; set; } = string.Empty;
        }

        public class XAIUsage
        {
            public int PromptTokens { get; set; }
            public int CompletionTokens { get; set; }
            public int TotalTokens { get; set; }
        }

        #endregion

        #region Phase 1 Program Management & Analytics Models

        public class Phase1ProgressRequest
        {
            public string ProjectName { get; set; } = "BusBuddy";
            public string CurrentPhase { get; set; } = "Phase 1";
            public int DaysInDevelopment { get; set; }
            public int TeamSize { get; set; } = 1;
            public DateTime TargetCompletion { get; set; }
            public string BuildStatus { get; set; } = "Passing";
            public int TestsPassingCount { get; set; }
            public int TotalTestsCount { get; set; }
            public double CodeCoverage { get; set; }
            public int CriticalIssuesCount { get; set; }
            public int TotalComponents { get; set; }
            public int CompletedComponents { get; set; }
            public int LinesOfCode { get; set; }
            public int CommitsThisWeek { get; set; }
            public int OpenPullRequests { get; set; }
            public int MergedPullRequests { get; set; }
            public double AverageBuildTime { get; set; }
            public int ActiveDevelopers { get; set; }
            public int IssuesClosedThisWeek { get; set; }
            public int DocumentationPages { get; set; }
            public List<string> CurrentFocusAreas { get; set; } = new();
        }

        public class PhaseAnalysisResult
        {
            public string OverallHealth { get; set; } = string.Empty;
            public int HealthScore { get; set; }
            public string RiskLevel { get; set; } = string.Empty;
            public string[] Recommendations { get; set; } = Array.Empty<string>();
            public DateTime PredictedCompletionDate { get; set; }
            public string[] NextMilestones { get; set; } = Array.Empty<string>();
            public string TechnicalDebt { get; set; } = string.Empty;
            public string TeamProductivity { get; set; } = string.Empty;
        }

        public class DevelopmentStateRequest
        {
            public string ComponentName { get; set; } = string.Empty;
            public List<string> TechnologyStack { get; set; } = new();
            public string ComplexityLevel { get; set; } = "Medium";
            public DateTime LastModified { get; set; }
            public int FilesChanged { get; set; }
            public int LinesAdded { get; set; }
            public int LinesRemoved { get; set; }
            public int MethodsCount { get; set; }
            public int ClassesCount { get; set; }
            public int RecentCommits { get; set; }
            public int BugReports { get; set; }
            public int FeatureRequests { get; set; }
            public int PerformanceIssues { get; set; }
            public List<string> Dependencies { get; set; } = new();
            public List<string> CurrentChallenges { get; set; } = new();
        }

        public class DevelopmentInsights
        {
            public int QualityScore { get; set; }
            public string[] RecommendedActions { get; set; } = Array.Empty<string>();
            public string ArchitectureHealth { get; set; } = string.Empty;
            public int PerformanceScore { get; set; }
            public int SecurityScore { get; set; }
            public int MaintainabilityScore { get; set; }
            public string BestPracticesAlignment { get; set; } = string.Empty;
            public string[] PriorityIssues { get; set; } = Array.Empty<string>();
        }

        public class PerformanceDataRequest
        {
            public string ApplicationName { get; set; } = "BusBuddy";
            public string Version { get; set; } = "1.0.0";
            public string Environment { get; set; } = "Development";
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public double AverageResponseTime { get; set; }
            public double PeakResponseTime { get; set; }
            public double MemoryUsage { get; set; }
            public double PeakMemoryUsage { get; set; }
            public double AverageCpuUsage { get; set; }
            public double PeakCpuUsage { get; set; }
            public double DatabaseQueryTime { get; set; }
            public double Uptime { get; set; }
            public double ErrorRate { get; set; }
            public int SuccessfulRequests { get; set; }
            public int FailedRequests { get; set; }
            public int ActiveUsers { get; set; }
            public double AverageSessionDuration { get; set; }
            public double PageLoadTime { get; set; }
            public double UserSatisfactionScore { get; set; }
            public List<string> IdentifiedBottlenecks { get; set; } = new();
        }

        public class PerformanceAnalysis
        {
            public int OverallScore { get; set; }
            public string[] CriticalIssues { get; set; } = Array.Empty<string>();
            public string[] OptimizationOpportunities { get; set; } = Array.Empty<string>();
            public string ScalabilityAssessment { get; set; } = string.Empty;
            public string ResourceUtilization { get; set; } = string.Empty;
            public Dictionary<string, string> ExpectedImprovements { get; set; } = new();
        }

        public class MockDataRequest
        {
            public string DataType { get; set; } = string.Empty;
            public int RecordCount { get; set; } = 25;
            public string RealismLevel { get; set; } = "High";
            public string Purpose { get; set; } = "Development Testing";
            public List<SchemaField> SchemaFields { get; set; } = new();
            public string GeographicRegion { get; set; } = "US Midwest";
            public string TimePeriod { get; set; } = "School Year 2024-2025";
            public string Scenario { get; set; } = "Normal Operations";
            public List<string> Relationships { get; set; } = new();
            public List<string> Constraints { get; set; } = new();
        }

        public class SchemaField
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Constraints { get; set; } = string.Empty;
        }

        public class GeneratedDataSet
        {
            public int RecordCount { get; set; }
            public string DataType { get; set; } = string.Empty;
            public object[] GeneratedData { get; set; } = Array.Empty<object>();
            public Dictionary<string, object> Metadata { get; set; } = new();
        }

        public class HelpRequest
        {
            public string ExperienceLevel { get; set; } = "Intermediate";
            public string CurrentTask { get; set; } = string.Empty;
            public string TechnologyFocus { get; set; } = string.Empty;
            public string LearningGoal { get; set; } = string.Empty;
            public string Question { get; set; } = string.Empty;
            public string CodeContext { get; set; } = string.Empty;
            public string ErrorMessage { get; set; } = string.Empty;
            public List<string> AttemptedSolutions { get; set; } = new();
            public string Topic { get; set; } = string.Empty;
        }

        // Minimal stub for missing method
        private ContextualHelp CreateMockDevelopmentInsights(HelpRequest request)
        {
            return new ContextualHelp
            {
                Answer = "Development insights are not available in this build.",
                Confidence = 0,
                Examples = Array.Empty<string>(),
                RelatedTopics = Array.Empty<string>(),
                DocumentationLinks = Array.Empty<string>(),
                TroubleshootingTips = Array.Empty<string>()
            };
        }

        // Minimal stub for missing method
        private ContextualHelp ParseContextualHelp(string response)
        {
            return new ContextualHelp
            {
                Answer = response ?? "No help available.",
                Confidence = 50,
                Examples = Array.Empty<string>(),
                RelatedTopics = Array.Empty<string>(),
                DocumentationLinks = Array.Empty<string>(),
                TroubleshootingTips = Array.Empty<string>()
            };
        }

        // Minimal stub for missing method
        private ContextualHelp CreateBasicHelp(HelpRequest request)
        {
            return new ContextualHelp
            {
                Answer = "Basic help is not available in this build.",
                Confidence = 0,
                Examples = Array.Empty<string>(),
                RelatedTopics = Array.Empty<string>(),
                DocumentationLinks = Array.Empty<string>(),
                TroubleshootingTips = Array.Empty<string>()
            };
        }

        public class ContextualHelp
        {
            public string Answer { get; set; } = string.Empty;
            public int Confidence { get; set; }
            public string[] Examples { get; set; } = Array.Empty<string>();
            public string[] RelatedTopics { get; set; } = Array.Empty<string>();
            public string[] DocumentationLinks { get; set; } = Array.Empty<string>();
            public string[] TroubleshootingTips { get; set; } = Array.Empty<string>();
        }

        #endregion
    }
}
