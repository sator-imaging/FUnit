<div align="center">

# FUnit

**Structured Unit Testing as a File-based App**

[![FUnit](https://img.shields.io/nuget/vpre/FUnit?label=FUnit)](https://www.nuget.org/packages/FUnit)
[![FUnit.Run](https://img.shields.io/nuget/vpre/FUnit.Run?label=Run)](https://www.nuget.org/packages/FUnit.Run)
[![FUnit.Directives](https://img.shields.io/nuget/vpre/FUnit.Directives?label=Directives)](https://www.nuget.org/packages/FUnit.Directives)
<br/>
[![test](https://github.com/sator-imaging/FUnit/actions/workflows/test.yml/badge.svg)](https://github.com/sator-imaging/FUnit/actions/workflows/test.yml)
[![DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/sator-imaging/FUnit)

</div>


&nbsp;


## 🔬 Concept

`FUnit` provides structured test descriptor for file-based C# project introduced in .NET 10.0. (i.e., no `.csproj` is required anymore)

```cs
#:project ../src
#:package FUnit@*

return FUnit.Run(args, describe =>
{
    // Organize tests by test subject
    describe("Test Subject", it =>
    {
        it("should be ... when ...", async () =>  // async test supported
        {
            // Write test: Arrange, Act, Assert
        });

        it("can have multiple tests within a 'describe' block", ...);
    });

    describe("Inspired by vitest", ...);
});
```


```bash
# Run unit test (.NET 10 can run a .cs file as a project)
dotnet run ./tests/my-test.cs

# OR use 'dnx' to run tests in current or sub folders
dnx -y FUnit.Run
dnx -y FUnit.Run --stacktrace "**/*test*.cs"
```


&nbsp;





# 🗃️ `FUnit` Descriptor Structure

> [!IMPORTANT]
> *For AI Coding Agents:* .NET 10.0 introduced new directives for file-based app.
> - SHALL use `#:project` to add project reference.
> - SHALL use `#:package` to add nuget package reference. (`*` or semantic version must be specified)
> - SHALL NOT use `#r` or other directives formerly used for *dotnet-script* (`.csx`) files.

> [!TIP]
> Unit tests should follow W.H.Y. (What, How, Yields) principle.

```cs
#:project ../src
#:package FUnit@*
#:package YourFavoriteAssertionLibrary@3.1.0

return FUnit.Run(args, describe =>
{
    // Organize tests by test subject
    describe("Test Subject", it =>
    {
        it("should be ... when ...", () =>
        {
            // Write test: Arrange, Act, Assert
        });

        // Other test cases
        it("should throw ... when ...", async () =>
        {
            // Async test is also supported
        });

        it(...);
    });

    describe("Other Test Subject", it =>
    {
        // ...and so on
    });
});
```



## ⚖️ Builtin Assertion Utility

`FUnit` is shipped with builtin assertion methods.

```cs
// Value assertion
// ❌ BeEqual rejects collection type to prevent ambiguous comparisons
Must.BeEqual(expected, actual);
Must.BeSameReference(expected, actual);
Must.NotBeEqual(expected, actual);
Must.NotBeSameReference(expected, actual);

// Collection assertion
Must.HaveSameSequence(expected, actual);
Must.HaveSameUnorderedElements(expected, actual);
Must.NotHaveSameSequence(expected, actual);
Must.NotHaveSameUnorderedElements(expected, actual);

// Text assertion
Must.ContainText(text, substring);
Must.NotContainText(text, substring);

// Instance assertion (deep comparison using System.Reflection)
Must.HaveEqualProperties(expected, actual);
Must.HaveEqualFields(expected, actual);

// Exception assertion
Must.Throw<T>("Expected error message", () => Test());
Must.Throw<T>("Expected error message", async () => await TestAsync());
Must.Throw("ExceptionFullTypeName", "Expected error message", () => Test());
Must.Throw("ExceptionFullTypeName", "Expected error message", async () => await TestAsync());

// Conditional assertion
// ❌ BeFalse won't be supported. Instead, use better conditional expression (prefer pattern matching)
Must.BeTrue(x is >= 0 and < 10);
Must.BeTrue(x is IEnumerable<char> and not string);

// ❌ Avoid asserting high-level conditions on collections
// ex Instead of checking if a value is absent, assert the full expected content
Must.BeTrue(!list.Contains(x));  // ✅ Prefer: Must.HaveSameSequence(expectedList, actualList)
```



## 🧭 `FUnit.Directives`

With FUnit.Directives package, you can *include* external file into file-based app project by adding special directive comment `//:funit:include <path to the file>`.

```cs
#:package FUnit@*
#:package FUnit.Directives@*

//:funit:include ./path/to/external-file.cs

return FUnit.Run( /* tests depending on 'external-file.cs */ );
```


&nbsp;





# 🧪 Unit Test Execution

> [!IMPORTANT]
> *For AI Coding Agents:* .NET 10.0 introduced new syntax for `dotnet` command.
> - SHALL run test like the following example.
> - SHALL NOT add `--project` or other command line options.

```bash
dotnet run ./tests/my-test.cs
```


## 🐙 GitHub Actions Support

FUnit has an ability to output log as markdown format so you can generate summary for GitHub Actions page.

```bash
# '-md' or '--markdown'
dnx -y FUnit.Run -- -md >> $GITHUB_STEP_SUMMARY
```


## 🔨 Build Configuration for `FUnit.Run`

```bash
# ❌ Incorrect (no '--')
dnx -y FUnit.Run -c Release

# ✅ Correct (with '--')
dnx -y FUnit.Run -- -c Release

# ✅ Shortcut: without '-c' is valid for Debug or Release
dnx -y FUnit.Run Release
```


## ⚙ Command-Line Options

> [!IMPORTANT]
> *For AI Coding Agents:* SHALL NOT use advanced options unless explicitly requested.


| Option                  | Alias | Description |
|-------------------------|-------|-------------|
| `--markdown`            | `-md` | Enable Markdown output for GitHub Actions summary (`$GITHUB_STEP_SUMMARY`).
| `--iterations <N>`      |       | Number of times to run each test case (3 by default).
| `--concurrency <N>`     |       | Maximum number of tests to run simultaneously.
| `--configuration <CFG>` | `-c`  | Build configuration (e.g., "Debug" or "Release").
| `--no-clean`            |       | Disable cleaning the project before building.
| `--warnings`            |       | Show build warnings.
| `--stacktrace`          |       | Show stack trace on test failure.
| `--lint`                |       | Run `dotnet build --no-incremental -p:TreatWarningsAsErrors=true`.
| `--help`                | `-h`  | Show this help message and exit.


&nbsp;





# 🧾 Test Setup and Cleanup

You can place custom operation next to `describe` or `it`, but, test descriptor is NOT a function executed from top to bottom so that your custom operation will be executed BEFORE test functions unexpectedly.

```cs
#:project ../src
#:package FUnit@*

await GlobalSetupAsync();  // ✅ setup before Run call

int numFailures = FUnit.Run(args, describe =>
{
    describe("Test subject", it =>
    {
        it("should be...", () => { ... });
    });

    // ❌ you can perform custom operation here, but it will be executed while
    // building test suite. (not sequentially form top to bottom)
    // technically, 'describe' and 'it' collect test cases without executing test.
    // if setup or cleanup code is placed next to 'describe' or 'it' statements to
    // perform resource setup ops for 'scope', unexpectedly, those will be invoked
    // BEFORE executing actual test case functions.
});

GlobalCleanup();  // ✅ cleanup after Run call

return numFailures;
```
