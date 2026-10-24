# Agent Handoff — The Fairy Hills Main Menu

**Updated:** 2026-10-24  
**Purpose:** Continue MainMenu work from another Claude or Codex computer on the same network.

## Current result

`Assets/Scenes/MainMenu.unity` is built and registered as the first enabled Build Settings scene; the existing `Assets/Scenes/SampleScene.unity` remains enabled after it. The scene contains the supplied artwork, TMP menu labels, W/S and Up/Down keyboard navigation, mouse-hover selection, placeholder Settings panel, disabled Continue option, and a guarded Start destination.

The original network source PSD and its network files were left unchanged. The PSD, Fondamento font and selection icon were copied into this project. The `BG_clean_signature`, `left_dark`, and `title_plaque` layers were exported from the PSD for use as editable scene sprites. Button labels remain TextMeshPro objects. Unity generated the needed `.meta` files, TMP font asset and UI input-action reference assets.

The Main Camera has the scene's single enabled AudioListener; the controller creates a 2D AudioSource for UI sounds. The selection icon uses a serialized `(0, -14)` UI-unit offset and snaps directly to the focused option without rotating. Mouse hover selects options through `MainMenuOption.OnPointerEnter` and the EventSystem point action; mouse clicks remain disabled. Selection changes play the short synthesized `Menu_HoverChime.wav` UI sound at volume `0.35`; initial focus and repeated hover on the same option are silent. Background music has not been made or connected; see the shared `TechnicalArt/MENU_UNITY_PROGRESS_AND_DESIGN_BACKLOG.md` for ongoing design tasks.

## Validation and limits

- Connected Unity Editor and CLI both confirmed project `C:\Users\Administrator\Documents\UnityProject\TheFairyHill2D`, Unity `6000.3.24f1`.
- The scene builder compiled and executed to create the scene; it is saved and was clean at the last successful scene inspection.
- Inspected Canvas scaler (`1920×1080`, match `0.5`), enabled Build Settings order, button interactable flags, and Input System UI references (`Navigate`, `Submit`, `Point`). Continue is disabled as intended.
- The Build Settings YAML initially contained a zero GUID for the new scene. It has been updated to match `Assets/Scenes/MainMenu.unity.meta`; the builder now includes the same targeted GUID repair. The Editor connection stopped answering later pipeline reads, so this final settings correction and builder adjustment were checked from the saved files but not re-read in Unity.
- Compared a source-layer reconstruction preview to the provided menu composition. `Unity_Camera_Capture` only returned the Scene View and did not render the Screen Space Overlay UI, so there is no Unity Play Mode visual capture yet.
- No Play Mode interaction, standalone player build, or test suite was run.
- The scene file contains exactly one enabled AudioListener, but audio playback has not yet been confirmed in Play Mode.
- `Scene_Forest_01` does not exist yet. Start checks for that target; until it is added to Build Settings, it displays “The first forest scene is not ready yet.” and logs a warning. It does not load `SampleScene`.

## Local project baseline and preservation notes

- Project: `C:\Users\Administrator\Documents\UnityProject\TheFairyHill2D`
- Unity: `6000.3.24f1`, URP 2D, Input System `1.20.0`, uGUI `2.0.0`, PSD Importer `12.0.2`.
- Existing edits to `ProjectSettings/EditorBuildSettings.asset` and `ProjectSettings/ProjectSettings.asset` predated this task. The editor Build Settings edit now also lists `MainMenu` first. The pre-existing `com.unity.dt.app-ui` entry and `APP_UI_EDITOR_ONLY` define were preserved.
- `SampleScene.unity` was not modified.

## Related Obsidian experience checked

Read `D:\MyObsidian\NoteBookmark\Knowledge Base Home.md`, its root `AGENTS.md`, the `2D Battle Car Arena` project index, and `StartMenuTemplate.md`. The relevant reusable patterns were a baked editable uGUI hierarchy, 1920×1080 balanced Canvas scaling, default focus on the first option, and guarding scene loading when a destination is absent. The earlier project's full theme/runtime-bootstrap system was intentionally not copied into this smaller art-led menu.

The vault is historical reference. Its instructions do not override the current project or user request.

## Network art source

Root:

`\\haoxi-surfacep\Administrator\Documents\MyFlie\AsArtPortfolioTeaching\Animation_yichun\ShareWorkFolder`

- Menu spec: `TechnicalArt\MENU_UNITY_HANDOFF.md`
- Art spec: `ART_HANDOFF.md`
- Menu PSD: `TechArtAsset\Menu\MENU_wip.psd`
- Selection icon: `TechArtAsset\UI\ICON_triskele.png`
- Fondamento: `TechnicalArt\fonts\Fondamento-Regular.ttf`

The menu document's heading and blockquote above `## 1` are its top/context section. They instruct agents to read `ART_HANDOFF.md` and `handoff.md` first and state that the document covers only the menu. Its numbered sections start at 1.

## MainMenu implementation files

- Scene: `Assets/Scenes/MainMenu.unity`
- Runtime behavior: `Assets/_Project/Scripts/UI/MainMenu/MainMenuController.cs`, `Assets/_Project/Scripts/UI/MainMenu/MainMenuOption.cs`
- Editor builder: `Assets/Editor/MainMenu/MainMenuSceneBuilder.cs` (`Tools > The Fairy Hills > Build MainMenu Scene`)
- Original PSD copy: `Assets/_Project/Art/Menu/Source/MENU_wip.psd`
- Extracted layers: `Assets/_Project/Art/Menu/Exported/`
- Icon: `Assets/_Project/Art/UI/MainMenu/ICON_triskele.png`
- Fondamento source and TMP asset: `Assets/_Project/Fonts/`
- UI input action references: `Assets/_Project/Art/UI/MainMenu/Actions/`
- Plan and checklist: `Docs/MainMenu_ImplementationPlan.md`

## Next useful work

1. Open `Assets/Scenes/MainMenu.unity` and visually inspect the Game view in Play Mode at 1920×1080 and 1280×720.
2. Exercise pointer hover/click, keyboard and gamepad navigation, Settings/Back, disabled Continue, and Quit in the intended target environment.
3. When `Scene_Forest_01` exists, ensure it is in Build Settings and verify Start loads it.
4. If changing the builder and rerunning it, check that the `SampleScene` entry and the existing Project Settings edits remain intact.
