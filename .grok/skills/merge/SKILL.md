---
name: merge
description: Merge `origin/main` (or another branch given as argument) into the current git branch, resolve merge conflicts, and run the full HDLG2 test suite. Use when the user runs `/merge`, asks to merge main, sync or update the current branch from `origin/main`, pull main into this branch, or fix merge conflicts after merging main.
---

# Merge incoming branch into current

Bring a remote (or named) branch into **the current branch**. Do not merge the current branch into `main`. Do not push. Do not skip verification.

Announce: "Merging `<source>` into `<current-branch>`."

## Source branch

| Argument | Source |
|---|---|
| omitted / `main` / `origin/main` | `origin/main` |
| `master` / `origin/master` | `origin/master` |
| `origin/<name>` | that remote-tracking branch |
| `<name>` | `origin/<name>` if it exists after fetch; otherwise local `<name>` |

If two candidates exist and the choice is unclear, stop and ask.

## Preconditions (stop if any fail)

1. `git status` — must be a git work tree on a named branch (not detached).
2. Current branch must not be `main` or `master`.
3. Working tree must be clean (`git status --porcelain` empty). If dirty, stop and ask (stash, commit, or abort).
4. `git fetch origin` must succeed before merging any `origin/*` ref.

Do not start a merge with uncommitted work.

## Merge

```text
git fetch origin
git merge --no-edit <source>
```

- Fast-forward is fine.
- Leave the merge commit message as git generated it (`--no-edit`).
- Never `--abort` after you have started resolving unless the user asks.

## Conflicts

1. `git diff --name-only --diff-filter=U` — list conflicted files.
2. Resolve every conflict yourself when the correct side (or combination) is obvious from surrounding code and `AGENTS.md`.
3. **Stop and ask** when any of these is true:
   - Both sides changed behavior in incompatible ways and the intended result is unclear
   - A conflict touches `AGENTS.md`, `ANTIGRAVITY.md`, or `.editorconfig`
   - A conflict is an architectural or API choice not documented in `AGENTS.md` / `ANTIGRAVITY.md`
   - You would be guessing
4. After each resolved file: `git add` it. Do not `git add` unresolved files.
5. Finish with `git commit` only if git did not already complete the merge (empty commit message editor → use the default merge message).

Do not delete the other side's unique work to "make it compile" unless that is clearly the intent.

## Verify (required)

From the repo root, after the merge is committed (or fast-forwarded):

```text
dotnet build HDLG.sln
dotnet test HDLG.sln
```

Required result: **0 build errors, 0 build warnings, 0 test failures**.

- Compile or test failures caused by the merge: fix them.
- Fix is obvious (missing using, leftover conflict markers, broken call after an API change on `<source>`): apply it.
- Fix is a product decision: stop and ask. Do not invent behavior.
- Post-merge fixes that are **not** part of conflict resolution: commit separately with a Conventional Commits message in English (e.g. `fix: restore tests after merge of origin/main`).
- Conflict-resolution edits belong in the merge commit, not a follow-up.

Do not claim success unless you ran both commands in this turn and read the output.

## Report

Tell the user:

- Current branch and source that was merged
- Fast-forward vs merge commit (hash)
- Conflicted files and how they were resolved (or that there were none)
- Any question you asked and the decision
- `dotnet build` and `dotnet test` results (errors / warnings / failed tests)

If verification fails and you cannot fix it without guessing, leave the merge in place (do not abort) and report the exact failures.

## Never

- Merge into `main` / `master` or check them out to "finish"
- Push, force-push, or delete branches
- Skip fetch and merge a stale local `main`
- Ignore conflict markers or leave the repo in `MERGING` without telling the user
- Add NuGet packages or change `AGENTS.md` / `ANTIGRAVITY.md` / `.editorconfig` to resolve a conflict
