using NUnit.Framework;

// Enable parallel execution for all tests in this assembly
[assembly: Parallelizable(ParallelScope.Children)]

// Set the degree of parallelism (use a reasonable number of workers)
[assembly: LevelOfParallelism(4)]

// Set execution timeout for the entire assembly
[assembly: Timeout(60000)] // 60 seconds max per test
