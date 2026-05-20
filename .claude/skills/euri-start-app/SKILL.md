---
name: start-app
description: Launches the dev stack (web + api) via `scripts/start.sh` in a Monitor background task and reports the Web and API URLs.
model: haiku
disable-model-invocation: true
---

# start-app

Start the dev stack and report URLs.

## When to invoke

ONLY when the user explicitly types `/start-app`. Do not auto-trigger on phrases like "start the app", "run the server", etc. — those should be handled normally without this skill.

## Steps

1. Launch `scripts/start.sh` with the **Monitor** tool, `persistent: true`. The filter reports the two URL summary lines once at startup, then nothing until something fails — routine readiness chatter (`Local:`, `ready in`, `Now listening`, `Application started`) is dropped. Use this exact command:

   ```bash
   bash scripts/start.sh 2>&1 | grep -E --line-buffered "Web : |API : |error|Error|ERROR|fail|Fail|FAIL|Exception|Traceback|EADDRINUSE|✗"
   ```

   Description: `app startup: URLs + errors only`. Timeout: max (`3600000`).

2. Call `TaskOutput` with `block: true`, `timeout: 15000` to capture the first batch of events.

3. Parse `Web : http://localhost:<port>` and `API : http://localhost:<port>` from output.

4. Report to the user in this exact shape (one terse block, no preamble):

   ```
   App running (monitor task `<task_id>`):

   - **Web**: http://localhost:<web_port>
   - **API**: http://localhost:<api_port>/openapi

   Stop with TaskStop on `<task_id>`.
   ```

## Notes

- The script auto-picks: web in `3000-3005`, api random in `5200-5999`. Do not hardcode ports.
- If URLs don't appear within the 15s window, surface whatever error lines came through and stop.
- Do not run `bun dev`, `dotnet run`, or any alternate launcher — only `scripts/start.sh`.
- After the initial URL report, do not relay routine monitor events to the user — only surface notifications that match error patterns (`error`, `Exception`, `EADDRINUSE`, `FAIL`, `✗`, etc.). Summarise instead of dumping log lines.
