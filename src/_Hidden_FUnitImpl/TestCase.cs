using System;
using System.Threading;
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
        /// Gets the number of times the test case has been executed.
        /// </summary>
        public int ExecutionCount => this.b_executionCount;
        private volatile int b_executionCount;

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
        /// <returns>An <see cref="Exception"/> if the test fails, otherwise null.</returns>
        public async Task<Exception?> ExecuteAsync(CancellationToken cancellationToke = default)
        {
            if (cancellationToke.IsCancellationRequested)
            {
                return null;
            }

            Exception? error = null;

            try
            {
                _ = Interlocked.Increment(ref this.b_executionCount);

                object? result = null;

                switch (this.testFunction)
                {
#pragma warning disable format
#pragma warning disable CA2012  // Use ValueTasks correctly
#pragma warning disable IDE2001
                    case Action test:          { test.Invoke(); break; }
                    case Func<Task> test:      { result = test.Invoke(); break; }
                    case Func<ValueTask> test: { result = test.Invoke(); break; }
                    // to allow assignment only test --> () => x = 310;
                    case Func<byte> test:      { test.Invoke(); break; }
                    case Func<sbyte> test:     { test.Invoke(); break; }
                    case Func<short> test:     { test.Invoke(); break; }
                    case Func<ushort> test:    { test.Invoke(); break; }
                    case Func<int> test:       { test.Invoke(); break; }
                    case Func<uint> test:      { test.Invoke(); break; }
                    case Func<long> test:      { test.Invoke(); break; }
                    case Func<ulong> test:     { test.Invoke(); break; }
                    case Func<float> test:     { test.Invoke(); break; }
                    case Func<double> test:    { test.Invoke(); break; }
                    case Func<decimal> test:   { test.Invoke(); break; }
                    case Func<nint> test:      { test.Invoke(); break; }
                    case Func<nuint> test:     { test.Invoke(); break; }
                    case Func<TimeSpan> test:  { test.Invoke(); break; }
                    case Func<DateTime> test:  { test.Invoke(); break; }
                    case Func<DateTimeOffset> test: { test.Invoke(); break; }
                    case Func<Guid> test:      { test.Invoke(); break; }
                    case Func<object> test:    { test.Invoke(); break; }
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
            }
            catch (Exception ex)
            {
                error = ex;
            }

            return error;
        }
    }
}
