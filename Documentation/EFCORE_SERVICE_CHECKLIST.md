# BusBuddy EF Core Service/Repository Checklist

Use this checklist for every PR, refactor, or new service/repository to ensure safe, scalable, and testable Entity Framework Core usage.

---

## 1. Constructor & Dependency Injection
- [ ] Service/repository injects `IBusBuddyDbContextFactory` (not `BusBuddyDbContext`).
- [ ] Service/repository injects `ILogger<T>` for logging.

## 2. DbContext Usage
- [ ] No class-level or field-level `BusBuddyDbContext` stored.
- [ ] Every method that accesses the database creates a new context:
  - [ ] For reads: `using var context = _contextFactory.CreateDbContext();`
  - [ ] For writes: `using var context = _contextFactory.CreateWriteDbContext();`


## 3. Method Implementation (Standard Template)
```csharp
public async Task<ReturnType> MethodNameAsync(ParamType param)
{
    try
    {
        _logger.LogInformation("Describe operation");
        using var context = _contextFactory.CreateDbContext(); // or CreateWriteDbContext() for writes
        // ...data access logic...
        return ...;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error message with context");
        throw;
    }
}
```
- [ ] All async methods use their own context instance.
- [ ] No context is shared between methods or across async calls.
- [ ] All data access is wrapped in try/catch with logging.

## 4. Validation & Business Logic
- [ ] Validation methods use a new context instance for any DB checks.
- [ ] No context is passed between methods.

## 5. Dependency Registration
- [ ] `IBusBuddyDbContextFactory` is registered as a singleton in DI.
- [ ] All services/repositories are registered as scoped or singleton as appropriate.

## 6. Testing & Mocking
- [ ] Service/repository can be unit tested by mocking `IBusBuddyDbContextFactory`.

## 7. Code Review
- [ ] Reviewer confirms all above points for every service/repository.
- [ ] Reviewer checks for accidental context sharing or field storage.

---

**Tip:**
Copy this checklist into your PR description or documentation so every team member can use it for code reviews and new code!
