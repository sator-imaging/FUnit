using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace FUnit.Directives
{
    internal interface IDirectiveOperator
    {
        string DirectiveKeyword { get; }
        (string HintName, string? GeneratedCode, ImmutableList<Diagnostic> Diagnostics) Apply(string args, string sourceFilePath, Location location);
    }
}
