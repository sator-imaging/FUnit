using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// NOTE: namespace 'FUnitImpl' should not be included in IntelliSense suggestion.
//       To achieve that, make all types in FUnitImpl internal and expose them
//       by this attribute.
//       --> type 'FUnit' in Sandbox project to verify FUnitImpl is not listed.
[assembly: InternalsVisibleTo("FUnit.Run")]  // for ConsoleLogger
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure")]

// these attributes are not required in Release build but should leave untouched
// to allow running test with '-c Release' option.
[assembly: InternalsVisibleTo("Must_test")]
[assembly: InternalsVisibleTo("Must_testDeepEquals")]
[assembly: InternalsVisibleTo("FUnit_test")]
[assembly: InternalsVisibleTo("FUnit_testAsync")]
