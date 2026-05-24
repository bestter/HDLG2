
## 2024-05-23 - [XSS in HTML Export]
**Vulnerability:** Found an XSS vulnerability where user-controlled metadata like file names, directory names, and file properties were directly written to HTML report via string interpolation in DirectoryBrowser.cs.
**Learning:** System directories and files can contain arbitrary characters, and metadata parsers extract tags that can be manipulated by an attacker to execute JS when the generated HTML is viewed.
**Prevention:** Always use WebUtility.HtmlEncode (and WebUtility.UrlEncode for href attributes) to sanitize user-controlled strings when manually generating HTML outputs.

## 2026-05-23 - [DoS via Unhandled Image Exception]
**Vulnerability:** Found an issue where unexpected exceptions during image parsing (`Image.Identify`) were re-thrown (`throw;`) instead of being safely caught and logged.
**Learning:** Re-throwing generic exceptions in background tasks without proper isolation can crash the main application, leading to Denial of Service (DoS) when a user scans a directory containing a maliciously crafted image file that triggers an unexpected exception in `ImageSharp`.
**Prevention:** Always catch and log generic exceptions when parsing files from untrusted sources, ensuring that a single corrupted or malicious file does not crash the entire application.
