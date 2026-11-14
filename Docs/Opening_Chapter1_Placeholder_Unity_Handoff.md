# Opening and Chapter I placeholder trial — 2026-11-14

Project: The Fairy Hills / Unity 6000.3.24f1. Readers: owner, production, students and future agents. Source brief and immutable snapshots: `Docs/References/Opening_Chapter1_Placeholder_v1/`.

## Execution baseline and plan

- Starting HEAD `3d1b9e6`, branch `main`; actual MCP target confirmed. MainMenu open, saved, Editor stopped. Existing AGENTS, Village and two font changes plus untracked AI context are preserved. Baseline copies are local under ignored `Temp/OpeningTrialBaseline/`.
- All eight opening manifest files match SHA-256 and byte counts. Use the staged image unchanged; OP02/03 reuse it and remain placeholder art. No art download or source edit.
- Batch 1: dedicated configurable Opening scene between menu Play and Village, independent TMP captions, Next/Skip, Chapter I card, debounce, opaque load mask, input-release guard, existing music fades. Standalone level debug bypasses it. No level/Core rebuild. Preserve timeScale=1.
- Batch 2 (separate commit after opening checkpoint): verify 23 OGG files; add opt-out motion audio reading the controller's true ground/jump information, explicit persistent surface mappings, and hay brush proximity without changing colliders.
- Compile and observe actual-project Play Mode. Automated suites and temporary projects remain paused. Capture actual output and report command-invoked paths separately from real keyboard/mouse and owner acceptance. No Recorder package is installed; screenshots are the allowed fallback.
- Update local handoffs/progress/current context; design repository stays read-only. Commit/push task allowlists, verify remote, finish stopped with only MainMenu open.

## Results

Implementation, validation and owner acceptance pending while work is in progress.
