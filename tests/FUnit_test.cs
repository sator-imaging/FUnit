#:project ../src
//#:package FUnit@*

using FUnitImpl;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
#pragma warning disable CA1822 // Mark members as static

// command line options
Must.Throw<ArgumentException>("Unknown command line option: foo", () => CommandLineOptions.Parse(["foo"]));
Must.Throw<ArgumentException>("'--concurrency' takes 1 positive integer parameter", () => CommandLineOptions.Parse(["--concurrency"]));
Must.Throw<ArgumentException>("'--iterations' takes 1 positive integer parameter", () => CommandLineOptions.Parse(["--iterations"]));
Must.Throw<ArgumentException>("'--configuration' takes 1 string parameter", () => CommandLineOptions.Parse(["--configuration"]));
Must.Throw<ArgumentOutOfRangeException>("Argument must be greater than 0: 0 (Parameter 'ConcurrencyLevel')", () => CommandLineOptions.Parse(["--concurrency", "0"]));
Must.Throw<ArgumentOutOfRangeException>("Argument must be greater than 0: 0 (Parameter 'Iterations')", () => CommandLineOptions.Parse(["--iterations", "0"]));


// build and execute
{
    var calls = new CallCounts();
    var suite = FUnit.Build(describe =>
    {
        describe("Build & Execute", it =>
        {
            it("should work", async () =>
            {
                await Task.Delay(10);
                calls.FuncTask++;
            });

            it("should throw", async () =>
            {
                await Task.Delay(10);
                Must.BeTrue(false);
            });
        });
    });

    Must.BeTrue(suite.Execute("Build & Execute", "should work") == null);
    Must.BeTrue(suite.Execute("Build & Execute", "should throw") != null);

    const int numIterations = 7;
    var result = suite.ExecuteTests([.. args, "--iterations", numIterations.ToString()]);

    // +1 because explicit execute was performed
    Must.BeEqual(numIterations + 1, calls.FuncTask);
    Must.BeEqual(numIterations + 1, result.TestsBySubject["Build & Execute"][0].ExecutionCount);
    Must.BeEqual(numIterations + 1, result.TestsBySubject["Build & Execute"][1].ExecutionCount);
}


// --iterations
{
    const int TestIterationCount = 7;
    var calls = new CallCounts();
    var desc = "--iterations " + TestIterationCount;
    FUnit.Run([.. args, "--iterations", TestIterationCount.ToString()], describe =>
    {
        describe(desc, it =>
        {
            it("should pass", () => calls.Action++);
        });
    });

    Must.BeEqual(TestIterationCount, FUnit.Result!.TestsBySubject[desc][0].ExecutionCount);
    Must.BeEqual(TestIterationCount, calls.Action);
}
{
    const int TestIterationCount = 1;
    var calls = new CallCounts();
    var desc = "--iterations " + TestIterationCount;
    FUnit.Run([.. args, "--iterations", TestIterationCount.ToString()], describe =>
    {
        describe(desc, it =>
        {
            it("should pass", () => calls.Action++);
        });
    });

    Must.BeEqual(TestIterationCount, FUnit.Result!.TestsBySubject[desc][0].ExecutionCount);
    Must.BeEqual(TestIterationCount, calls.Action);
}


// --concurrency & --iterations
{
    const int DelayMilliseconds = 1000;
    var ts = Stopwatch.GetTimestamp();

    var calls = new CallCounts();
    Must.BeEqual(0, FUnit.Run([.. args, "--concurrency", "1", "--iterations", "10"], describe =>
    {
        describe("should run simutaneously", it =>
        {
            it("should work", async () =>
            {
                await Task.Delay(DelayMilliseconds);
                calls.FuncTask++;
            });
        });
    }));

    // all iterations will run even if concurrency is set to 1
    var elapsedMillis = SR.GetElapsedTime(ts).TotalMilliseconds;
    Must.BeTrue(elapsedMillis is > DelayMilliseconds and <= (DelayMilliseconds * 2));

    var result = FUnit.Result ?? throw new Exception("must not be reached");
    Must.BeEqual(10, result.TestsBySubject.First().Value[0].ExecutionCount);
    Must.BeEqual(10, calls.FuncTask);
}


const int AsyncTestDelayMilliseconds = 1000;
var startTimestamp = Stopwatch.GetTimestamp();

var callCounts = new CallCounts();
var sync = new object();

int failedTestCount = FUnit.Run(args, describe =>
{
    describe("Action", it =>
    {
        it("should increment action call count", () =>
        {
            callCounts.Action++;
        });
    });

    describe("Func<Task>", it =>
    {
        it("should increment func task call count", task);
        async Task task()
        {
            await Task.Delay(AsyncTestDelayMilliseconds);  // must wait a second to check
            callCounts.FuncTask++;
        }
    });

    describe("Func<ValueTask>", it =>
    {
        it("should increment func value task call count", task);
        async ValueTask task()
        {
            await Task.Delay(AsyncTestDelayMilliseconds);  // must wait a second to check
            callCounts.FuncValueTask++;
        }
    });

    describe("Func<object>", it => it("should pass", () => new object()));

    // negative tests
    describe("Empty test is not allowed", it => { });
    describe("Action<T>", it => it("should throw", (object obj) => { }));
    describe("Func<MyValueType>", it => it("should throw", () => new MyValueType()));
    describe("Func<ValueTask<T>>", it =>
    {
        it("should throw", task);
        async ValueTask<int> task() { await Task.CompletedTask; return 310; }
    });
    describe("Incorrect global state", it =>
    {
        it("should pass only once", () =>
        {
            lock (sync)
            {
                using var x = new StringBuilderCache();
                x.Append("test");
                Must.BeEqual("test", x.ToString());
            }
        });
    });

    // Must tests
    describe("Error messages", it => it("should throw", () => { Must.HaveSameSequence([1, 2], [1, 2, 3]); }));

    // test descriptor tests
    describe("Error outside of 'it' scope", it =>
    {
        it("should pass empty test function", () => { });
        it("should pass empty test function", () => { });  // should get error due to naming conflict

        throw new Exception($"Failed outside of '{nameof(it)}'");
    });

    throw new Exception($"Failed outside of '{nameof(describe)}'");
});


// async tests must be awaited correctly
var elapsedTime = Stopwatch.GetElapsedTime(startTimestamp);
var expectedTime = AsyncTestDelayMilliseconds * 2;
Must.BeTrue(elapsedTime.TotalMilliseconds > expectedTime);

var resultText = FUnit.Result?.ToString() ?? throw new Exception("must not be reached");
Must.BeEqual(1, Regex.Count(resultText, @" 'it' must be called at least once"));
Must.BeEqual(1, Regex.Count(resultText, @" Test case conflict"));
Must.BeEqual(1, Regex.Count(resultText, @" FUnit got error from outside of 'it' scope"));
Must.BeEqual(1, Regex.Count(resultText, @" FUnit got error from outside of 'describe' scope"));
Must.BeEqual(1, Regex.Count(resultText, @" Flaky Test Detected: Inconsistent results across multiple runs"));

// errors outside of test descriptor scope should be captured
const int ExpectedErrorCount = 9;
Must.BeEqual(ExpectedErrorCount, failedTestCount);

Must.BeEqual(3, callCounts.Action);
Must.BeEqual(3, callCounts.FuncTask);
Must.BeEqual(3, callCounts.FuncValueTask);

ConsoleLogger.LogInfoRaw($@"
<details><summary><b>TEST</b>: <code>{nameof(FUnit)}.{nameof(FUnit.Result)}.{nameof(FUnit.Result.ToString)}()</code></summary>

```md

{resultText}

```

</details>
");


ConsoleLogger.LogPassed($"{SR.MarkdownPassed} OK: {ExpectedErrorCount} expected errors are captured correctly");
ConsoleLogger.LogPassed($"{SR.MarkdownPassed} All tests successfully completed");

return 0;



file sealed class CallCounts
{
    public int Action;
    public int FuncTask;
    public int FuncValueTask;
}

file struct MyValueType { }

file sealed class StringBuilderCache : IDisposable
{
    readonly static StringBuilder sb = new();
    public override string ToString() => sb.ToString();
    public void Append(string text) => sb.Append(text);
    public void Dispose() { /* cache not cleared */ }
}
