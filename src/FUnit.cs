// Licensed under the MIT License
// https://github.com/sator-imaging/FUnit

using FUnitImpl;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Descriptor = System.Action<string, System.Action<System.Action<string, System.Delegate>>>;

#pragma warning disable CA1050 // Declare types in namespaces
/// <summary>
/// Provides the main entry point for running FUnit tests.
/// </summary>
public static partial class FUnit
#pragma warning restore CA1050
{
    private const int InitialSubjectMapCapacity = 8;
    private const int InitialTestCaseListCapacity = 4;
    private const string TimeSpanFormat = @"hh\:mm\:ss\.fff";

    /// <summary>
    /// Last execution result.
    /// </summary>
    public static TestResult? Result { get; private set; }

    /// <summary>
    /// Builds a new <see cref="TestSuite"/> instance using the provided builder action.
    /// </summary>
    /// <param name="builder">An action that describes the test subjects and test cases.</param>
    /// <returns>A new <see cref="TestSuite"/> instance.</returns>
    public static TestSuite Build(Action<Descriptor> builder) => new TestSuite(builder);

    /// <summary>
    /// Executes the FUnit test runner.
    /// </summary>
    /// <param name="args">Command line arguments for the test run.</param>
    /// <param name="builder">An action that describes the test subjects and test cases.</param>
    /// <returns>The number of failed test cases.</returns>
    public static int Run(string[] args, Action<Descriptor> builder)
    {
        return Task.Run(async () => await RunAsync(args, builder)).Result;  // ok
    }

    /// <summary>
    /// Executes the FUnit test runner asynchronously.
    /// </summary>
    /// <param name="args">Command line arguments for the test run.</param>
    /// <param name="builder">An action that describes the test subjects and test cases.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of failed test cases if not canceled, otherwise a negative number that is the bitwise complement of the failed test case count.</returns>
    public static async Task<int> RunAsync(string[] args, Action<Descriptor> builder, CancellationToken cancellationToken = default)
    {
        // always clear!!
        Result = null;

        var options = CommandLineOptions.Parse(args);
        var testSuite = Build(builder);

        testSuite.OnTestStarting += () =>
        {
            ConsoleLogger.LogInfo();
            ConsoleLogger.LogInfo("## Test Result");
        };

        testSuite.OnTestSubjectStarting += subject => ConsoleLogger.LogInfo($"- {subject}");
        testSuite.OnTestCasePassed += (args) =>
        {
            ConsoleLogger.LogPassed($"  {SR.MarkdownPassed} {args.description}");
        };
        testSuite.OnTestCaseFailed += (args) =>
        {
            var annotation = args.errors.Count() == args.maxIterations
                ? string.Empty
                : $" {SR.FlakyTestResultAnnotation}"
                ;
            var error = args.errors.First();
            var result = error is FUnitException ? "Failed" : "Error";
            ConsoleLogger.LogFailed($"  {SR.MarkdownFailed} {args.description}{annotation} - [{result}] {error.Message}");
        };

        testSuite.OnCanceledExecutionReportStarting += () => ConsoleLogger.LogFailed($"- [{nameof(FUnit)}] Tests Canceled");
        testSuite.OnCanceledExecutionReportTestCase += (args) =>
        {
            ConsoleLogger.LogFailed($"  {SR.MarkdownFailed} [{args.subject}] {args.description}");
        };

        Result = await testSuite.ExecuteCoreAsync(options, cancellationToken);

        // finish up
        var failedTestCases = Result.TestsBySubject
            .SelectMany(x => x.Value.Select(test => (subject: x.Key, test)))
            .Where(x => x.test.Errors?.Count is > 0)
            .ToList();
        var skippedTestCases = Result.TestsBySubject
            .SelectMany(x => x.Value)
            .Where(x => x.ExecutionCount == 0)
            .ToList();

        // NOTE: remove system errors from result
        var totalTestCaseCount = Result.TestsBySubject.Sum(x => x.Value.Count(y => y.Errors?.All(error => error.IsFUnitSystemError) != true));
        var failedTestCaseCountWithoutSystemErrors = failedTestCases.Count(x => x.test.Errors?.Count(error => !error.IsFUnitSystemError) is > 0);

        ConsoleLogger.LogInfo();
        ConsoleLogger.LogInfo("## Test Summary");
        ConsoleLogger.LogInfo($"Options:  {Result.Options}  ");
        ConsoleLogger.LogInfo($"Duration: {Result.TotalExecutionTime.ToString(TimeSpanFormat)}  ");
        if (skippedTestCases.Count > 0)
        {
            ConsoleLogger.LogFailed($"{SR.MarkdownFailed} Total {skippedTestCases.Count} tests canceled");

            return ~(failedTestCases.Count);  // ok: bash treats -1 as 255 (byte)
        }
        else
        {
            if (failedTestCases.Count == 0)
            {
                ConsoleLogger.LogPassed($"{SR.MarkdownPassed} All Tests Passed: {totalTestCaseCount}");
            }
            else
            {
                var failedCount = failedTestCases.Count(x => x.test.Errors?.Any(y => y.IsAssertionFailure) ?? false);
                var erroredCount = failedTestCases.Count - failedCount;
                ConsoleLogger.LogPassed($"Passed: {totalTestCaseCount - failedTestCaseCountWithoutSystemErrors} ({totalTestCaseCount})  ");
                ConsoleLogger.LogFailed($"Failed: {failedCount}  ");
                if (erroredCount > 0)
                {
                    ConsoleLogger.LogFailed($"Error: {erroredCount}  ");
                }

                foreach (var (subject, test) in failedTestCases)
                {
                    if (test.Errors is not null)
                    {
                        var annotation = test.ExecutionCount == test.Errors.Count
                            ? string.Empty
                            : $" {SR.FlakyTestResultAnnotation}"
                            ;

                        var error = test.Errors[0];
                        var result = error.IsAssertionFailure ? "Failed" : "Error";
                        ConsoleLogger.LogFailed($"{SR.MarkdownFailed} [{subject}] {test.Description}{annotation} - [{result}] {error.Message}");

                        if (options.ShowStackTrace && error.StackTrace is not null)
                        {
                            foreach (var st in error.StackTrace.Split("\n"))
                            {
                                if (string.IsNullOrWhiteSpace(st))
                                {
                                    continue;
                                }

                                ConsoleLogger.LogInfo($"{new string(' ', SR.MarkdownFailed.Length)} {st.TrimEnd()}");
                            }
                        }
                    }
                }
            }

            return failedTestCases.Count;
        }
    }
}
