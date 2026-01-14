#:project ../src
//#:package FUnit@*

using FUnitImpl;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


const int AsyncTestDelayMilliseconds = 1000;
var startTimestamp = Stopwatch.GetTimestamp();

const string NotExecutedMessage = "*NOT EXECUTED*";

Must.BeEqual(0, await FUnit.RunAsync(args, describe => { }));
Must.NotContainText(FUnit.Result?.ToString() ?? throw new Exception(), NotExecutedMessage);


string firstTestMessage;
{
    using var cts = new CancellationTokenSource(AsyncTestDelayMilliseconds / 3);

    int result = await FUnit.RunAsync([/* no args! */], describe =>
    {
        describe("sync", it => it("should pass", () => { }));
        describe("async", it =>
        {
            it("should pass", async () =>
            {
                await Task.Delay(AsyncTestDelayMilliseconds);
            });

            // required because above test will be queued immediately.
            it("should not be executed", async () =>
            {
                await Task.Delay(AsyncTestDelayMilliseconds);
            });
        });
    },
    cts.Token);

    firstTestMessage = FUnit.Result?.ToString() ?? throw new Exception("must not be reached");

    Must.BeEqual(-1, result);
    Must.ContainText(firstTestMessage, NotExecutedMessage);
}


string secondTestMessage;
{
    using var cts = new CancellationTokenSource(AsyncTestDelayMilliseconds / 3);

    int result = await FUnit.RunAsync([/* no args! */], describe =>
    {
        describe("sync", it => it("should pass", () => { }));
        describe("async", it =>
        {
            it("should throw", async () =>
            {
                await Task.Delay(AsyncTestDelayMilliseconds);
                Must.BeEqual(false, true);
            });

            // required because above test will be queued immediately.
            it("should not be executed", async () =>
            {
                await Task.Delay(AsyncTestDelayMilliseconds);
            });
        });
    },
    cts.Token);

    secondTestMessage = FUnit.Result?.ToString() ?? throw new Exception("must not be reached");

    Must.BeEqual(-2, result);
    Must.ContainText(secondTestMessage, NotExecutedMessage);
}


ConsoleLogger.LogInfoRaw();
ConsoleLogger.LogInfoRaw($"<details><summary><b>TEST (1st)</b>: <code>{nameof(FUnit)}.{nameof(FUnit.Result)}.{nameof(FUnit.Result.ToString)}()</code></summary>");
ConsoleLogger.LogInfoRaw();
ConsoleLogger.LogInfoRaw("```md");
ConsoleLogger.LogInfoRaw(firstTestMessage);
ConsoleLogger.LogInfoRaw("```");
ConsoleLogger.LogInfoRaw();
ConsoleLogger.LogInfoRaw("</details>");
ConsoleLogger.LogInfoRaw();

ConsoleLogger.LogInfoRaw();
ConsoleLogger.LogInfoRaw($"<details><summary><b>TEST (2nd)</b>: <code>{nameof(FUnit)}.{nameof(FUnit.Result)}.{nameof(FUnit.Result.ToString)}()</code></summary>");
ConsoleLogger.LogInfoRaw();
ConsoleLogger.LogInfoRaw("```md");
ConsoleLogger.LogInfoRaw(secondTestMessage);
ConsoleLogger.LogInfoRaw("```");
ConsoleLogger.LogInfoRaw();
ConsoleLogger.LogInfoRaw("</details>");
ConsoleLogger.LogInfoRaw();


ConsoleLogger.LogPassed($"{SR.MarkdownPassed} OK: `RunAsync` cancellation tests completed correctly");
ConsoleLogger.LogPassed($"{SR.MarkdownPassed} All async tests successfully completed");

return 0;
