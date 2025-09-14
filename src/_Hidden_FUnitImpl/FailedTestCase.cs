using System;

namespace FUnitImpl
{
    /// <summary>
    /// Represents a test case that failed during execution.
    /// </summary>
    /// <param name="TestSubject">The subject of the test.</param>
    /// <param name="Description">The description of the test case.</param>
    /// <param name="Error">The exception that caused the test to fail.</param>
    /// <param name="IsSystemError">Indicates if the error is a system error (e.g., in test setup) rather than a test assertion failure.</param>
    internal sealed record FailedTestCase(
        string TestSubject,
        string Description,
        Exception Error,
        bool IsSystemError = false
    )
    { }
}
