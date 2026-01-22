// Licensed under the MIT License
// https://github.com/sator-imaging/FUnit

using FUnitImpl;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

const string AnsiColorRed = "\u001b[97;41m";
const string AnsiColorYellow = "\u001b[97;43m";
const string AnsiColorReset = "\u001b[0m";


// --no-clean
bool noClean = false;
{
    const string ARG_NO_CLEAN = "--no-clean";

    int index = args.IndexOf(ARG_NO_CLEAN);
    if (index >= 0)
    {
        noClean = true;
        args = [.. args[..index], .. args[(index + 1)..]];
    }
}

// --warnings
bool showWarnings = false;
{
    const string ARG_WARNINGS = "--warnings";

    int index = args.IndexOf(ARG_WARNINGS);
    if (index >= 0)
    {
        showWarnings = true;
        args = [.. args[..index], .. args[(index + 1)..]];
    }
}

// NOTE: This logic separates arguments into options (and their values) and file glob patterns.
// It assumes that any argument starting with a hyphen is an option. Any subsequent argument not
// starting with a hyphen is treated as its value (e.g., "--threshold 1").
// Arguments like "--threshold -1" will fail because the parser will treat "-1" as a new option.
// All other arguments are considered glob patterns. Flags that require special handling
// (like "--no-clean") are processed before this loop.
var fileGlobs = new List<string>();
var remainingArgs = new List<string>();
for (int i = 0; i < args.Length; i++)
{
    string arg = args[i];
    if (arg.StartsWith("-"))
    {
        remainingArgs.Add(arg);
        
        // If the next argument doesn't start with a hyphen, it's a value for the current option
        if (i + 1 < args.Length)
        {
            if (!args[i + 1].StartsWith("-"))
            {
                remainingArgs.Add(args[i + 1]);
                i++; // Skip the next argument since we've already processed it
            }
        }
    }
    else
    {
        // ummmmm..... (FUnit allows specifying Release or Debug configuration without leading '-c')
        if (arg is SR.Flag_Debug or SR.Flag_Release)
        {
            remainingArgs.Add(arg);
        }
        else
        {
            fileGlobs.Add(arg);
        }
    }
}
var options = CommandLineOptions.Parse([.. remainingArgs]);

#if DEBUG
if (args.Contains(SR.Flag_TEST))
{
    RunAllTests();
    return 0;
}
#endif

const string FUnit = nameof(FUnit);
const int MinimumRequiredDotnetVersion = 10;
byte[] utf8Bom = [0xef, 0xbb, 0xbf];

if (EnsureEnvironment() != 0)
{
    Environment.Exit(1);
}

// Collect all files with *.cs file extension (case insensitive)
// Assuming the tool should search in the current directory and its subdirectories
string currentDirectory = Directory.GetCurrentDirectory();
var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
if (fileGlobs.Count == 0)
{
    matcher.AddInclude("**/*test*.cs");
}
else
{
    matcher.AddIncludePatterns(fileGlobs);
}
var csFiles = matcher.GetResultsInFullPath(currentDirectory).Where(File.Exists).ToList();

List<string> validFUnitFiles = [];

foreach (string file in csFiles)
{
    byte[] fileBytes = File.ReadAllBytes(file);

    string fileContent = Encoding.UTF8.GetString(fileBytes);
    bool startsWithHash = fileContent.StartsWith('#');

    // Check file content starts with a character '#'
    // if not, try to remove UTF-8 BOM and try step 3 again (once only)
    if (!startsWithHash)
    {
        // Check for UTF-8 BOM
        if (fileBytes.Length >= 3 && fileBytes[0] == utf8Bom[0] && fileBytes[1] == utf8Bom[1] && fileBytes[2] == utf8Bom[2])
        {
            fileContent = Encoding.UTF8.GetString(fileBytes, 3, fileBytes.Length - 3);
            startsWithHash = fileContent.StartsWith('#');
        }
    }

    // Check file content contains word 'FUnit' (case sensitive)
    if (startsWithHash && fileContent.Contains(FUnit, StringComparison.Ordinal))
    {
        validFUnitFiles.Add(file);
    }
}

if (validFUnitFiles.Count > 0)
{
    int failedSuiteCount = 0;
    int failedTestCaseCount = 0;

    bool needNewLine = !ConsoleLogger.EnableMarkdownOutput;
    int currentNumber = 0;
    foreach (string filePath in validFUnitFiles)
    {
        currentNumber++;

        if (needNewLine)
        {
            ConsoleLogger.LogInfo();
        }
        needNewLine = true;

        var relFilePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath).Replace('\\', '/');

        ConsoleLogger.LogInfo($"# 🔬 `{relFilePath}` ({currentNumber} of {validFUnitFiles.Count})");

        var exitCode = await ExecuteTestAsync(filePath, [.. remainingArgs], noClean, showWarnings);
        if (exitCode != 0)
        {
            failedSuiteCount++;
        }
        else
        {
            failedTestCaseCount += exitCode;
        }
    }

    ConsoleLogger.LogInfo();
    ConsoleLogger.LogInfo();
    ConsoleLogger.LogInfo($"# {FUnit} Result");

    if (failedSuiteCount == 0 && failedTestCaseCount == 0)
    {
        ConsoleLogger.LogPassed($"{SR.MarkdownPassed} Total {validFUnitFiles.Count} test suites successfully completed");
    }
    else
    {
        ConsoleLogger.LogInfo();
        ConsoleLogger.LogFailed($"> [!CAUTION]");

        if (failedSuiteCount > 0)
        {
            ConsoleLogger.LogFailed($"> {SR.MarkdownFailed} {failedSuiteCount} of {validFUnitFiles.Count} test suites were failed to run");
        }
        if (failedTestCaseCount > 0)
        {
            ConsoleLogger.LogFailed($"> {SR.MarkdownFailed} Total {failedTestCaseCount} test cases were failed");
        }

        Environment.Exit(1);
    }
}
else
{
    ConsoleLogger.LogInfo();
    ConsoleLogger.LogFailed($"> [!CAUTION]");
    var patterns = fileGlobs.Count > 0 ? string.Join(", ", fileGlobs) : "**/*test*.cs";
    ConsoleLogger.LogFailed($"> No valid {FUnit} test files found matching the criteria: `{patterns}`");

    Environment.Exit(1);
}





return 0;





int EnsureEnvironment()
{
    if (!ConsoleLogger.EnableMarkdownOutput)
    {
        ConsoleLogger.LogInfo("Checking .NET SDK version...");
    }

    ProcessStartInfo startInfo = new()
    {
        FileName = "dotnet",
        Arguments = "--version",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using Process? process = Process.Start(startInfo);
    if (process == null)
    {
        ConsoleLogger.LogInfo();
        ConsoleLogger.LogFailed("> [!CAUTION]");
        ConsoleLogger.LogFailed("> Error: 'dotnet' command could not be started. Please ensure .NET SDK is installed and 'dotnet' command is accessible in your system's PATH.");
        return 1;
    }

    process.WaitForExit();

    string output = process.StandardOutput.ReadToEnd().Trim();

    if (string.IsNullOrEmpty(output))
    {
        ConsoleLogger.LogInfo();
        ConsoleLogger.LogFailed("> [!CAUTION]");
        ConsoleLogger.LogFailed("> Error: 'dotnet --version' returned empty output.");
        return 1;
    }

    string[] versionParts = output.Split('.');
    if (versionParts.Length == 0)
    {
        ConsoleLogger.LogInfo();
        ConsoleLogger.LogFailed($"> [!CAUTION]");
        ConsoleLogger.LogFailed($"> Error: Could not parse .NET SDK version from output: '{output}' (no dot found).");
        return 1;
    }

    string majorVersionString = versionParts[0];
    if (int.TryParse(majorVersionString, out int majorVersion))
    {
        if (majorVersion < MinimumRequiredDotnetVersion)
        {
            ConsoleLogger.LogInfo();
            ConsoleLogger.LogFailed($"> [!CAUTION]");
            ConsoleLogger.LogFailed($"> Error: .NET SDK major version {output} is less than the required {MinimumRequiredDotnetVersion}.");
            ConsoleLogger.LogFailed($"> Please update your .NET SDK to version {MinimumRequiredDotnetVersion} or higher.");
            return 1;
        }

        if (!ConsoleLogger.EnableMarkdownOutput)
        {
            ConsoleLogger.LogInfo($".NET SDK major version {output} meets the requirement (>= {MinimumRequiredDotnetVersion}).");
        }
    }
    else
    {
        ConsoleLogger.LogInfo();
        ConsoleLogger.LogFailed($"> [!CAUTION]");
        ConsoleLogger.LogFailed($"> Error: Could not parse .NET SDK major version from output: '{output}'");

        return 1;
    }

    return 0;
}

static string BuildEscapedArguments(string[] args)
{
    var sb = new StringBuilder();

    for (int i = 0; i < args.Length; i++)
    {
        var arg = args[i];

        if (arg.Any(c => c is ' ' or '\\' or '"' or '\'' || !char.IsAscii(c)))
        {
            arg = $"\"{arg.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
        }

        if (sb.Length > 0)
        {
            _ = sb.Append(' ');
        }
        _ = sb.Append(arg);
    }

    return sb.ToString();
}


// TODO: can run tests simultaneously but console log will be messed up.
async ValueTask<int> ExecuteTestAsync(string filePath, string[] args, bool noClean, bool showWarnings)
{
    var escapedArguments = BuildEscapedArguments(args);

    ConsoleLogger.LogInfo($"Arguments: `{(args.Length == 0 ? "<NOTHING>" : escapedArguments)}`  ");

    var subCommandOptions = BuildEscapedArguments(["-c", options.BuildConfiguration, filePath]);

    // restore
    {
        int exitCode = await RunDotnetAsync(
            $"restore {BuildEscapedArguments([filePath])}",
            "",
            requireStdOutLogging: false,
            requireDetailsTag: true,
            addNoWarn: false);

        if (exitCode != 0)
        {
            ConsoleLogger.LogInfo();
            ConsoleLogger.LogFailed($"> [!CAUTION]");
            ConsoleLogger.LogFailed($"> Error: 'dotnet restore' command failed with exit code {exitCode}.");

            return exitCode;
        }
    }

    // clean
    if (!noClean)
    {
        int exitCode = await RunDotnetAsync(
            $"clean {subCommandOptions}",
            "",
            requireStdOutLogging: false,
            requireDetailsTag: true,
            addNoWarn: false);

        if (exitCode != 0)
        {
            ConsoleLogger.LogInfo();
            ConsoleLogger.LogFailed($"> [!CAUTION]");
            ConsoleLogger.LogFailed($"> Error: 'dotnet clean' command failed with exit code {exitCode}.");

            return exitCode;
        }
    }

    // build
    {
        var exitCode = await RunDotnetAsync(
            $"build {subCommandOptions}",
            "",
            requireStdOutLogging: true,
            requireDetailsTag: true,
            addNoWarn: !showWarnings);

        if (exitCode != 0)
        {
            ConsoleLogger.LogInfo();
            ConsoleLogger.LogFailed($"> [!CAUTION]");
            ConsoleLogger.LogFailed($"> Error: 'dotnet build' command failed with exit code {exitCode}.");

            return exitCode;
        }
    }

    // run
    {
        var exitCode = await RunDotnetAsync(
            $"run {subCommandOptions} --no-build",
            escapedArguments,
            requireStdOutLogging: true,
            requireDetailsTag: false,
            addNoWarn: !showWarnings);

        if (exitCode != 0)
        {
            ConsoleLogger.LogInfo();
            ConsoleLogger.LogFailed($"> [!CAUTION]");
            ConsoleLogger.LogFailed($"> Error: 'dotnet run' command failed with exit code {exitCode}.");
        }

        return exitCode;
    }
}

async ValueTask<int> RunDotnetAsync(
    string subCommand,
    string arguments,
    bool requireStdOutLogging,
    bool requireDetailsTag,
    bool addNoWarn = false)
{
    var subCommandWithoutFilePath = string.Join(" ", subCommand.Split(' ').Take(3));
    if (addNoWarn)
    {
        subCommand += " -p:NoWarn=\\\"*\\\"";
    }
    arguments = subCommand + (string.IsNullOrWhiteSpace(arguments) ? string.Empty : $" -- {arguments}");

    if (!ConsoleLogger.EnableMarkdownOutput)
    {
        ConsoleLogger.LogInfo($"> dotnet {arguments}");
    }
    else
    {
        if (requireDetailsTag)
        {
            ConsoleLogger.LogInfoRaw();
            ConsoleLogger.LogInfoRaw($"<details><summary>dotnet {subCommandWithoutFilePath}</summary>");
            ConsoleLogger.LogInfoRaw();
        }
        else
        {
            ConsoleLogger.LogInfoRaw();
            ConsoleLogger.LogInfoRaw();
        }

        ConsoleLogger.LogInfo($"```bash");
        ConsoleLogger.LogInfo($"dotnet {arguments}");
        ConsoleLogger.LogInfo($"```");

        if (requireDetailsTag)
        {
            ConsoleLogger.LogInfoRaw();
            ConsoleLogger.LogInfoRaw("```");
        }
    }

    using var proc = new Process()
    {
        StartInfo = new()
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            FileName = "dotnet",
            Arguments = $"{arguments}",
            CreateNoWindow = true,
        },
    };

    var callCounts = new ProcessCallbackCallCounts();

    proc.ErrorDataReceived += (sender, args) =>
    {
        if (args.Data != null)
        {
            Interlocked.Increment(ref callCounts.Error);
            Console.Error.WriteLine(Colorize(args.Data));  // DO NOT use ConsoleLogger here!
        }
    };

    if (requireStdOutLogging)
    {
        proc.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                Interlocked.Increment(ref callCounts.Stdout);
                Console.WriteLine(Colorize(args.Data));  // DO NOT use ConsoleLogger here!
            }
        };
    }

    if (!proc.Start())
    {
        ConsoleLogger.LogInfo();
        ConsoleLogger.LogFailed("> [!CAUTION]");
        ConsoleLogger.LogFailed("> Error: 'dotnet' command could not be started. Please ensure .NET SDK is installed and 'dotnet' command is accessible in your system's PATH.");

        return -1;
    }

    proc.BeginErrorReadLine();
    proc.BeginOutputReadLine();

    await proc.WaitForExitAsync();

    if (ConsoleLogger.EnableMarkdownOutput)
    {
        if (requireDetailsTag)
        {
            if (callCounts.Stdout == 0 && callCounts.Error == 0)
            {
                ConsoleLogger.LogInfoRaw($"[{FUnit}] 🎉 No errors or warnings!");
            }

            ConsoleLogger.LogInfoRaw("```");
            ConsoleLogger.LogInfoRaw();
            ConsoleLogger.LogInfoRaw("</details>");

            if (callCounts.Stdout > 0 || callCounts.Error > 0)
            {
                ConsoleLogger.LogInfoRaw();
                ConsoleLogger.LogInfoRaw($"> [!WARNING]");
                ConsoleLogger.LogInfoRaw($"> `{subCommandWithoutFilePath}` has stdout: **{callCounts.Stdout}** error: **{callCounts.Error}**");
            }
        }
    }

    return proc.ExitCode;
}


#if DEBUG
static void RunAllTests()
{
    Debug.Assert(BuildEscapedArguments([@" "]) == @""" """, "space");
    Debug.Assert(BuildEscapedArguments([@"\"]) == @"""\\""", "back slash");
    Debug.Assert(BuildEscapedArguments([@""""]) == @"""\""""", "double quote");
    Debug.Assert(BuildEscapedArguments([@"'"]) == @"""'""", "single quote");

    var complex = @"Hello, \"" ' \ "" world.";
    var expected = @"""Hello, \\\"" ' \\ \"" world.""";
    var actual = BuildEscapedArguments([complex]);

    Debug.Assert(actual == expected, $"'{actual}' != '{expected}'");

    ConsoleLogger.LogInfo($"Complex string escape result:");
    ConsoleLogger.LogInfo($"  source: {complex}");
    ConsoleLogger.LogInfo($"  result: {actual}");
    ConsoleLogger.LogPassed("All tests successfully completed");
}
#endif


static string Colorize(string message)
{
    if (string.IsNullOrWhiteSpace(message))
    {
        return message;
    }

    var ansiEscapeIndex = message.IndexOf('\u001b');
    if (ansiEscapeIndex == -1)
    {
        message = message.Replace("error", $"{AnsiColorRed}error{AnsiColorReset}", StringComparison.OrdinalIgnoreCase);
        message = message.Replace("warning", $"{AnsiColorYellow}warning{AnsiColorReset}", StringComparison.OrdinalIgnoreCase);
    }
    else
    {
        var head = message[..ansiEscapeIndex];
        var tail = message[ansiEscapeIndex..];
        head = head.Replace("error", $"{AnsiColorRed}error{AnsiColorReset}", StringComparison.OrdinalIgnoreCase);
        head = head.Replace("warning", $"{AnsiColorYellow}warning{AnsiColorReset}", StringComparison.OrdinalIgnoreCase);
        message = head + tail;
    }
    return message;
}
file sealed class ProcessCallbackCallCounts
{
    public volatile int Error;
    public volatile int Stdout;
}
