# Production Readiness Checklist (Layered Steps)

## 1. Build & Test Foundation
- [ ] Scripted build/test runs clean (no errors, all tests pass)
- [ ] All warnings reviewed and either fixed or consciously accepted
- [ ] All code is in version control (Git), with no uncommitted changes

## 2. UI/UX Consistency
- [ ] All views use Syncfusion controls where appropriate (e.g., SfDataGrid, SfChart)
- [ ] XAML bindings match ViewModel properties/commands (no binding errors in Output window)
- [ ] No business logic in code-behind (only UI-specific code allowed)
- [ ] Consistent naming for Views, ViewModels, and controls

## 3. Data & MVVM Best Practices
- [ ] All data exposed via ObservableCollection or properties in ViewModel
- [ ] All actions use ICommand/RelayCommand (no direct event handlers in XAML)
- [ ] Async/await used for all data operations (no blocking UI)
- [ ] Error handling/logging in ViewModel (user-friendly messages in UI)

## 4. Visual & Theming
- [ ] Resource dictionaries used for styles/themes
- [ ] Consistent look and feel across all views
- [ ] Syncfusion theme applied and working

## 5. Testing & Validation
- [ ] Manual test of all critical features (add, edit, delete, navigation, etc.)
- [ ] Automated tests cover all business logic and ViewModel code
- [ ] Test results reviewed after every change

## 6. Configuration & Secrets
- [ ] Connection strings and secrets are not hardcoded (use appsettings.json or environment variables)
- [ ] Production appsettings.json reviewed and correct

## 7. Logging & Diagnostics
- [ ] Global exception handler in App.xaml.cs logs all unhandled errors
- [ ] Log files are easy to find and review

## 8. Deployment Prep
- [ ] Build output tested on a clean machine (no “it works on my box” surprises)
- [ ] All dependencies (DLLs, themes, etc.) included in output
- [ ] Version number updated

---

**How to Use:**
- Work through one section at a time.
- Don’t move on until everything in the current section is checked off and working.
- Ask for help on any step you don’t understand.
