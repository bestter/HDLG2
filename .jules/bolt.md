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
## 2024-06-02 - Merge consecutive directory loops during Browse
**Learning:** Traversing the same collection in multiple, consecutive loops can be combined to reduce O(N) overhead iterations, lowering instruction counts and cache misses especially when processing thousands of entries recursively.
**Action:** Always combine loops iterating over the same collection doing independent sub-tasks if doing so does not compromise readability.
## 2024-05-19 - Merging Loops for Aggregate Counts
**Learning:** Merging consecutive iterations of the same collection, when their side-effects are independent (e.g. aggregating distinct running totals), yields significant and immediate CPU/time savings with minimal code alteration.
**Action:** Always inspect subsequent loops to identify identical iteration sets and, when logic allows, fold their inner contents into a single unified pass.

## 2024-05-24 - Fast-Pathing String Sanitization to Avoid Allocations
**Learning:** In string sanitization routines (like removing invalid XML characters), eagerly allocating a `StringBuilder` for every string causes significant garbage collection overhead, particularly because most strings (e.g., standard file paths) do not contain invalid characters.
**Action:** When writing sanitization or validation methods, always scan the string first to find the first invalid character. If none are found, return the original string immediately. Only allocate a `StringBuilder` and perform character-by-character appending if an invalid character is actually detected.

## 2026-05-25 - Avoid empty enumerator allocation in foreach loops
**Learning:** Using `foreach` on collections that might be empty (like dictionaries returned when a file has no extractable properties) causes an unnecessary allocation of the enumerator object. When processing thousands of files, this adds up to significant garbage collection overhead.
**Action:** When iterating over a collection that is frequently empty, wrap the `foreach` loop in an `if (collection.Count > 0)` check to prevent the enumerator allocation.
## 2026-05-25 - Avoid Dictionary allocation and merging when only one property getter returns data
**Learning:** During directory file enumeration, allocating a `new Dictionary<string, IConvertible>()` and copying values `O(N)` for every file causes massive GC overhead. In most directory scenarios, files like `.mp3`, `.docx`, or `.jpg` only trigger ONE specific property getter (e.g. `ImagePropertyGetter`).
**Action:** When gathering properties from multiple providers, use an `IReadOnlyDictionary<string, IConvertible>?` return type. Hold the reference to the first returned dictionary and only allocate a new `Dictionary` to merge keys if a *second* getter also returns data. This fast-path avoids O(N) copying and dictionary instantiation for the vast majority of files.

## 2026-05-25 - Arrays over Lists in hot loops
**Learning:** Iterating over a `List<T>` in a high-frequency hot loop (like checking every single file against a list of property getters) forces the allocation of a `List<T>.Enumerator` struct and introduces slight overhead compared to a primitive array.
**Action:** When a collection's size is fixed at instantiation and is iterated continuously in a hot path, use an array (`T[]`) instead of a `List<T>` to eliminate enumerator overhead and improve sequential access speed.
## 2024-05-25 - Use for loops for arrays in hot loops
**Learning:** While iterating over an array with `foreach` is generally fast, it still incurs minor overhead for the iterator state machine and enumerator struct allocation. In a hot path (like `FilePropertyBrowser.GetFileProperty` which executes for every single file in a recursive directory scan), explicitly changing an array `foreach` to a `for (int i = 0; i < array.Length; i++)` loop eliminates this overhead completely for a measurable reduction in instruction count.
**Action:** To strictly avoid `IEnumerator` struct allocation and iterator state machine overhead in C# hot paths, explicitly use standard `for` loops with index-based access instead of `foreach` loops when iterating over primitive arrays.

## 2024-06-10 - Avoid enumerator boxing on IReadOnlyList<T> in recursive tree iterations
**Learning:** Iterating over an `IReadOnlyList<T>` using `foreach` forces the compiler to use `IEnumerator<T>` via the interface implementation rather than a direct duck-typed struct enumerator (like `List<T>.Enumerator`), causing a heap allocation per iteration loop. In deep recursive trees (like directory structure serialization), this creates O(N) garbage allocations (where N is number of directories).
**Action:** Always prefer `for (int i = 0; i < collection.Count; i++)` when iterating over collections typed as `IReadOnlyList<T>` or `IList<T>` in hot loops to guarantee zero enumerator allocations.
## 2026-06-14 - Defer String Allocations to Serilog Native Formatting
**Learning:** Eagerly formatting collections with `string.Join` inside Serilog logging statements forces string allocations even when the log level is disabled, causing unnecessary garbage collection overhead in hot paths.
**Action:** Always pass collections directly to structured logging methods (e.g., `Logger?.Warning("... {Items}", items)`) instead of manually joining them. Serilog handles `IEnumerable` serialization natively and safely.

## 2024-06-15 - Prevent UI Thread Blocking for Metadata Extraction
**Learning:** Extracting file properties (especially for complex types or from slow storage) inside an event handler can significantly block the UI thread, causing WinForms applications to freeze and become unresponsive to user input.
**Action:** When performing potentially slow I/O or CPU-bound operations in a UI event handler, always wrap the heavy operation in `await Task.Run(...)` to offload it to a background thread. Since it's a WinForms application, use `.ConfigureAwait(true)` (which is the default, but good to be explicit for CA2007) to ensure the continuation (UI updates) resumes on the main UI thread.
## 2024-06-15 - Eliminate ToArray() allocation in TreeView expansion
**Learning:** `TreeNodeCollection.AddRange(TreeNode[])` forces unnecessary `ToArray()` allocations when data is collected in `List<TreeNode>`.
**Action:** Use a simple `for` loop to directly add items from `IList` to avoid allocations, wrapping the loop in `TreeView.BeginUpdate()` / `EndUpdate()` to maintain bulk insertion performance.
## 2024-06-25 - Avoid layout thrashing in ListView population
**Learning:** Adding multiple items sequentially to a UI `ListView` without calling `BeginUpdate()`/`EndUpdate()` causes the control to recalculate its layout and repaint after every single insertion. When displaying many file properties, this causes significant UI lag and layout thrashing.
**Action:** When adding multiple items to a WinForms `ListView`, always wrap the insertion loop in `listView.BeginUpdate()` and `listView.EndUpdate()` to suspend layout logic and drastically improve rendering performance.

## 2026-06-25 - Avoid redundant HtmlEncode allocations
**Learning:** Calling `WebUtility.HtmlEncode()` multiple times on the same string (e.g. `file.Name`) within a single output generation block, or wrapping inherently safe types like numbers (`file.Size.ToString()`) and ISO DateTimes in `HtmlEncode()`, causes massive, entirely redundant string allocation and garbage generation in hot loops.
**Action:** When formatting text for web outputs, only call `HtmlEncode` once per variable and cache it if used multiple times in the template. Never apply `HtmlEncode` to purely numeric values or standard safe date formats.

## 2026-06-25 - Caching HtmlEncode results in DirectoryBrowser output
**Learning:** The `HtmlEncode` method was still being called unnecessarily or on inherently safe types. Specifically, caching `WebUtility.HtmlEncode(directory.Path)` inside `WritHtmlDirectoryAsync` reduces garbage collection overhead in hot recursive trees.
**Action:** Review uses of `HtmlEncode` and ensure results are cached if used multiple times in the block.
## 2026-06-25 - Cache XmlConvert.EncodeLocalName results
**Learning:** `XmlConvert.EncodeLocalName` can be relatively slow and allocates memory. In a directory browser application generating XML exports, files consistently share the same metadata property keys. Repeatedly calling this method in the hot loop for every property of every file incurs unnecessary CPU cycles and memory allocations.
**Action:** Cache the XML-safe local name encoding in a dictionary (e.g., `_xmlEncodedPropertyKeys`) to prevent redundant string allocations and parsing operations, matching the performance pattern used for HTML outputs.
## 2026-06-25 - Avoid Unhelpful Micro-Optimizations Involving Async/Await
**Learning:** In C#, extracting the body of a hot loop into an `async Task` method (e.g. to avoid duplicating the loop body for a dictionary cast unboxing) introduces massive state machine overhead that completely negates any micro-optimization benefits and actively degrades performance.
**Action:** Never sacrifice code readability or introduce async state machines in hot paths for trivial micro-optimizations (like avoiding a single enumerator allocation).

## 2026-06-25 - Cache Invariant Paths in UI Components
**Learning:** Evaluating `Path.GetFullPath(rootDirectory)` inside hot UI events (like `TreeView.BeforeExpand`) causes redundant I/O checks and string allocations. Because `rootDirectory` is invariant after construction, its resolved path can be calculated once.
**Action:** When a UI component is bound to a specific root directory, resolve and cache its full path (with and without the trailing separator) in the constructor to optimize hot-path traversal checks.

## 2026-06-25 - Prevent redundant OS stat calls with FileInfo
**Learning:** Instantiating `new FileInfo(path)` forces the OS to perform a file system stat operation to retrieve attributes. When this happens downstream inside hot loops (like `GetFileProperty` or `IsFileSizeWithinLimit` checking every file yielded by `DirectoryInfo.EnumerateFiles()`), it causes massive I/O overhead and completely bypasses the pre-populated attributes already returned by the directory enumeration.
**Action:** Always accept and pass the existing `FileInfo` or `FileSystemInfo` object through the method chain instead of string paths whenever possible to eliminate redundant OS calls and object allocations.

## 2026-06-25 - Avoid FileInfo instantiation in IsFileSizeWithinLimit
**Learning:** Instantiating `FileInfo` from a string path multiple times during file traversal to check limits adds unnecessary overhead.
**Action:** When filtering or checking paths for properties, accept the existing `FileInfo` object yielded by the directory enumerator.
## 2026-06-25 - Prevent redundant OS stat calls with FileInfo
**Learning:** Instantiating `new FileInfo(path)` forces the OS to perform a file system stat operation to retrieve attributes. When this happens downstream inside hot loops (like `GetFileProperty` or `IsFileSizeWithinLimit` checking every file yielded by `DirectoryInfo.EnumerateFiles()`), it causes massive I/O overhead and completely bypasses the pre-populated attributes already returned by the directory enumeration.
**Action:** Always accept and pass the existing `FileInfo` or `FileSystemInfo` object through the method chain instead of string paths whenever possible to eliminate redundant OS calls and object allocations.
## 2026-07-02 - Suppressing CA1062 on FileInfo arguments in FilePropertyGetters
**Learning:** After modifying `IFilePropertyGetter` to accept `FileInfo` instead of `string path`, the .NET compiler generated CA1062 warnings (`validate parameter 'fileInfo' is non-null before using it`) across all getter implementations. Since these getters are tightly coupled to `FilePropertyBrowser`, which already validates the `FileInfo` object for nullness before invoking them, adding explicit null checks inside every getter is redundant and clutters the code.
**Action:** When updating interface parameters to objects that are already validated by the caller in the same assembly, suppress CA1062 at the file level using `#pragma warning disable CA1062` to keep the implementations clean while maintaining the compiler's strictness elsewhere.
## 2026-06-25 - Avoid FileInfo instantiation in IsSupportedFile and GetFileProperties
**Learning:** During property extraction, passing string paths to `IFilePropertyGetter.IsSupportedFile` and `GetFileProperties` causes redundant `FileInfo` instantiations within the getters (e.g. to check `.Length` or `.Exists`), triggering unnecessary OS stat calls. When processing thousands of files, this creates significant I/O and CPU overhead. Additionally, when using Moq to match `FileInfo` objects in tests, `f.FullName` returns absolute paths, which will fail to match relative paths like `"test.jpg"` used in unit tests.
**Action:** Update interfaces and getters to accept the existing `FileInfo` object yielded by the directory enumerator. Update test mocks to use `f.Name == path` when verifying relative paths against `FileInfo` arguments.

## 2024-07-03 - Prevent UI Thread Blocking for TreeView Expand
**Learning:** Performing directory enumeration synchronously in WinForms `TreeView.BeforeExpand` events causes the application UI to freeze completely, especially when dealing with large nested directories or slow network drives.
**Action:** Always wrap heavy I/O operations (like `dirInfo.EnumerateFileSystemInfos()`) inside `await Task.Run(...)` in a UI event handler, change the handler to `async void`, and append `.ConfigureAwait(true)` to ensure that the continuation logic (such as populating the UI control) resumes smoothly on the main UI thread without causing layout thrashing or unresponsiveness.

## 2026-07-14 - Use early returns to un-nest hot loops
**Learning:** Deep conditional nesting inside `foreach` loops that iterate over properties for thousands of files adds visual clutter and creates unnecessary indentation blocks. By replacing checks like `if (!string.IsNullOrEmpty)` with an early exit pattern `if (string.IsNullOrEmpty) continue;`, the serialization logic is flattened. Furthermore, doing `if (file.Properties == null || file.Properties.Count == 0) return;` eliminates the need to wrap the whole loop inside a null/count check.
**Action:** Always prefer early `continue` statements in hot loops to eliminate deep conditional nesting, and use early method returns or early block exits for null/empty collections to organically flatten the code structure.
