🎯 **What:** Added tests for handling oversized image dimensions and general exceptions in `ImagePropertyGetter`.
📊 **Coverage:** Now tests the edge cases where an image's dimensions exceed `MaxImageDimension` (32,768) and a general exception is thrown while extracting image properties.
✨ **Result:** Improved test coverage for `ImagePropertyGetter` to ensure appropriate warnings are logged and empty properties are gracefully returned in these error conditions.
