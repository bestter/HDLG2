
## 2024-05-23 - [XSS in HTML Export]
**Vulnerability:** Found an XSS vulnerability where user-controlled metadata like file names, directory names, and file properties were directly written to HTML report via string interpolation in DirectoryBrowser.cs.
**Learning:** System directories and files can contain arbitrary characters, and metadata parsers extract tags that can be manipulated by an attacker to execute JS when the generated HTML is viewed.
**Prevention:** Always use WebUtility.HtmlEncode (and WebUtility.UrlEncode for href attributes) to sanitize user-controlled strings when manually generating HTML outputs.
