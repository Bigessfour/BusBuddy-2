using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Bus_Buddy.Services;
using Bus_Buddy.Utilities;

namespace Bus_Buddy.Forms
{
    /// <summary>
    /// AI Assistant Panel integrating xAI (Grok) chat functionality into the Bus Buddy Dashboard
    /// </summary>
    public partial class AIAssistantPanel : UserControl
    {
        private readonly ILogger<AIAssistantPanel> _logger;
        private readonly XAIService _xaiService;
        private readonly GoogleEarthEngineService _geeService;

        // UI Components
        private TabControlAdv mainTabControl;
        private TabPageAdv summaryTab;
        private TabPageAdv chatTab;

        // Chat Components
        private Panel chatPanel;
        private RichTextBox chatHistory;
        private TextBox chatInput;
        private SfButton sendButton;
        private SfButton clearButton;
        private ComboBox queryTypeCombo;
        private Panel chatControlsPanel;

        // Fleet Summary Components (existing)
        private Panel summaryContentPanel;
        private AutoLabel summaryTitle;
        private AutoLabel statsLabel;
        private HubTile fleetTile;
        private HubTile routesTile;
        private HubTile activeTile;
        private HubTile maintenanceTile;
        private HubTile capacityTile;

        // Chat history storage
        private List<ChatMessage> chatMessages = new List<ChatMessage>();

        public AIAssistantPanel(ILogger<AIAssistantPanel> logger, XAIService xaiService, GoogleEarthEngineService geeService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _xaiService = xaiService ?? throw new ArgumentNullException(nameof(xaiService));
            _geeService = geeService ?? throw new ArgumentNullException(nameof(geeService));

            InitializeComponent();
            InitializeAIChat();

            _logger.LogInformation("AI Assistant Panel initialized with xAI and Google Earth Engine integration");
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main container setup
            this.Size = new Size(420, 420);
            this.BackColor = Color.FromArgb(248, 249, 250);
            this.BorderStyle = BorderStyle.FixedSingle;

            // Initialize TabControl
            this.mainTabControl = new TabControlAdv();
            this.mainTabControl.Size = new Size(418, 418);
            this.mainTabControl.Location = new Point(1, 1);
            this.mainTabControl.TabStyle = typeof(TabRendererMetro);
            this.mainTabControl.ActiveTabColor = Color.FromArgb(46, 125, 185);
            this.mainTabControl.InactiveTabColor = Color.FromArgb(224, 224, 224);
            this.mainTabControl.BorderVisible = false;

            // Fleet Summary Tab
            this.summaryTab = new TabPageAdv();
            this.summaryTab.Text = "Fleet Summary";
            this.summaryTab.Image = null;
            this.summaryTab.BackColor = Color.FromArgb(248, 249, 250);

            // AI Chat Tab
            this.chatTab = new TabPageAdv();
            this.chatTab.Text = "AI Assistant";
            this.chatTab.Image = null;
            this.chatTab.BackColor = Color.FromArgb(248, 249, 250);

            this.mainTabControl.TabPages.Add(this.summaryTab);
            this.mainTabControl.TabPages.Add(this.chatTab);

            // Initialize Fleet Summary content
            InitializeFleetSummary();

            // Initialize AI Chat content
            InitializeAIChatUI();

            this.Controls.Add(this.mainTabControl);
            this.ResumeLayout();
        }

        private void InitializeFleetSummary()
        {
            this.summaryContentPanel = new Panel();
            this.summaryContentPanel.Size = new Size(410, 390);
            this.summaryContentPanel.Location = new Point(5, 5);
            this.summaryContentPanel.BackColor = Color.FromArgb(248, 249, 250);

            // Summary title
            this.summaryTitle = new AutoLabel();
            this.summaryTitle.Text = "Fleet Summary";
            this.summaryTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.summaryTitle.ForeColor = Color.FromArgb(46, 125, 185);
            this.summaryTitle.Location = new Point(10, 10);
            this.summaryTitle.AutoSize = true;

            // Initialize HubTiles
            this.fleetTile = CreateHubTile("Total Fleet", "0", "Vehicles", Color.FromArgb(63, 81, 181), 10, 45);
            this.routesTile = CreateHubTile("Active Routes", "0", "Routes", Color.FromArgb(76, 175, 80), 210, 45);
            this.activeTile = CreateHubTile("Active Buses", "0", "In Service", Color.FromArgb(255, 152, 0), 10, 140);
            this.maintenanceTile = CreateHubTile("Maintenance", "0", "In Shop", Color.FromArgb(244, 67, 54), 210, 140);
            this.capacityTile = CreateHubTile("Total Capacity", "0", "Passengers", Color.FromArgb(156, 39, 176), 110, 235);

            // Stats label
            this.statsLabel = new AutoLabel();
            this.statsLabel.Text = "Fleet statistics will be updated...";
            this.statsLabel.Font = new Font("Segoe UI", 9F);
            this.statsLabel.ForeColor = Color.FromArgb(95, 99, 104);
            this.statsLabel.Location = new Point(10, 330);
            this.statsLabel.Size = new Size(390, 50);

            // Add controls to summary panel
            this.summaryContentPanel.Controls.Add(this.summaryTitle);
            this.summaryContentPanel.Controls.Add(this.fleetTile);
            this.summaryContentPanel.Controls.Add(this.routesTile);
            this.summaryContentPanel.Controls.Add(this.activeTile);
            this.summaryContentPanel.Controls.Add(this.maintenanceTile);
            this.summaryContentPanel.Controls.Add(this.capacityTile);
            this.summaryContentPanel.Controls.Add(this.statsLabel);

            this.summaryTab.Controls.Add(this.summaryContentPanel);
        }

        private void InitializeAIChatUI()
        {
            this.chatPanel = new Panel();
            this.chatPanel.Size = new Size(410, 390);
            this.chatPanel.Location = new Point(5, 5);
            this.chatPanel.BackColor = Color.FromArgb(248, 249, 250);

            // Query type selector
            this.queryTypeCombo = new ComboBox();
            this.queryTypeCombo.Size = new Size(200, 25);
            this.queryTypeCombo.Location = new Point(10, 10);
            this.queryTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this.queryTypeCombo.Items.AddRange(new string[]
            {
                "General Transportation Query",
                "Route Optimization Analysis",
                "Maintenance Prediction",
                "Safety Risk Assessment",
                "Student Assignment Optimization",
                "Fuel Efficiency Analysis",
                "Cost Optimization",
                "Fleet Performance Review"
            });
            this.queryTypeCombo.SelectedIndex = 0;

            // Chat history display
            this.chatHistory = new RichTextBox();
            this.chatHistory.Size = new Size(390, 280);
            this.chatHistory.Location = new Point(10, 45);
            this.chatHistory.ReadOnly = true;
            this.chatHistory.BackColor = Color.White;
            this.chatHistory.Font = new Font("Segoe UI", 9F);
            this.chatHistory.ScrollBars = RichTextBoxScrollBars.Vertical;
            this.chatHistory.Text = "🤖 AI Assistant: Hello! I'm your Bus Buddy AI assistant powered by xAI (Grok). I can help with:\n\n" +
                                   "• Route optimization and planning\n" +
                                   "• Predictive maintenance analysis\n" +
                                   "• Safety risk assessments\n" +
                                   "• Student assignment optimization\n" +
                                   "• Fleet performance analytics\n" +
                                   "• Cost optimization strategies\n\n" +
                                   "What would you like to know about your transportation system?\n\n";

            // Chat controls panel
            this.chatControlsPanel = new Panel();
            this.chatControlsPanel.Size = new Size(390, 50);
            this.chatControlsPanel.Location = new Point(10, 335);
            this.chatControlsPanel.BackColor = Color.Transparent;

            // Chat input
            this.chatInput = new TextBox();
            this.chatInput.Size = new Size(250, 25);
            this.chatInput.Location = new Point(0, 15);
            this.chatInput.PlaceholderText = "Ask about routes, maintenance, safety...";
            this.chatInput.Font = new Font("Segoe UI", 9F);
            this.chatInput.KeyPress += ChatInput_KeyPress;

            // Send button
            this.sendButton = new SfButton();
            this.sendButton.Size = new Size(60, 25);
            this.sendButton.Location = new Point(260, 15);
            this.sendButton.Text = "Send";
            this.sendButton.BackColor = Color.FromArgb(46, 125, 185);
            this.sendButton.ForeColor = Color.White;
            this.sendButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.sendButton.Click += SendButton_Click;

            // Clear button
            this.clearButton = new SfButton();
            this.clearButton.Size = new Size(60, 25);
            this.clearButton.Location = new Point(330, 15);
            this.clearButton.Text = "Clear";
            this.clearButton.BackColor = Color.FromArgb(158, 158, 158);
            this.clearButton.ForeColor = Color.White;
            this.clearButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.clearButton.Click += ClearButton_Click;

            // Add controls to chat controls panel
            this.chatControlsPanel.Controls.Add(this.chatInput);
            this.chatControlsPanel.Controls.Add(this.sendButton);
            this.chatControlsPanel.Controls.Add(this.clearButton);

            // Add controls to chat panel
            this.chatPanel.Controls.Add(this.queryTypeCombo);
            this.chatPanel.Controls.Add(this.chatHistory);
            this.chatPanel.Controls.Add(this.chatControlsPanel);

            this.chatTab.Controls.Add(this.chatPanel);
        }

        private HubTile CreateHubTile(string banner, string title, string body, Color color, int x, int y)
        {
            var tile = new HubTile();
            tile.Banner.Text = banner;
            tile.Title.Text = title;
            tile.Title.TextColor = Color.White;
            tile.Title.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            tile.Body.Text = body;
            tile.Body.TextColor = Color.WhiteSmoke;
            tile.Body.Font = new Font("Segoe UI", 9F);
            tile.Location = new Point(x, y);
            tile.Size = new Size(180, 80);
            tile.BackColor = color;
            tile.TileType = Syncfusion.Windows.Forms.Tools.HubTileType.DefaultTile;
            return tile;
        }

        private void InitializeAIChat()
        {
            // Log xAI service status
            if (_xaiService.IsConfigured)
            {
                _logger.LogInformation("xAI service is configured and ready for live AI assistance");
            }
            else
            {
                _logger.LogWarning("xAI service not configured - using mock responses. Set XAI_API_KEY environment variable for live AI.");
            }
        }

        #region Chat Event Handlers

        private void ChatInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                ProcessChatMessage();
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            ProcessChatMessage();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            chatHistory.Clear();
            chatMessages.Clear();
            chatHistory.Text = "🤖 AI Assistant: Chat cleared. How can I assist you with your transportation management?\n\n";
        }

        private async void ProcessChatMessage()
        {
            var userMessage = chatInput.Text.Trim();
            if (string.IsNullOrEmpty(userMessage)) return;

            try
            {
                // Add user message to chat
                AddChatMessage("👤 You", userMessage, Color.FromArgb(46, 125, 185));
                chatInput.Clear();

                // Show typing indicator
                AddChatMessage("🤖 AI Assistant", "Analyzing your request...", Color.FromArgb(76, 175, 80));

                // Process with xAI based on query type
                var queryType = queryTypeCombo.SelectedItem?.ToString() ?? "General Transportation Query";
                var response = await ProcessAIQuery(userMessage, queryType);

                // Remove typing indicator and add actual response
                RemoveLastMessage();
                AddChatMessage("🤖 AI Assistant", response, Color.FromArgb(76, 175, 80));

                _logger.LogInformation("AI chat query processed: {QueryType}", queryType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI chat message");
                RemoveLastMessage();
                AddChatMessage("🤖 AI Assistant", "I apologize, but I encountered an error processing your request. Please try again.", Color.FromArgb(244, 67, 54));
            }
        }

        private async Task<string> ProcessAIQuery(string userMessage, string queryType)
        {
            try
            {
                switch (queryType)
                {
                    case "Route Optimization Analysis":
                        return await ProcessRouteOptimizationQuery(userMessage);

                    case "Maintenance Prediction":
                        return await ProcessMaintenanceQuery(userMessage);

                    case "Safety Risk Assessment":
                        return await ProcessSafetyQuery(userMessage);

                    case "Student Assignment Optimization":
                        return await ProcessStudentOptimizationQuery(userMessage);

                    case "Fuel Efficiency Analysis":
                    case "Cost Optimization":
                    case "Fleet Performance Review":
                        return await ProcessGeneralTransportationQuery(userMessage, queryType);

                    default:
                        return await ProcessGeneralTransportationQuery(userMessage, "General Transportation");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI query processing");
                return "I encountered an issue processing your specific request. Could you please rephrase your question?";
            }
        }

        private async Task<string> ProcessRouteOptimizationQuery(string userMessage)
        {
            // Create a sample route analysis request based on user input
            var request = new XAIService.RouteAnalysisRequest
            {
                RouteId = 1,
                CurrentDistance = 25.5,
                StudentCount = 45,
                VehicleCapacity = 60,
                // Add mock data for demonstration
                HistoricalData = new XAIService.HistoricalPerformanceData
                {
                    AverageFuelConsumption = 8.5,
                    OnTimePerformance = 92.3,
                    SafetyIncidents = 1
                }
            };

            var analysis = await _xaiService.AnalyzeRouteOptimizationAsync(request);

            return $"📍 **Route Optimization Analysis**\n\n" +
                   $"💰 Estimated Fuel Savings: {analysis.OptimalRoute.EstimatedFuelSavings:F1}%\n" +
                   $"⏱️ Time Savings: {analysis.OptimalRoute.EstimatedTimeSavings:F1} minutes\n" +
                   $"🛡️ Safety Score: {analysis.OptimalRoute.SafetyScore:F1}/100\n" +
                   $"⚠️ Risk Level: {analysis.RiskAssessment.OverallRiskLevel}\n\n" +
                   $"📋 **Recommendations:**\n" +
                   string.Join("\n", analysis.OptimalRoute.RecommendedChanges.Take(3).Select(r => $"• {r}")) + "\n\n" +
                   $"🔍 **Risk Factors:**\n" +
                   string.Join("\n", analysis.RiskAssessment.IdentifiedRisks.Take(2).Select(r => $"• {r}")) + "\n\n" +
                   $"✅ Confidence: {analysis.ConfidenceLevel:P0}";
        }

        private async Task<string> ProcessMaintenanceQuery(string userMessage)
        {
            var request = new XAIService.MaintenanceAnalysisRequest
            {
                BusId = 1,
                VehicleMake = "Blue Bird",
                VehicleModel = "Vision",
                VehicleYear = 2020,
                CurrentMileage = 75000,
                LastMaintenanceDate = DateTime.Now.AddDays(-30),
                DailyMiles = 150,
                TerrainDifficulty = "Moderate"
            };

            var prediction = await _xaiService.PredictMaintenanceNeedsAsync(request);

            return $"🔧 **Maintenance Prediction Analysis**\n\n" +
                   $"📅 Next Service Date: {prediction.PredictedMaintenanceDate:MM/dd/yyyy}\n" +
                   $"💵 Estimated Cost: ${prediction.TotalEstimatedCost:N2}\n" +
                   $"💰 Potential Savings: ${prediction.PotentialSavings:N2}\n\n" +
                   $"🔩 **Component Predictions:**\n" +
                   string.Join("\n", prediction.ComponentPredictions.Take(3).Select(c =>
                       $"• {c.Component}: {c.PredictedWearDate:MM/dd} (${c.EstimatedCost:N0})")) + "\n\n" +
                   $"📝 {prediction.Reasoning.Substring(0, Math.Min(200, prediction.Reasoning.Length))}...";
        }

        private async Task<string> ProcessSafetyQuery(string userMessage)
        {
            var request = new XAIService.SafetyAnalysisRequest
            {
                RouteType = "Urban",
                TrafficDensity = "Moderate",
                RoadConditions = "Good",
                WeatherConditions = "Clear",
                TotalStudents = 45,
                PreviousIncidents = 0,
                NearMissReports = 2
            };

            var analysis = await _xaiService.AnalyzeSafetyRisksAsync(request);

            return $"🛡️ **Safety Risk Analysis**\n\n" +
                   $"📊 Overall Safety Score: {analysis.OverallSafetyScore:F1}/100\n" +
                   $"✅ Compliance Status: {analysis.ComplianceStatus}\n" +
                   $"🎯 Confidence Level: {analysis.ConfidenceLevel:P0}\n\n" +
                   $"⚠️ **Risk Factors:**\n" +
                   string.Join("\n", analysis.RiskFactors.Take(3).Select(r =>
                       $"• {r.Factor}: {r.RiskLevel} - {r.Impact}")) + "\n\n" +
                   $"💡 **Safety Recommendations:**\n" +
                   string.Join("\n", analysis.Recommendations.Take(3).Select(r => $"• {r}"));
        }

        private async Task<string> ProcessStudentOptimizationQuery(string userMessage)
        {
            var request = new XAIService.StudentOptimizationRequest
            {
                TotalStudents = 180,
                AvailableBuses = 4,
                GeographicConstraints = "Suburban area with dispersed stops",
                SpecialRequirements = "2 wheelchair accessible buses needed"
            };

            var optimization = await _xaiService.OptimizeStudentAssignmentsAsync(request);

            return $"🚌 **Student Assignment Optimization**\n\n" +
                   $"⏱️ Total Time Saved: {optimization.EfficiencyGains.TotalTimeSaved:F1} minutes\n" +
                   $"⛽ Fuel Savings: {optimization.EfficiencyGains.FuelSavings:F1}%\n" +
                   $"📊 Capacity Optimization: {optimization.EfficiencyGains.CapacityOptimization:P0}\n\n" +
                   $"🚌 **Optimal Assignments:**\n" +
                   string.Join("\n", optimization.OptimalAssignments.Take(3).Select(a =>
                       $"• Bus {a.BusId}: {a.StudentsAssigned} students ({a.CapacityUtilization:P0} capacity)")) + "\n\n" +
                   $"🎯 Confidence: {optimization.ConfidenceLevel:P0}\n" +
                   $"📝 {optimization.Reasoning.Substring(0, Math.Min(150, optimization.Reasoning.Length))}...";
        }

        private async Task<string> ProcessGeneralTransportationQuery(string userMessage, string queryType)
        {
            // For general queries, we can create a more flexible prompt
            await Task.Delay(1500); // Simulate AI processing

            var responses = new Dictionary<string, string[]>
            {
                ["Fuel Efficiency Analysis"] = new[]
                {
                    "🛢️ **Fuel Efficiency Insights**\n\nBased on current fleet data:\n• Average MPG: 8.2 (Industry avg: 7.5)\n• Top performer: Bus #12 at 9.8 MPG\n• Improvement opportunity: Route optimization could save 12-15%\n• Consider driver training on eco-driving techniques",
                    "⛽ **Fuel Cost Analysis**\n\nMonthly fuel analysis:\n• Current spend: $8,450/month\n• Potential savings: $1,200/month\n• Key recommendations: Route consolidation, regular maintenance, tire pressure monitoring\n• Weather impact: 8% higher consumption in winter"
                },
                ["Cost Optimization"] = new[]
                {
                    "💰 **Cost Optimization Opportunities**\n\n• Maintenance: Predictive scheduling could save 20%\n• Fuel: Route optimization saves $1,200/month\n• Labor: Efficient scheduling reduces overtime by 15%\n• Insurance: Safety improvements may reduce premiums",
                    "📊 **Budget Analysis**\n\nAnnual cost breakdown:\n• Fuel: 35% ($102,000)\n• Maintenance: 25% ($73,000)\n• Labor: 30% ($88,000)\n• Insurance: 10% ($29,000)\n\nTop savings opportunity: Preventive maintenance program"
                },
                ["Fleet Performance Review"] = new[]
                {
                    "📈 **Fleet Performance Summary**\n\n✅ On-time performance: 94.2%\n✅ Safety record: Excellent (0 incidents)\n⚠️ Maintenance backlog: 3 buses\n📊 Utilization rate: 87%\n💡 Recommendation: Focus on preventive maintenance scheduling",
                    "🚌 **Fleet Health Report**\n\nOverall grade: B+\n• Reliability: 96%\n• Efficiency: 89%\n• Safety: 98%\n• Cost management: 85%\n\nPriority: Implement AI-driven maintenance predictions"
                }
            };

            if (responses.ContainsKey(queryType))
            {
                var options = responses[queryType];
                return options[new Random().Next(options.Length)];
            }

            // Default general response
            return $"🤖 I understand you're asking about: \"{userMessage}\"\n\n" +
                   "I can help you with detailed analysis of:\n" +
                   "• Route planning and optimization\n" +
                   "• Maintenance scheduling and predictions\n" +
                   "• Safety assessments and improvements\n" +
                   "• Cost reduction strategies\n" +
                   "• Fleet performance analytics\n\n" +
                   "Please select a specific query type above for more detailed AI analysis, or ask me a specific question about your transportation operations.";
        }

        private void AddChatMessage(string sender, string message, Color senderColor)
        {
            chatMessages.Add(new ChatMessage { Sender = sender, Message = message, Timestamp = DateTime.Now });

            chatHistory.SelectionStart = chatHistory.TextLength;
            chatHistory.SelectionLength = 0;
            chatHistory.SelectionColor = senderColor;
            chatHistory.SelectionFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            chatHistory.AppendText($"{sender}: ");

            chatHistory.SelectionColor = Color.Black;
            chatHistory.SelectionFont = new Font("Segoe UI", 9F, FontStyle.Regular);
            chatHistory.AppendText($"{message}\n\n");

            chatHistory.SelectionStart = chatHistory.TextLength;
            chatHistory.ScrollToCaret();
        }

        private void RemoveLastMessage()
        {
            if (chatMessages.Count > 0)
            {
                chatMessages.RemoveAt(chatMessages.Count - 1);
                RefreshChatHistory();
            }
        }

        private void RefreshChatHistory()
        {
            chatHistory.Clear();
            foreach (var msg in chatMessages)
            {
                AddChatMessage(msg.Sender, msg.Message,
                    msg.Sender.Contains("You") ? Color.FromArgb(46, 125, 185) : Color.FromArgb(76, 175, 80));
            }
        }

        #endregion

        #region Public Methods for Dashboard Integration

        /// <summary>
        /// Updates the fleet summary with current data
        /// </summary>
        public void UpdateFleetSummary(int totalBuses, int totalRoutes, int activeBuses, int maintenanceBuses, int totalCapacity)
        {
            try
            {
                fleetTile.Title.Text = totalBuses.ToString();
                routesTile.Title.Text = totalRoutes.ToString();
                activeTile.Title.Text = activeBuses.ToString();
                maintenanceTile.Title.Text = maintenanceBuses.ToString();
                capacityTile.Title.Text = totalCapacity.ToString("N0");

                var utilizationRate = activeBuses > 0 ? (double)activeBuses / totalBuses * 100 : 0;
                var maintenanceRate = totalBuses > 0 ? (double)maintenanceBuses / totalBuses * 100 : 0;
                var avgCapacity = totalBuses > 0 ? (double)totalCapacity / totalBuses : 0;

                statsLabel.Text = $"Fleet Utilization: {utilizationRate:F1}%\n" +
                                 $"Maintenance Rate: {maintenanceRate:F1}%\n" +
                                 $"Average Capacity: {avgCapacity:F0} seats/bus";

                _logger.LogInformation("Fleet summary updated: {Buses} buses, {Routes} routes", totalBuses, totalRoutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating fleet summary");
            }
        }

        /// <summary>
        /// Switches to the AI Chat tab
        /// </summary>
        public void ShowAIChat()
        {
            mainTabControl.SelectedTab = chatTab;
        }

        /// <summary>
        /// Switches to the Fleet Summary tab
        /// </summary>
        public void ShowFleetSummary()
        {
            mainTabControl.SelectedTab = summaryTab;
        }

        #endregion

        #region Helper Classes

        private class ChatMessage
        {
            public string Sender { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
        }

        #endregion
    }
}
