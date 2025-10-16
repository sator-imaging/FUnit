using FUnitImpl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Descriptor = System.Action<string, System.Action<System.Action<string, System.Delegate>>>;

#pragma warning disable CA1050 // Declare types in namespaces
partial class FUnit
#pragma warning restore CA1050
{
    /// <summary>
    /// Represents a test suite containing multiple test cases.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]  // should not be included in IntelliSense suggestion
    public sealed class TestSuite
    {
        /// <summary>
        /// Occurs when the test suite is about to start running tests.
        /// </summary>
        public event Action? OnTestStarting;
        /// <summary>
        /// Occurs when a new test subject (group of test cases) is about to start.
        /// </summary>
        public event Action<string>? OnTestSubjectStarting;
        /// <summary>
        /// Occurs when a test case passes successfully.
        /// </summary>
        public event Action<(string subject, string description, int maxIterations, IEnumerable<Exception> errors)>? OnTestCasePassed;
        /// <summary>
        /// Occurs when a test case fails.
        /// </summary>
        public event Action<(string subject, string description, int maxIterations, IEnumerable<Exception> errors)>? OnTestCaseFailed;
        /// <summary>
        /// Occurs when the report for canceled executions is about to start.
        /// </summary>
        public event Action? OnCanceledExecutionReportStarting;
        /// <summary>
        /// Occurs when a canceled test case is reported.
        /// </summary>
        public event Action<(string subject, string description)>? OnCanceledExecutionReportTestCase;

        private readonly Dictionary<string, List<TestCase>> _testCasesBySubject = new(InitialSubjectMapCapacity);
        private readonly List<(string description, Exception error)> _buildErrors = new();

        /// <summary>
        /// Gets a read-only dictionary where the key is the test subject and the value is a list of test case descriptions.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<string>> TestsBySubject =>
            this.cache_TestsBySubject ??= this._testCasesBySubject.ToDictionary(k => k.Key, v => v.Value.Select(x => x.Description).ToList() as IReadOnlyList<string>);
        private Dictionary<string, IReadOnlyList<string>>? cache_TestsBySubject;

        internal TestSuite(Action<Descriptor> builder)
        {
            var testCasesBySubject = this._testCasesBySubject;

            // 'describe' may contain error
            try
            {
                // collect subjects
                builder.Invoke((subjectDesc, it) =>
                {
                    // 'it' may contain error
                    try
                    {
                        if (!testCasesBySubject.TryGetValue(subjectDesc, out var testCases))
                        {
                            testCases = new(InitialTestCaseListCapacity);
                            testCasesBySubject[subjectDesc] = testCases;
                        }

                        // run 'it' (case aggregator) to collect test cases
                        it.Invoke((testCaseDesc, testCaseFunc) =>
                        {
                            if (testCases.Any(x => x.Description == testCaseDesc))
                            {
                                this._buildErrors.Add(($"Test case conflict", new FUnitException($"Description must be unique in test subject")));
                            }
                            else
                            {
                                testCases.Add(new(subjectDesc, testCaseDesc, testCaseFunc));
                            }
                        });

                        if (testCases.Count == 0)
                        {
                            this._buildErrors.Add(($"No test case", new FUnitException($"'{nameof(it)}' must be called at least once")));
                            testCasesBySubject.Remove(subjectDesc);
                        }
                    }
                    catch (Exception ex)
                    {
                        this._buildErrors.Add(($"{nameof(FUnit)} got error from outside of 'it' scope", ex));
                    }
                });
            }
            catch (Exception ex)
            {
                this._buildErrors.Add(($"{nameof(FUnit)} got error from outside of 'describe' scope", ex));
            }
        }


        /// <summary>
        /// Executes a specific test case synchronously.
        /// </summary>
        /// <param name="subject">The subject of the test case.</param>
        /// <param name="test">The description of the test case.</param>
        /// <returns>An <see cref="Exception"/> if the test failed, otherwise <see langword="null"/>.</returns>
        public Exception? Execute(string subject, string test)
        {
            return Task.Run(async () => await this.ExecuteAsync(subject, test)).Result;  // ok
        }

        /// <summary>
        /// Executes a specific test case asynchronously.
        /// </summary>
        /// <param name="subject">The subject of the test case.</param>
        /// <param name="test">The description of the test case.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{Exception}"/> representing the asynchronous operation, containing an <see cref="Exception"/> if the test failed, otherwise <see langword="null"/>.</returns>
        public async Task<Exception?> ExecuteAsync(string subject, string test, CancellationToken cancellationToken = default)
        {
            if (!this._testCasesBySubject.TryGetValue(subject, out var testCases))
            {
                throw new FUnitException($"Test subject is not found: {subject}");
            }

            var testCase = testCases.FirstOrDefault(x => x.Description == test)
                ?? throw new FUnitException($"Test case is not found for the subject '{subject}': {test}");

            var result = await testCase.ExecuteAsync(cancellationToken);
            return result;
        }


        /// <summary>
        /// Executes the tests based on the provided command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>The <see cref="TestResult"/> of the execution.</returns>
        public TestResult ExecuteTests(string[] args)
        {
            return Task.Run(async () => await this.ExecuteCoreAsync(CommandLineOptions.Parse(args), cancellationToken: default)).Result;  // ok
        }

        /// <summary>
        /// Executes the tests asynchronously based on the provided command-line arguments.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TestResult}"/> representing the asynchronous operation, containing the test results.</returns>
        public Task<TestResult> ExecuteTestsAsync(
            string[] args,
            CancellationToken cancellationToken = default)
        {
            return this.ExecuteCoreAsync(CommandLineOptions.Parse(args), cancellationToken);
        }


        // making this method public will force internal types need to be exposed.
        internal async Task<TestResult> ExecuteCoreAsync(
            CommandLineOptions options,
            CancellationToken cancellationToken)
        {
            var timestamp_runStart = Stopwatch.GetTimestamp();

            OnTestStarting?.Invoke();

            var failedTestCases = new List<(string subject, string description, Exception error)>();
            List<TestCase>? skippedTestCases = null;

            // run tests
            {
                // TODO: ready to run tests simultaneously but functionality is not tested
                int concurrencyLevel = options.ConcurrencyLevel;
                int maxIterations = options.Iterations;
                var activeTasks = new List<Task<(string subject, string description, Exception? error)>>(capacity: concurrencyLevel);

                foreach (var (testSubject, testCases) in this._testCasesBySubject.Where(x => x.Value.Count > 0))
                {
                    OnTestSubjectStarting?.Invoke(testSubject);

                    int currentCaseIndex = 0;

                CONSUME_QUEUE:
                    if (cancellationToken.IsCancellationRequested)
                    {
                        skippedTestCases ??= new();
                        skippedTestCases.AddRange(testCases.Skip(currentCaseIndex));
                        continue;  // collect all skipped test cases
                    }

                    // TODO: iterations == 3, concurrency == 1 --> 3 tests may run in current impl.
                    //       --> to verify concurrent execution robustness, all iterations should be started at the same moment.
                    //           so, need to force concurrency level must be greater than or equal to max iterations?
                    using (var startingSignal = new ManualResetEventSlim(initialState: false, spinCount: 0))
                    {
                        while (currentCaseIndex < testCases.Count &&
                            (activeTasks.Count == 0 || activeTasks.Count + maxIterations <= concurrencyLevel))
                        {
                            for (int i = 0; i < maxIterations; i++)
                            {
                                // NOTE: need to capture variables for lambda
                                var testCase = testCases[currentCaseIndex];
                                var i_captured = i + 1;

                                var task = Task.Run(async () =>
                                {
                                    startingSignal.Wait();

                                    var error = await testCase.ExecuteAsync(cancellationToken);
                                    return (testCase.Subject, testCase.Description, error);
                                });

                                activeTasks.Add(task);
                            }

                            currentCaseIndex++;
                        }

                        if (activeTasks.Count == 0)
                        {
                            continue;
                        }

                        startingSignal.Set();
                        _ = await Task.WhenAll(activeTasks);
                    }

                    foreach (var completedTasksByTest in activeTasks.Select(x => x.Result).GroupBy(x => (x.subject, x.description)))
                    {
                        var failedCases = completedTasksByTest.Where(x => x.error != null).Cast<(string, string, Exception error)>();
                        failedTestCases.AddRange(failedCases);

                        var errors = failedCases.Select(x => x.error);
                        if (errors.Any())
                        {
                            this.OnTestCaseFailed?.Invoke((completedTasksByTest.Key.subject, completedTasksByTest.Key.description, maxIterations, errors));
                        }
                        else
                        {
                            this.OnTestCasePassed?.Invoke((completedTasksByTest.Key.subject, completedTasksByTest.Key.description, maxIterations, errors));
                        }
                    }

                    activeTasks.Clear();

                    goto CONSUME_QUEUE;
                }

                if (skippedTestCases?.Count is > 0)
                {
                    OnCanceledExecutionReportStarting?.Invoke();

                    foreach (var skipped in skippedTestCases)
                    {
                        OnCanceledExecutionReportTestCase?.Invoke((skipped.Subject, skipped.Description));
                    }
                }
            }

            // don't include result build time
            var elapsedTime = SR.GetElapsedTime(timestamp_runStart);
            var result = new Dictionary<string, IReadOnlyList<TestResult.Test>>(this._testCasesBySubject.Count);
            {
                if (this._buildErrors.Count > 0)
                {
                    var errors = new List<TestResult.Test>(this._buildErrors.Count);
                    result.Add(new(nameof(FUnit)), errors);

                    foreach (var (description, error) in this._buildErrors)
                    {
                        errors.Add(
                            new(description, 1, new List<TestResult.Error>()
                            {
                            new(error.Message, error.StackTrace, IsFUnitError: true),
                            }));
                    }
                }

                foreach (var (subject, testCases) in this._testCasesBySubject)
                {
                    var tests = new List<TestResult.Test>(testCases.Count);

                    if (result.TryGetValue(subject, out var only_when_subject_is_set_to_FUnit))
                    {
                        tests.AddRange(only_when_subject_is_set_to_FUnit);
                    }
                    result[subject] = tests;

                    foreach (var tc in testCases)
                    {
                        List<TestResult.Error>? errors = null;

                        var failedCases = failedTestCases.Where(x => x.subject == tc.Subject && x.description == tc.Description);
                        if (failedCases.Any())
                        {
                            errors = failedCases.Select(failedCase =>
                            {
                                var e = failedCase.error;
                                return new TestResult.Error(e.Message, e.StackTrace, IsFUnitError: false);
                            })
                            .ToList();
                        }

                        tests.Add(new(tc.Description, tc.ExecutionCount, errors));
                    }
                }
            }

            return new TestResult(options, elapsedTime, result);
        }
    }
}
