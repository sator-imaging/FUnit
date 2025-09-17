using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;

namespace FUnit.Directives
{
    internal class IncludeOperator : IDirectiveOperator
    {
        public string DirectiveKeyword => "include";

        // ConcurrentBag doesn't have Clear method in .NET Standard 2.0...!?
        private readonly ConcurrentDictionary<string, byte> _processedFilePaths = new();

        public void Setup()
        {
        }

        public void Cleanup()
        {
            this._processedFilePaths.Clear();
        }

        private static string? NormalizePathFileName(string? path)
        {
            return path?.Replace('\\', '/').TrimEnd('/');
        }

        public (string HintName, string? GeneratedCode, ImmutableList<Diagnostic> Diagnostics) Apply(string args, string sourceFilePath, Location location)
        {
            var diagnostics = ImmutableList<Diagnostic>.Empty;
            string hintName = string.Empty;

            string targetFileName = NormalizePathFileName(args) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(targetFileName))
            {
                diagnostics = diagnostics.Add(Diagnostic.Create(SR.MissingFileNameDiagnostic, location, args));
                return (hintName, null, diagnostics);
            }

            // Resolve the path relative to the source file being inspected
            var sourceDirectory = NormalizePathFileName(Path.GetDirectoryName(sourceFilePath));
            if (sourceDirectory == null)
            {
                // basically, it's found.
                return (hintName, null, diagnostics);
            }

            const string CURR_DIR = "./";
            while (targetFileName.StartsWith(CURR_DIR, StringComparison.Ordinal))
            {
                targetFileName = targetFileName.Substring(CURR_DIR.Length);
            }

            var absoluteFilePath = Path.Combine(sourceDirectory, targetFileName);

            if (!Path.GetExtension(absoluteFilePath).Equals(".cs", StringComparison.OrdinalIgnoreCase))
            {
                diagnostics = diagnostics.Add(Diagnostic.Create(SR.FileExtensionNotSupportedDiagnostic, location, absoluteFilePath));
                return (hintName, null, diagnostics);
            }

            // check before file access
            if (!this._processedFilePaths.TryAdd(absoluteFilePath, 0))
            {
                return (hintName, null, diagnostics);
            }

            if (!File.Exists(absoluteFilePath))
            {
                diagnostics = diagnostics.Add(Diagnostic.Create(SR.IncludedFileNotFoundDiagnostic, location, absoluteFilePath));
                return (hintName, null, diagnostics);
            }

            // Generate a unique hint name for the generated source file
            hintName = $"{Path.GetFileName(targetFileName)} - {Path.GetDirectoryName(absoluteFilePath)}.g.cs";
            foreach (var invalid in SR.InvalidChars)
            {
                hintName = hintName.Replace(invalid, '+');
            }

            var fileContent = File.ReadAllText(absoluteFilePath);

            return (hintName, fileContent, diagnostics);
        }
    }
}
