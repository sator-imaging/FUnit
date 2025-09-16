using System;
using System.Text.RegularExpressions;

namespace FUnitImpl
{
    // NOTE: FUnit writes markdown-formatted logs to console to allow outputting formatted result in
    //       GitHub Actions page easily by simple '>> $GITHUB_STEP_SUMMARY' redirect.
    //       As stderr only log is not meaningful, we write both passed and failed message to stdout.
    //       --> don't say "use '&>' or '&>>' instead".
    internal static class ConsoleLogger
    {
        /// <summary>
        /// Gets or sets a value indicating whether markdown output is enabled.
        /// </summary>
        public static bool EnableMarkdownOutput { get; set; } = false;

#pragma warning disable SYSLIB1045 // Use GeneratedRegexAttribute to generate the regular expression implementation at compile time.
        private static readonly Regex re_markdownUnorderedList = new(
            @"(^(>[ ]+)?[ ]*([\*\-+]?([ ]+\[[ x]\])?)?[ ]+)|(>[ ]+)",
            RegexOptions.Compiled);

        private static readonly Regex re_markdownQuoteAwareTagCloser = new(
            @"(?<!^)>",
            RegexOptions.Compiled | RegexOptions.Multiline);
#pragma warning restore SYSLIB1045

        private static void Write(object? obj)
        {
            var message = obj?.ToString();
            if (message == null)
            {
                return;
            }

            if (EnableMarkdownOutput)
            {
                // As GitHub doesn't allow changing text color, use more eye-catching emojis.
                message = re_markdownQuoteAwareTagCloser.Replace(message, "&gt;")
                    .Replace("<", "&lt;", StringComparison.Ordinal)
                    .Replace(SR.EmojiPassed, SR.EmojiPassedGitHub, StringComparison.Ordinal)
                    .Replace(SR.EmojiFailed, SR.EmojiFailedGitHub, StringComparison.Ordinal)
                    ;
            }

            // always!!
            message = message.Replace("\n", $"\n{new string(' ', SR.IndentationAdjustment)}", StringComparison.Ordinal);

            Console.Write(message);
        }

        private static void WriteLine(object? obj = null)
        {
            Write(obj);
            NewLine();
        }

        private static void NewLine()
        {
            Console.WriteLine();
        }

        private static void Color(string? ansiColor, object obj)
        {
            var message = obj?.ToString();

            // No!! --> Debug.Assert(message?.Contains('\n') != true);

            // fix for markdown
            int numTrailingSpaces = 0;
            if (message != null)
            {
                numTrailingSpaces = message.Length - message.TrimEnd(' ').Length;
                if (numTrailingSpaces > 0)
                {
                    message = message.TrimEnd(' ');
                }

                var match = re_markdownUnorderedList.Match(message);
                if (match.Success)
                {
                    Write(match.Value);
                    message = message[match.Value.Length..];
                }
            }

            // write color
            if (ansiColor != null)
            {
                if (!EnableMarkdownOutput)
                {
                    Console.Write(ansiColor);
                }
                else
                {
                    Console.Write(ansiColor switch
                    {
                        SR.AnsiColorFailed => SR.MarkdownColorFailed,
                        SR.AnsiColorPassed => SR.MarkdownColorPassed,
                        SR.AnsiColorReset or _ => SR.MarkdownColorReset,
                    });
                }
            }

            // write remaining message
            if (message != null)
            {
                Write(message);
            }

            // reset color
            if (ansiColor != null)
            {
                if (!EnableMarkdownOutput)
                {
                    Console.Write(SR.AnsiColorReset);
                }
                else
                {
                    Console.Write(SR.MarkdownColorReset);
                }

            }

            // trailing spaces
            if (numTrailingSpaces > 0)
            {
                Write(new string(' ', numTrailingSpaces));
            }
        }

#pragma warning disable IDE0330 // Prefer 'System.Threading.Lock'
        // NOTE: required to fix console log racing.
        //       --> ex) color may not be applied expectedly in concurrent execution.
        // TODO: is there better way?
        private static readonly object sync = new();
#pragma warning restore IDE0330

        /// <summary>
        /// Logs a message directly to the console without any formatting or markdown escaping.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogInfoRaw(object? message = null)
        {
            lock (sync)
            {
                Console.WriteLine(message);
            }
        }

        /// <summary>
        /// Logs an informational message to the console, applying markdown escaping if enabled.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogInfo(object? message = null)
        {
            lock (sync)
            {
                WriteLine(message);
            }
        }

        /// <summary>
        /// Logs a passed test message to the console, applying appropriate coloring/markdown.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogPassed(object message)
        {
            lock (sync)
            {
                Color(SR.AnsiColorPassed, message);
                NewLine();
            }
        }

        /// <summary>
        /// Logs a failed test message to the console, applying appropriate coloring/markdown.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void LogFailed(object message)
        {
            lock (sync)
            {
                Color(SR.AnsiColorFailed, message);
                NewLine();
            }
        }

        /// <summary>
        /// Logs details of a failed test case to the console.
        /// </summary>
        /// <param name="prefix">A prefix to prepend to the log message.</param>
        /// <param name="detail">The <see cref="FailedTestCase"/> object containing details of the failure.</param>
        /// <param name="ansiColor">Optional ANSI color code to apply to the message.</param>
        public static void LogFailedTestCase(string prefix, FailedTestCase detail, string? ansiColor = null)
        {
            lock (sync)
            {
                Color(ansiColor, $"{prefix}{detail.Description} - {detail.Error.Message}");
                NewLine();
            }
        }
    }
}
