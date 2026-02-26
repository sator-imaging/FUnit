// Licensed under the MIT License
// https://github.com/sator-imaging/FUnit

using Microsoft.CodeAnalysis;

#pragma warning disable RS2008 // Enable analyzer release tracking for the analyzer project

namespace FUnit.Directives
{
    internal class SR
    {
        public const string WarningDirectivePrefix = "#warning";
        public const string FUnitKeyword = "funit";
        public const string DiagnosticCategory = nameof(FUnit);

        public static readonly char[] DirectiveSeparators = new[] { ' ', '\t' };
        public static readonly char[] InvalidChars = new[] { '/', '\\', ':' };  // File.Exists will reject invalid path so remove only directory separators

        public static DiagnosticDescriptor MissingFileNameDiagnostic = new(
            id: "FUNIT001",
            title: "Missing file name for include directive",
            messageFormat: "Include directive '{0}' is missing a file name argument",
            category: DiagnosticCategory,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor IncludedFileNotFoundDiagnostic = new(
            id: "FUNIT002",
            title: "Included file not found",
            messageFormat: "Included file '{0}' not found",
            category: DiagnosticCategory,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor FileExtensionNotSupportedDiagnostic = new(
            id: "FUNIT005",
            title: "File extension not supported",
            messageFormat: "Included file '{0}' has an unsupported extension. Only .cs files are allowed.",
            category: DiagnosticCategory,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static DiagnosticDescriptor DebugDiagnostic = new(
            id: "FUNIT999",
            title: "FUnit Debug Diagnostic",
            messageFormat: "FUnit directive encountered: {0}",
            category: DiagnosticCategory,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
