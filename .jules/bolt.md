## 2024-05-24 - FileInfo instantiation in hot loops is a performance bottleneck
**Learning:** Using `new FileInfo(path).Extension.ToUpperInvariant()` to check file extensions inside a hot loop (like checking every file in a directory) causes significant unnecessary allocations and CPU overhead compared to static string operations.
**Action:** When filtering or checking paths, avoid `FileInfo` allocations. Prefer static helper methods like `Path.GetExtension(path)` and fast string operations like `path.EndsWith(..., StringComparison.OrdinalIgnoreCase)`.

## 2025-02-14 - FileInfo Allocation Redundancy
**Learning:** During directory enumeration using `DirectoryInfo.EnumerateFiles()`, creating a new `FileInfo` from the generated file's `FullName` inside the `HdlgFile` class is redundant and causes extra allocations and potential duplicate file system trips.
**Action:** Always reuse the existing `FileInfo` object yielded by `EnumerateFiles()` instead of instantiating new wrappers via path string constructors, particularly in tight inner loops iterating over many files.
