[![FUnit](https://img.shields.io/nuget/vpre/FUnit?label=FUnit)](https://www.nuget.org/packages/FUnit)
[![FUnit.Run](https://img.shields.io/nuget/vpre/FUnit.Run?label=Run)](https://www.nuget.org/packages/FUnit.Run)
[![FUnit.Directives](https://img.shields.io/nuget/vpre/FUnit.Directives?label=Directives)](https://www.nuget.org/packages/FUnit.Directives)
[![build](https://github.com/sator-imaging/FUnit/actions/workflows/build.yml/badge.svg)](https://github.com/sator-imaging/FUnit/actions/workflows/build.yml)
&nbsp;
[![DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/sator-imaging/FUnit)





# 🔬 Unit Testing Framework for File-based Apps

`FUnit` provides structured test descriptor for file-based apps introduced in .NET 10.0. (i.e., no `.csproj` is required anymore)

```cs
#:project ../src
#:package FUnit@*

return FUnit.Run(args, describe =>
{
    // Organize tests by test subject
    describe("Test Subject", it =>
    {
        it("should be ...", async () =>  // async can be used
        {
            // Write actual test here: Arrange, Act, Assert
        });

        it("can have multiple test cases", ...);
    });

    describe("Inspired by vitest", ...);
});
```


```bash
# Run unit test
dotnet run ./tests/my-test.cs

# OR use 'dnx' to run all "*test*.cs" (case-insensitive) in project folder
dnx FUnit.Run
```





# 🗃️ `FUnit` Descriptor Structure

> [!IMPORTANT]
> *For AI Coding Agents:* .NET 10.0 introduced new directives for file-based app.
> - SHALL use `#:project` to add project reference.
> - SHALL use `#:package` to add nuget package reference. (`*` or semantic version must be specified)
> - SHALL NOT use `#r` or other directives formerly used for *dotnet-script* (`.csx`) files.

```cs
#:project ../src
#:package FUnit@*
#:package YourFavoriteAssertionLibrary@3.1.0

return FUnit.Run(args, describe =>
{
    // Organize tests by test subject
    describe("Test Subject", it =>
    {
        // Expected result and test function
        it("should be ...", () =>
        {
            // Write actual test here: Arrange, Act, Assert
        });

        // Other test cases
        it("should not ...", async () =>
        {
            // Async test can also be written
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

// Collection assertion
Must.HaveSameSequence(expected, actual);
Must.HaveSameUnorderedElements(expected, actual);

// Text assertion
Must.ContainText(text, substring);
Must.NotContainText(text, substring);

// Exception assertion
Must.Throw<T>("Expected error message", () => Test());

// Conditional assertion
Must.BeTrue(x is >= 0 and < 10);

// ❌ Avoid asserting high-level conditions on collections
// ex Instead of checking if a value is absent, assert the full expected content
Must.BeTrue(!list.Contains(x));  // ✅ Prefer: Must.HaveSameSequence(expectedList, actualList)
```





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
dnx FUnit.Run -c Release

# ✅ Correct (with '--')
dnx FUnit.Run -- -c Release

# ✅ Shortcut: without '-c' is valid for Debug or Release
dnx FUnit.Run Release
```


## ⚙ Advanced Options

> [!IMPORTANT]
> *For AI Coding Agents:* SHALL NOT use advanced options unless explicitly requested.


### `--concurrency int` (Experimental)

```bash
# run max 10 tests simultaneously subject by subject
dotnet run ./tests/my-test.cs --concurrency 10
```





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
