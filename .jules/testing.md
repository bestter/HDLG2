## 2026-06-09 - Fix Flaky Tests by Eliminating File System Deletion Anti-Patterns

**Learning:** When generating temporary file fixtures in test setup (e.g. `ImageSetup.cs` writing Base64 decoded images to `Environment.CurrentDirectory`), actively deleting those shared files via a class-level teardown (like `IDisposable.Dispose()`) is a major anti-pattern if the test framework runs classes in parallel. It introduces a race condition where one class destroys the fixtures while another class is attempting to read them, leading to flaky `FileNotFoundException` crashes.

**Action:** When handling globally accessible or parallelized file fixtures, embed them as Base64 strings directly in setup, write them, and avoid implementing destructive cleanup methods unless the files are strictly isolated to a uniquely generated temporary directory per test instance.

## 2026-06-26 - WinForms UI Structural Tests

**Learning:** WinForms/Krypton form instantiation requires an STA thread. Use a dedicated STA thread wrapper in tests (`WinFormsUiTests.RunSta`) rather than assuming the default xUnit thread apartment state.

**Action:** UI tests should validate control presence and form type (`KryptonForm`, key `Krypton*` controls) without requiring visual rendering or user input simulation.

## 2026-06-26 - Branding Tests

**Learning:** Logo tests (`AppBrandingTests`, `AppLogoRendererTests`) belong to the serialized `WinFormsUiTestCollection` to avoid GDI+ cross-thread conflicts with other tests.

**Action:** After editing SVG sources under `HDLG winforms/Assets/`, run `scripts/GenerateAppLogoAssets.ps1` (Inkscape) before validating UI assets manually.
## 2024-05-14 - Simulating SystemExceptions for Fallback Catch Blocks

**What:** When a framework method like `FileStream` is wrapped in multiple `catch` blocks—e.g., catching specific `IOException`s first, then falling back to a general `Exception` block—testing the fallback block can be tricky if you try to use typical I/O errors (like passing invalid file data, which throws `FileFormatException` or `InvalidDataException`, caught by the first block).

**Coverage:** We needed to test the generic `catch (Exception ex)` block in `WordPropertyGetter.cs` without throwing an `IOException` or OpenXml-specific exception.

**Result:** By passing a directory path to a `FileInfo` object and feeding it to a `FileStream`, the framework consistently throws an `UnauthorizedAccessException` (which inherits from `SystemException`, not `IOException`) on both Linux and Windows. This cleanly bypasses the specific `IOException` catch block and perfectly triggers the general exception fallback block, allowing for reliable testing without injecting internal test hooks or mocking un-mockable framework classes.

## 2026-09-28 - [Add tests for Mp3PropertyGetter missing test cases]
**What:** Missing tests for Mp3PropertyGetter null checks, generic exception handling, and corrupted file cases.
**Coverage:**
- ArgumentNullException for null FileInfo in GetFileProperties and IsSupportedFile.
- General exception handling by passing a directory path to trigger UnauthorizedAccessException across all platforms without Unix-only permissions.
- Handling of PossiblyCorrupt tags by generating a minimal M4A audio file with an oversized box header.
**Result:** Increased line/branch coverage and reliability of Mp3PropertyGetter by confirming all error cases return EmptyProperties and log warnings/errors appropriately as defined in the contract.

