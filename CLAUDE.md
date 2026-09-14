# Claude Code Guidelines for HDLG2

## Authoritative Reference

All work must comply with [`AGENTS.md`](./AGENTS.md), which is the source of truth for this repository.

## Commands

- **Build**: `dotnet build HDLG.sln` (must produce **0 errors, 0 warnings**)
- **Test**: `dotnet test HDLG.sln` (must pass **100% of tests**)

## Golden Rules

1. **Do not modify `AGENTS.md`, `ANTIGRAVITY.md`, or `.editorconfig`** without explicit authorization from the repository owner.
2. **Extreme minimalism**: Never add new NuGet packages or dependencies without explicit owner approval.
3. **Ask before improvising**: If any design, architecture pattern, or requirement is unclear, ask before assuming.
4. **Language & Code style**:
   - All code comments, commit messages, and technical documentation must be written **strictly in English** (except `AGENTS.md` and `ANTIGRAVITY.md` which remain in French).
   - Follow `.editorconfig` rules (tabs and CRLF for production C# projects; 4 spaces for `HDLG.Tests`).
5. **Testing**:
   - Every new feature or modified behavior must have unit tests in `HDLG.Tests` (xUnit, FluentAssertions, Moq).
   - UI tests require an STA thread (`WinFormsUiTests.RunSta`).

## Git Conventions

- Never commit directly to `main` or `master`. Always work on a descriptive branch.
- Use Conventional Commits strictly in English (`feat:`, `fix:`, `refactor:`, `chore:`, etc.).
- Sign commits with tool and model name (e.g., `Generated-by: Claude Code (Claude 3.7 Sonnet)`).

## Critical Rule: Google Labs Jules Pull Requests

Before interacting with any Pull Request, inspect its metadata:
`gh pr view <id> --json author,body,commits`

Treat the PR as created by **Google Labs Jules** if any of the following is true:
- `author.login` belongs to `{google-labs-jules, google-labs-jules[bot]}`.
- The PR description (`body`) contains `PR created automatically by Jules`.
- Any commit has Jules as author or co-author (`google-labs-jules` or `google-labs-jules[bot]`).

When working on a Jules PR:
- **STRICTLY FORBIDDEN to post comments or PR reviews**: Do NOT post any general comment, review comment, diff comment, thread reply, or GitHub review (`APPROVE`, `REQUEST_CHANGES`, `COMMENT`), even empty, to avoid waking up or desynchronizing Jules.
- **No active notifications**: Never mention `@google-labs-jules` or `@google-labs-jules[bot]` in descriptions or commit messages (always wrap in backticks).
- **Update description only**: To document changes, update only the PR body/description (`gh pr edit <id> --body ...`), preserving the initial content and appending a section at the bottom (e.g. `### Modifications apportées`).
- Commits and pushes (`git push`) to the PR branch are allowed when requested.
