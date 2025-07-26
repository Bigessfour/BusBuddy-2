#Requires -Version 7.5

<#
.SYNOPSIS
    BusBuddy AI Integration Functions
.DESCRIPTION
    Implements AI-powered features for BusBuddy including chat, code review, routing optimization, and task management
.NOTES
    Requires powershai module for AI integration
    Uses xAI Grok-4 for enhanced transportation domain expertise
#>

# Import required modules
if (-not (Get-Module powershai)) {
    try {
        Import-Module powershai -Force
        Write-Host "✅ powershai module loaded" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ Failed to load powershai module. Install with: Install-Module powershai -Force" -ForegroundColor Red
        return
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# 🤖 BUSBUDDY AI CHAT FUNCTION
# ═══════════════════════════════════════════════════════════════════════════════

function Invoke-BusBuddyAIChat {
    <#
    .SYNOPSIS
        Interactive AI chat for BusBuddy development and transportation planning
    .DESCRIPTION
        Provides AI-powered assistance for:
        - Sports scheduling optimization
        - Route safety analysis
        - Code review and suggestions
        - NHTSA compliance guidance
    .PARAMETER Message
        The message to send to the AI
    .PARAMETER Context
        Additional context about the current BusBuddy session
    .EXAMPLE
        Invoke-BusBuddyAIChat -Message "Help me optimize routes for football team transportation"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [string]$Message,

        [Parameter(Mandatory = $false)]
        [string]$Context = "BusBuddy Transportation Management System"
    )

    try {
        Write-Host "🚌 BusBuddy AI Assistant" -ForegroundColor Cyan
        Write-Host "=========================" -ForegroundColor Cyan

        # Initialize AI chat with BusBuddy context
        $systemPrompt = @"
You are a specialized AI assistant for BusBuddy, a school transportation management system.
Your expertise includes:

🚌 Transportation Management:
- School bus routing and scheduling
- Driver assignment optimization
- Vehicle maintenance tracking
- Sports event transportation

🏈 Sports Schedule Phase 2:
- Creating efficient transportation schedules for sports teams
- Implementing NHTSA safety guidelines
- Route optimization for various sports venues
- Emergency contact management
- Weather-aware route planning

🔧 Technical Implementation:
- C# .NET 8.0 development
- Entity Framework Core
- WPF with Syncfusion controls
- PowerShell 7.5.2 automation
- MVVM architecture patterns

🛡️ Safety Compliance:
- NHTSA Safe System approach
- Student safety protocols (5-min early arrival, 10ft spacing)
- Blind spot awareness
- Weather condition monitoring
- Emergency response procedures

Always provide practical, safety-focused, and technically sound advice for school transportation scenarios.
"@

        if (-not $Message) {
            Write-Host "💭 What can I help you with today?" -ForegroundColor Yellow
            Write-Host "Examples:" -ForegroundColor Gray
            Write-Host "  • 'Help me implement sports event scheduling'" -ForegroundColor Gray
            Write-Host "  • 'Review my Entity Framework model for safety compliance'" -ForegroundColor Gray
            Write-Host "  • 'Optimize routes for away football games'" -ForegroundColor Gray
            Write-Host "  • 'Create PowerShell functions for schedule automation'" -ForegroundColor Gray
            Write-Host ""
            $Message = Read-Host "Your question"
        }

        if ([string]::IsNullOrWhiteSpace($Message)) {
            Write-Host "❌ No message provided" -ForegroundColor Red
            return
        }

        # Add BusBuddy context to the message
        $enhancedMessage = @"
Context: $Context

Current working directory: $(Get-Location)
Current date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')

User Question: $Message
"@

        Write-Host "🤖 Processing your request..." -ForegroundColor Yellow

        # Use powershai to send the message
        $response = Send-PowershaiChat -message $enhancedMessage -SystemMessagesParam $systemPrompt

        Write-Host ""
        Write-Host "🚌 BusBuddy AI Response:" -ForegroundColor Green
        Write-Host "========================" -ForegroundColor Green
        Write-Host $response -ForegroundColor White

        return $response
    }
    catch {
        Write-Host "❌ Error in AI chat: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "💡 Make sure you have AI credentials configured" -ForegroundColor Yellow
        Write-Host "   Run: Enter-AiCredential to set up API keys" -ForegroundColor Gray
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# 📋 BUSBUDDY AI TASK FUNCTION
# ═══════════════════════════════════════════════════════════════════════════════

function Invoke-BusBuddyAITask {
    <#
    .SYNOPSIS
        AI-powered task planning and execution for BusBuddy development
    .DESCRIPTION
        Analyzes development tasks and provides step-by-step implementation guidance
    .PARAMETER Task
        The development task to analyze and plan
    .PARAMETER Priority
        Priority level (High, Medium, Low)
    .EXAMPLE
        Invoke-BusBuddyAITask -Task "Implement sports event scheduling with safety compliance" -Priority High
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Task,

        [Parameter(Mandatory = $false)]
        [ValidateSet("High", "Medium", "Low")]
        [string]$Priority = "Medium"
    )

    try {
        Write-Host "📋 BusBuddy AI Task Planner" -ForegroundColor Cyan
        Write-Host "===========================" -ForegroundColor Cyan

        $taskPrompt = @"
Please analyze this BusBuddy development task and provide a detailed implementation plan:

Task: $Task
Priority: $Priority

Provide:
1. 🎯 Objective Analysis
2. 📋 Step-by-Step Implementation Plan
3. 🔧 Required Technologies/Components
4. ⚠️ Safety Considerations (NHTSA compliance)
5. 📊 Testing Strategy
6. 🚀 Deployment Notes

Focus on practical, actionable steps that integrate with the existing BusBuddy Phase 1 infrastructure.
"@

        Write-Host "🤖 Analyzing task: $Task" -ForegroundColor Yellow

        $response = Invoke-BusBuddyAIChat -Message $taskPrompt -Context "BusBuddy Task Planning"

        return $response
    }
    catch {
        Write-Host "❌ Error in AI task planning: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# 🛣️ BUSBUDDY AI ROUTE FUNCTION
# ═══════════════════════════════════════════════════════════════════════════════

function Invoke-BusBuddyAIRoute {
    <#
    .SYNOPSIS
        AI-powered route optimization and safety analysis
    .DESCRIPTION
        Analyzes transportation routes for safety, efficiency, and NHTSA compliance
    .PARAMETER Origin
        Starting location
    .PARAMETER Destination
        Destination location
    .PARAMETER TeamSize
        Number of team members
    .PARAMETER Weather
        Current weather conditions
    .EXAMPLE
        Invoke-BusBuddyAIRoute -Origin "Central High School" -Destination "Memorial Stadium" -TeamSize 25 -Weather "Clear"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Origin,

        [Parameter(Mandatory = $true)]
        [string]$Destination,

        [Parameter(Mandatory = $false)]
        [int]$TeamSize = 15,

        [Parameter(Mandatory = $false)]
        [string]$Weather = "Unknown"
    )

    try {
        Write-Host "🛣️ BusBuddy AI Route Optimizer" -ForegroundColor Cyan
        Write-Host "==============================" -ForegroundColor Cyan

        $routePrompt = @"
Analyze this transportation route for a school sports team:

🏫 Origin: $Origin
🏟️ Destination: $Destination
👥 Team Size: $TeamSize members
🌤️ Weather: $Weather

Please provide:
1. 🛣️ Route Safety Analysis
2. ⏰ Recommended Departure Time (5-10 min early arrival)
3. 🚌 Vehicle Requirements (capacity, type)
4. ⚠️ Safety Checkpoints and Protocols
5. 🌧️ Weather-Specific Considerations
6. 📱 Emergency Contact Recommendations
7. 👀 Blind Spot Awareness Points
8. 🚶‍♂️ Student Loading/Unloading Procedures

Focus on NHTSA Safe System approach and practical safety measures.
"@

        Write-Host "🤖 Analyzing route: $Origin → $Destination" -ForegroundColor Yellow

        $response = Invoke-BusBuddyAIChat -Message $routePrompt -Context "BusBuddy Route Planning"

        return $response
    }
    catch {
        Write-Host "❌ Error in AI route analysis: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# 📝 BUSBUDDY AI CODE REVIEW FUNCTION
# ═══════════════════════════════════════════════════════════════════════════════

function Invoke-BusBuddyAIReview {
    <#
    .SYNOPSIS
        AI-powered code review for BusBuddy development
    .DESCRIPTION
        Reviews code for best practices, safety compliance, and transportation domain expertise
    .PARAMETER FilePath
        Path to the file to review
    .PARAMETER ReviewType
        Type of review (Security, Performance, Safety, Architecture)
    .EXAMPLE
        Invoke-BusBuddyAIReview -FilePath "BusBuddy.Core\Models\SportsEvent.cs" -ReviewType Safety
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [string]$FilePath,

        [Parameter(Mandatory = $false)]
        [ValidateSet("Security", "Performance", "Safety", "Architecture", "General")]
        [string]$ReviewType = "General"
    )

    try {
        Write-Host "📝 BusBuddy AI Code Reviewer" -ForegroundColor Cyan
        Write-Host "============================" -ForegroundColor Cyan

        if ($FilePath -and (Test-Path $FilePath)) {
            $fileContent = Get-Content $FilePath -Raw
            $fileName = Split-Path $FilePath -Leaf

            $reviewPrompt = @"
Please review this BusBuddy code file for $ReviewType considerations:

📄 File: $fileName
🔍 Review Type: $ReviewType

```
$fileContent
```

Please analyze for:
1. 🔒 Code Quality and Best Practices
2. 🏗️ Architecture Patterns (MVVM, Entity Framework)
3. 🛡️ Safety and Transportation Domain Compliance
4. ⚡ Performance Considerations
5. 🧪 Testability
6. 📚 Documentation Quality
7. 🚨 Potential Issues or Improvements

Focus on transportation management context and school safety requirements.
"@
        }
        else {
            # Generic code review guidance
            $reviewPrompt = @"
Provide general code review guidance for BusBuddy development focusing on $($ReviewType):

Please cover:
1. 🏗️ Architecture Best Practices for Transportation Systems
2. 🔒 Security Considerations for School Management Software
3. 🛡️ Safety Compliance (NHTSA guidelines integration)
4. ⚡ Performance Optimization for Real-Time Scheduling
5. 🧪 Testing Strategies for Mission-Critical Systems
6. 📝 Code Documentation Standards
7. 🚀 Deployment and Maintenance Considerations

Provide specific examples relevant to school transportation management.
"@
        }

        Write-Host "🤖 Reviewing code for $ReviewType..." -ForegroundColor Yellow

        $response = Invoke-BusBuddyAIChat -Message $reviewPrompt -Context "BusBuddy Code Review"

        return $response
    }
    catch {
        Write-Host "❌ Error in AI code review: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# ⚙️ BUSBUDDY AI CONFIGURATION FUNCTION
# ═══════════════════════════════════════════════════════════════════════════════

function Invoke-BusBuddyAIConfig {
    <#
    .SYNOPSIS
        Configure AI settings for BusBuddy
    .DESCRIPTION
        Sets up and manages AI integration configuration
    .PARAMETER ShowStatus
        Display current AI configuration status
    .PARAMETER SetupCredentials
        Interactive credential setup
    .EXAMPLE
        Invoke-BusBuddyAIConfig -ShowStatus
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [switch]$ShowStatus,

        [Parameter(Mandatory = $false)]
        [switch]$SetupCredentials
    )

    try {
        Write-Host "⚙️ BusBuddy AI Configuration" -ForegroundColor Cyan
        Write-Host "============================" -ForegroundColor Cyan

        if ($ShowStatus) {
            Write-Host "📊 Current AI Configuration Status:" -ForegroundColor Yellow

            # Check available providers
            $providers = Get-AiProviders
            Write-Host "🤖 Available AI Providers:" -ForegroundColor Green
            $providers | ForEach-Object { Write-Host "   • $_" -ForegroundColor Gray }

            # Check current provider
            $currentProvider = Get-AiCurrentProvider
            Write-Host "🎯 Current Provider: $currentProvider" -ForegroundColor Green

            # Check credentials
            $credentials = Get-AiCredentials
            Write-Host "🔐 Configured Credentials:" -ForegroundColor Green
            $credentials | ForEach-Object { Write-Host "   • $_" -ForegroundColor Gray }

            # Check models
            try {
                $models = Get-AiModels
                Write-Host "🧠 Available Models:" -ForegroundColor Green
                $models | Select-Object -First 5 | ForEach-Object { Write-Host "   • $_" -ForegroundColor Gray }
            }
            catch {
                Write-Host "   ⚠️ No models available (check credentials)" -ForegroundColor Yellow
            }
        }

        if ($SetupCredentials) {
            Write-Host "🔐 Interactive AI Credential Setup:" -ForegroundColor Yellow
            Write-Host "1. OpenAI API Key"
            Write-Host "2. xAI API Key (Recommended for BusBuddy)"
            Write-Host "3. Anthropic API Key"

            $choice = Read-Host "Select provider (1-3)"

            switch ($choice) {
                "1" { Enter-AiCredential -Provider "openai" }
                "2" { Enter-AiCredential -Provider "xai" }
                "3" { Enter-AiCredential -Provider "anthropic" }
                default { Write-Host "❌ Invalid choice" -ForegroundColor Red }
            }
        }

        if (-not $ShowStatus -and -not $SetupCredentials) {
            Write-Host "📋 Available Configuration Options:" -ForegroundColor Yellow
            Write-Host "   Invoke-BusBuddyAIConfig -ShowStatus      # Show current config"
            Write-Host "   Invoke-BusBuddyAIConfig -SetupCredentials # Setup API keys"
            Write-Host ""
            Write-Host "💡 Recommended: Use xAI Grok-4 for transportation domain expertise"
        }
    }
    catch {
        Write-Host "❌ Error in AI configuration: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# ✅ FUNCTIONS LOADED
# ═══════════════════════════════════════════════════════════════════════════════

# Functions are now available for use
# Note: Export-ModuleMember only works in .psm1 module files, not .ps1 script files

Write-Host "✅ BusBuddy AI Functions loaded successfully!" -ForegroundColor Green
Write-Host "Available commands:" -ForegroundColor Yellow
Write-Host "  • bb-ai-chat     → Interactive AI assistant" -ForegroundColor Gray
Write-Host "  • bb-ai-task     → AI task planning" -ForegroundColor Gray
Write-Host "  • bb-ai-route    → AI route optimization" -ForegroundColor Gray
Write-Host "  • bb-ai-review   → AI code review" -ForegroundColor Gray
Write-Host "  • bb-ai-config   → AI configuration" -ForegroundColor Gray
