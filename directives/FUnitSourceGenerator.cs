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
        private readonly IncludeOperator _includeOperator = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="FUnitSourceGenerator"/> class.
        /// </summary>
        public FUnitSourceGenerator()
        {
            this._directiveOperators.Add(this._includeOperator.DirectiveKeyword, this._includeOperator);
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
            this._includeOperator.Initialize();

            foreach (var syntaxTree in context.Compilation.SyntaxTrees)
            {
                var root = syntaxTree.GetCompilationUnitRoot();
                var comments = root
                    .DescendantTrivia()
                    .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia))
                    .ToList();  // ToList is better than ToImmutableList in this case

                foreach (var trivia in comments)
                {
                    if (trivia.SyntaxTree is null)
                    {
                        // Handle the case where SyntaxTree is null, though it should be rare.
                        // For now, we'll just skip processing this trivia.
                        // A more robust solution might involve reporting a diagnostic.
                        continue;
                    }

                    if (!trivia.ToString().StartsWith(SR.DirectivePrefix, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    // Check for no indentation
                    var line = trivia.GetLocation().GetLineSpan().StartLinePosition.Character;
                    if (line != 0)
                    {
                        continue;
                    }

                    var fullText = trivia.ToString().TrimEnd();

#if DEBUG
                    context.ReportDiagnostic(
                        Diagnostic.Create(SR.DebugDiagnostic, trivia.GetLocation(), fullText));
#endif

                    var keywordAndArgs = fullText.Substring(SR.DirectivePrefix.Length);

                    var parts = keywordAndArgs.Split(SR.DirectiveSeparators, 2, StringSplitOptions.RemoveEmptyEntries);
                    var keyword = parts.FirstOrDefault()?.Trim() ?? string.Empty;
                    var args = parts.Length > 1 ? parts[1].Trim() : string.Empty;

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
        }
    }
}
