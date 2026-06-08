## 2024-05-23 - [XSS in HTML Export]
**Vulnerability:** Found an XSS vulnerability where user-controlled metadata like file names, directory names, and file properties were directly written to HTML report via string interpolation in DirectoryBrowser.cs.
**Learning:** System directories and files can contain arbitrary characters, and metadata parsers extract tags that can be manipulated by an attacker to execute JS when the generated HTML is viewed.
**Prevention:** Always use WebUtility.HtmlEncode (and WebUtility.UrlEncode for href attributes) to sanitize user-controlled strings when manually generating HTML outputs.

## 2026-05-23 - [DoS via Unhandled Image Exception]
**Vulnerability:** Found an issue where unexpected exceptions during image parsing (`Image.Identify`) were re-thrown (`throw;`) instead of being safely caught and logged.
**Learning:** Re-throwing generic exceptions in background tasks without proper isolation can crash the main application, leading to Denial of Service (DoS) when a user scans a directory containing a maliciously crafted image file that triggers an unexpected exception in `ImageSharp`.
**Prevention:** Always catch and log generic exceptions when parsing files from untrusted sources, ensuring that a single corrupted or malicious file does not crash the entire application.

## 2024-05-23 - [DoS via Unhandled Parser Exceptions]
**Vulnerability:** Discovered that background parsing of Mp3, Word, and Excel files did not catch generic exceptions from their respective third-party libraries (TagLib, DocumentFormat.OpenXml).
**Learning:** In a bulk processing application, unhandled exceptions from third-party parsing libraries when encountering malformed or maliciously crafted files can bubble up and cause the entire background scan to crash, leading to a Denial of Service (DoS) vulnerability.
**Prevention:** Always use a generic exception catch block (with `#pragma warning disable CA1031`) when passing untrusted file data to third-party parsers, logging the error and failing gracefully.

## 2024-05-23 - [Process Injection/Security Check Bypass via File Extension]
**Vulnerability:** The application was vulnerable to Process Injection because the blocklist for dangerous file extensions (like .exe, .bat) in `OpenWithDefaultProgram` could be bypassed by appending trailing spaces or dots to the file name.
**Learning:** Windows Shell `Process.Start` ignores trailing spaces and dots when executing a file, but `System.IO.Path.GetExtension()` retains them. Therefore, an attacker could create a file named `malicious.bat. ` and the blocklist logic checking `Path.GetExtension` would test `.bat. ` against the list of dangerous extensions, resulting in a false negative and execution of the malicious file.
**Prevention:** Always sanitize input paths by removing trailing spaces and dots (`TrimEnd(' ', '.')`) before extracting the file extension for security validation.

## 2026-05-25 - XSS Vulnerability in DirectoryBrowser
**Vulnerability:** XSS via unencoded file/directory paths.
**Learning:** The prompt indicated an XSS vulnerability, but codebase analysis confirmed that `WebUtility.HtmlEncode()` had already been implemented in `WritHtmlDirectoryAsync` and `WriteHtmlFileAsync` by a recent commit, mitigating the risk.
**Prevention:** Continue enforcing output encoding for all user-controllable input rendered in HTML.

## 2024-05-25 - [Fail-Open UI State Authorization Bypass]
**Vulnerability:** A "fail-open" pattern existed where an actionable UI button (`btnOpenFile`) was aggressively enabled *before* executing security validations (like `IsPathWithinRoot`), meaning an unexpected exception during validation would leave the dangerous action available.
**Learning:** Security validations should gate state changes. Enabling UI elements that trigger privileged or dangerous actions prior to confirming the safety of those actions can result in an authorization bypass if error-handling logic returns early.
**Prevention:** To prevent 'fail-open' authorization bypasses in UI components, ensure all security and validation checks (such as path traversal verifications) execute and pass successfully before enabling actionable UI controls. Additionally, implement defense-in-depth by re-verifying security constraints directly inside the button click execution handler.

## 2026-05-25 - [DoS via Symlink Infinite Recursion]
**Vulnerability:** The application was vulnerable to Denial of Service (DoS) due to a potential `StackOverflowException` caused by recursive directory traversal not checking for directory junctions or symbolic links (`FileAttributes.ReparsePoint`).
**Learning:** If an attacker creates a symbolic link or directory junction pointing back to its parent directory, `EnumerateDirectories` will follow it infinitely during a recursive scan.
**Prevention:** Always check `(directoryInfo.Attributes & FileAttributes.ReparsePoint) != 0` and skip such directories when performing deep recursive traversals to prevent infinite loops and unauthorized out-of-bounds file access.

## 2024-05-29 - [DoS/Unauthorized Access via Directory Traversal]
**Vulnerability:** Recursive directory traversals using `EnumerateDirectories` did not check for reparse points (symbolic links or directory junctions).
**Learning:** Failing to check for `FileAttributes.ReparsePoint` when enumerating directories can lead to infinite recursion (Denial of Service) if a cyclic symbolic link exists, or allow an attacker to traverse outside the intended directory scope by creating a symbolic link to an unauthorized location.
**Prevention:** Always check for and skip directories with the `FileAttributes.ReparsePoint` attribute during recursive directory traversals (`if ((dir.Attributes & FileAttributes.ReparsePoint) != 0) continue;`).

## 2026-05-30 - [XSS Defense in Depth via CSP]
**Vulnerability:** While XSS vulnerabilities in HTML exports were previously mitigated via `WebUtility.HtmlEncode`, relying solely on encoding can be brittle if future changes introduce unencoded output paths.
**Learning:** Static HTML exports should use a restrictive Content Security Policy (CSP) to ensure scripts cannot be executed and data cannot be exfiltrated, even if an XSS vulnerability is introduced later.
**Prevention:** Add a strict CSP meta tag (`<meta http-equiv="Content-Security-Policy" content="default-src 'none'; style-src 'unsafe-inline'; base-uri 'none'; form-action 'none';">`) to all generated HTML exports.

## 2026-06-05 - [MOTW Bypass via Container/Shortcut Extensions]
**Vulnerability:** Container files (like .iso, .img, .vhd) and shortcut/theme files bypass Mark of the Web (MOTW) and can lead to arbitrary code execution if opened automatically.
**Learning:** Certain extensions not traditionally considered executables (e.g., .iso, .url, .theme) can be abused by attackers to bypass security warnings and execute code.
**Prevention:** Always include MOTW-bypass and container extensions in blocklists when directly launching user-supplied files via `Process.Start`.

## 2024-06-01 - [XML DoS via Unhandled Control Characters in Export]
**Vulnerability:** Found an issue where the XML export function in `DirectoryBrowser.cs` could crash if file metadata (such as an MP3 tag) contained characters that are invalid in XML (e.g., spaces in element names like "Camera Model", or control characters in the content).
**Learning:** `XmlWriter` throws an unhandled exception if it attempts to write invalid element names or values, causing the entire directory export to fail. This constitutes a Denial of Service via malformed third-party files.
**Prevention:** When generating XML with `XmlWriter` using untrusted data, encode element keys with `XmlConvert.EncodeLocalName()` and sanitize values with a custom method that filters out invalid XML control characters.

## 2026-06-05 - [DLL Hijacking via Missing WorkingDirectory]
**Vulnerability:** Found `Process.Start` calls opening external URLs via `UseShellExecute = true` without explicitly setting the `WorkingDirectory`.
**Learning:** If an application launches an external process without specifying a safe working directory, and the current working directory happens to point to an untrusted location, it may be vulnerable to DLL search order hijacking when the target process launches.
**Prevention:** To prevent DLL hijacking vulnerabilities when launching files via `Process.Start` with `UseShellExecute = true`, always explicitly set `WorkingDirectory` to a safe, trusted location (like `Environment.GetFolderPath(Environment.SpecialFolder.System)`).
