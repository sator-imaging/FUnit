// Licensed under the MIT License
// https://github.com/sator-imaging/FUnit

using System;

namespace FUnitImpl
{
    /// <summary>
    /// Represents an exception specific to the FUnit test framework.
    /// </summary>
    internal class FUnitException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FUnitException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public FUnitException(string message, Exception? inner = null) : base(message, inner)
        { }
    }
}
