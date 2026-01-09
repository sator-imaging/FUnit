// Licensed under the MIT License
// https://github.com/sator-imaging/FUnit

using FUnitImpl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// --filter <string>
const string AnsiColorRed = "\u001b[31m";
const string AnsiColorYellow = "\u001b[33m";
const string AnsiColorReset = "\u001b[0m";
string fileFilter = "*test*.cs";
{
    const string ARG_FILTER = "--filter";

    int index = args.IndexOf(ARG_FILTER);
    if (index >= 0)
    {
        if (index == args.Length - 1)
        {
            ConsoleLogger.LogFailed($"> [!CAUTION]");
            ConsoleLogger.LogFailed($"> `{ARG_FILTER}` takes a string parameter.");
            Environment.Exit(1);
        }

        fileFilter = args[index + 1];
        if (!fileFilter.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            fileFilter += ".cs";
        }

        // remove filter arguments
        args = [.. args[..index], .. args[(index + 2)..]];
    }
}

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

// then, parse shared args
var options = CommandLineOptions.Parse(args);

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
List<string> csFiles = [.. Directory.GetFiles(currentDirectory, fileFilter, new EnumerationOptions{
    IgnoreInaccessible = true,
    MatchCasing = MatchCasing.CaseInsensitive,
    RecurseSubdirectories = true,
    ReturnSpecialDirectories = false,
})];

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

        var exitCode = await ExecuteTestAsync(filePath, args, noClean);
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
    ConsoleLogger.LogFailed($"> No valid {FUnit} test files found matching the criteria: `{fileFilter}`");

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
async ValueTask<int> ExecuteTestAsync(string filePath, string[] args, bool noClean)
{
    var escapedArguments = BuildEscapedArguments(args);

    ConsoleLogger.LogInfo($"Arguments: `{(args.Length == 0 ? "<NOTHING>" : escapedArguments)}`  ");

    var subCommandOptions = BuildEscapedArguments(["-c", options.BuildConfiguration, filePath]);

    // clean
    if (!noClean)
    {
        int exitCode = await RunDotnetAsync(
            $"clean {subCommandOptions}",
            "",
            requireStdOutLogging: false,
            requireDetails: true);

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
            requireDetails: true);

        if (exitCode != 0)
        {
            ConsoleLogger.LogInfo();
            ConsoleLogger.LogFailed($"> [!CAUTION]");
            ConsoleLogger.LogFailed($"> Error: 'dotnet run' command failed with exit code {exitCode}.");

            return exitCode;
        }
    }

    // run
    {
        var exitCode = await RunDotnetAsync(
            $"run {subCommandOptions} --no-build",
            escapedArguments,
            requireStdOutLogging: true,
            requireDetails: false);

        if (exitCode != 0)
        {
            ConsoleLogger.LogInfo();
            ConsoleLogger.LogFailed($"> [!CAUTION]");
            ConsoleLogger.LogFailed($"> Error: 'dotnet run' command failed with exit code {exitCode}.");
        }

        return exitCode;
    }
}

async ValueTask<int> RunDotnetAsync(string subCommand, string arguments, bool requireStdOutLogging, bool requireDetails)
{
    var subCommandWithoutFilePath = string.Join(" ", subCommand.Split(' ').Take(3));
    arguments = subCommand + (string.IsNullOrWhiteSpace(arguments) ? string.Empty : $" -- {arguments}");

    if (!ConsoleLogger.EnableMarkdownOutput)
    {
        ConsoleLogger.LogInfo($"> dotnet {arguments}");
    }
    else
    {
        if (requireDetails)
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

        if (requireDetails)
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
            callCounts.Error++;
            Console.Error.WriteLine(Colorize(args.Data));  // DO NOT use ConsoleLogger here!
        }
    };

    if (requireStdOutLogging)
    {
        proc.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                callCounts.Stdout++;
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
        if (requireDetails)
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
    if (string.IsNullOrEmpty(message))
    {
        return message;
    }

    message = message.Replace("error", $"{AnsiColorRed}error{AnsiColorReset}", StringComparison.OrdinalIgnoreCase);
    message = message.Replace("warning", $"{AnsiColorYellow}warning{AnsiColorReset}", StringComparison.OrdinalIgnoreCase);
    return message;
}
file sealed class ProcessCallbackCallCounts
{
    public int Error;
    public int Stdout;
}
