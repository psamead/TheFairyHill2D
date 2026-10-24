# MainMenu Unity Implementation Plan

**Status:** Scene built; Play Mode visual review remains.  
**Prepared:** 2026-10-24  
**Unity project:** `C:\Users\Administrator\Documents\UnityProject\TheFairyHill2D`

## Goal

Build the game's PC start scene as `MainMenu`, matching the approved static menu artwork in `MENU_wip.psd`. Keep the title and menu text in English and reproduce the supplied 1920×1080 composition with responsive scaling.

## Source handoffs and assets

Network source folder:

`\\haoxi-surfacep\Administrator\Documents\MyFlie\AsArtPortfolioTeaching\Animation_yichun\ShareWorkFolder`

Read for this plan:

- `TechnicalArt\MENU_UNITY_HANDOFF.md` — menu scene structure, layer names, button states and interactions.
- `ART_HANDOFF.md` — visual direction, language, project conventions and outstanding art tasks.
- `handoff.md` — overall demo scope and platform order.

Confirmed readable source assets:

- `TechArtAsset\Menu\MENU_wip.psd` (19,325,870 bytes)
- `TechArtAsset\UI\ICON_triskele.png` (18,453 bytes)
- `MJ_ref\TileArtFont_wip.psd` (1,945,917 bytes)
- `TechnicalArt\fonts\Fondamento-Regular.ttf` (87,160 bytes)
- `TechnicalArt\fonts\UncialAntiqua-Regular.ttf` (63,404 bytes)

The menu handoff's heading and blockquote above `## 1` are its top/context section. They say to read `ART_HANDOFF.md` and `handoff.md` first and clarify that this handoff covers only the menu. The numbered sections begin at 1.

## Relevant prior experience checked in the Obsidian vault

Read `D:\MyObsidian\NoteBookmark\Knowledge Base Home.md`, root `AGENTS.md`, the 2D Battle Car Arena project index, and its canonical `StartMenuTemplate.md`. The earlier project used an editable baked uGUI hierarchy, a 1920×1080 balanced Canvas Scaler, first-option focus for keyboard/controller input, and a guarded Start action that checks whether its destination is available. These behaviors are reused here. Its larger theme asset/runtime-bootstrap system is not needed for this small, art-led menu.

## Confirmed local project baseline

- Unity `6000.3.24f1`, URP 2D project settings, Input System package `1.20.0`, uGUI `2.0.0`, TMP available through Unity UI, PSD Importer `12.0.2`, and 2D Animation packages are present in `Packages/manifest.json`.
- Only `Assets/Scenes/SampleScene.unity` exists as a scene, and it is the sole enabled Build Settings scene. No menu-specific scripts, fonts or UI assets are currently present under `Assets/`.
- `Scene_Forest_01` is named as a provisional Start destination in the menu handoff, but it does not currently exist in this checkout.
- The working tree already has edits in `ProjectSettings/EditorBuildSettings.asset` and `ProjectSettings/ProjectSettings.asset` (`com.unity.dt.app-ui` config and `APP_UI_EDITOR_ONLY` define). Preserve these edits; do not reset or overwrite them while building the menu.

## Implementation delivered

1. **Assets:** retained `MENU_wip.psd` under `Assets/_Project/Art/Menu/Source/`, copied Fondamento and the 128px triskele icon, and exported the named background, dark overlay and title plaque layers as transparent PNGs. The network originals were left unchanged.
2. **Scene:** created `Assets/Scenes/MainMenu.unity` with an orthographic camera, a screen-space-overlay Canvas, 1920×1080 reference resolution, match `0.5`, and editable images and TextMeshPro labels positioned from the PSD layer coordinates.
3. **Typography and states:** generated a dynamic Fondamento TMP asset; labels use cream normal, warm-gold selected, dark underlay glow, and 50% cream disabled Continue. The title remains the provided plaque artwork.
4. **Input and actions:** added an EventSystem/InputSystemUIInputModule. W/S and Up/Down navigate; mouse hover changes selection, while mouse clicks are disabled. The selection icon snaps to focus at a fixed offset, without easing or rotation.
5. **Actions:** Start loads `Scene_Forest_01` only if it can be loaded and otherwise displays a status message; Continue is disabled pending a save flow; Settings opens a placeholder panel with Back; Quit calls `Application.Quit()`.
6. **Build Settings:** placed MainMenu first while retaining the existing enabled `SampleScene` entry. Pre-existing `ProjectSettings` changes were preserved.
7. **Audio and camera:** added a short synthesized hover/selection chime, played through a runtime 2D AudioSource. The Main Camera has exactly one enabled AudioListener in the saved scene. Background music has not been made or connected; track this in `TechnicalArt/MENU_UNITY_PROGRESS_AND_DESIGN_BACKLOG.md` on the network share.
8. **Validation so far:** Unity Editor builder compiled and executed for the scene. The saved scene and its controls were inspected, and a source-layer preview was compared against the supplied composition. Later Unity pipeline reads stopped responding, so recent audio/listener changes have only been checked in the saved files; no Play Mode visual/audio check, Player build, or test suite is claimed.

## Explicitly out of scope for this menu pass

- Three-layer parallax (the source background has not been separated into three scene layers).
- Doorway effects, Ogham particles, fog, silhouette lighting, background music and gameplay sound effects. A short menu selection chime is included separately.
- Save-game implementation, gameplay scenes, or broader demo systems.

## Completion checklist

- [x] Copy required menu sources into the Unity project; preserve network originals.
- [x] Create `MainMenu.unity` and align the composition to PSD coordinates.
- [x] Add TMP labels, theme states and selection icon behavior.
- [x] Implement menu button actions with the missing gameplay destination handled safely.
- [x] Register the scene in Build Settings without losing existing entries/configuration.
- [x] Add a selection chime and one enabled AudioListener to the Main Camera.
- [ ] Inspect actual Game view layout and interactions in Play Mode at 1920×1080 and 1280×720; no Player build or test suite is claimed.
- [ ] Update `ART_HANDOFF.md` section 6 and section 9 on the network share only after the menu work is actually complete and the share is writable.
