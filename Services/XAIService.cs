using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Bus_Buddy.Models;
using BusBuddy.Configuration;

namespace Bus_Buddy.Services
{
    /// <summary>
    /// xAI (Grok) Integration Service for Advanced AI-Powered Transportation Intelligence
    /// Programmatically locked documentation references maintained in XAIDocumentationSettings
    /// </summary>
    public class XAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<XAIService> _logger;
        private readonly IConfiguration _configuration;
        private readonly XAIDocumentationSettings _documentationSettings;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly bool _isConfigured;

        // API Endpoints
        public static readonly string CHAT_COMPLETIONS_ENDPOINT = "/chat/completions";

        public XAIService(HttpClient httpClient, ILogger<XAIService> logger, IConfiguration configuration,
            IOptions<XAIDocumentationSettings> documentationOptions)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _documentationSettings = documentationOptions?.Value ?? new XAIDocumentationSettings();

            // Load xAI configuration
            _apiKey = _configuration["XAI:ApiKey"] ?? Environment.GetEnvironmentVariable("XAI_API_KEY") ?? string.Empty;
            _baseUrl = _configuration["XAI:BaseUrl"] ?? "https://api.x.ai/v1";
            var useLiveAPI = _configuration.GetValue<bool>("XAI:UseLiveAPI", true);

            _isConfigured = !string.IsNullOrEmpty(_apiKey) && !_apiKey.Contains("YOUR_XAI_API_KEY") && useLiveAPI;

            if (!_isConfigured)
            {
                _logger.LogWarning("xAI not configured or disabled. Using mock AI responses. Please set XAI_API_KEY environment variable and enable UseLiveAPI in appsettings.json.");
                _logger.LogInformation("xAI Documentation: {ChatGuideUrl}", _documentationSettings.GetChatGuideUrl());
            }
            else
            {
                _logger.LogInformation("xAI configured for live AI transportation intelligence");
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
                _logger.LogInformation("Requesting xAI route optimization analysis");

                if (!_isConfigured)
                {
                    return await GenerateMockAIRecommendations(request);
                }

                var prompt = BuildRouteOptimizationPrompt(request);
                var xaiRequest = new XAIRequest
                {
                    Model = _configuration["XAI:DefaultModel"] ?? "grok-3-latest",
                    Messages = new[]
                    {
                        new XAIMessage { Role = "system", Content = GetTransportationExpertSystemPrompt() },
                        new XAIMessage { Role = "user", Content = prompt }
                    },
                    Temperature = _configuration.GetValue<double>("XAI:Temperature", 0.3),
                    MaxTokens = _configuration.GetValue<int>("XAI:MaxTokens", 4000)
                };

                var response = await CallXAIAPI(CHAT_COMPLETIONS_ENDPOINT, xaiRequest);
                return ParseRouteRecommendations(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in xAI route optimization analysis");
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
                _logger.LogInformation("Requesting xAI maintenance prediction analysis");

                var prompt = BuildMaintenancePredictionPrompt(request);
                var xaiRequest = new XAIRequest
                {
                    Model = _configuration["XAI:DefaultModel"] ?? "grok-3-latest",
                    Messages = new[]
                    {
                        new XAIMessage { Role = "system", Content = GetMaintenanceExpertSystemPrompt() },
                        new XAIMessage { Role = "user", Content = prompt }
                    },
                    Temperature = 0.2, // Lower temperature for more precise technical predictions
                    MaxTokens = _configuration.GetValue<int>("XAI:MaxTokens", 4000) / 2
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
                _logger.LogError(ex, "Error in xAI maintenance prediction");
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
                _logger.LogInformation("Requesting xAI safety risk analysis");

                var prompt = BuildSafetyAnalysisPrompt(request);
                var xaiRequest = new XAIRequest
                {
                    Model = _configuration["XAI:DefaultModel"] ?? "grok-3-latest",
                    Messages = new[]
                    {
                        new XAIMessage { Role = "system", Content = GetSafetyExpertSystemPrompt() },
                        new XAIMessage { Role = "user", Content = prompt }
                    },
                    Temperature = 0.1, // Very low temperature for safety-critical analysis
                    MaxTokens = _configuration.GetValue<int>("XAI:MaxTokens", 4000) / 2
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
                _logger.LogError(ex, "Error in xAI safety analysis");
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
                _logger.LogInformation("Requesting xAI student assignment optimization");

                var prompt = BuildStudentOptimizationPrompt(request);
                var xaiRequest = new XAIRequest
                {
                    Model = _configuration["XAI:DefaultModel"] ?? "grok-3-latest",
                    Messages = new[]
                    {
                        new XAIMessage { Role = "system", Content = GetLogisticsExpertSystemPrompt() },
                        new XAIMessage { Role = "user", Content = prompt }
                    },
                    Temperature = _configuration.GetValue<double>("XAI:Temperature", 0.3),
                    MaxTokens = _configuration.GetValue<int>("XAI:MaxTokens", 4000)
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
                _logger.LogError(ex, "Error in xAI student optimization");
                return await GenerateMockStudentOptimization(request);
            }
        }

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
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogDebug("Calling xAI API: {Endpoint} with model: {Model}", endpoint, request.Model);

                var response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("xAI API response received: {ResponseLength} characters", responseJson.Length);

                var result = JsonSerializer.Deserialize<XAIResponse>(responseJson, options);
                return result ?? new XAIResponse();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling xAI API: {Message}", ex.Message);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error from xAI API: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling xAI API: {Message}", ex.Message);
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

        #endregion

        #region Response Parsing (Future Implementation)

        private AIRouteRecommendations ParseRouteRecommendations(XAIResponse response)
        {
            try
            {
                if (response?.Choices?.Length > 0)
                {
                    var content = response.Choices[0].Message.Content;
                    _logger.LogDebug("Parsing xAI route optimization response: {Content}", content);

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
                _logger.LogError(ex, "Error parsing xAI route recommendations");
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
                    _logger.LogDebug("Parsing xAI maintenance prediction response");

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
                _logger.LogError(ex, "Error parsing xAI maintenance prediction");
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
                    _logger.LogDebug("Parsing xAI safety analysis response");

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
                _logger.LogError(ex, "Error parsing xAI safety analysis");
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
                    _logger.LogDebug("Parsing xAI student optimization response");

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
                _logger.LogError(ex, "Error parsing xAI student optimization");
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
                    if (trimmed.StartsWith("-") || trimmed.StartsWith("•") ||
                        trimmed.StartsWith("*") || char.IsDigit(trimmed[0]))
                    {
                        recommendations.Add(trimmed.TrimStart('-', '•', '*', ' ', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.'));
                    }
                    else if (trimmed.ToLower().Contains("recommend") || trimmed.ToLower().Contains("suggest"))
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
            var lowerContent = content.ToLower();
            if (lowerContent.Contains("high risk") || lowerContent.Contains("critical"))
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
                    if (line.ToLower().Contains("risk") && line.Length > 10)
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
                    var lower = line.ToLower();
                    if ((lower.Contains("mitigate") || lower.Contains("prevent") ||
                         lower.Contains("reduce") || lower.Contains("improve")) && line.Length > 10)
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
                    if (content.ToLower().Contains(component))
                    {
                        components.Add(new ComponentPrediction
                        {
                            Component = char.ToUpper(component[0]) + component.Substring(1),
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
                    if (content.ToLower().Contains(riskType.ToLower()))
                    {
                        factors.Add(new SafetyRiskFactor
                        {
                            Factor = riskType,
                            RiskLevel = ExtractRiskLevel(content),
                            Impact = $"{riskType}-related safety considerations",
                            Mitigation = $"Enhanced {riskType.ToLower()} monitoring and protocols"
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
            var lowerContent = content.ToLower();
            if (lowerContent.Contains("non-compliant") || lowerContent.Contains("violation"))
                return "Non-Compliant";
            if (lowerContent.Contains("partial") || lowerContent.Contains("minor"))
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
    }

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
    }
}
