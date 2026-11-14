# Unity trial evidence — 2026-11-14

This directory contains actual Unity output for the six-part Niamh v8 trial. The parent directory's GIF/contact sheet remain the original offline references. Source parts and the JSON were already staged in commit `17d61b3`; `Source_SHA256.txt` records the unchanged PNGs used here.

- `Unity_Magenta_Poses.png`: Unity-rendered preview scene, unlit on magenta. Top row: Idle, Walk, Run, JumpUp. Bottom: Apex, Fall, Land, Fall facing left. The matching `Seam_*.png` files are individual cells. These inspect pose geometry, not gameplay input.
- `Village_Live_Idle.png`: actual Village Play Mode Game view at 1920 × 1080.
- `Forest_Live_*.png`: actual Forest Game view. `Additional_JumpUp` was initially captured under an idle filename, but the simultaneous log showed airborne movement; it was relabeled to preserve the true observation.
- `Observed_Ch1_*.png`: scene-aware captures during the owner's live keyboard review. State screenshots record state entry and can show the 0.06 s blend; they are not guarantees of the full pose extreme. Use the magenta sheet for full pose inspection.
- `Asset_Validation.txt`: imports, materials, capsule, sorting, Core persistence, listener and Forest-light values read from the actual Editor.
- `Live_Input_Observation.txt` and `Live_Scene_Observation.txt`: state-change observations during the live review. No gameplay inputs or assertions were injected by these observers; no automated test suite was run. The first observer had a fixed Village filename prefix even after switching scenes; files were renamed to Forest using its scene log. The second observer uses scene-qualified names.

Owner completed review and said the trial was basically OK except for some stutter. Cause is unverified; other running applications and live PNG capture are possible contributors, not established causes. Run/5.6u/s was not captured in the state log. The owner's later Village → Forest scene switch was observed at game time 175.363 s.

End state: Editor stopped, Village open without unsaved changes, compilation successful, eight prefab renderers and no missing scripts/sprites; one-line Run In Background setting enabled. Full implementation, rollback and evidence limits: `Docs/Niamh_Motion_v8_Unity_Handoff.md`.
