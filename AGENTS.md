# The Fairy Hills 项目接手入口

本文面向项目负责人、学生、制作与排版设计人员，以及任何现在或未来查阅、接手和复用资料的 Agent。先确认文件与工程现状，再执行任务；历史交接记录描述的是当时的状态，不自动代表今天已完成或已验证。

查阅、摘编或排版时，保留来源版本、事实状态和可编辑源文件，另存衍生成品；长期资料整理遵循设计仓库 `AGENT_PROCESS_GUIDELINES.md` §5.7。关键结论应写入项目文件，让后续使用者无需原聊天或特定 Agent 的记忆也能理解。

## 两个仓库

- 本仓库 `TheFairyHill2D` 是 Unity 工程。场景、脚本、导入设置和当前 Git 状态，以这个工程的实际文件为准。
- 设计仓库 `TheFairyHill_Design` 保存故事、Demo 范围、设计说明、Prompt、源素材与经验。学生将两个仓库克隆到同一上级目录时，设计仓库通常在 `../TheFairyHill_Design/`。维护者的工作目录也可能叫 `Animation_yichun/ShareWorkFolder/`。
- 旧指令里的 `Z:` 指设计仓库根目录，`W:` 指 Unity 工程根目录；它们是维护者电脑的盘符示例。请换成自己电脑上的路径，不要把局域网电脑名或盘符写成必需条件。
- 设计仓库对学生和协作者只读。把要用的素材复制进自己的 Unity 工程；不要移动、删除或改写设计仓库里的原件。设计修改意见交给维护者和原作者。
- 美术文件传输按 `Docs/AI/ART_ASSET_TRANSFER.md`：优先使用工程内预先复制的素材；缺料时向维护者报精确清单并通过共享网络取得。不要把 GitHub 当作默认美术下载渠道。

## 先读什么

1. 设计仓库 `README.zh-CN.md` 或 `README.md`：项目入口、目录与更新方法。
2. 设计仓库 `ART_HANDOFF.md` 第 0、9 节及 `TechnicalArt/CH1_UNITY_PROGRESS.md` 末尾：最近的美术与第一章交接；再看本仓库 `Docs/AI/UnityProjectContext.md`。这些都是有日期的快照。
3. 设计仓库 `handoff.md`、`DEMO_IMPLEMENTATION_PLAN.md`、`STORY_BIBLE.md`：项目设定和 Demo 范围。`handoff.md` 有较早的“尚无代码”阶段描述，不能用它判断现在的 Unity 进度。
4. 按任务补读：角色动画看 `TechnicalArt/NIAMH_RIG_PARTS.md`、`NIAMH_ANIM_LIST_DEMO.md`、`SCENE_SCALE_SPEC.md`；关卡看 `CH1_SCENE_PLAN.md` 与各轮 Unity 修正；图片来源看 `MJ_ref/ASSETS.md`；背包看 `INVENTORY_SYSTEM_DESIGN.md`。
5. 遇到已解决的问题，查 `TechnicalArt/LESSONS_SCENE_PIPELINE.md`；要看某轮 Agent 收到的具体任务，查 `CommandForAgentToInitialRun/`。这些任务单是过程记录，执行前须核对是否已完成或被取代。

## 2026-11-14 接手快照

- Unity `6000.3.24f1`，URP 2D。Build Settings 中有 `MainMenu`、`Core_Persistent`、`Ch1_S1_Village`、`Ch1_S1b_ForestPath`；第一章现有村庄与森林小径。S2–S4 尚未制作。
- 第一章 v3 A1–G 在设计记录中标为完成。最新 G 轮有编译和场景配置检查记录，但最终画面与声音的 Play Mode 验收仍需核对。妮芙目前只有静态 Sprite；下一阶段是拆件与骨骼动画。
- 接手时工程已有未提交的村庄场景和 TMP 字体资产改动。任何人或 Agent 开始改动前都要重新运行 `git status --short` 并查看相关差异；不要重置或覆盖别人的工作。
- 本快照由文件检查得到，没有连接实时 Unity Editor。后续进度以新日期的记录和实际工程为准。

## 每项任务怎么交付

1. 说明任务目标、涉及文件、设计依据，以及这次只做哪些内容。开始前查当前 Git 状态和相关场景是否有未保存的人工修改。
2. 先按设计仓库 `TechnicalArt/UNITY_AGENT_RULES.md` 处理工程约束。2026-11-01 用户暂停了自动测试和临时副本工程：不要把该文件下方的旧测试分类表误读成当前必须运行的要求。分小批实施，在实际打开的工程中做所需的 Play Mode 检查，并让用户判断画面和听感。
3. 场景由 Builder 生成的内容，先把用户手调的碰撞、位置、排序等值写进布局数据和 Builder，再考虑重建；保留常驻 Core 中的角色、相机、HUD 与单个 AudioListener。
4. 交付记录分开写清：**已计划、素材已备、已接入、已编译、已在 Play Mode 验证、用户已确认**。没有做过的检查明确写“未验证”，不要把源码存在、Git 提交或编辑器序列化检查等同于试玩通过。
5. 更新本仓库相关 `Docs/` 进度，并把可复用的原因与处理方法写入设计仓库经验文档，由维护者同步。新文档、Prompt 和任务单的写法见设计仓库 `AGENT_PROCESS_GUIDELINES.md` 的“接手材料写法”一节。

游戏对外定位仍按 `DEMO_IMPLEMENTATION_PLAN.md`：面向实际独立游戏的短周期垂直切片；“给学生阅读”仅指这套交接资料的读者。
