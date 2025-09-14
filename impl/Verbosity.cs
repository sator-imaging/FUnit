namespace FUnitImpl
{
    /// <summary>
    /// Specifies the verbosity level for FUnit output.
    /// </summary>
    internal enum Verbosity
    {
        // to allow comparing verbosity level by <=, >= or etc.

        /// <summary>
        /// Output nothing except for critical errors.
        /// </summary>
        Quiet =   /**/ -2,
        /// <summary>
        /// Output minimal information.
        /// </summary>
        Minimal = /**/ -1,
        /// <summary>
        /// Output normal information (default).
        /// </summary>
        Normal =  /**/ 0,
        /// <summary>
        /// Output detailed information.
        /// </summary>
        Detailed,
        /// <summary>
        /// Output diagnostic information.
        /// </summary>
        Diagnostic,
    }
}
