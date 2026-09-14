# GitHub Copilot Instructions for HDLG2

## Authoritative Reference

All AI agents and assistants must comply with [`AGENTS.md`](../AGENTS.md), which is the source of truth for this repository.

## Golden Rules

1. **Do not modify `AGENTS.md`, `.editorconfig`, or `.github/copilot-instructions.md`** without explicit authorization from the repository owner.
2. **Extreme minimalism**: Never add new NuGet packages or dependencies without explicit owner approval.
3. **Ask before improvising**: If any design, architecture pattern, or requirement is unclear, ask before assuming.
4. **Language & Code style**:
   - All code comments, commit messages, PR descriptions, and technical documentation must be written **strictly in English** (except `AGENTS.md` which remains in French).
   - Respect `.editorconfig` rules (tabs and CRLF for `HDLG winforms`; 4 spaces and CRLF for `HDLG file property`, `HDLG.Tests`, and `Benchmark`).
5. **Testing**:
   - Every new feature or modified behavior must have unit tests in `HDLG.Tests` (xUnit v3, FluentAssertions, Moq).
   - UI tests must run on an STA thread (`RunSta`) within `WinFormsUiTestCollection`.
   - Never show modal dialogs in tests; mock/inject test delegates instead.
6. **Build quality**:
   - `dotnet build HDLG.sln` must yield **0 errors, 0 warnings**.
   - `dotnet test HDLG.sln` must pass **100% of tests**.

## Git Conventions

- Never commit directly to `main` or `master`. Always check `git branch --show-current` first.
- Use Conventional Commits in English (`feat:`, `fix:`, `refactor:`, `chore:`, etc.).
- Sign commits with a standard trailer in the commit body:
  `Generated-by: <ToolName> (<ModelName>)`
  (e.g., `Generated-by: GitHub Copilot (GPT-4o)`).

## Critical Rule: Google Labs Jules Pull Requests

Before interacting with any Pull Request, inspect its metadata:
`gh pr view <id> --json author,body,commits`

Treat the PR as created by **Google Labs Jules** if any of the following is true:
- `author.login` is `google-labs-jules` or `google-labs-jules[bot]`.
- The PR description (`body`) contains `PR created automatically by Jules`.
- Any commit has Jules as author or co-author (`google-labs-jules` or `google-labs-jules[bot]`).

When working on a Jules PR:
- **STRICTLY FORBIDDEN to post comments or PR reviews**: Do NOT post any general comment, review comment, diff comment, thread reply, or GitHub review (`APPROVE`, `REQUEST_CHANGES`, `COMMENT`), even empty, to avoid waking up or desynchronizing Jules.
- **No active notifications**: Never mention `@google-labs-jules` or `@google-labs-jules[bot]` in descriptions or commit messages (always wrap in backticks).
- **Update description only**: To document changes, update only the PR body/description (`gh pr edit <id> --body ...`), preserving the initial content and appending a section at the bottom (e.g. `### Modifications apportées`).
- Commits and pushes (`git push`) to the PR branch are allowed when requested.
