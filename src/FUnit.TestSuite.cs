using FUnitImpl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static partial class FUnit
{
    private class TestSuite
    {
        private const int InitialSubjectMapCapacity = 8;
        private const int InitialTestCaseListCapacity = 4;
        private readonly IReadOnlyDictionary<string, IReadOnlyList<TestCase>> _testCasesBySubject;

        private TestSuite(IReadOnlyDictionary<string, IReadOnlyList<TestCase>> testCasesBySubject)
        {
            _testCasesBySubject = testCasesBySubject;
        }

        public static TestSuite Build(Action<Action<string, Action<Action<string, Delegate>>>> builder, out List<FailedTestCase> setupErrors)
        {
            var testCasesBySubject = new Dictionary<string, List<TestCase>>(InitialSubjectMapCapacity);
            setupErrors = new List<FailedTestCase>();

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
                            testCases.Add(new(subjectDesc, testCaseDesc, testCaseFunc));
                        });
                    }
                    catch (Exception ex)
                    {
                        setupErrors.Add(
                            new(nameof(FUnit), $"{nameof(FUnit)} got error from outside of 'it' scope", ex, IsSystemError: true));
                    }
                });
            }
            catch (Exception ex)
            {
                setupErrors.Add(
                    new(nameof(FUnit), $"{nameof(FUnit)} got error from outside of 'describe' scope", ex, IsSystemError: true));
            }

            return new TestSuite(testCasesBySubject.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<TestCase>)kvp.Value));
        }

        public async Task<(TestResult testResult, IReadOnlyList<FailedTestCase> failedTestCases, int skippedCount)> ExecuteAsync(CommandLineOptions options, IReadOnlyList<FailedTestCase> setupErrors, CancellationToken cancellationToken)
        {
            var timestamp_runStart = Stopwatch.GetTimestamp();
            var failedTestCases = new List<FailedTestCase>(setupErrors);
            var skippedTestCases = new List<TestCase>();

            // TODO: ready to run tests simultaneously but functionality is not tested
            int concurrencyLevel = options.ConcurrencyLevel;
            var activeTasks = new List<Task<FailedTestCase?>>(capacity: concurrencyLevel);

            ConsoleLogger.LogInfo();
            ConsoleLogger.LogInfo("## Test Result");

            foreach (var (testSubject, testCases) in _testCasesBySubject.Where(x => x.Value.Count != 0))
            {
                ConsoleLogger.LogInfo($"- {testSubject}");

                int currentCaseIndex = 0;

            CONSUME_QUEUE:
                if (cancellationToken.IsCancellationRequested)
                {
                    skippedTestCases.AddRange(testCases.Skip(currentCaseIndex));
                    continue;  // collect all skipped test cases
                }

                while (currentCaseIndex < testCases.Count && activeTasks.Count < concurrencyLevel)
                {
                    var task = Task.Run(() => testCases[currentCaseIndex].ExecuteAsync(), cancellationToken);
                    currentCaseIndex++;

                    activeTasks.Add(task);
                }

                if (activeTasks.Count == 0)
                {
                    continue;
                }

                await Task.WhenAll(activeTasks);

                failedTestCases.AddRange(
                    activeTasks
                        .Where(x => x.Result != null)
                        .Select(x => x.Result)
                        .OfType<FailedTestCase>()
                        );

                activeTasks.Clear();

                goto CONSUME_QUEUE;
            }

            var elapsedTime = SR.GetElapsedTime(timestamp_runStart);
            var testResult = new TestResult(options, elapsedTime, _testCasesBySubject, failedTestCases);

            if (skippedTestCases.Count > 0)
            {
                foreach (var skipped in skippedTestCases)
                {
                    ConsoleLogger.LogFailed($"- [FUnit] Tests Canceled");
                    ConsoleLogger.LogFailed($"  {SR.MarkdownFailed} [{skipped.Subject}] {skipped.Description}");
                }
            }

            return (testResult, failedTestCases, skippedTestCases.Count);
        }
    }
}
