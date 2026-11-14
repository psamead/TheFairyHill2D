# The Fairy Hills Unity project context

Checked 2026-11-14 from the network checkout. This technical snapshot supports the entry point for the project owner, students, production and layout designers, and current or future agents at the Unity repository root `AGENTS.md`. It does not claim a current Play Mode review; recheck time-sensitive facts before work.

## Project and source of truth

- Unity root: `TheFairyHill2D`. Design and art source: `Animation_yichun/ShareWorkFolder` (`TheFairyHill_Design` repository).
- Read design `README.zh-CN.md`, `ART_HANDOFF.md` section 9, `TechnicalArt/CH1_UNITY_PROGRESS.md`, and the relevant handoff before changing a system. The design repository describes itself as read only; copy source art into the Unity project and preserve the originals.
- Historical `handoff.md` says there was no code in October. Use the later November progress files and the current Unity assets for implementation status.

## Confirmed environment and structure

- Unity `6000.3.24f1`; URP 2D, Input System `1.20.0`, 2D Animation `13.0.6`, PSD Importer `12.0.2`, Cinemachine `3.1.7`, uGUI `2.0.0`, Unity Test Framework `1.6.0` in `Packages/manifest.json`. The game camera currently uses `ChapterCameraFollow`, not Cinemachine.
- First party runtime code is under `Assets/_Project/Scripts/Gameplay` and `Assets/_Project/Scripts/UI/MainMenu`. Scene generators are under `Assets/Editor/Chapter1` and `Assets/Editor/MainMenu`. No first party `.asmdef` or test directory was found in `Assets/` during this inspection.
- Enabled Build Settings order: `MainMenu`, `Core_Persistent`, `Ch1_S1_Village`, `Ch1_S1b_ForestPath`. Menu Start goes to Village; Village goes to Forest; Forest currently returns to Menu while S2 is absent.
- `Core_Persistent` owns Niamh, the camera, HUD, `GameState`, and the single gameplay AudioListener. Levels load additively through `ChapterSceneLoader`. `LevelInfo` applies level lighting. `PlayerController2D` reads keyboard input directly and owns movement, jump, interaction, and IronPulse input. `GameState` retains iron cross ownership and shamrock count between the two level scenes.
- Gameplay tuning and asset placement are partly generated from layout JSON. Preserve user adjusted scene values by updating layout data and the relevant builder before rebuilding scenes.

## Current chapter state

- The design repository records Chapter 1 Village and Forest v3 fixes A1 through G as complete on 2026-11-05, with user feedback that the result was broadly acceptable. The Unity repository last committed on 2026-11-05 (`d81b08e` at inspection). The saved project has the four enabled scenes and the associated scripts, art, audio, and shaders.
- Fix G was compiled and inspected as serialized scene content; its final visual and audio checks were not reported as completed in Play Mode. `Docs/CH1_UNITY_PROGRESS.md` lists the specific checks. MainMenu layout and interaction review at 1920x1080 and 1280x720, and a standalone build, also remain unverified in its handoff.
- The working tree already contains user or prior agent edits to `Ch1_S1_Village.unity` and two TMP font assets. Preserve them. The Village scene diff includes left gate rubble alignment and a particle repeat interval.
- Future Demo content remains substantial: S2 to S4, Existence, inventory, dialogue, Ogham puzzle, trade, end card, and a refined transition. The documented next production step after Chapter 1 scene work is Niamh's skeletal animation.

## Niamh animation entry point

**Historical baseline, superseded on 2026-11-14:** the static-only findings below describe the earlier inspection. The current six-part v8 trial is implemented, compiled, and basically accepted by the owner, with stutter cause unverified. See `Docs/Niamh_Motion_v8_Unity_Handoff.md` for actual implementation and evidence.

- Design references: `TechnicalArt/NIAMH_RIG_PARTS.md`, `TechnicalArt/NIAMH_ANIM_LIST_DEMO.md`, `TechnicalArt/SCENE_SCALE_SPEC.md`, `MJ_ref/Niamh_v3_turnaround.png`, and `MJ_ref/Niamh_v3_wip.psd` in the design repository.
- In Unity, `Assets/_Project/Art/Characters/Niamh/Niamh_side_static_v3.png` is the only character visual found. `Chapter1SceneBuilder.CreatePlayer` builds a child `Niamh Static Sprite` scaled to `0.30` and passes its single `SpriteRenderer` to `PlayerController2D`, which uses `flipX` when direction changes. The root CapsuleCollider2D is size `(0.23, 2.08)` and offset `(0.08, 1.04)`; preserve this gameplay collision during visual replacement.
- Source PSD inspection found illustration and retouch layers, but no named per-limb rig layer set or Unity-ready PSB. First prepare a minimum viable side-view parts sheet or PSB with filled overlap regions, then rig and check idle, walk, run, and jump before replacing the persistent Core visual. The ten-action Demo list is still marked draft.

## Reference from The Lantern Keeper

`TheLanternKeeper2D` is a separate, shorter project, not a source of Fairy Hills story or visual design. Its Unity checkout has a 17-layer Maela PSB, a prefab, and generated Idle/Walk/Run/JumpUp/Fall/Lantern clips. The most useful established workflow for Niamh is in its design `TechnicalArt/LESSONS_ART_PIPELINE.md` section 8 and Unity `Docs/PROJECT_HANDOFF.md`:

- Keep PSB canvas, layer names, and layer IDs stable once imported; changing layer bounds can leave stale sprite rectangles or shift parts. Compare source layer bounds with imported `sprite.rect` and check for missing prefab references before animation review.
- Use a bright magenta background to inspect seams, especially at shoulders, knees, hair, and clothing overlap, at the actual game size.
- Define poses in one source that can generate both a preview and Unity clips. Review the preview first, then verify actual movement and foot sliding in Play Mode.
- Preserve the Fairy Hills root collider and existing level routing. Maela's 17-part hierarchy, PPU 200, exact pose values, and animation code are reference examples, not drop-in assets for Niamh.

## Validation and collaboration constraints

- `TechnicalArt/UNITY_AGENT_RULES.md` records the user's 2026-11-01 decision to pause automated test runs and temporary project copies after those results diverged from the open Unity project. Deliver small batches; user Play Mode review is the visual and audio acceptance point. Do not interpret existing Test Framework package presence as a passing test suite.
- At the time of this inspection, no Unity Editor MCP tools were available to the inspecting Codex chat. Findings here came from repository files, serialized scene data, and prior progress records, not a live Editor connection. A later agent should check its own connection and not claim compilation, Play Mode behavior, or audio playback from this inspection.
- Relevant project lessons: `TechnicalArt/LESSONS_SCENE_PIPELINE.md` (scale before art generation, real image fill behind parallax layers, fixed ground pivot, visual/collider separation, one Global Light in Core, `MaterialPropertyBlock` for additive SpriteRenderer color, consistent particle curve modes, and saving manual adjustments in layout data).

## Files inspected

`Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt`, `ProjectSettings/EditorBuildSettings.asset`, `Assets/Editor/Chapter1/Chapter1SceneBuilder.cs`, `Assets/_Project/Scripts/Gameplay/{PlayerController2D,ChapterSceneLoader,GameState}.cs`, `Docs/{CH1_UNITY_PROGRESS,MainMenu_AgentHandoff,MainMenu_ImplementationPlan}.md`, and the design files named above. Git state was read on 2026-11-14; recheck it before edits.

## 2026-11-14 晚间：当前已授权的待执行任务

负责人已批准 OP01 放大候选图作为占位，要求接入菜单 Play 后的故事引入与第一章封面，在实际工程运行并回报。任务与素材已通过共享网络预置；读取并执行 `Docs/References/Opening_Chapter1_Placeholder_v1/2026-11-14_Opening_Chapter1_Placeholder_Unity_Trial.md`，先核对 transfer_manifest.json。之前批准的动作音任务按该任务单分批核对/执行。

此条是待办入口：设计端只有共享复制通道，未启动远程 Agent，未验证接入、编译或 Play Mode。dispatch_status.json 记录了传输与连接事实，不把复制当执行。完成后写 `Docs/Opening_Chapter1_Placeholder_Unity_Handoff.md`，声音另写 `Docs/Niamh_Surface_Audio_Unity_Handoff.md`，用户下次新对话将先读反馈再继续 Niamh 推门与举十字架动作。

旧接手快照中“妮芙只有静态 Sprite”的状态已被 `Docs/Niamh_Motion_v8_Unity_Handoff.md` 取代；基础 v8 动作已经试玩。保持已有分层角色、输入/碰撞与手调场景/字体。本轮设计端仅追加了 AGENTS.md 和 AI context 的这条待办入口，未改 Unity 代码/场景；将这两处本轮文档改动与原有 AI context 内容区分后再决定提交归属。

## 2026-11-14 opening implementation update

Menu Play now enters `Opening_Chapter1`, then the existing Village/Core bootstrap. The three placeholder story beats and Chapter I card are configurable in `Assets/_Project/Data/Storyboards/Opening_Chapter1.asset`. Standalone level debugging bypasses the sequence. Normal playback, replay and command-invoked Skip were observed in the actual Editor; owner acceptance of this new sequence is pending. See `Docs/Opening_Chapter1_Placeholder_Unity_Handoff.md` and its screenshot evidence. Earlier unavailable-MCP statements above are dated history; this run verified the official MCP against the actual project. The surface audio batch follows separately.
