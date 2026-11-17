# 2026-11-14 — Niamh Motion v8 Unity Trial

**Project / readers:** The Fairy Hills; Unity Codex on the project computer, project owner, students, and future agents.
**Purpose:** Integrate the user-reviewed Niamh v8 motion study into the existing game as a reversible playable trial.
**Status (2026-11-14):** Imported, assembled, hooked into Core, compiled and tried in the actual project's Village and Forest. Owner completed review and said the result is basically OK apart from some stutter. Stutter cause is unverified; no separate run-speed measurement was captured. See the result log below; the implementation outline is retained as the original brief.
**Design source:** `TechArtAsset/Characters/Niamh/Motion_v8/`.
**Unity target:** `TheFairyHill2D`, `Core_Persistent` player used by `Ch1_S1_Village` and `Ch1_S1b_ForestPath`.

## Asset transfer route (network-first)

- The six source PNGs and `motion_preview_spec.json` are already staged under `Assets/_Project/Art/Characters/Niamh/Motion_v8_Source/`. Use these project files; do not download art from GitHub or copy the same files again.
- If a listed input is missing or corrupt, send the maintainer its exact design-repository relative path, purpose, target Unity path, and needed format. The maintainer will place the approved file in the shared design repository under `TechnicalArt/unity_drop/<date_task>/` and notify you.
- Resolve that relative path from the shared repository root available on the Unity workstation. Network drive letters and computer-specific absolute paths vary. Copy only requested files into the Unity project; never move, delete, or edit shared source files. Record the copied files and source version in the result log.
- GitHub remains for repository synchronization, code review, and backup. Use it for art transfer only if the shared network is unavailable and the maintainer explicitly selects that fallback.

For the general student / future-agent procedure, read `Docs/AI/ART_ASSET_TRANSFER.md` and design repo `TechnicalArt/UNITY_AGENT_RULES.md`.

## Read first

1. Unity repo root `AGENTS.md`, `Docs/AI/UnityProjectContext.md`, and current `git status --short` plus diffs.
2. Design repo `README.zh-CN.md`, `ART_HANDOFF.md` §0 / §6.3 / latest change log, `TechnicalArt/CH1_UNITY_PROGRESS.md`, and `TechnicalArt/UNITY_AGENT_RULES.md`.
3. `TechnicalArt/NIAMH_RIG_PARTS.md`, `TechnicalArt/NIAMH_ANIM_LIST_DEMO.md`, `TechnicalArt/SCENE_SCALE_SPEC.md`, `TechArtAsset/Characters/Niamh/Motion_v8/README.md`, and `TechArtAsset/Characters/Niamh/Motion_v8/UNITY_TRIAL_README.md`.
4. Unity's existing `PlayerController2D.cs`, `Chapter1SceneBuilder.cs`, `Core_Persistent.unity`, and relevant art import settings. Check the latest actual scene and code before assuming this task's snapshot is current.

Design repo is read-only for students and agents. Do not alter or move its Photoshop source, snapshots, original PNG parts, user references, or previews. The files already copied to Unity are the only source parts for this task.

## Goal

Create a reversible layered Niamh prefab with a basic Animator trial and hook it to the existing persistent player so the owner can try idle, walk, run, jump ascent / descent, and landing in Village and Forest. The current root remains authoritative for gameplay. This trial should let the owner judge the art and motion in the real character scale, lighting, and controls.

This is a **six-part transform-puppet trial**, not completion of the planned 22-bone 2D Animation rig. The available art has one full leg, one boot, one arm, one head, one back-hair piece, and one combined body/dress/cape. Do not invent separate elbow/knee/cape/hair artwork. A duplicate far leg/boot with a darker tint is acceptable for this trial and must be identified as derived.

## Inputs already staged

- `Assets/_Project/Art/Characters/Niamh/Motion_v8_Source/parts/{head,hair_back,body,arm,leg,boot}.png`
- `Assets/_Project/Art/Characters/Niamh/Motion_v8_Source/motion_preview_spec.json`
- Visual reference only: `Docs/References/Niamh_Motion_v8/Niamh_motion_v8.gif` and `Niamh_motion_v8_contact.png`
- Unity-side background note: `Docs/Niamh_Motion_v8_Unity_Handoff.md`

All six art parts are RGBA images on the exact shared 327 × 699 canvas. Import as individual Sprites, retain alpha, and keep common dimensions/origin so the layers align. Existing static Niamh is 327 × 699, 100 PPU, bottom-center pivot, and displayed at 0.30 scale (~2.1u). Keep the new puppet the same visible height. JSON pivots use a top-left origin; convert `yUnityPx = 699 - yJsonPx`:

| Pivot | JSON px | Unity bottom-left px |
|---|---:|---:|
| Head | (180, 214) | (180, 485) |
| Arm / shoulder | (219, 323) | (219, 376) |
| Hip | (187, 532) | (187, 167) |
| Knee / boot | (185, 579) | (185, 120) |

Use the rest of the pivot / motion details and known limitations in the staged JSON and Unity-side handoff. Treat these as starting landmarks, then inspect and adjust the pivot visually.

## Protect existing work

- Current observed Unity worktree already has a modified Village scene, two modified TMP font assets, and untracked `AGENTS.md` / `Docs/AI/`. Re-run status and inspect diffs because this can change. Do not reset, overwrite, stage, or commit those unrelated changes.
- Do not rebuild Chapter 1 scenes through the builder for this trial. That builder creates the static visual and can overwrite manual scene settings.
- Preserve `Core_Persistent` player root, `Rigidbody2D`, `CapsuleCollider2D` size `(0.23, 2.08)` / offset `(0.08, 1.04)`, movement/input/interaction scripts, camera, HUD, light, audio listener, and level routing.
- Prefer a new prefab and Animator assets, with only the narrow Core/player visual hookup needed for the test. Save only the intended Core scene change if required. Keep the old static SpriteRenderer or prefab reference available as a one-click rollback path.
- Keep original visual sorting relationship with environment. The old player sprite sorts at order 10; inspect overlap against foreground layers and village gate, and adjust only the puppet part orders as needed.
- Do not install packages or attempt conversion that edits the user PSD. PNGs are already separated; use Unity's Sprite import and a simple hierarchy unless project inspection shows a specific safe alternative.

## Implementation outline

1. Verify each staged source PNG and JSON are present. Import each PNG as a Sprite with alpha, common 100 PPU, compression suitable for hand-painted art, and the same bottom-center origin. Confirm the transparent parts overlay the static source without offsets.
2. Create a clearly named trial prefab under `Assets/_Project/Art/Characters/Niamh/` with a root visual scale matching the old 0.30 static child. Preserve one common canvas origin for all sprite layers. A reasonable draw order from back to front is far leg/boot, near leg/boot, back hair, body, arm, head; inspect actual source overlap and correct if necessary. Derive a far leg by duplicating the leg/boot SpriteRenderer in Unity, tinting it around 0.86, and offsetting it only if needed to match the reference. Do not alter the source.
3. Place part transforms at the converted pivot landmarks, using parent pivots and child artwork offsets so the initial composite exactly matches the static pose. Add modest, clear clips: idle; walk (~0.8 s loop); run (~0.5 s loop); jump ascent / apex; descent; landing/recovery. These timings are art-study starting points, not fixed game durations.
4. Connect presentation to the existing player's state with the smallest compatible change. Keep movement and physics in `PlayerController2D`. Prefer existing state / velocity / grounded data; do not duplicate jump physics. `flipX` must still face left correctly with all part renderers. Idle and locomotion loops should follow horizontal speed; jump pose should follow the real grounded and vertical-velocity phases; land should be brief and return to idle / locomotion. Ensure transitions can be interrupted and do not lock player input.
5. Test seams and overlap against bright magenta in the prefab/staging view, then inspect the actual game size and both scene lighting palettes. Specifically review arm backdrop during the user's requested wide swing, leg/boot seam, head pivot, cape/arm occlusion, floor contact, and facing left/right.

## Acceptance

- Unity imports assets without missing sprites or new Console errors; compilation completes.
- Initial assembled silhouette aligns to the static reference at about 2.1u height. No visible shift between same-canvas pieces at rest.
- Animator responds while the original controls still work: idle, walk, Shift-run, jump upward, fall, land, and face either direction.
- Ascent reads as legs back / arm forward / head slightly up; descent reads as head down / arm back / legs forward. Landing compression stays small and feet remain grounded.
- Play Mode review in both `Ch1_S1_Village` and `Ch1_S1b_ForestPath`; verify collisions, gaps, gate interaction and transitions still work. Capture screenshots or a short recording at actual game scale, plus a magenta seam check.
- Save the prefab, clips/controller, necessary source `.meta` files, and only necessary scene / runtime code changes. Update Unity `Docs/CH1_UNITY_PROGRESS.md` and `Docs/Niamh_Motion_v8_Unity_Handoff.md`; update the design progress / lesson only with what the Unity evidence actually proves.
- Report the Git status and diff. If pushing is possible, commit and push **only this task's paths**; do not include the pre-existing modified Village/TMP files or untracked AI docs unless they are directly changed by this task. Record the commit hash and branch.

## Direct message to the Unity Codex

Paste this task into a new Codex conversation opened on the `TheFairyHill2D` project. The UNC path is only a location on the maintainer's computer; use the project root opened in your workspace. Begin by reading this handoff and the linked project instructions. Then execute the task, not just summarize or rewrite the plan. If the Unity Editor connection or Play Mode is unavailable, complete all safe import/prefab work possible and clearly report the exact remaining checks; never claim a Play Mode pass without one.

## Result log — fill after Unity work

### Execution plan — 2026-11-14, Unity workstation

- Baseline: Unity 6000.3.24f1, official Unity MCP confirmed `Application.dataPath` in this checkout; MainMenu open, not playing, no unsaved scene. Console error baseline: 0. Git HEAD `a0e96ff`; preserve Village and both modified TMP font assets, and the existing untracked `Docs/AI/UnityProjectContext.md`.
- Use the six existing RGBA 327 × 699 PNGs and JSON; no new art transfer. Import at 100 PPU, bottom-center pivot, full canvas, uncompressed alpha. Build a separate six-part Transform puppet (derived far leg/boot), seven Animator states, and an explicit presentation component.
- Expose only read-only grounded/facing/speed information from PlayerController2D. Keep its input, movement, physics and interaction authority. Preserve the old static renderer as the facing reference and rollback visual. Group the puppet at original sorting order 10.
- Save only the narrow Core visual hookup. Do not run Chapter1SceneBuilder or rebuild/save Village or Forest. Keep the rig generator separate and rerunnable; it must refuse dirty Core edits.
- Inspect Unity imports, compilation, rig references and magenta seams; then inspect the actual open project's Play Mode in Village and Forest. Automated test suites and duplicate projects remain paused under UNITY_AGENT_RULES.md. Record observed checks separately from owner acceptance and unperformed input/route checks.
- Update this log and CH1_UNITY_PROGRESS. Keep proposed design progress/lessons in local Docs for the maintainer; the design repository stays read-only. Commit/push an explicit task allowlist and verify the remote ref.

- Imported: All six existing PNGs confirmed RGBA 327 × 699; individual full-rectangle Sprites, 100 PPU, pivot `(163.5, 0)` px, alpha, Bilinear, Clamp, no mipmaps, uncompressed. PNG contents were not edited or recopied; generated source/folder `.meta` files belong to this task.
- Prefab / clips / controller: `Assets/_Project/Art/Characters/Niamh/Motion_v8_Trial/Niamh_Motion_v8_Trial.prefab`; `Animation/{Idle,Walk,Run,JumpUp,Apex,Fall,Land}.anim` and `Niamh_Motion_v8.controller`. Eight renderers use the six originals: far leg and boot are duplicates tinted 0.86. Hip/head/shoulder/knee pivots convert the JSON top-left coordinates. Far/near leg offsets are -9/+4 source px, matching the preview. Root scale is 0.30. SortingGroup order 10 preserves the original relationship with foreground order 12. All game materials match the old `Sprite-Lit-Default`; the diagnostic sheet temporarily uses unlit rendering.
- Player and scene hookup: Added `NiamhMotionV8Presentation.cs`; PlayerController2D only gains read-only grounded, facing and speed properties plus assignment of its existing grounded result. Original input, physics, interaction and scene-loader logic are unchanged. Core gains one nested trial prefab and its references; original static child/reference remains. No Chapter1SceneBuilder run or level scene save/rebuild occurred. Capsule remains `(0.23, 2.08)`, offset `(0.08, 1.04)`; one gameplay AudioListener.
- Compilation / Console: Unity Editor compilation successful (`scriptCompilationFailed=False`). Error baseline was 0; no trial errors observed at inspection. Unity AI certificate/account warnings are separate tooling warnings. No automated test suite, batchmode test, duplicate project, or standalone build was run.
- Village Play Mode: Opened the saved Village in the actual Editor. Confirmed Core loads, eight trial renderers enable, old renderer hides, grounded idle continues over many normalized animation cycles; 1920 × 1080 screenshot saved. Later live observations record Village Walk/JumpUp/Apex/Fall/Land and the subsequent Village → Forest scene switch at game time 175.363 s while the owner was playing. This later switch was not invoked by the agent. Gate open/close was not separately instrumented.
- Forest Play Mode: Called the existing `ChapterSceneLoader.LoadLevel` for a direct additive-loader inspection; this first switch does not prove an exit trigger. Core player survives; one listener; Global Light RGB approximately `(0.408, 0.471, 0.627)`, intensity 0.28. Subsequent live user input produced Walk, JumpUp, Apex, Fall, Land and Idle, with left/right facing and observed walking velocity ±3.2. Logs include landings at y=-1.6/-1.8 and jumping back out. This evidence does not establish every collision, intended run-jump gap clearance, gate, pickup, or exit trigger.
- Screenshots or recording: `Docs/References/Niamh_Motion_v8/Unity_Trial/` contains Unity-rendered magenta poses (top: Idle/Walk/Run/JumpUp; bottom: Apex/Fall/Land/Fall-left), actual Game view screenshots, `Asset_Validation.txt` and live state observations. No video was recorded. Named state screenshots capture state entry and can include the 0.06-second crossfade; use the magenta sheet for full pose extremes. The original offline GIF/contact sheet are unchanged.
- User review: Owner completed the session and reported “已经试玩结束，除了些卡顿，可能是各种软件一起运行的问题，其他基本ok”. Record basic trial acceptance with a stutter concern. The owner's suggested cause (many applications running) has not been profiled. Screenshot capture was also active during the review and may add overhead; this is another unverified contributor. No per-feature acceptance beyond the owner's overall statement is inferred. Play Mode was stopped after this feedback; Village remains open, no level scene was saved.
- Known limits / next action: Six rigid parts only; no elbow/knee skinning or independent dress/cape. Modest landing affects upper pieces (3/4 px), leaving feet fixed; horizontal input can interrupt recovery after 0.10 s and jumping interrupts immediately. Walk/run curves compensate boot alpha bounds for floor contact but do not implement planted-foot IK. Fall is retimed to 0.32 s for the controller's real jump, not the offline 2 s loop. Recorded live states do not include Run/5.6u/s, so that specific check remains uncaptured despite the overall basic acceptance. If investigating stutter, first compare a review without screenshot capture and with other applications closed, then profile the actual project; do not diagnose from this session alone. Do not rebuild Chapter 1; if a later builder rebuild removes the hookup, use the dedicated install command below.
- Git branch / commit / push: `main`; plan checkpoint `bb77510`; implementation `1374a0fa3b982491aa596a2897b25b9a7c8bc93b`. Task allowlist committed and pushed to `origin/main`; `git ls-remote` confirmed the same implementation hash. First push hit a TLS bad-record-MAC error and did not update the remote; a bounded retry with `git -c http.version=HTTP/1.1 push origin main` succeeded. This final publication note is a separate documentation commit.

### Reproduce or roll back

1. Assets are already built. To regenerate explicitly, use **Tools → The Fairy Hills → Niamh v8 → 1 Build Trial Assets** outside Play Mode. This reads the staged JSON/PNGs and updates only trial assets/import settings.
2. **2 Install In Core Only** opens Core additively if necessary and saves only that scene; it refuses an already dirty Core and skips an existing hookup. It does not rebuild either level. **3 Capture Magenta Pose Sheet** makes a temporary preview scene/camera and closes it, preserving the open scene.
3. Enter Village or Forest Play Mode with the normal controls. In Edit Mode the static renderer stays visible and trial parts stay hidden; the presentation component switches them in Play Mode.
4. For a one-click runtime rollback, disable `Niamh_Motion_v8_Trial` under Niamh, or disable its `NiamhMotionV8Presentation` component. It hides all puppet renderers and restores the static renderer; PlayerController2D still updates its facing. Re-enable the trial to resume. To persist the rollback, disable that child in Edit Mode and save only Core.

### Protected work and maintainer handoff

- The original Village scene, LiberationSans fallback, and untracked `Docs/AI/UnityProjectContext.md` remained byte-identical to the execution baseline at the post-install check. Fondamento was already modified at baseline and its dynamic cache changed further during Editor operations; it remains unstaged and uncommitted. No user edits were reset.
- Found `runInBackground: 0` despite the explicit design rule. Enabled the live setting and saved the single `runInBackground: 1` line in `ProjectSettings/ProjectSettings.asset` after the review. This prevents the next session from stopping when the Editor loses focus; it is not a stutter fix or performance measurement.
- Design source is read-only. `Docs/Niamh_Motion_v8_Design_Update.md` contains a factual update/lesson proposal for the maintainer to sync; no shared design files were changed.


## 2026-11-17 — Front garment ornament update

The owner requested matching the additional gold front-panel ornament from the new raised-cross artwork across the chest action and existing character. Only the body texture at `Assets/_Project/Art/Characters/Niamh/Motion_v8_Source/parts/body.png` and the static fallback `Assets/_Project/Art/Characters/Niamh/Niamh_side_static_v3.png` were copied from the verified design source through the shared network. The trial prefab references the body texture GUID `8799bb43cb941f94aa586e3b320b0c2c`. Both images remain 327 × 699; all alpha bytes and visible pixels outside the ornament patch are unchanged. `.meta` hashes, GUIDs, import settings, prefab, clips, scenes, physics and controller files were preserved.

Source overlay: design `TechArtAsset/Characters/Niamh/Motion_v8/appearance_front_ornament_v1/`. The original user-source PSD snapshot remains historical; use the new editable overlay sources or current parts PNGs for this appearance. Copy manifest and hashes: `Docs/References/Niamh_Front_Ornament_2026-11-17/sync_manifest.json`. Backups are in the design task drop `TechnicalArt/unity_drop/2026-11-17_Niamh_Front_Ornament/previous/`.

Validation status: asset identity, dimensions, alpha, localized pixel change, prefab GUID reference and unchanged meta verified from repository files. No connected Editor/Console or Play Mode observation was available in this session, so import completion and in-game appearance are not claimed. No scripts were changed or automated tests run. Review the new ornament in Village/Forest when the Editor refreshes. This update does not integrate the new cross poses, animation or beam VFX.
