## 2024-06-15 - Eliminate ToArray() allocation in TreeView expansion
**Learning:** `TreeNodeCollection.AddRange(TreeNode[])` forces unnecessary `ToArray()` allocations when data is collected in `List<TreeNode>`.
**Action:** Use a simple `for` loop to directly add items from `IList` to avoid allocations, wrapping the loop in `TreeView.BeginUpdate()` / `EndUpdate()` to maintain bulk insertion performance.
