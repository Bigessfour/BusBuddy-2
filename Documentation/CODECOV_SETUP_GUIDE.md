# Codecov Setup Guide for Bus Buddy

This guide walks through setting up Codecov for the Bus Buddy Syncfusion project.

## Prerequisites

- GitHub repository: `Bigessfour/BusBuddy_Syncfusion`
- Admin access to the repository
- .NET 8 project with test coverage collection

## Step 1: Create Codecov Account

1. Go to [codecov.io](https://codecov.io)
2. Sign up with your GitHub account
3. Grant necessary permissions to access your repositories

## Step 2: Add Repository to Codecov

1. Once logged in, click "Add new repository"
2. Select your GitHub organization: `Bigessfour`
3. Find and add `BusBuddy_Syncfusion` repository
4. Copy the repository upload token from the settings page

## Step 3: Configure GitHub Secrets

1. Go to your GitHub repository settings
2. Navigate to **Settings** > **Secrets and variables** > **Actions**
3. Add the following secrets:

   - **Name**: `CODECOV_TOKEN`
   - **Value**: [Your Codecov upload token from Step 2]

   - **Name**: `SYNCFUSION_LICENSE_KEY` (if not already added)
   - **Value**: `Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhec3RSRGRYU0R2WUBWYEk=`

## Step 4: Verify CI/CD Configuration

The CI/CD pipeline is already configured with Codecov integration. The workflow includes:

```yaml
- name: Upload coverage reports
  uses: codecov/codecov-action@v5
  if: always()
  with:
    files: TestResults/*/coverage.cobertura.xml
    token: ${{ secrets.CODECOV_TOKEN }}
    fail_ci_if_error: false
    verbose: true
    flags: unittests
    name: codecov-umbrella
```

## Step 5: Test Coverage Collection

The project is configured to collect coverage using:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

This generates coverage reports in Cobertura XML format, which Codecov can parse.

## Step 6: Codecov Configuration

The `codecov.yml` file in the repository root configures:

- **Coverage Targets**: 75% for project and patches
- **Ignored Paths**: Test files, migrations, generated code
- **Flags**: Separate tracking for unit tests and integration tests
- **Status Checks**: PR and commit status reporting

## Step 7: Verify Setup

1. Push a commit or create a pull request
2. Check GitHub Actions to ensure the workflow runs successfully
3. Verify that coverage is uploaded to Codecov
4. Check your Codecov dashboard at: `https://codecov.io/gh/Bigessfour/BusBuddy_Syncfusion`

## Step 8: Optional Enhancements

### GitHub Status Checks
1. In GitHub repository settings, go to **Settings** > **Branches**
2. Add branch protection rules for `main`/`master`
3. Enable "Require status checks to pass"
4. Add Codecov status checks

### Browser Extension
Install the [Codecov browser extension](https://github.com/codecov/browser-extension) for GitHub to see coverage overlays on pull requests.

### Slack Integration (Optional)
1. Add `SLACK_WEBHOOK_URL` secret to GitHub
2. Codecov will send coverage notifications to your Slack channel

## Troubleshooting

### Coverage Reports Not Uploading
- Check that `CODECOV_TOKEN` is correctly set in GitHub Secrets
- Verify the coverage file path in the workflow: `TestResults/*/coverage.cobertura.xml`
- Ensure tests are generating coverage reports

### Syncfusion Test Freezing
- The project includes freeze mitigation strategies in the test suite
- See `SYNCFUSION_TEST_FREEZE_MITIGATIONS.md` for details
- Tests use STA threading and proper resource disposal

### Low Coverage Warnings
- Adjust coverage targets in `codecov.yml` if needed
- Review ignored paths to ensure they're appropriate
- Consider adding more unit tests for core business logic

## Coverage Dashboard Features

Once set up, your Codecov dashboard will show:

- **Overall coverage percentage**
- **Coverage trends over time** 
- **File-by-file coverage breakdown**
- **Sunburst visualization** of coverage by directory
- **Pull request coverage impact**
- **Coverage comments on PRs**

## Repository Badges

The README includes badges for:
- CI/CD status: ![CI/CD Pipeline](https://github.com/Bigessfour/BusBuddy_Syncfusion/actions/workflows/ci-cd.yml/badge.svg)
- Code coverage: ![codecov](https://codecov.io/gh/Bigessfour/BusBuddy_Syncfusion/branch/master/graph/badge.svg)

These automatically update based on your latest commits and test runs.

## Best Practices

1. **Set Realistic Targets**: Start with achievable coverage targets (e.g., 75%) and increase over time
2. **Focus on Business Logic**: Prioritize coverage for core services and utilities over UI components
3. **Review Coverage Reports**: Regularly check which areas need more test coverage
4. **Use Status Checks**: Enable Codecov status checks to prevent coverage regressions
5. **Monitor Trends**: Watch coverage trends to ensure quality doesn't decline over time

## Support

- **Codecov Documentation**: [docs.codecov.com](https://docs.codecov.com)
- **GitHub Actions Integration**: [codecov/codecov-action](https://github.com/codecov/codecov-action)
- **Syncfusion Testing Guide**: See project's `SYNCFUSION_TEST_FREEZE_MITIGATIONS.md`
