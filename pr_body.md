🎯 **What:**
Refactored `WriteXmlFileAsync` in `HDLG winforms/DirectoryBrowser.cs` to eliminate deeply nested conditionals.

💡 **Why:**
The method previously contained multiple levels of nested `if` statements (checking `if (file.Properties != null)` and then within `foreach` loops checking `if (!string.IsNullOrWhiteSpace( property.Key ) && property.Value != null)`). This indentation drift made the code difficult to read. By employing early returns (`if (file.Properties == null) { return; }`) and loop continuations (`if (string.IsNullOrWhiteSpace(...) || property.Value == null) continue;`), we organically flatten the code structure without introducing performance-degrading overhead from async helper method extractions.

✅ **Verification:**
I verified that the XML structures generated (`ExtentedProperties` node, its children, and the enclosing `File` tags) are perfectly preserved. No logic was altered, only the flow control structures were inverted. A successful local compilation was achieved (and UI tests pass up to their known limit on Linux environments).

✨ **Result:**
The refactored code is significantly easier to read, maintain, and reason about without altering the behavior of the XML generation logic.
