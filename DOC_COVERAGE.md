# Documentation Coverage Report

## Overall Check Result
The FUnit documentation (`README.md`) provides a high-level overview of the framework's concept, usage, and core features. Most user-facing functionality, including the assertion library, CLI options, and the `FUnit.Directives` package, is covered with clear examples. However, there are gaps regarding advanced assertion parameters, specific technical constraints of directives, and internal CLI logic for test file discovery.

## Detailed Result Follows

### 1. Core Framework & Test Descriptor
*   **Status:** ✅ Mostly Covered
*   **Details:** The `describe` and `it` syntax, as well as the distinction between `Run` and `RunAsync`, are well-documented. The README correctly illustrates setup/cleanup patterns.

### 2. Assertion Library (`Must`)
*   **Status:** ⚠️ Partially Covered
*   **Details:**
    *   Basic value, reference, collection, and exception assertions are well-documented.
    *   **Gap:** `HaveEqualProperties` and `HaveEqualFields` support optional `propertyNamesToSkip` and `logger` parameters which are missing from the documentation.
    *   **Gap:** The documentation doesn't explicitly mention support for `ReadOnlySpan<T>` in collection assertions, though it is a key feature for modern .NET development.

### 3. CLI Runner (`dnx -y FUnit.Run`)
*   **Status:** ⚠️ Partially Covered
*   **Details:**
    *   All command-line flags (`--markdown`, `--iterations`, etc.) are listed in the options table.
    *   **Gap:** The internal logic for test file discovery is not documented. Users may not know that a file must start with `#` and contain the string `FUnit` to be recognized as a valid test file.
    *   **Gap:** Flaky test detection (results differing across iterations) is implemented and color-coded in the output, but this feature is not highlighted in the README.
    *   **Gap:** Compatibility with UTF-8 BOM is implemented but not mentioned.

### 4. FUnit Directives
*   **Status:** ⚠️ Partially Covered
*   **Details:**
    *   The `#warning funit include` syntax is documented with examples.
    *   **Gap:** The technical requirement that these directives MUST start at column 0 (no indentation) is not documented, which could lead to silent failures or "Unknown directive" errors if indented.

### 5. GitHub Actions Support
*   **Status:** ✅ Covered
*   **Details:** The use of the `-md` flag for `$GITHUB_STEP_SUMMARY` is clearly explained.
