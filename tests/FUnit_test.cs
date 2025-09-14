#:project ../src

using FUnitImpl;
using System.Diagnostics;


const int AsyncTestDelayMilliseconds = 1000;
var startTimestamp = Stopwatch.GetTimestamp();

var callCounts = new CallCounts();

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

    // negative tests
    describe("Action<T>", it => it("should throw", (object obj) => { }));
    describe("Func<int>", it => it("should throw", () => 310));
    describe("Func<object>", it => it("should throw", () => new object()));
    describe("Func<ValueTask<T>>", it =>
    {
        it("should throw", task);
        async ValueTask<int> task() { await Task.CompletedTask; return 310; }
    });

    describe("Empty", it => { });

    describe("Error outside of 'it' scope", it =>
    {
        it("is empty test case", () => { });

        throw new Exception($"Failed outside of '{nameof(it)}'");
    });

    throw new Exception($"Failed outside of '{nameof(describe)}'");
});


// async tests must be awaited correctly
var elapsedTime = Stopwatch.GetElapsedTime(startTimestamp);
Must.BeTrue(AsyncTestDelayMilliseconds * 2 <= elapsedTime.TotalMilliseconds);

// errors outside of test descriptor scope should be captured
const int ExpectedErrorCount = 6;
Must.BeEqual(ExpectedErrorCount, failedTestCount);

Must.BeEqual(1, callCounts.Action);
Must.BeEqual(1, callCounts.FuncTask);
Must.BeEqual(1, callCounts.FuncValueTask);

ConsoleLogger.LogInfoRaw();
ConsoleLogger.LogInfoRaw($"<details><summary><b>TEST</b>: <code>{nameof(FUnit)}.{nameof(FUnit.Result)}.{nameof(FUnit.Result.ToString)}()</code></summary>");
ConsoleLogger.LogInfoRaw();
ConsoleLogger.LogInfoRaw("```md");
ConsoleLogger.LogInfoRaw(FUnit.Result?.ToString().TrimEnd() ?? throw new Exception("must not be reached"));
ConsoleLogger.LogInfoRaw("```");
ConsoleLogger.LogInfoRaw();
ConsoleLogger.LogInfoRaw("</details>");
ConsoleLogger.LogInfoRaw();


ConsoleLogger.LogPassed($"{SR.MarkdownPassed} OK: {ExpectedErrorCount} expected errors are captured correctly");
ConsoleLogger.LogPassed($"{SR.MarkdownPassed} All tests successfully completed");

return 0;



file sealed class CallCounts
{
    public int Action;
    public int FuncTask;
    public int FuncValueTask;
}
