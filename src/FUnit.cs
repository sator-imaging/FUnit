using FUnitImpl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    /// Executes the FUnit test runner.
    /// </summary>
    /// <param name="args">Command line arguments for the test run.</param>
    /// <param name="builder">An action that describes the test subjects and test cases.</param>
    /// <returns>The number of failed test cases.</returns>
    public static int Run(string[] args, Action<Descriptor> builder)
    {
        return Task.Run(async () => await RunAsync(args, builder)).Result;  // ok
    }

    // TODO: refactor Run method by creating new methods and types.
    //       --> Build(Descriptor) -> TestSuite
    //       --> TestSuite (class)
    //           + ExecuteTests(CommandLineOptions, out TestResult) -> int numberOfFailedTestCases
    //           + event OnSubjectStarting, OnTestCaseStarting and etc
    //             - for console logging
    //       --> Revised Run method
    //           - call CommandLineOptions.Parse
    //           - call Build to get testSuite
    //           - setup testSuite events
    //           - call testSuite.ExecuteTests
    //           - set static Result property and return failed test case count from ExecuteTests
    // TODO: add command line option '--times' to specify the number of times to run tests.
    //       --> verify test subject's (hidden) global state is correctly handled or not.
    //       --> 3 by default

    /// <summary>
    /// Executes the FUnit test runner asynchronously.
    /// </summary>
    /// <param name="args">Command line arguments for the test run.</param>
    /// <param name="builder">An action that describes the test subjects and test cases.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of failed test cases if not canceled, otherwise a negative number that is the bitwise complement of the failed test case count.</returns>
    public static async ValueTask<int> RunAsync(string[] args, Action<Descriptor> builder, CancellationToken cancellationToken = default)
    {
        // always clear!!
        Result = null;

        var options = CommandLineOptions.Parse(args);
        var testSuite = TestSuite.Build(builder, out var setupErrors);
        var (testResult, failedTestCases, skippedCount) = await testSuite.ExecuteAsync(options, setupErrors, cancellationToken);
        Result = testResult;

        // finish up
        var totalTestCaseCount = testResult.TotalCount;
        var failedCount = failedTestCases.Count;
        ConsoleLogger.LogInfo();
        ConsoleLogger.LogInfo("## Test Summary");
        ConsoleLogger.LogInfo($"Options:  {options}  ");
        ConsoleLogger.LogInfo($"Duration: {testResult.TotalExecutionTime.ToString(TimeSpanFormat)}  ");
        if (skippedCount > 0)
        {
            ConsoleLogger.LogFailed($"{SR.MarkdownFailed} Total {skippedCount} tests canceled");

            return ~failedCount;  // ok: bash treats -1 as 255 (byte)
        }
        else
        {
            if (failedCount == 0)
            {
                ConsoleLogger.LogPassed($"{SR.MarkdownPassed} All Tests Passed: {totalTestCaseCount}");
            }
            else
            {
                int failedCountExceptForSystemError = failedTestCases.Count(x => !x.IsSystemError);

                ConsoleLogger.LogPassed($"Passed: {totalTestCaseCount - failedCountExceptForSystemError} ({totalTestCaseCount})  ");
                ConsoleLogger.LogFailed($"Failed: {failedCount}  ");

                foreach (var detail in failedTestCases)
                {
                    ConsoleLogger.LogFailedTestCase($"{SR.MarkdownFailed} [{detail.TestSubject}] ", detail, SR.AnsiColorFailed);
                }
            }

            return failedCount;
        }
    }


    // TODO: JSON-convertible

    /// <summary>
    /// Represents the result of a test run.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]  // should not be included in IntelliSense suggestion
    public sealed class TestResult
    {
        /// <summary>
        /// Represents an error that occurred during a test case execution.
        /// </summary>
        /// <param name="Message">The error message.</param>
        /// <param name="StackTrace">The stack trace where the error occurred.</param>
        /// <param name="IsFUnitError">Indicates whether the error occurred outside the scope of 'describe' or 'it'.</param>
        public sealed record Error(string Message, string? StackTrace, bool IsFUnitError) { }
        // TODO: FilePath and FileLineNumber
        //       --> CallerFilePath and CallerLineNumber can be used in lambda?
        /// <summary>
        /// Represents a test subject.
        /// </summary>
        /// <param name="Title">The title of the test subject.</param>
        public sealed record Subject(string Title) { }
        /// <summary>
        /// Represents a single test case.
        /// </summary>
        /// <param name="Description">The description of the test case.</param>
        /// <param name="ExecutionCount">The number of times the test case was executed.</param>
        /// <param name="Error">The error that occurred during the test case execution, if any.</param>
        public sealed record Test(string Description, int ExecutionCount, Error? Error) { }


        /// <summary>
        /// Gets the command line options used for the test run.
        /// </summary>
        internal CommandLineOptions Options { get; }
        /// <summary>
        /// Gets the total execution time of the test run.
        /// </summary>
        public TimeSpan TotalExecutionTime { get; }
        /// <summary>
        /// Gets a read-only dictionary of test subjects and their associated test cases.
        /// </summary>
        public IReadOnlyDictionary<Subject, IReadOnlyList<Test>> TestsBySubject { get; }
        public int TotalCount { get; }

        internal TestResult(
            CommandLineOptions options,
            TimeSpan totalExecutionTime,
            IReadOnlyDictionary<string, IReadOnlyList<TestCase>> testCasesBySubject,
            IReadOnlyList<FailedTestCase> failedTestCases
        )
        {
            this.Options = options;
            this.TotalExecutionTime = totalExecutionTime;
            this.TotalCount = testCasesBySubject.Sum(x => x.Value.Count);

            var result = new Dictionary<Subject, IReadOnlyList<Test>>(testCasesBySubject.Count);
            foreach (var (subjectTitle, testCases) in testCasesBySubject)
            {
                var subject = new Subject(subjectTitle);
                var tests = new List<Test>(testCases.Count);

                result[subject] = tests;

                foreach (var tc in testCases)
                {
                    Error? error = null;

                    var failedCase = failedTestCases.FirstOrDefault(x => x.TestSubject == tc.Subject && x.Description == tc.Description);
                    if (failedCase != null)
                    {
                        var e = failedCase.Error;
                        error = new(e.Message, e.StackTrace, IsFUnitError: failedCase.IsSystemError);
                    }

                    tests.Add(new(tc.Description, tc.ExecutionCount, error));
                }
            }
            this.TestsBySubject = result;
        }

        /// <summary>
        /// Returns a string representation of the test results.
        /// </summary>
        /// <returns>A string containing the test results.</returns>
        public override string ToString()
        {
            return this.ToString(string.Empty, string.Empty, string.Empty);
        }

        /// <summary>
        /// Returns a string representation of the test results with color tags.
        /// </summary>
        /// <param name="passedColorTag">The color tag for passed tests.</param>
        /// <param name="failedColorTag">The color tag for failed tests.</param>
        /// <param name="colorResetTag">The color reset tag.</param>
        /// <returns>A colored string containing the test results.</returns>
        public string ToString(string passedColorTag, string failedColorTag, string colorResetTag)
        {
            var sb = new StringBuilder();

            _ = sb.AppendLine($"## Test Results");

            foreach (var (subject, tests) in this.TestsBySubject)
            {
                _ = sb.AppendLine($"- {subject.Title}");

                foreach (var test in tests)
                {
                    var error = test.Error;

                    var prefix = test.ExecutionCount == 0
                        ? "- [ ] *NOT EXECUTED*:"
                        : error == null
                            ? $"- [x]{passedColorTag}"
                            : $"- [ ]{failedColorTag}"
                            ;

                    _ = sb.AppendLine($"  {prefix} {test.Description}{colorResetTag}");

                    if (error != null)
                    {
                        // always!!
                        var message = error.Message.Replace("\n", $"\n{new string(' ', SR.IndentationAdjustment)}", StringComparison.Ordinal);

                        _ = sb.AppendLine($"    {failedColorTag}--> {message}{colorResetTag}");
                    }
                }
            }

            _ = sb
                .AppendLine()
                .AppendLine($"## Test Options")
                .AppendLine($"{this.Options}")
                .AppendLine()
                .AppendLine($"## Total Execution Time")
                .AppendLine($"{this.TotalExecutionTime.ToString(TimeSpanFormat)}")
                ;

            return sb.ToString().TrimEnd();
        }
    }
}
