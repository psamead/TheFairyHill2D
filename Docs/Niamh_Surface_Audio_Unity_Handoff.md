# Niamh surface audio trial — 2026-11-14

Project: The Fairy Hills / Unity 6000.3.24f1. Readers: owner, production, students and future agents. Status: **imported, integrated, compiled and observed in the actual Editor; audible mix/owner acceptance pending**. This is the canonical result requested by the opening task. The original versioned task is `Docs/References/Niamh_Surface_Audio_v1/2026-11-14_Niamh_Surface_Audio_Unity_Trial.md`.

## Implementation and configuration

- All 23 pre-staged OGGs match `transfer_manifest.json`; none recopied or edited. Imported mono, Decompress On Load, preload on, Vorbis quality 0.85. No WAV master, listening reel or new package included.
- `NiamhSurfaceAudio` is a separately disableable component on the existing Core Niamh. Core saved only a 20-line component/reference addition; existing capsule, rigidbody, v8 prefab and curves unchanged. Three reusable runtime 2D AudioSources handle footsteps, actions and brush, with no added listener.
- PlayerController2D retains the nearest valid collider/point from its **existing** ground cast and emits `Jumped` only inside its existing successful jump branch. Movement/input/physics tuning is unchanged. Audio reads that grounded state, animator phase and velocity; it does not perform another ground cast.
- Steps follow v8 Walk/Run contacts at phases .25/.75, gate on grounded/moving/not-paused and use a 0.12 s minimum interval. All three variants of a surface avoid immediate repeats; pitch 0.97–1.03 and volume factor 0.96–1.04. Current curves remain unchanged.
- Jump uses the actual event. Landing requires an observed air interval of at least 0.08 s and descent at least 1.5u/s; heavy uses descent ≥10u/s. Initial settling/near-zero changes do not emit landing. Jump/Land are generic clips, without pretending there are separate land recordings for every material; this first mix does not add extra Step layers on top of them.
- Hay brush checks proximity between the player and the existing solid hay collider (≤0.18u), moving only, at most every 0.55 s. No trigger, wood crate, collision geometry or route was added. Wood samples are assigned but unused until a real mapped wood surface exists.
- Volumes are editable on the Core component: walk 0.34, run 0.43, jump 0.36, light land 0.40, heavy land 0.52, brush 0.20. These are mix starting points under existing BGM 0.5, not owner-approved final values.

## Explicit persistent surface mapping

Source is `Assets/_Project/Data/Layouts/Chapter1_SurfaceAudio_v1.json`; `NiamhSurfaceAudioBuilder` imports it into `Assets/_Project/Resources/NiamhSurfaceAudio_v1.asset` with clip references. `LevelBootstrap` applies mappings to exact collider paths on each load; `GroundSurface2D` then resolves material from data/contact X. Names locate explicitly configured objects; material is never guessed from a name substring.

| Scene / existing collider | Trial material |
|---|---|
| Village ground | Dirt; grass at x [0,2.5), [30,34) |
| Village low/tall stone walls | Stone |
| Village solid hay bale | Hay + proximity brush |
| Forest ground 0–30 | Grass; dirt at x [8,14), [24,29) |
| Forest ground 31.4–42 | Grass |
| Forest ground 44.6–60 | Grass; dirt at x [48,52) |
| Both Forest pit floors | Dirt |

These are authored trial classifications, not proof of material semantics from the painted background. Owner should judge grass/dirt boundaries. The dedicated JSON/generator is persistent; Chapter1SceneBuilder is not run or changed. Rebuilding the existing levels later still reapplies the bootstrap mapping; renamed collider paths produce a warning requiring a data update. Village and Forest files were not saved.

## Actual Editor evidence

`Docs/References/Niamh_Surface_Audio_v1/Unity_Trial/Live_Cue_Observation.txt` records map/collider bounds and cue counts. Controls were exercised by explicitly queued Input System keyboard states on the actual controller, then released; this was not hardware keyboard listening or an automated test suite. Some short sound-selection checks used clearly logged placements on existing colliders, not natural obstacle clearance.

- Village starts grounded with **0 steps / 0 jumps / 0 lands**. Four marker components attach to the intended colliders.
- Actual walk velocity 3.2u/s: dirt steps approximately 0.40 s apart; observed variants 03→02→01→02. Shift-run at 5.6u/s: stone steps approximately 0.25 s apart, 03→02. No continuous step while stopped against a wall.
- Real jump input produced one successful jump cue, followed by one light landing. Staged drop from y4 selected heavy landing. Staged grass/hay-top walks produced corresponding Step Grass / Step Hay, and brush variants alternated while near the existing hay collider.
- Pause at timeScale 0: counts stayed 14/1/5/4 and all motion sources stopped; restored to **timeScale 1**. Disabling the audio component stopped its sources and left the player enabled. Re-enabled without source duplication.
- Forest load retained Core, v8 and one AudioListener; five mapped colliders confirmed. Staged small-pit contact resolved the actual Dirt marker. This does not independently prove a natural running clearance of the large gap or final sound balance.
- Integrated opening replay then Village: cue counts 0/0/0 at handoff, original spawn `(3.5,0)`, cross false and shamrocks 0; one looping Level Music at volume 0.5, one listener, no opening object/input lock.
- Unity compilation succeeded. An initial compiler error used obsolete `AudioImporter.preloadAudioData`; moved to sample settings and recompiled successfully. Console also retains earlier diagnostic errors from settings-save and a mistimed opening probe, identified in the opening handoff; no new unresolved gameplay error observed. No automated suite, duplicate project, standalone build or recorded audible clip.

## Rollback and remaining review

Disable `NiamhSurfaceAudio` on Core Niamh to turn off only the new movement sounds; original gate/pickup/thorn/BGM systems remain separate. Re-enable to resume. To rebuild imports/profile and narrow Core hookup, outside Play Mode run **Tools → The Fairy Hills → Audio → Install Niamh Surface Audio Only**; it refuses an unsaved Core.

Owner still needs to hear foot timing and material differences under BGM, judge levels/repetition/brush rate, confirm rapid jumps/failed jump input/uneven contacts, and try the complete menu→Village→Forest route with existing gate, pickup and thorn sounds. API playback/counters do not prove audible output. Any future refined art or animation events may require retiming the contact phases. Design repository remains read-only; this handoff is the maintainer's factual sync source.

## Publication

Opening checkpoint `673bbcc`; sound implementation is a separate task-only commit. Final commit/push and working-tree preservation are recorded in the opening handoff after verification.
