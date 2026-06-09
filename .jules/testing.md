## 2026-06-09 - Fix Flaky Tests by Eliminating File System Deletion Anti-Patterns

**Learning:** When generating temporary file fixtures in test setup (e.g. `ImageSetup.cs` writing Base64 decoded images to `Environment.CurrentDirectory`), actively deleting those shared files via a class-level teardown (like `IDisposable.Dispose()`) is a major anti-pattern if the test framework runs classes in parallel. It introduces a race condition where one class destroys the fixtures while another class is attempting to read them, leading to flaky `FileNotFoundException` crashes.

**Action:** When handling globally accessible or parallelized file fixtures, embed them as Base64 strings directly in setup, write them, and avoid implementing destructive cleanup methods unless the files are strictly isolated to a uniquely generated temporary directory per test instance.
