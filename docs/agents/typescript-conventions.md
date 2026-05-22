# Typescript guidelines

- Use strict TypeScript
- Avoid using any
- Prefer unknown for untrusted input, then narrow with guards or schemas
- After TypeScript changes, run bun check and fix all errors
- Avoid regexes for format guards — the `detect-unsafe-regex` ESLint rule fires on patterns like `^\d+(\.\d+)?$` even when they are safe. Use character-exclusion checks instead (e.g. `str.includes('e')`, `str.includes('x')`) or char-by-char whitelisting.
