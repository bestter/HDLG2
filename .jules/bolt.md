## 2024-05-24 - FileInfo instantiation in hot loops is a performance bottleneck
**Learning:** Using `new FileInfo(path).Extension.ToUpperInvariant()` to check file extensions inside a hot loop (like checking every file in a directory) causes significant unnecessary allocations and CPU overhead compared to static string operations.
**Action:** When filtering or checking paths, avoid `FileInfo` allocations. Prefer static helper methods like `Path.GetExtension(path)` and fast string operations like `path.EndsWith(..., StringComparison.OrdinalIgnoreCase)`.

## 2025-02-14 - FileInfo Allocation Redundancy
**Learning:** During directory enumeration using `DirectoryInfo.EnumerateFiles()`, creating a new `FileInfo` from the generated file's `FullName` inside the `HdlgFile` class is redundant and causes extra allocations and potential duplicate file system trips.
**Action:** Always reuse the existing `FileInfo` object yielded by `EnumerateFiles()` instead of instantiating new wrappers via path string constructors, particularly in tight inner loops iterating over many files.
## 2024-10-24 - File metadata dictionary allocation
**Learning:** `FilePropertyBrowser` was aggressively allocating empty dictionaries for every single file parsed, even if no matching property getter supported it or returned properties. This creates significant garbage generation when parsing thousands of files in a directory list generator.
**Action:** When a method conditionally builds a collection based on iteration or file metadata lookup, avoid eagerly allocating the container. Wait until at least one item is found to instantiate the collection. Additionally, files without metadata can safely use a static readonly `EmptyProperties` dictionary rather than fresh empty instances.
## 2025-02-14 - Redundant string allocation in case-insensitive HashSet lookups
**Learning:** Checking a HashSet that was initialized with `StringComparer.OrdinalIgnoreCase` using an explicitly upper-cased string (`extension.ToUpperInvariant()`) is redundant and causes unnecessary string allocations, which can add up in hot loops like checking supported extensions for every file.
**Action:** When a collection like `HashSet<string>` is already configured to be case-insensitive, avoid any `.ToUpperInvariant()` or `.ToLowerInvariant()` calls on strings passed to its methods (e.g., `.Contains()`).

## 2026-05-24 - Redundant string allocation with case-insensitive collections
**Learning:** When using case-insensitive collections like `HashSet<string>(StringComparer.OrdinalIgnoreCase)`, calling `.ToUpperInvariant()` or similar methods on the checked string is redundant and causes unnecessary memory allocations.
**Action:** Always check the comparer used by a collection before transforming strings for lookup. Avoid string transformations in hot loops like file enumeration.
## 2024-05-19 - Avoid LINQ `.Any()` in Hot Loops
**Learning:** Using `.Any()` on Collections/Lists (like `IReadOnlyList`) inside recursive or hot loops (such as XML/HTML directory serialization) causes a LINQ enumerator allocation, which creates unnecessary garbage collection overhead and reduces performance.
**Action:** Always prefer `.Count > 0` over `.Any()` when checking if a `List` or `IReadOnlyList` has elements in performance-critical code paths.
## 2024-05-25 - Replace Synchronous Wait on Async Task in XML/HTML Browse
**Learning:** Calling `.Wait()` on an async method like `SaveAsXMLAsync` inside a background thread pool task (created via `Task.Run`) blocks the thread until the task completes. While this doesn't block the UI thread here, it needlessly ties up a thread pool thread, which can lead to thread pool starvation and poor scalability if many such operations occur concurrently.
**Action:** Always prefer `await` over `.Wait()` when interacting with async methods to yield the thread back to the thread pool while waiting for I/O operations (like file writing) to complete. Ensure the caller method is marked `async Task` to support this properly.
## 2025-02-14 - String Allocations on Path Checks in Hot Loops
**Learning:** Using `Path.GetExtension(path)` and `path.EndsWith(string, ...)` inside hot loops creates string allocations. With .NET 9+, we can eliminate these allocations by using spans. For checking if an extension is in a `HashSet<string>`, use `Path.GetExtension(path.AsSpan())` in combination with `_supportedExtensions.GetAlternateLookup<ReadOnlySpan<char>>().Contains(extension)`. For `EndsWith`, use `path.AsSpan().EndsWith(...)`.
**Action:** When filtering or checking paths, avoid `FileInfo` allocations and avoid `Path.GetExtension(string)` string allocations. Prefer static helper methods like `Path.GetExtension(path.AsSpan())` combined with `.GetAlternateLookup<ReadOnlySpan<char>>()` for hash set lookups, and fast span operations like `path.AsSpan().EndsWith(..., StringComparison.OrdinalIgnoreCase)`.
## 2026-05-24 - Serilog structured logging vs string interpolation in hot loops
**Learning:** Using string interpolation (`$"{var}"`) in Serilog method calls (like `log.Debug`) forces the .NET runtime to allocate strings even if the log level is disabled. In hot paths, like recursive directory scanning and serialization for every single file/folder, this generates massive amounts of garbage.
**Action:** Always use Serilog's structured logging message templates (e.g., `log.Debug("Value: {Value}", value)`) instead of string interpolation. This defers execution and completely avoids string allocation overhead when the log level is not active.

## 2024-06-03 - Prevent eager string allocations in logging
**Learning:** Using interpolated strings (`$""`) inside logging calls (e.g., `Logger?.Warning($"File {path} might be corrupted...");`) forces the string to be allocated and formatted eagerly *before* the logger checks if the log level is actually enabled. This creates unnecessary garbage collection overhead, especially during repetitive operations or inside loops.
**Action:** Always use structured logging message templates provided by Serilog (e.g., `Logger?.Warning("File {Path} might be corrupted...", path);`) instead of string interpolation to ensure strings are only allocated if the corresponding log level is enabled.
## 2024-05-19 - Combine Directory and File Enumeration for Single-Pass I/O
**Learning:** Making separate consecutive calls to `DirectoryInfo.EnumerateDirectories()` and `DirectoryInfo.EnumerateFiles()` on the same path forces the OS to scan the directory contents twice. In hot paths like deep recursive traversal, this severely degrades performance and wastes I/O bandwidth.
**Action:** Always prefer a single pass using `DirectoryInfo.EnumerateFileSystemInfos()`, checking `if (info is DirectoryInfo)` and `else if (info is FileInfo)` inside the loop, halving system calls and allocation overhead for directory reads.

## 2024-05-24 - Remove redundant List allocation during directory enumeration
**Learning:** Using `EnumerateDirectories().ToList().ForEach(...)` performs a completely redundant allocation of a `List<T>` to hold all items in memory just to iterate them. This increases memory pressure and slows down iteration compared to lazy enumeration.
**Action:** Prefer using a standard `foreach` loop to consume `IEnumerable<T>` sequentially without allocating intermediate collections unless randomly indexing elements is strictly required.
## 2024-05-15 - Loop Fusion in Directory Traversal
**Learning:** Consecutive `foreach` loops iterating over the same list (like aggregating totals from subdirectories) cause unnecessary redundant iteration and increase overhead, especially on deep or wide folder hierarchies.
**Action:** When aggregating multiple independent properties from a collection of objects, fuse the loops and calculate all aggregates in a single iteration pass over the collection to immediately halve the loop iteration time.
