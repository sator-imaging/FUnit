using System;
using System.Threading.Tasks;

namespace FUnitImpl
{
    /// <summary>
    /// Represents a single test case to be executed.
    /// </summary>
    internal sealed class TestCase
    {
        /// <summary>
        /// Gets the subject of the test case.
        /// </summary>
        public string Subject { get; }
        /// <summary>
        /// Gets the description of the test case.
        /// </summary>
        public string Description { get; }
        private readonly Delegate testFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCase"/> class.
        /// </summary>
        /// <param name="subject">The subject of the test case.</param>
        /// <param name="description">The description of the test case.</param>
        /// <param name="testFunction">The delegate representing the test function to execute.</param>
        public TestCase(string subject, string description, Delegate testFunction)
        {
            this.Subject = subject;
            this.Description = description;
            this.testFunction = testFunction;
        }

        /// <summary>
        /// Executes the test case asynchronously.
        /// </summary>
        /// <returns>A <see cref="FailedTestCase"/> if the test fails, otherwise null.</returns>
        public async Task<FailedTestCase?> ExecuteAsync()
        {
            FailedTestCase? failedCase = null;

            try
            {
                object? result = null;

                switch (this.testFunction)
                {
#pragma warning disable format
#pragma warning disable CA2012  // Use ValueTasks correctly
#pragma warning disable IDE2001
                    case Action test:          { test.Invoke(); break; }
                    case Func<Task> test:      { result = test.Invoke(); break; }
                    case Func<ValueTask> test: { result = test.Invoke(); break; }
#pragma warning restore IDE2001
#pragma warning restore CA2012
#pragma warning restore format

                    default:
                        {
                            var funcType = this.testFunction.GetType();
                            if (funcType.IsGenericType &&
                                funcType.GetGenericTypeDefinition() == typeof(Func<>) &&
                                funcType.GenericTypeArguments[0].IsGenericType &&
                                funcType.GenericTypeArguments[0].GetGenericTypeDefinition() == typeof(ValueTask<>))
                            {
                                throw new FUnitException("ValueTask<T>-returning async method is not supported.");
                            }
                            else
                            {
                                throw new FUnitException($"Unsupported delegate type: {funcType}");
                            }
                        }
                }

                if (result is Task task)
                {
                    await task;
                }
                else if (result is ValueTask valueTask)
                {
                    await valueTask;
                }

                ConsoleLogger.LogPassed($"  {SR.MarkdownPassed} {this.Description}");
            }
            catch (Exception ex)
            {
                failedCase = new FailedTestCase(this.Subject, this.Description, ex);
                ConsoleLogger.LogFailedTestCase($"  {SR.MarkdownFailed} ", failedCase, SR.AnsiColorFailed);
            }

            return failedCase;
        }
    }
}
