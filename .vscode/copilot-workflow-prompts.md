# ⚡ Managing Multiple/Background Tasks
- **Prompt:**
  > Start multiple tasks, run long-running jobs in the background, and capture their output to log files. Monitor and retrieve output as needed while continuing with other work.

- **Copilot Command:**
  > Start a background job:
  > ```powershell
  $job = Start-Job { bb-run -EnableDebug }
  ```
  > Capture output to a log file:
  > ```powershell
  bb-run -EnableDebug *> logs/bb-run-$(Get-Date -Format 'yyyyMMdd-HHmmss').log
  ```
  > Monitor and retrieve output:
  > ```powershell
  Receive-Job -Job $job -Keep
  Get-Content -Path logs/bb-run-*.log -Wait
  ```

# BusBuddy Copilot Workflow Prompts

This file contains selectable workflow prompts for GitHub Copilot and developers to streamline and standardize development, troubleshooting, and automation in the BusBuddy project. Use these prompts to quickly instruct Copilot or team members to perform common, production-compliant tasks.

---


## 🚀 Development Session
- **Prompt:**
  > Start a new development session, open the IDE (do not launch a new or duplicate IDE—use your existing VS Code window), and prepare the environment using all production-compliant PowerShell workflows. **Always use the persistent (existing) terminal session for all commands to maintain workflow state.**

- **Copilot Command:**
  > In the persistent terminal (in your current VS Code window), run:
  > ```powershell
  bb-dev-session -OpenIDE
  ```

---

## 🏗️ Build & Restore
- **Prompt:**
  > Clean and build the solution with full package restore and detailed output. If errors occur, run a health check and attempt a restore.

- **Copilot Command:**
  > Run: `bb-build -Clean -Restore -Verbosity detailed`
  > If failed: `bb-health -Quick` then `bb-restore`

---

## 🧪 Testing & Coverage
- **Prompt:**
  > Run the full test suite with coverage analysis and validate PowerShell 7.5 compatibility.

- **Copilot Command:**
  > Run: `bb-test -Coverage`
  > Then: `Test-PowerShell75Features -ShowBenchmarks`

---

## 🩺 Health & Diagnostics
- **Prompt:**
  > Perform a comprehensive health check and advanced diagnostics on the project and environment.

- **Copilot Command:**
  > Run: `bb-health -Detailed`
  > Then: `bb-info`

---

## 📊 Warning & Dependency Analysis
- **Prompt:**
  > Analyze warnings with a focus on null safety and scan all dependencies for vulnerabilities.

- **Copilot Command:**
  > Run: `bb-warning-analysis -FocusNullSafety`
  > Then: `bb-manage-dependencies -ScanVulnerabilities`

---

## 📝 Commit & Push
- **Prompt:**
  > Stage all changes, generate a smart commit message, push to GitHub, and monitor the workflow.

- **Copilot Command:**
  > Run: `bb-stage-all` (or use the smart stage task)
  > Then: `bb-commit -Smart -GenerateMessage`
  > Then: `bb-push`
  > Then: `bb-get-workflow-results -Count 1`

---


## 📋 Custom Prompt
- **Prompt:**
  > [Describe your custom workflow or troubleshooting scenario here.]

- **Copilot Command:**
  > [Insert the corresponding bb-tool or PowerShell command.]

---


---

## 🛠️ How to Use This Prompt File

1. **Browse Prompts:** Open `.vscode/copilot-workflow-prompts.md` in VS Code.
2. **Select a Prompt:** Find the workflow or troubleshooting scenario you want to run.
3. **Copy the Prompt:** Copy the "Prompt" or "Copilot Command" section.
4. **Paste to Copilot or Terminal:**
    - For Copilot: Paste the prompt into the chat to instruct Copilot to perform the workflow.
    - For Terminal: Paste the Copilot Command directly into your PowerShell terminal.
5. **Monitor Output:** For background/long-running tasks, check the logs directory or use job management commands as described above.
6. **Add New Prompts:** As new workflows emerge, add them to this file for team-wide reuse.

---

---

**Location:** Place this file at `.vscode/copilot-workflow-prompts.md` for easy access in VS Code.
