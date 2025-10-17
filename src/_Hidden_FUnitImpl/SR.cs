using System;
using System.Diagnostics;

namespace FUnitImpl
{
    internal static class SR
    {
        public const string Flag_Iterations = "--iterations";
        public const string Flag_Concurrency = "--concurrency";
        public const string Flag_MarkdownOutput = "--markdown";
        public const string Flag_MarkdownOutputShort = "-md";
        public const string Flag_TEST = "--TEST";
        public const string Flag_Configuration = "--configuration";
        public const string Flag_ConfigurationShort = "-c";
        public const string Flag_Debug = "Debug";
        public const string Flag_Release = "Release";
        public const string Flag_Delimiter = "--";

        public const string EmojiPassed = "✓";
        public const string EmojiFailed = "×";

        public const string EmojiPassedGitHub = "✅";
        public const string EmojiFailedGitHub = "⛔";

        public const string MarkdownPassed = "- [x] " + EmojiPassed;
        public const string MarkdownFailed = "- [ ] " + EmojiFailed;

        public const string AnsiColorFailed = "\u001b[31m";  // red
        public const string AnsiColorPassed = "\u001b[32m";  // green
        public const string AnsiColorReset = "\u001b[0m";

        public const string MarkdownColorFailed = "<span style='color:red'>";
        public const string MarkdownColorPassed = "<span style='color:green'>";
        public const string MarkdownColorReset = "</span>";

        public const int IndentationAdjustment = 6;

        public const string FlakyTestResultAnnotation = "🆘 Flaky Test Detected: Inconsistent results across multiple runs";


        #region  GetElapsedTime(long) & GetElapsedTime(long, long)

        // https://github.com/dotnet/runtime/blob/v9.0.2/src/libraries/Microsoft.Extensions.Http/src/ValueStopwatch.cs#L11
        private static readonly double s_timestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        /// <summary>
        /// Calculates the elapsed time since a given starting timestamp.
        /// </summary>
        /// <param name="startingTimestamp">The starting timestamp obtained from <see cref="Stopwatch.GetTimestamp"/>.</param>
        /// <returns>A <see cref="TimeSpan"/> representing the elapsed time.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static TimeSpan GetElapsedTime(long startingTimestamp)
        {
            long endingTimestamp = Stopwatch.GetTimestamp();
            return new TimeSpan((long)((endingTimestamp - startingTimestamp) * s_timestampToTicks));
        }

        /// <summary>
        /// Calculates the elapsed time between a starting and ending timestamp.
        /// </summary>
        /// <param name="startingTimestamp">The starting timestamp obtained from <see cref="Stopwatch.GetTimestamp"/>.</param>
        /// <param name="endingTimestamp">The ending timestamp obtained from <see cref="Stopwatch.GetTimestamp"/>.</param>
        /// <returns>A <see cref="TimeSpan"/> representing the elapsed time.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
        {
            return new TimeSpan((long)((endingTimestamp - startingTimestamp) * s_timestampToTicks));
        }

        #endregion
    }
}
