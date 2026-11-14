# 2026-11-14 开场故事引入与第一章封面占位试装

项目/读者：The Fairy Hills；Unity Codex/制作人员、负责人、学生与后续 Agent。
状态：用户已授权执行；素材与任务包由设计端预先复制。实际启动、接入、编译、Play Mode 与用户验收须写回反馈，不能由本任务单推定。
目标：从菜单 Play 体验故事引入、第一章封面，再在现有村庄控制妮芙；用户下次在新对话查看截图与反馈。
依据：设计 `STORY_BIBLE.md` §8.7–8.9、`DEMO_IMPLEMENTATION_PLAN.md`、`TechnicalArt/SLIDESHOW_OPENING_v1.md`、`CHAPTER_TRANSITIONS_AND_SURFACE_AUDIO_v1.md`、`STORYBOARD_KNOWLEDGE_BASE.md` 和用户本轮批准占位试装。

## 0. 开始与保护现场

先读工程 AGENTS.md、Docs/AI/UnityProjectContext.md、Docs/Niamh_Motion_v8_Unity_Handoff.md、设计 TechnicalArt/UNITY_AGENT_RULES.md。重新核对 git status 和实际编辑器，旧 context 中“只有静态角色”已被 v8 记录取代。2026-11-14 检查 HEAD 为 3d1b9e6；村庄和两份 TMP 字体有已有改动；音效与 AI docs 有预置未跟踪文件。不要 git reset、全量 add、强推、重建 Village/Forest/Core 或提交他人修改。有可归属的本任务修改才做检查点；否则记录起始 HEAD。

本轮不恢复自动化测试或临时工程。应在实际工程编译和观察 Play Mode（用户已要求跑出效果），结果需证据。遇到场景人工未保存改动先保护，不覆盖。

## 1. 输入已预放，不从 GitHub 取素材

- `Assets/_Project/Art/Storyboards/Opening_v1/placeholder/OP01_friends_dusk_placeholder_v1.png`，2912×1632；保持原像素和宽高比。
- `Docs/References/Opening_Chapter1_Placeholder_v1/opening_sequence_v1.json`：独立图片/字幕/时长数据和已知占位限制。
- 同目录 `transfer_manifest.json`：来源和 SHA-256；先校验再用，已有相同文件不重复复制。
- 设计源在共享网络 `Animation_yichun/ShareWorkFolder/`；当前包中有规格与知识库快照。缺料报精确相对路径、用途与目标路径，通过共享网络补齐；不改写共享原图。

这张画仅获准占位：人物仍偏 Q、轮廓过重、妮芙眼睛偏绿。OP02/03 尚无专门图片，三节拍暂复用同图，由字幕讲故事。不将任何一张标为最终美术或改写角色设定。

## 2. 最小可回退实现

1. 检查现有 MainMenu Start 回调和 ChapterSceneLoader；接入一个可配置 opening sequence。优先沿用已有加载结构、字体与 UI。参考守灯人 Assets/_Project/Scripts/Storyboard/StoryboardData.cs 与 StoryboardPlayer.cs 的数据、淡入和输入流程；适配本工程 Core 加法加载，不直接复制项目依赖或美术。
2. 菜单 Play → 3 个故事节拍 → 第一章封面 → 原村庄出生点。图片用独立配置，便于换正式画。可为序列建独立场景或可卸载 UI，实际方案由现有架构决定，文档写清。单独从 Village/Forest 调试不强制播序章。
3. 三页初值 6、7、6 秒，淡入/交叉淡入约 0.4 秒，轻推近最多 2%；若轻推引起跳页突跳，先关闭推镜保证稳定。每页计时重置，自动/手动换页走同一路径。不能靠调高全局 timeScale 加速；检查为 1。
4. 第一章封面默认沿用该占位图并在 UI 中压暗，叠 `CHAPTER I` 与 `THE THRESHOLD`（可替换工作标题，源于现有设计草稿，非新剧情定案）；约 2 秒，可前进。标题不烘焙进图。放在序章结束和村庄之前，本轮不做第二章封面或新 S2–S4。
5. 图片 Fit/保持比例；16:9 下不拉伸、不裁掉脸和脚。字幕用独立 TMP，在 1920×1080 与 1280×720 检查；必要时给底部独立字幕栏预留位置，避免遮住人物。优先项目已有 OFL 字体并核对英文和 Oisín 的重音。Skip 位于独立安全区，与字幕不重叠。暂不新增配音和未制作的低语。
6. 提供可见 `Next` / `Skip` 按钮及项目支持的键盘输入：Space/Enter 前进、Esc 跳过。末页前进或整段跳过都经过第一章封面并进入相同村庄初始状态；封面可前进。不让同一次按键穿透触发角色跳跃/互动。覆盖双击、连续按键，保证加载完成仅调用一次。
7. 叙事与封面时锁定游戏输入/隐藏玩法 HUD，不让玩家漂移、相机先跑或漏出错误光照。完成、跳过及返回路径释放输入和遮罩；保持 Core、单个 AudioListener、Niamh v8、背包/圣物已有状态结构。新游戏/返回菜单/再次 Play 都应按既有新游戏语义播放一遍，不新增永久存档跳过规则。
8. 音乐沿用当前可用素材做淡出/淡入，具体复用写入反馈。画面加载时不重复播放多个 BGM。

## 3. 同批已授权的声音任务

23 条动作音已在工程中预置，用户已批准试装。先查 `Docs/References/Niamh_Surface_Audio_v1/2026-11-14_Niamh_Surface_Audio_Unity_Trial.md` 是否完成；若尚未完成，在故事序列批次检查点之后按原任务接入并反馈，分别提交。避免把已存在的文件误称为声音已生效。不为木材声音新造木箱。

## 4. 运行与交付

- 在实际工程编译；从 MainMenu 完整播放、手动换页、各页 Skip、封面前进、返回菜单后再次 Play；检查村庄可走跑跳、进森林、v8 未回退、无叠加场景/遮罩/输入锁残留。按当前流程核对音效试玩，尚缺验证明示。
- 截图：三节拍各一张含字幕、封面、进入村庄后的画面；1920×1080，另留一张 1280×720 的字幕/Skip 画面。放在 `Docs/References/Opening_Chapter1_Placeholder_v1/Unity_Trial/`。Recorder 已可用则录正常和跳过各一段短视频；不可用就给截图和观察记录，不安装新包拖延。
- 写 `Docs/Opening_Chapter1_Placeholder_Unity_Handoff.md`：实现路径、配置、采用字体/音乐、实际验证、截图/视频、已知占位问题、回退办法、未验证项、下一步；更新 Docs/CH1_UNITY_PROGRESS.md 与旧 AI context 中被本轮取代的状态，保留日期。
- 如声音执行，单独写 `Docs/Niamh_Surface_Audio_Unity_Handoff.md`，供设计端下次读取。
- 仅提交任务拥有的代码、配置、图片、必要 .meta 与证据/文档；检查 scene/font diff 的归属。按用户此前授权 git push Unity 当前分支，报告提交与推送状态；不推他人改动、不强推。失败写原始原因。
- 退出 Play Mode，卸载临时加载场景，最终只打开 MainMenu，保持方便用户下一次试玩。

## 可直接发送给 Unity Codex

请读取并执行 `Docs/References/Opening_Chapter1_Placeholder_v1/2026-11-14_Opening_Chapter1_Placeholder_Unity_Trial.md`。用户已批准用预置 OP01 放大图作为占位，加入菜单 Play 后的故事引入与第一章封面，跑实际工程并留下截图/反馈，按任务处理之前已批准的动作音试装，然后仅提交和推送自己文件。素材和清单已通过共享网络放进工程，设计源只读。下次设计新对话会先读你写的 Handoff，再继续 Niamh 推门与举十字架动作。
