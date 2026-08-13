# Current plan

**Status:** complete (Task 7 done 2026-08-13 — AGENTS.md authorized)
**Date:** 2026-08-12
**Branch:** `Improvements20260812`
**Plan:** `Documentation/plans/2026-08-12-address-json-export-review.md`

## Reload

In a new session say:

```
Continue the plan in Documentation/plans/2026-08-12-address-json-export-review.md
```

## Why this exists

Code review of the JSON export branch requested changes before merge. Main blocker: collapse the triplicated XML/HTML/JSON export orchestration in `MainWindow`. The durable plan lives in `Documentation/plans/` so it survives session and temp-file cleanup.

## Do not

- Do not edit `AGENTS.md` / `ANTIGRAVITY.md` unless the owner explicitly authorizes it (Task 7 is gated).
- Do not start work on `main`.
