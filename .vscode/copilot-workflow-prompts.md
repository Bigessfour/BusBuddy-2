# BusBuddy Production-Focused Workflow Prompts

⚡ **Focus:** Get features built and shipped fast. Less PowerShell, more production code.

Quick access prompts to keep development moving toward production goals with minimal workflow overhead.

---


## 🚀 Quick Dev Start
- **Prompt:**
  > Start coding immediately. Build, test, run—no PowerShell tweaking.

- **Command:**
  > `bb-daily` (builds, tests, runs app in one command)

---

## 🏗️ Build & Ship
- **Prompt:**
  > Build the solution, run tests, fix any warnings. Focus on production readiness.

- **Command:**
  > `bb-build && bb-test && bb-warning-analysis`

---

## 🔧 Quick Fix Cycle
- **Prompt:**
  > Fast iteration: build → test → fix → commit. Stay in the code, not the tooling.

- **Command:**
  > Build: `bb-build -Quick`
  > Test: `bb-test -Fast`
  > Commit: `git add . && git commit -m "fix: quick iteration" && git push`

---

## 📦 Ship to GitHub
- **Prompt:**
  > Commit changes and push to production. Let GitHub Actions handle the validation.

- **Command:**
  > ```powershell
  git add .
  git commit -m "feat: production ready changes"
  git push
  # GitHub Actions will test automatically
  ```

## �️ Production Development Focus

**Use these prompts to:**
1. **🎯 Stay in production code** - Less tooling, more features
2. **⚡ Move fast** - Quick build-test-ship cycles
3. **🚀 Ship often** - Let GitHub Actions handle validation
4. **� Fix and iterate** - Stay in the development flow

**Simple workflow:** `bb-daily` → code → `bb-build && bb-test` → `git push` → repeat

---
