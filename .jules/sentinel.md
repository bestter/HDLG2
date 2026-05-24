
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
