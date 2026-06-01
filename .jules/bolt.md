## 2024-05-15 - Loop Fusion in Directory Traversal
**Learning:** Consecutive `foreach` loops iterating over the same list (like aggregating totals from subdirectories) cause unnecessary redundant iteration and increase overhead, especially on deep or wide folder hierarchies.
**Action:** When aggregating multiple independent properties from a collection of objects, fuse the loops and calculate all aggregates in a single iteration pass over the collection to immediately halve the loop iteration time.
