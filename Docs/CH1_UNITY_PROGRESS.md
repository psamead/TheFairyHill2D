# Chapter 1 Unity Progress — S1 Village and Forest Path

Date: 2026-10-30
Project: `C:\Users\Administrator\Documents\UnityProject\TheFairyHill2D`
Shared design source: `TechnicalArt/CH1_UNITY_HANDOFF.md`, `CommandForAgentToInitialRun/2026-10-29_Ch1_S1_Forest_Scenes.md`, and `TechnicalArt/SCENE_SCALE_SPEC.md`.

## Delivered

- Created `Assets/Scenes/Ch1_S1_Village.unity` and `Assets/Scenes/Ch1_S1b_ForestPath.unity`.
- MainMenu Start now loads `Ch1_S1_Village`; the village exit loads the forest; the forest endpoint shows a prompt and returns to MainMenu on E. All three routes were exercised in Play Mode.
- Added the 19 requested art/layout files by copy only. No source files, `_src`, `_ditch_fix`, previews, or `deprecated` material were copied or edited.
- Implemented JSON-driven ground and pit colliders, static Niamh, keyboard movement/jump, horizontal camera follow, parallax, village props/gate, Iron Cross pickup, thornwall and a smoke/fade IronPulse placeholder.
- Added both chapter scenes to Build Settings. `SampleScene` was present at initial delivery, then deleted by the user on 2026-10-30 and removed from Build Settings.

## Scene and input settings

- PPU 100; background pivot `(0, 270 / source height)`; props bottom-left; Niamh bottom-center; Max Size 8192; compressed at quality 100.
- Camera orthographic size 5.4, fixed Y 2.7, horizontal follow; scene ground y=0.
- Walk 3.2 u/s, run 5.6 u/s, jump velocity 12, rising/falling gravity 30/45, coyote 0.10s, jump buffer 0.12s. Controls: A/D or arrows, Shift run, Space jump, E interact, Q IronPulse.
- Unity 6000.3.24f1 project did not have Cinemachine in `Packages/manifest.json`; used the small `ChapterCameraFollow` component to provide the documented fixed-Y horizontal camera behavior. No package was installed.
- TextMesh Pro dynamic font assets gained the left/right arrow glyphs and slash needed by the controls label; keep these imported glyph updates.
- Unity 6 requires custom sprite alignment/pivot to be applied through `TextureImporterSettings`; assigning `spritePivot` alone left sprites centered. The importer now sets Custom alignment and the requested pivot through importer settings. Nested ground coordinate arrays are parsed explicitly because Unity `JsonUtility` does not deserialize the raw jagged-array layout as expected.

## Manual collision update — 2026-10-30

- The user manually refined the `StoneWall_Tall_1.8u` BoxCollider2D in `Ch1_S1_Village`. Its saved values are Offset `(2.0378294, 0.9)`, Size `(3.1901226, 1.8)`, with the GameObject at `(16.8, 0, 0)`. This gives world bounds approximately X `17.2428–20.4329`, Y `0–1.8`; the collider is narrower than the 4.19u sprite to follow the visible wall shape.
- The scene asset on disk contains these values. The scene builder still creates a full-width collider from the layout block dimensions, so rebuilding Chapter 1 scenes will replace this manual override. Preserve/reapply these values or encode an explicit collider override in the builder before rebuilding.

## Hay bale collision alignment — 2026-10-30

- The user refined the `HayBale_0.9u` BoxCollider2D in `Ch1_S1_Village`. Current saved values: Offset `(-0.15596342, 0.42)`, Size `(1.3790255, 0.84)`. The GameObject remains at `(15, 0, 0)`.
- Updated `Chapter1SceneBuilder.CreateBlock` to recreate these exact scene collider values for the hay bale; other block colliders remain layout-sized. Player/controller code has no position-specific hay-bale assumptions and did not need a change.
- The layout JSON still has the original generated bale placement. Rebuilding Chapter 1 scenes restores that position; update the JSON separately if the moved bale position should become canonical.
## SampleScene removal — 2026-10-30

- The user deleted `Assets/Scenes/SampleScene.unity`. No runtime script directly references it. Its stale Build Settings entry was removed, and the project default scene now points to `MainMenu`.
- Both scene builders now skip Build Settings entries whose scene assets no longer exist, so rebuilding will not re-add a stale deleted-scene path.

## Play Mode checks

- 1.2u village wall: jump reached and landed on the top (x=12.33, y=1.20).
- 1.4u forest gap: walk-jump crossed (x=33.00, y=0.01).
- 2.6u forest gap: walk-jump did not clear and reached the pit floor; running jump crossed (x=48.68, y=0.00).
- 3u thornwall: blocked walking before IronPulse (player stopped at x=51.85).
- Iron Cross pickup succeeded; Q activated IronPulse, the wall faded and its collider disabled.
- MainMenu Start button callback loaded S1; the S1 exit loaded the forest; forest endpoint E returned to MainMenu.
- MainMenu, village, and forest each contain exactly one AudioListener. The chapter scenes each contain one player Rigidbody2D.

## Remaining design / production work

- Niamh uses the static image until the planned skeletal animation is ready.
- IronPulse uses a simple fade and dark particle smoke; final cold-light ring and thornwall crack treatment remain art/VFX work.
- The forest endpoint returns to MainMenu as the allowed temporary substitute while S2 is unavailable.
- No backpack/inventory UI was added; Iron Cross possession is held as a player state for this playable slice.
- The layout `lights` points and optional URP lighting/post-processing polish are not implemented yet.
- Build Settings and scene routing were checked in the Editor; no standalone PC player build was produced. MainMenu button callback was invoked directly during Play Mode, not tested with a physical mouse click.

## Code map

- Builder: `Assets/Editor/Chapter1/Chapter1SceneBuilder.cs` (Tools → The Fairy Hills → Build Chapter 1 Scenes)
- Gameplay: `Assets/_Project/Scripts/Gameplay/PlayerController2D.cs`, `PlayerInteractionTarget.cs`, `VillageGate.cs`, `IronCrossPickup.cs`, `ThornWall.cs`, `ChapterCameraFollow.cs`, `ParallaxLayer.cs`, `ChapterHUD.cs`, `ChapterSceneLoader.cs`, `ChapterSceneExit.cs`
- MainMenu destination: `Assets/_Project/Scripts/UI/MainMenu/MainMenuController.cs` and `Assets/Scenes/MainMenu.unity`


## Revision v2 implementation — 2026-10-30

Implemented from `TechnicalArt/CH1_UNITY_REVISION_v2.md` and `CommandForAgentToInitialRun/2026-10-30_Ch1_Revision_v2.md`.

- Added `Core_Persistent` with the single player, Main Camera/AudioListener, HUD, EventSystem, `GameState`, and `ChapterSceneLoader`. Chapter scenes are generated as level-only scenes with `LevelInfo`, `SpawnPoint`s, and `LevelBootstrap`; Build Settings contain MainMenu, Core, Village, and Forest, with SampleScene removed.
- Updated MainMenu Start to load the selected first chapter as a single scene. The level bootstrap then adds Core and attaches the loaded level. This avoids relying on a coroutine owned by the menu while the menu unloads.
- Split the Village gate into left post, boards, and right post render layers; added a reversible tall blocker and leave-range close behavior.
- Added Celtic frame UI, including the Settings panel frame. Added the v2 forest props and layout, near-player Iron Cross reveal with sparkle particles, and URP soft/additive particle materials.
- Applied Forest foreground alpha 0.55 / parallax 1.15, S1 L4 v2, and Forest L3b grass edge at parallax 1.0 / order -9.
- Added layout-driven collider offsets and sizes. The user's saved `StoneWall_Tall_1.8u` collider remains Offset (2.0378294, 0.9), Size (3.1901226, 1.8); the hay bale remains Offset (-0.15596342, 0.42), Size (1.3790255, 0.84). These values are stored in the local S1 layout data and respected by the builder.

### Validation status

- Unity Editor scene builder generated the Core, Village, and Forest scenes; source compilation through Unity MCP succeeded, and the Console query showed zero errors (pre-existing Unity Account API / MCP warnings remain unrelated).
- Scene structure and collider values were inspected after generation. Core has one player, one camera/listener, and one EventSystem; level scenes contain level metadata and spawn points without duplicate persistent objects.
- MainMenu Start was invoked in Play Mode and loaded Village. The Unity MCP Play Mode session reported `Time.time == 0` and did not advance lifecycle frames; therefore the automatic `LevelBootstrap` completion and full transition/gate/pickup checklist could not be confirmed through that session. An explicit additive Core load showed exactly one listener, player, and EventSystem. Recheck the complete menu-to-level bootstrap and gameplay interactions in a normally advancing Play Mode session.
- No standalone player build, commit, or push was made.

### Remaining design / art follow-up

- User visual comfort check for S1 and Forest foreground/parallax remains pending.
- Niamh skeletal animation, final IronPulse ring/crack VFX, and backpack/inventory UI remain design/production tasks. The v2 soft smoke and sparkle treatment is a technical placeholder/material setup.
- Confirm gate crossing/auto-close, Iron Cross reveal/pickup persistence across level changes, and ThornWall removal once Play Mode advances normally.
## Hay bale sprite alignment — 2026-10-30

- The user moved only the `HayBale_0.9u/Visual` child left by 1.0 unit; the root remains at `(15, 0, 0)`. The existing BoxCollider2D remains Offset `(-0.15596342, 0.42)`, Size `(1.3790255, 0.84)`. Unity reports sprite world-bounds center `(14.86, 0.45)` and collider-bounds center `(14.84, 0.42)`, aligning their horizontal centers within 0.02 units.
- Added `visual_offset_x/y` to the village layout block data and builder. `S1_Village_layout_v2.json` now stores `visual_offset_x: -1.0`, `visual_offset_y: 0.0` for the hay bale, so rebuilding preserves this sprite-only adjustment alongside the existing collider override. Other props default to zero offset.
- No player/gameplay script requires a hay-bale-specific change. The existing scene position and collider were preserved; the builder was updated to reproduce the visual offset. Scene rebuild/runtime validation was not run for this focused update.

## GitHub publication scope — 2026-10-30

- The Chapter 1 publication includes the chapter scenes, gameplay/editor code, layouts, art, font glyph updates, and progress note. Pending MainMenu scene/controller/builder edits are intentionally excluded from this push, so the GitHub MainMenu-to-Chapter-1 route remains a separate pending integration.

## Niamh collision update — 2026-11-01

- Preserved the user-adjusted capsule in `Assets/Scenes/Core_Persistent.unity`: vertical `CapsuleCollider2D`, Offset `(0.08, 1.04)`, Size `(0.23, 2.08)`, not a trigger. Niamh remains at `(3.5, 0, 0)` with a Dynamic Rigidbody2D and Gravity Scale 0. The capsule's lower edge is at y=0.
- Updated `Chapter1SceneBuilder.CreatePlayer` to use the same size and offset, so rebuilding `Core_Persistent` keeps this adjustment. No movement or collision-query code needed changes.
- The saved Core scene asset on disk contains the collider values shown in the user's screenshot. No scene rebuild or Play Mode test was run.

## Revision v3 rollback — 2026-11-01

- After the Revision v3 implementation attempt, the user reported a problem and requested a return to the pre-task project state. The v3-only scene, asset, script, test, and project-setting changes were rolled back; the earlier Niamh collider update and other pre-existing project changes were preserved.
- The problem's specific symptom and reproduction steps were not provided, so its cause was not diagnosed. Revision v3 implementation changes were not published to GitHub; the published update is the Niamh collision progress note only.

## Revision v3 — Batch A: Village UI (2026-11-01)

- Imported the two v3 gate sprites and HUD sprites as single sprites at 100 PPU with uncompressed texture data. Set 24 px 9-slice borders on the two 96×96 HUD sprites.
- Updated the builder to read `S1_Village_layout_v3.json`, create one closed/open gate sprite with JSON-driven blocker/range, and create fitted dark-backed thin-frame HUD panels. The controls hint and interaction/message panels use 12 px padding; Core has no Celtic HUD frame. The MainMenu Settings frame remains unchanged.
- Rebuilt and saved only `Core_Persistent` and `Ch1_S1_Village`. Preserved Niamh's capsule (Size `(0.23, 2.08)`, Offset `(0.08, 1.04)`). Build Settings stayed at four scenes; the exit at x=37.8 still targets `Ch1_S1b_ForestPath`. Forest and MainMenu scene files were not changed.
- Missing-script scan found 0 components in Core, Village, Forest, and MainMenu. Unity Editor compilation and scene generation succeeded. The first build attempt exposed a duplicated `props/` path; it was corrected and the rebuild succeeded. No automated tests were run per the Batch A rule. Manual Play Mode checks remain pending for HUD appearance, gate open/close, and the Village-to-Forest transition.

## Revision v3 — Batch B: Forest visuals (2026-11-01)

- Imported Forest v4 layers and v3 props as single sprites at 100 PPU with uncompressed textures. The 1335 px layers use pivot `(0, 270/1335)`. Copied `F_prop_thornwall_v3.png` from the shared folder; earlier art files remain intact.
- Updated `Chapter1SceneBuilder` to read `S1b_ForestPath_layout_v4.json`. Forest layers now use L2 undergrowth at 0.65 / order -20, continuous L3b v3 at 1.0 / order -9, and L4 Fore v2 at alpha 1.0 / parallax 1.15. Bramble is decorative at order 12; the cross is order 11.
- Added post-gap cross stars (activated after x=44.6; scattered stars reduce on reveal while the main star remains until pickup). Added the additive B0202A thorn glow and radius-6 Point Light. IronPulse turns the light off over 0.3 s, emits the configured 80 + 40/s smoke with red ember sub-emitter, fades the wall over 0.6 s, then disables its blocker.
- Rebuilt and saved only `Ch1_S1b_ForestPath`. The exit remains at x=59.5 in a 60-unit level and returns to MainMenu. No automated tests were run per the Batch B rule. Unity compilation, builder execution, sprite import settings, and scene structure were inspected; Play Mode visual/gameplay checks remain for the user.

## Black Forest Game View — 2026-11-01

- The user's Play Mode screenshot showed `Core_Persistent` and `Ch1_S1b_ForestPath` loaded, while the Game view was black. Build Settings included Forest and Unity Console had no errors, so this was a rendering issue rather than a failed scene transition.
- Forest and Core sprites use URP `Sprite-Lit-Default`; neither Core nor the chapter scenes had a Global Light 2D. Forest's only light was the red point light near x=55.5, radius 6, far from the x=3.5 entry spawn.
- Added a neutral white Global Light 2D at intensity 1 to `Core_Persistent`, and updated `Chapter1SceneBuilder.BuildCoreScene` to create it on future Core rebuilds. It targets the Default sorting layer.
- Unity saved and reloaded the Core scene with no Console errors. Play Mode was stopped to save the persistent scene change; the user should start from MainMenu and verify the visuals in Play Mode.

## Village Gate Foreground Order — 2026-11-01

- Raised the WoodenGate visual sorting order from -5 to 11 so it renders in front of Niamh's sprite (order 10), while remaining behind the foreground grass layer (order 20). Updated both the saved Village scene and its layout JSON so rebuilds retain the change.
- Gate collider, interaction range, open/close behavior, and position were left unchanged.

## Village Gate Layer Reverted — 2026-11-01

- Reverted the gate visual to sorting order -5 after the foreground placement caused a visual/design issue. Scene and layout JSON now agree; collider and interaction settings remain unchanged.

## Revision v3 Fix A1/B1 — 2026-11-01

- A1: Added the supplied gate front-post art without overwriting the source PNG. The supplied 414×240 PNG contains both posts despite the spec describing only the right post, so the builder now derives a right-post-only Sprite asset from the right-hand 114 px of that image. It overlays the gate back at sorting order 11; the closed/open gate renderer remains at -5. Niamh is order 10, so 11 is used instead of the spec's order 5 to actually render in front. Gate collider, range, and open/close logic are unchanged.
- B1: ThornWall now applies `#B0202A` tint and pulse alpha through a `MaterialPropertyBlock` `_BaseColor`; the SpriteRenderer stays white. The additive material defaults to `#B0202A` at 0.5 alpha. Existing red point-light fading and Q → smoke → wall removal behavior are unchanged.
- Updated the builder with a focused A1/B1 rebuild command and saved targeted changes to the Village and Forest scenes. Niamh capsule and Village wall/hay-bale collider overrides were preserved. B2 lighting was not changed.
- Unity Editor source compilation and scene save completed. No automated tests were run, per task instructions. Play Mode appearance and interaction need the user's manual check.

## Village Gate Right Post Alignment — 2026-11-01

- Recorded the user's saved Scene-view placement for `Gate_PostR_Front` at local position `(0.99, 0, 0)` under `Village Gate` (world X `27.4`). The Inspector screenshot showed X `0.988`; Unity reports the serialized Transform as `0.99`.
- Updated `Chapter1SceneBuilder.CreateVillageGate` to use the same position so a Village rebuild will retain the alignment. Kept sorting order 11 and left the gate backing, collider, interaction range, and behavior unchanged.
- The Village scene was already saved at this position and was left untouched in this follow-up. No Play Mode/gameplay test was run.

## Revision v3 Fix B2/B3 — 2026-11-05

- B3: Smoke and ember velocity axes are all Two Constants in Local space. Smoke uses `#3B2D4D` fading from alpha 220 to 0; embers use `#E0303A`. The first focused build showed Unity's JSON utility did not read the nested clover coordinate array; the builder now parses those coordinate pairs explicitly, and the rebuilt Forest contains all nine clover lights.
- B2: `LevelInfo` now carries per-level ambient color/intensity and player-light enablement. `LevelInfo.Awake` applies them as an additive level activates, before its first rendered frame; `ChapterSceneLoader` also applies them when attaching a level and when Core loads over a level opened directly in the Editor. Forest is `#6878A0` at 0.28 with the player light on; Village defaults remain white at 1.0 with the player light off.
- Added the Core-root Global Light and Niamh's warm Point Light (position `(0, 1.2)`, intensity 1.2, radii 0.8/5.5, falloff 0.7, no shadows, 1-second fade and ±4%/3-second breathing). Forest layers use the layout's unlit/tint data; L0/L1/L4 are unlit and L2/L3/L3b remain lit.
- Added nine layout-driven clover lights and additive dots, the cross light that turns on with the post-gap stars and fades after pickup, a 7u thorn light at `1.2 × glow alpha`, and a 1-second exit-light fade after the wall disperses.
- Rebuilt and saved only `Core_Persistent` and `Ch1_S1b_ForestPath`; the Village scene stayed unchanged. Scene inspection found exactly one Global Light at the Core scene root, none under Niamh or in Forest, all nine clover lights, and Two Constants/Local velocity on both particle systems. Unity compilation and scene generation succeeded. No automated tests or Play Mode run were performed per task instructions.
- Please manually check both entering Forest from Village and pressing Play directly in Forest, including the first frame, layer darkness, Niamh's light, clovers, cross, wall smoke/embers, exit light, and unchanged Village lighting.


## Revision v3 Fix C — 2026-11-05

- Task classification: visual/VFX and scene-layout changes; interaction triggers and gameplay rules remain unchanged. Per the shared task rule, no automated tests or temporary project copy were used.
- C1: Copied the two supplied gate-rubble PNGs from the shared art folder without changing the source files. Imported them as uncompressed Sprites at 100 PPU with bottom-center pivots; the builder reads the gate `rubble` entries. Left rubble is at `(24.49, -0.12)`, order `-4`; right rubble is at `(27.65, -0.15)`, order `6`.
- C2/C4: Rebuilt nine Forest clover Point Lights without visible dot sprites. The bramble now sorts at `-3` and the cross at `-4`, keeping the cross behind the bramble.
- C3: The main cross star is 1.6u with a 2.5u `#F2D28A` additive halo. It breathes over about 1.2 seconds, alpha 0.6–1.0, scale 0.85–1.15, and spins 360 degrees with a 1.4x pop every 2.5–4 seconds. Scattered stars remain in place within 1.5u, use 0.8–1.4u sizes and 1.2-second lifetimes, with about one third assigned to the slow-spin system.
- C5: Added a `#FF6A2A` Point Light (10u radius, peak intensity 3), a 0.2-second `#FFB070` wall burn followed by a 0.6-second fade, additive sparks (150 burst plus 60/s for 1.5 seconds, trails enabled), and additive glow smoke (40 burst). Existing dark smoke remains as the base layer. These IronPulse particle renderers sort at 25 and their velocity axes use matching Two Constants modes. The exit light begins its existing one-second fade after the orange flash completes.
- Added the focused `Build Revision v3 Fix C` command. Rebuilt and saved only Village and Forest; Core, MainMenu, HUD, audio, and Build Settings were unchanged. The first build attempt exposed a trailing comma in the edited Forest layout JSON; it was removed, and the subsequent Unity compile and focused rebuild completed successfully.
- Scene inspection confirmed both rubble world positions/sizes/orders, the two rubble import settings, nine clover lights with no child SpriteRenderers, bramble/cross sorting orders, star sprite sizes, IronPulse light/particle settings, and matching velocity modes. No Play Mode or visual gameplay test was run.
- Please manually check the C acceptance list in `TechnicalArt/CH1_UNITY_REVISION_v3_fix.md`: rubble appearance while the gate is present, clover glow without visible dots, cross star breathing/spins and scattered stars, bramble occlusion of Niamh, and the orange IronPulse light/burn/sparks/smoke followed by the exit light.
