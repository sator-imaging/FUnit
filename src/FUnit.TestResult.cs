// Licensed under the MIT License
// https://github.com/sator-imaging/FUnit

using FUnitImpl;
using Jsonable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

#pragma warning disable CA1050 // Declare types in namespaces
partial class FUnit
#pragma warning restore CA1050
{
    /// <summary>
    /// Represents the result of a test run.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]  // should not be included in IntelliSense suggestion
    [ToJson]
    public sealed partial class TestResult
    {
        /// <summary>
        /// Represents an error that occurred during a test case execution.
        /// </summary>
        /// <param name="Message">The error message.</param>
        /// <param name="StackTrace">The stack trace where the error occurred.</param>
        /// <param name="IsFUnitError">Indicates whether the error occurred outside the scope of 'describe' or 'it'.</param>
        /// <param name="IsFailure">The value that indicates whether the test case is failed or errored.</param>
        [ToJson]
        public sealed partial record Error(string Message, string? StackTrace, bool IsFUnitError, bool IsFailure) { }

        ///// <summary>
        ///// Represents a test subject.
        ///// </summary>
        ///// <param name="Title">The title of the test subject.</param>
        //public sealed record Subject(string Title) { }

        // TODO: FilePath and FileLineNumber
        //       --> CallerFilePath and CallerLineNumber can be used in lambda?

        /// <summary>
        /// Represents a single test case.
        /// </summary>
        /// <param name="Description">The description of the test case.</param>
        /// <param name="ExecutionCount">The number of times the test case was executed.</param>
        /// <param name="Errors">The errors that occurred during the test case execution, if any.</param>
        [ToJson]
        public sealed partial record Test(string Description, int ExecutionCount, IReadOnlyList<Error>? Errors) { }


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
        public IReadOnlyDictionary<string, IReadOnlyList<Test>> TestsBySubject { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestResult"/> class.
        /// </summary>
        /// <param name="Options">The command line options used for the test run.</param>
        /// <param name="TotalExecutionTime">The total execution time of the test run.</param>
        /// <param name="TestsBySubject">A dictionary of test subjects and their associated test cases.</param>
        internal TestResult(
            CommandLineOptions Options,
            TimeSpan TotalExecutionTime,
            IReadOnlyDictionary<string, IReadOnlyList<Test>> TestsBySubject
        )
        {
            this.Options = Options;
            this.TotalExecutionTime = TotalExecutionTime;
            this.TestsBySubject = TestsBySubject;
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
                _ = sb.AppendLine($"- {subject}");

                foreach (var test in tests)
                {
                    var errors = test.Errors;

                    var prefix = test.ExecutionCount == 0
                        ? "- [ ] *NOT EXECUTED*:"
                        : (errors is null || errors.Count == 0)
                            ? $"- [x]{passedColorTag}"
                            : $"- [ ]{failedColorTag}"
                            ;

                    var annotation = errors is null || errors.Count == test.ExecutionCount
                        ? string.Empty
                        : $" {SR.FlakyTestResultAnnotation}"
                        ;

                    _ = sb.AppendLine($"  {prefix} {test.Description}{annotation}{colorResetTag}");

                    if (errors is not null)
                    {
                        // always!!
                        var message = errors[0].Message.Replace("\n", $"\n{new string(' ', SR.IndentationAdjustment)}", StringComparison.Ordinal);

                        sb.AppendLine($"    {failedColorTag}--> {message}{colorResetTag}");
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
