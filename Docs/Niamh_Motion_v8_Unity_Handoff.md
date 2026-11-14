# 2026-11-14 — Niamh Motion v8 Unity Trial

**Project / readers:** The Fairy Hills; Unity Codex on the project computer, project owner, students, and future agents.
**Purpose:** Integrate the user-reviewed Niamh v8 motion study into the existing game as a reversible playable trial.
**Status:** Task ready. Six source PNGs and the JSON spec have been copied to the Unity repository. Unity import, rig, Animator, player hookup, compilation, Play Mode, and user acceptance are pending.
**Design source:** `TechArtAsset/Characters/Niamh/Motion_v8/`.
**Unity target:** `TheFairyHill2D`, `Core_Persistent` player used by `Ch1_S1_Village` and `Ch1_S1b_ForestPath`.

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

- Imported:
- Prefab / clips / controller:
- Player and scene hookup:
- Compilation / Console:
- Village Play Mode:
- Forest Play Mode:
- Screenshots or recording:
- User review:
- Known limits / next action:
- Git branch / commit / push:
