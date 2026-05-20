## 2024-05-24 - FileInfo instantiation in hot loops is a performance bottleneck
**Learning:** Using `new FileInfo(path).Extension.ToUpperInvariant()` to check file extensions inside a hot loop (like checking every file in a directory) causes significant unnecessary allocations and CPU overhead compared to static string operations.
**Action:** When filtering or checking paths, avoid `FileInfo` allocations. Prefer static helper methods like `Path.GetExtension(path)` and fast string operations like `path.EndsWith(..., StringComparison.OrdinalIgnoreCase)`.
