## 2024-05-19 - Merging Loops for Aggregate Counts
**Learning:** Merging consecutive iterations of the same collection, when their side-effects are independent (e.g. aggregating distinct running totals), yields significant and immediate CPU/time savings with minimal code alteration.
**Action:** Always inspect subsequent loops to identify identical iteration sets and, when logic allows, fold their inner contents into a single unified pass.
