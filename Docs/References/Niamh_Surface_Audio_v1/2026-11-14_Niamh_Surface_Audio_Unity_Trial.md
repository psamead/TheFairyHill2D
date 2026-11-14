# 2026-11-14 妮芙动作与地面音效 Unity 试装

**项目**：The Fairy Hills / TheFairyHill2D。
**读者**：网络电脑上的 Unity Codex、制作人员、项目负责人和后续接手者。
**授权**：负责人单独试听后反馈“没有太大问题”，明确要求放进游戏试试。允许本轮试装，游戏内混音与听感仍待试玩。
**范围**：现有 Village / Forest、Core 常驻妮芙 v8 的脚步、起跳、落地与草垛摩擦声。章节故事画在设计端另行制作，本任务不等待新画面。
**交接状态**：23 条独立 OGG 经共享网络预先复制到工程；复制与 SHA-256 清单见 `Docs/References/Niamh_Surface_Audio_v1/transfer_manifest.json`。文件存在不代表已接入或已试玩。结果由执行者补记。

## 执行前

1. 读工程 `AGENTS.md`、`Docs/AI/ART_ASSET_TRANSFER.md`、`Docs/Niamh_Motion_v8_Unity_Handoff.md`，核对当前 Git 状态。
2. 读设计仓库 `TechnicalArt/UNITY_AGENT_RULES.md`、`TechnicalArt/CHAPTER_TRANSITIONS_AND_SURFACE_AUDIO_v1.md` 的音频部分以及 `TechArtAsset/Audio/SFX/Niamh_Motion_v1/README.md`。
3. 当前观察到 Village 场景、两份 TMP 字体已有非本任务修改，`Docs/AI/UnityProjectContext.md` 未跟踪；执行时重新核对。不得重置、覆盖或顺手提交这些改动。不要重建现有村庄和森林场景。
4. 遵守当前暂停自动测试/临时副本的决定：在实际工程编译、观察 Play Mode，并请负责人实际听感确认。开始前建立仅包含自己工作且可回退的提交；若没有自己的未提交文件，记录当前 HEAD，不把别人的改动纳入提交。

## 输入和去向

直接使用工程内 `Assets/_Project/Audio/SFX/Niamh_Motion_v1/` 的 23 条 OGG：

- `SFX_Niamh_Step_{dirt,grass,wood,stone,hay}_{01,02,03}_v1.ogg`：15 条。
- `SFX_Niamh_Jump_{01,02}_v1.ogg`：2 条。
- `SFX_Niamh_Land_{light,heavy}_{01,02}_v1.ogg`：4 条。
- `SFX_Niamh_HayBrush_{01,02}_v1.ogg`：2 条。

WAV 母带、生成脚本和 `listening_reel` 试听拼盘留在设计仓库，不能把拼盘接成游戏事件。缺文件先按清单通过共享网络补齐，不默认从 GitHub 下载。

## 实施顺序

1. 检查 OGG 导入，使用现有音频组织和音量设置。给妮芙加可单独关闭的声音组件；保留单个 AudioListener，不改移动速度、跳跃、碰撞体或 v8 动作曲线。
2. 沿用控制器实际 grounded、速度、跳跃生效事件。检查现有接地 Cast，可在同次计算中保留实际命中的 Collider，给声音组件读取；不要维护另一套相互矛盾的接地状态。
3. 地面声使用明确的 surface 数据/组件（dirt、grass、stone、hay、wood），不要按物体名字猜测。将需长期保留的配置写入关卡布局/Builder 数据；本轮通过小范围组件挂接实现，不为此重建场景。
4. Walk/Run：匹配 v8 脚底接地时刻，每次落脚只播一次；同一材质 3 条避免连续重复，允许小幅音高/音量变化。跑步节奏更快、略重。静止、空中、暂停和禁用角色时无脚步。
5. 起跳：真实跳跃生效时播一次，空按跳跃键不播。现有 Jump 是通用蹬地/衣料短音；可轻叠起跳处的地面 Step 作为材质层，但总响度需控制。
6. 落地：只在空中→着地的有效转换触发，按着地前的下降速度选择 light/heavy；设置合理的最小滞空/速度门槛避免开场站稳、台阶抖动连响。现有 Land 是通用冲击底层；可用实际落点 Step 的轻叠层表达材质，不声称当前已有每种材质的独立 Land 录音。
7. 村庄先测土地、草地、石墙顶部和草垛顶部；森林先测地面与坑底。草垛摩擦由实际接近/穿过区域触发，移动时才播放且限频；核对碰撞路线，不能让实体草垛变成可穿越物。木材音色作为后续物件备用，当前场景没找到木箱，不为测试增加关卡物件。
8. 与当前 BGM、门、拾取十字、荆棘墙声音一起调混音，避免多个叠层过响；原有音效事件继续保留。

## 必须记录的验收

- 脚步与鞋底接触大致同步；走/跑节奏正确，停下或起跳即停止脚步。
- 起跳一次、落地一次；不因按键失败或接地抖动连响。
- 实际站在石墙、草垛和土地时，能听出对应材质；边界命中稳定。
- MainMenu→Village→Forest 路由和 Core 保持正常，过场或暂停不会继续脚步；没有第二个 AudioListener 或 Console 新错误。
- 音量在游戏 BGM 下判断，不以文件峰值归一化代表混音完成。
- 编译、场景配置、Play Mode 观察、负责人听感确认分别记录；未实际观察到的项目明确写未验证。

## 交付

将结果写入工程 `Docs/Niamh_Surface_Audio_v1_Unity_Handoff.md`，包含新增/修改文件、材质映射、触发方式、音量初值、编译与试玩证据、尚需用户听感确认项、关闭组件的回退方法。

仅提交本任务拥有的代码、音频及其 `.meta`、必要配置和文档；场景若与人工修改重叠先逐段检查差异，不把无关变化提交。完成后可按已授权流程 git push，报告分支、提交、推送结果及剩余 dirty 文件。设计仓库只读，由维护者同步本轮结果。

## 给 Unity Codex 的直接指令

请读取并执行本任务单。23 条独立 OGG 已预先通过共享网络放入工程，先核对 transfer_manifest.json，再把声音接到 Core 妮芙和现有 Village/Forest 的真实动作与地面材质上。负责人允许游戏内试装；最终听感待其试玩。按上述范围保护现有人工修改，编译并在实际工程观察 Play Mode，记录结果，然后只提交和推送自己的任务文件。
