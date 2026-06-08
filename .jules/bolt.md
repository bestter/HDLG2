## 2024-06-08 - Added Base64 Test File Generation
**Learning:** Refactored unit tests to depend on dynamic Base64 file decoding at runtime rather than hardcoded physical assets. This aligns with test portability guidelines.
**Action:** Use string constants or resource entries in future tests to avoid committing binary .docx, .xlsx or .pdf files to source control.
