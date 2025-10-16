using System;
using System.Text;

namespace FUnitImpl
{
    // TODO: record is not required for FUnit use.
    //       --> currently, just use to beautify ToString output.

    /// <summary>
    /// Represents the command line options for the FUnit test runner.
    /// </summary>
    /// <param name="ConcurrencyLevel">Maximum number of tests run simultaneously in each test subject.</param>
    /// <param name="Verbosity">The verbosity level of the test output.</param>
    /// <param name="BuildConfiguration">The build configuration (e.g., "Debug" or "Release").</param>
    /// <param name="Iterations">The number of times to run each test case.</param>
    internal sealed record CommandLineOptions(
        int ConcurrencyLevel = 1,
        Verbosity Verbosity = Verbosity.Normal,
        string BuildConfiguration = "Debug",
        int Iterations = 3
    )
    {
        // NOTE: BuildConfiguration should be hidden from log because it may be different from
        //       actual build configuration if command line flag '-c Release' is used incorrectly.
        //       (ex. missing '--' delimiter)
        internal string BuildConfiguration { get; private set; } = BuildConfiguration;

        private int b_concurrencyLevel = ConcurrencyLevel;
        public int ConcurrencyLevel
        {
            get => this.b_concurrencyLevel;
            private set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.ConcurrencyLevel), $"Argument must be greater than 0: {value}");
                }

                this.b_concurrencyLevel = value;
            }
        }

        private int b_iterations = Iterations;
        public int Iterations
        {
            get => this.b_iterations;
            private set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(this.Iterations), $"Argument must be greater than 0: {value}");
                }

                this.b_iterations = value;
            }
        }


        /// <summary>
        /// Parses the command line arguments and returns a <see cref="CommandLineOptions"/> object.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>A <see cref="CommandLineOptions"/> object populated with the parsed arguments.</returns>
        public static CommandLineOptions Parse(string[] args)
        {
            var ret = new CommandLineOptions();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case SR.Flag_Iterations:
                        {
                            i++;
                            if (i < args.Length)
                            {
                                if (int.TryParse(args[i], out var iterations))
                                {
                                    ret.Iterations = iterations;
                                    continue;
                                }
                            }
                        }
                        throw new ArgumentException($"'{args[i - 1]}' takes 1 positive integer parameter");

                    case SR.Flag_Concurrency:
                        {
                            i++;
                            if (i < args.Length)
                            {
                                if (int.TryParse(args[i], out var concurrency))
                                {
                                    ret.ConcurrencyLevel = concurrency;
                                    continue;
                                }
                            }
                        }
                        throw new ArgumentException($"'{args[i - 1]}' takes 1 positive integer parameter");

                    case SR.Flag_Configuration:
                    case SR.Flag_ConfigurationShort:
                        {
                            i++;
                            if (i < args.Length)
                            {
                                ret.BuildConfiguration = args[i];
                                continue;
                            }
                        }
                        throw new ArgumentException($"'{args[i - 1]}' takes 1 string parameter");

                    case SR.Flag_MarkdownOutput:
                    case SR.Flag_MarkdownOutputShort:
                        {
                            ConsoleLogger.EnableMarkdownOutput = true;
                            continue;
                        }

                    case SR.Flag_Debug:
                    case SR.Flag_Release:
                        {
                            ret.BuildConfiguration = args[i];
                            continue;
                        }

                    case SR.Flag_Delimiter:
                    case SR.Flag_TEST:
                        {
                            continue;
                        }

                    default:
                        break;
                }

                throw new ArgumentException($"Unknown command line option: {args[i]}");
            }

            return ret;
        }


        /// <summary>
        /// Returns a string representation of the command line options.
        /// </summary>
        /// <returns>A string containing the command line options.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            return !this.PrintMembers(sb)
                ? nameof(CommandLineOptions)
                : sb.ToString();
        }
    }
}
