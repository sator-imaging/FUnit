// Licensed under the MIT License
// https://github.com/sator-imaging/FUnit

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FUnit.Directives
{
    // NOTE: DO NOT use IIncrementalGenerator because the list of whole FUnit directives in
    //       project must be processed in sequential at once.

    /// <summary>
    /// An FUnit source generator that processes FUnit directives in source files.
    /// </summary>
    [Generator]
    public class FUnitSourceGenerator : ISourceGenerator
    {
        private readonly Dictionary<string, IDirectiveOperator> _directiveOperators = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="FUnitSourceGenerator"/> class.
        /// </summary>
        public FUnitSourceGenerator()
        {
            foreach (var op in new IDirectiveOperator[]
            {
                new IncludeOperator(),
            })
            {
                this._directiveOperators.Add(op.DirectiveKeyword, op);
            }
        }

        /// <summary>
        /// Initializes the source generator.
        /// </summary>
        /// <param name="context">The generator initialization context.</param>
        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this source generator.
        }

        /// <summary>
        /// Executes the source generator to process FUnit directives.
        /// </summary>
        /// <param name="context">The generator execution context.</param>
        public void Execute(GeneratorExecutionContext context)
        {
            foreach (var op in this._directiveOperators.Values)
            {
                op.Setup();
            }

            foreach (var syntaxTree in context.Compilation.SyntaxTrees)
            {
                var root = syntaxTree.GetCompilationUnitRoot();
                var directives = root
                    .DescendantTrivia()
                    .Where(t => t.IsKind(SyntaxKind.WarningDirectiveTrivia))
                    .ToList();

                foreach (var trivia in directives)
                {
                    if (trivia.SyntaxTree is null)
                    {
                        continue;
                    }

                    // Check for no indentation
                    var charPos = trivia.GetLocation().GetLineSpan().StartLinePosition.Character;
                    if (charPos != 0)
                    {
                        continue;
                    }

                    var fullText = trivia.ToString();
                    var trimmedText = fullText.Trim();
                    if (!trimmedText.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var afterHash = trimmedText.Substring(1).TrimStart();
                    if (!afterHash.StartsWith("warning", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var keywordAndArgs = afterHash.Substring(7); // skip "warning"

                    var parts = keywordAndArgs.Split(SR.DirectiveSeparators, 2, StringSplitOptions.RemoveEmptyEntries);
                    var keyword = parts.FirstOrDefault()?.Trim() ?? string.Empty;
                    var args = parts.Length > 1 ? parts[1].Trim() : string.Empty;

#if DEBUG
                    context.ReportDiagnostic(
                        Diagnostic.Create(SR.DebugDiagnostic, trivia.GetLocation(), trimmedText));
#endif

                    if (!keyword.Equals(SR.FUnitKeyword, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Shift!
                    var subParts = args.Split(SR.DirectiveSeparators, 2, StringSplitOptions.RemoveEmptyEntries);
                    keyword = subParts.FirstOrDefault()?.Trim() ?? string.Empty;
                    args = subParts.Length > 1 ? subParts[1].Trim() : string.Empty;

                    if (keyword.Length == 0)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(SR.EmptyFUnitDirectiveDiagnostic, trivia.GetLocation()));
                        continue;
                    }

                    if (this._directiveOperators.TryGetValue(keyword, out IDirectiveOperator? op))
                    {
                        var (hintName, generatedContent, diagnostics) = op.Apply(args, trivia.SyntaxTree.FilePath, trivia.GetLocation());
                        foreach (var diagnostic in diagnostics)
                        {
                            context.ReportDiagnostic(diagnostic);
                        }
                        if (generatedContent is not null)
                        {
                            context.AddSource(hintName, generatedContent);
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(SR.UnknownFUnitDirectiveDiagnostic, trivia.GetLocation(), keyword));
                    }
                }
            }

            foreach (var op in this._directiveOperators.Values)
            {
                op.Cleanup();
            }
        }
    }
}
