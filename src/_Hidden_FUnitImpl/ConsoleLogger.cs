// Licensed under the MIT License
// https://github.com/sator-imaging/FUnit

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
        public static bool EnableMarkdownOutput { get; set; }

#pragma warning disable SYSLIB1045 // Use GeneratedRegexAttribute to generate the regular expression implementation at compile time.
        private static readonly Regex re_markdownUnorderedList = new(
            @"(^(>[ ]+)?[ ]*([\*\-+]?([ ]+\[[ x]\])?)?[ ]+)|(^(>[ ]+(\[\![^\]]+\]([^\n]*\n)?)?))",
            RegexOptions.Compiled);

        private static readonly Regex re_markdownQuoteAwareTagCloser = new(
            @"(?<!^)>",
            RegexOptions.Compiled | RegexOptions.Multiline);
#pragma warning restore SYSLIB1045

        private static void Write(System.IO.TextWriter writer, bool useMarkdown, object? obj)
        {
            var message = obj?.ToString();
            if (message == null)
            {
                return;
            }

            if (useMarkdown)
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

            writer.Write(message);
        }

        private static void WriteLine(System.IO.TextWriter writer, bool useMarkdown, object? obj = null)
        {
            Write(writer, useMarkdown, obj);
            NewLine(writer);
        }

        private static void NewLine(System.IO.TextWriter writer)
        {
            writer.WriteLine();
        }

        private static void Color(System.IO.TextWriter writer, bool useMarkdown, string? ansiColor, object obj)
        {
            var message = obj?.ToString();

            // No!! --> Debug.Assert(message?.Contains('\n') != true);

            // fix for markdown
            int numTrailingSpaces = 0;
            if (message != null && useMarkdown)
            {
                numTrailingSpaces = message.Length - message.TrimEnd(' ').Length;
                if (numTrailingSpaces > 0)
                {
                    message = message.TrimEnd(' ');
                }

                var match = re_markdownUnorderedList.Match(message);
                if (match.Success)
                {
                    Write(writer, useMarkdown, match.Value);
                    message = message[match.Value.Length..];
                }
            }

            bool writeColor = ansiColor != null && message?.Length is > 0;

            // write color
            if (writeColor)
            {
                if (!useMarkdown)
                {
                    writer.Write(ansiColor);
                }
                else
                {
                    writer.Write(ansiColor switch
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
                Write(writer, useMarkdown, message);
            }

            // reset color
            if (writeColor)
            {
                if (!useMarkdown)
                {
                    writer.Write(SR.AnsiColorReset);
                }
                else
                {
                    writer.Write(SR.MarkdownColorReset);
                }

            }

            // trailing spaces
            if (numTrailingSpaces > 0)
            {
                Write(writer, useMarkdown, new string(' ', numTrailingSpaces));
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
                Console.Out.WriteLine(message);
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
                WriteLine(Console.Out, EnableMarkdownOutput, message);
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
                Color(Console.Out, EnableMarkdownOutput, SR.AnsiColorPassed, message);
                NewLine(Console.Out);
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
                Color(Console.Out, EnableMarkdownOutput, SR.AnsiColorFailed, message);
                NewLine(Console.Out);

                // failure should be also visible in standard error stream if stdout is redirected.
                if (Console.IsOutputRedirected)
                {
                    Color(Console.Error, false, SR.AnsiColorFailed, message);
                    NewLine(Console.Error);
                }
            }
        }
    }
}
