// Licensed under the MIT License
// https://github.com/sator-imaging/FUnit

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace FUnit.Directives
{
    internal interface IDirectiveOperator
    {
        string DirectiveKeyword { get; }
        void Setup();
        void Cleanup();
        (string HintName, string? GeneratedCode, ImmutableList<Diagnostic> Diagnostics) Apply(string args, string sourceFilePath, Location location);
    }
}
