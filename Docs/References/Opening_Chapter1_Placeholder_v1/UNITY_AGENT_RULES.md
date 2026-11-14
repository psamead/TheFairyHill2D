# Unity 工作规则（给 Codex / Claude，每次任务开始先读）

> 2026-11-01 建立。之前的指令里写的「`MENU_UNITY_HANDOFF.md` §0 环境规则」实际不存在，以本文件为准。

## 1. 环境

- 共享文件夹映射为 `Z:`（盘符不同就用你自己的）。Unity 工程：`C:\Users\Administrator\Documents\UnityProject\TheFairyHill2D`。
- 只从 `Z:` **复制**素材进工程，不改动、不移动、不删除共享文件夹里的任何文件。`.wav` 是母带，不进工程，只复制 `.ogg`。
- 不覆盖别人的修改：改 `ART_HANDOFF.md` 等共享文档时，读磁盘上当前的版本再追加。
- 用户在 Unity 里手调过的值（碰撞、偏移）必须写进布局数据，由场景生成工具读取，重建场景不能把它们冲掉。
- 项目设置保持 **Player → Resolution and Presentation → Run In Background = 开**，否则 Unity 窗口不在前台时 Play Mode 不会推进（之前 `Time.time` 停在 0 就是这个原因）。

## 2. 自动化测试：历史分类（当前暂停执行）

> **2026-11-01 用户决定：暂停自动测试。** v3 第一次尝试在临时副本工程里跑测试全部通过，但用户实际打开的工程里村庄进不去森林，测试结果和实际对不上。之后的任务不跑自动测试、不建副本工程，改为分小批交付、每批开始前先 git commit、由用户在 Play Mode 里手动确认。本节保留作参考，用户明确要求时再启用。

以下分类保留为历史参考；当前依照上面的用户决定，不运行自动测试、不建临时副本工程。只有用户明确恢复自动测试时，才按任务类型重新决定测试范围并记录判断。

| 任务类型 | 例子 | 测试要求 |
|---|---|---|
| **玩法 / 规则 / 碰撞 / 状态 / 场景流程** | 跳跃距离、门的开关、拾取与跨场景状态、荆棘墙驱散、出生点、场景加载 | **必须**新写或扩充 PlayMode 测试，并全部跑通再交付 |
| **数值微调** | 改手感参数、碰撞大小、触发范围 | 不必新写测试，但要**跑一遍已有测试**，确认没有改坏 |
| **纯美术 / UI 外观 / 音频 / 特效** | 换图、调视差、改颜色、加粒子、接背景音乐 | 不写测试；跑一遍已有测试；外观和听感交给用户在 Play Mode 里确认 |
| **只改文档或数据整理** | — | 不需要 |

拿不准就按更严格的一类处理。

## 3. 测试怎么写、怎么跑（恢复自动测试后参考）

- 用 Unity Test Framework（`com.unity.test-framework`），PlayMode 测试放在 `Assets/_Project/Tests/PlayMode/`，单独一个 asmdef。
- 测试里直接加载 `Core_Persistent` 和关卡场景，把妮芙放到指定位置，用代码模拟输入（给 PlayerController 一个可注入的输入接口，测试里驱动它，不依赖真实键盘），`yield` 若干帧后检查结果。
- 每个测试只验证一件事，名字写清楚，例如 `RunJump_Clears_2_6u_Gap`、`WalkJump_Fails_2_6u_Gap`、`Gate_Closes_After_Leaving_Range`、`IronCross_Persists_Across_Scenes`、`ThornWall_Collider_Off_After_IronPulse`。
- 可以在后台用命令行跑，不需要窗口焦点：
  ```
  Unity.exe -batchmode -projectPath <工程路径> -runTests -testPlatform PlayMode -testResults <路径>/results.xml
  ```
- 进度报告里写：新增 / 修改了哪些测试、通过数 / 失败数、失败的原因和处理。

## 4. 测试管不到的

画面好不好看、视差晕不晕、音乐和音效好不好听、UI 看不看得清，这些要用户亲自看、亲自听。报告最后列出「请用户手动确认」的清单。

## 文件分工（2026-11-01）
- 素材交接优先走双方可访问的共享网络文件夹，不默认从 GitHub 下载美术文件。任务开始时先检查 Unity 工程中是否已预先放好输入素材；已放好的文件直接使用，不要重复复制。
- 缺少素材时，Unity Agent 先向维护者给出精确清单：设计仓库相对路径、用途、目标工程路径、所需格式/版本。维护者将批准素材放入 `TechnicalArt/unity_drop/<日期_任务名>/`，通知 Unity 端从共享网络复制。只复制，不移动、删除或改写共享源文件；完成后在交接记录中写明复制清单。
- 只有共享网络不可用或维护者明确指定时才使用其他素材传输方式。`Z:` 是维护者电脑上的共享盘映射示例，不是固定路径；接手者按本机可访问的共享根目录解析相对路径。
- 若素材已随 Unity 仓库预先提交并存在于工程中，直接使用工程内版本。GitHub 用于仓库版本同步、审阅和备份，不作为 Unity 端默认索取美术素材的渠道。
- 共享素材文件夹已改名：`Z:\TechArtAsset\Backgrounds` → `Z:\TechArtAsset\Scenes`。Unity 工程内 `Assets/_Project/Art/Backgrounds/` 不改名。

## 2D 光照与叠加层（2026-11-01）
- 全局光（Global Light 2D）只能有一盏，在 `Core_Persistent`。关卡场景里不要加 Global Light。每关的环境色 / 强度写在 `LevelInfo`，切关时在淡入前应用。
- 给关卡加任何 Light2D 前，先确认该关的环境光设置，避免画面变黑。
- SpriteRenderer 上用 URP `Particles/*` 材质时，`SpriteRenderer.color` 不起作用：颜色和 Alpha 一律通过 `MaterialPropertyBlock` 写 `_BaseColor`，材质默认颜色设成目标色。
- 需要角色部分被物件遮挡时，用 Claude 提供的「同画布前层图」放在角色前面，不要自己拆分原图。
- 粒子：脚本设置 Velocity / Force over Lifetime 时，x / y / z 三轴必须同一种 MinMaxCurve 模式（不用的轴设 `MinMaxCurve(0,0)`），否则每帧报错、粒子不显示。
- 角色随身光用 Point Light 2D，绝不能用 Global Light（全局光放在哪都照全屏）。

## Character animation trial handoff — 2026-11-14

The Niamh v8 task stages user-edited, same-canvas PNG parts in `Assets/_Project/Art/Characters/Niamh/Motion_v8_Source/`. These are not a ready PSB rig. Keep the design repository read-only; do not trim the transparent canvas or overwrite source art. Read `TechnicalArt/LESSONS_CHARACTER_ANIMATION.md` and the dated task before integrating. Start with a reversible prefab; preserve the Core player root, collider, controls, and scene routing; report import, integration, compilation, Play Mode, and user acceptance as distinct states. Protect pre-existing worktree changes and commit only task-owned paths.
