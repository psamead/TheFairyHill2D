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
