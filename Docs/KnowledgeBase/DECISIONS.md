# 决策记录

## 2026-08-12：项目基础方向

- 决策：项目为俯视角反向塔防轻策略游戏，玩家作为进攻者。
- 决策：首发 Android，Windows 编辑器使用鼠标进行开发验证。
- 决策：单机、仅中文、长期迭代，首期完整内容约十余关并包含成长。
- 决策：正式美术、音乐和语音由用户提供；AI 使用基本图形占位。
- 决策：需要设计便捷素材替换能力，玩法不得硬绑定占位素材。
- 决策：使用本地存档；具体模型和槽位规则稍后确认。
- 决策：保留 `BaseFramework`，后续在其基础上审查与修改。
- 决策：由用户亲自试玩验收，不要求截图或自动化测试。
- 决策：AI 不再检查现有插件许可。

## 2026-08-12：AI 操作边界

- 决策：禁止 AI 执行任何 Git 操作，包括上传和下载相关工作流。
- 决策：建立项目内知识库，作为后续 AI 的持续上下文和事实来源。
- 理由：项目将长期由 AI 协助开发，需要稳定记录需求、约束、架构和未决问题，减少
  不同轮次之间的上下文丢失与冲突。

## 2026-08-12：Control + Component + Data 对象架构

- 决策：保留用户习惯的 Control + Component + Data 组合方式，Control 是物体唯一公开
  入口，Component 只依赖 Data。
- 决策：Data 不再知道或持有 Control 和 Component；Data 只保存运行时共享状态、提供
  领域方法并发布内部/领域事件。推荐编译期依赖为 `Control -> Data <- Component`。
- 决策：外部输入使用 Control 的命令方法，不使用“Control 事件 -> Data 事件”的机械
  中转；对外事件只由 Control 选择性暴露，来源是 Control 订阅 Data 的领域事件。
- 决策：生命周期由 Control 统一驱动；Component 按优先级打开、逆序关闭，并在
  `OnOpen` / `OnClose` 中对称订阅和解绑 Data 事件。
- 理由：保留物体内部行为拆分和共享数据的开发习惯，同时消除循环依赖、公共 Data
  绕过边界、事件链路过长、对象池重复订阅及生命周期顺序不可靠等问题。
- 影响范围：`Assets/Scripts/BaseFramework/Object`、组件排序工具及后续所有运行时物体。

## 2026-08-24：首个垂直切片（M1）

- 决策：按用户确认的规则实现首个可玩垂直切片，规则全文见 `GAMEPLAY.md`
  “已确认：首个垂直切片规则”章节。
- 决策：游戏模块位于 `Assets/Scripts/Game/`，分 Core / Content / Combat / Level /
  UI / Save / Editor 七个子目录；静态配置用 ScriptableObject（稳定 contentId），
  运行时物体遵循 Control + Component + Data 架构。
- 决策：对象池沿用 `BF.PoolManager`（键 = 预制体名称）；战斗层生成/回收统一走
  `Game.Combat.CombatPool`，不直接调用池接口。
- 决策：存档直接构建在 Easy Save 3 之上（`Game.Save.SaveService`，
  文件 `persistentDataPath/GameSave.es3`，schemaVersion=1）。
  `BF.MomentoManager` 的多态加载尚不成熟，玩法侧暂不经过它；两者共用同一存储底座。
- 决策：场景、预制体与定义资产由 `Game.Editor.BattleSceneBuilder` 一键生成
  （菜单 “AIOnly/构建垂直切片场景” 或批处理
  `-executeMethod Game.Editor.BattleSceneBuilder.BuildAll`），幂等可重建。
  手工修改的场景引用会被重建覆盖，调数值应改定义资产。
- 理由：用户明确要求可扩展、便于改值、便于后续 AI 接手；资产生成脚本化保证
  结构可复现、引用一致，并避免手写场景 YAML 的脆弱性。
- 影响范围：`Assets/Scripts/Game/**`、`Assets/Game/**`、
  `Assets/Scenes/Level001.unity`、构建设置（Level001 为首场景）。

## 2026-08-24：BaseFramework 最小修复

- 决策：`BF.PoolManager` 的 TransitManager 订阅增加空保护，场景中没有
  TransitManager 时不再抛 NRE。
- 决策：移除 `BF.AudioManager` 中误引入的
  `using static UnityEditor.ObjectChangeEventStream;`（编辑器专用 API，
  会阻断 Android 构建）。
- 理由：两者都是垂直切片与 Android 构建的实际阻断项；改动最小，不改对外 API。
- 影响范围：`Assets/Scripts/BaseFramework/Manager/System/PoolManager.cs`、
  `AudioManager.cs`。

## 2026-08-24：引入 Unity MCP

- 决策：按用户明确要求安装 `com.coplaydev.unity-mcp`（git 包，MIT），
  并在 Codex 配置中注册 MCP 服务器（`uvx mcpforunityserver`，stdio 传输）。
- 理由：用于编辑器自动化（场景操作、编译检查、控制台读取）。该包为编辑器侧
  工具，不参与 Android 运行时。
- 影响范围：`Packages/manifest.json`、Codex `config.toml`。

## 2026-08-25：批处理 PlayMode 验证的经验教训

- 教训：强杀批处理 Unity 进程可能损坏 Bee 构建缓存，之后批处理会报“找不到
  新加的方法/类型”，而实际编译是通过的。处理：重跑一次批处理，或先执行
  `Game.Editor.BattleSceneBuilder.BuildAll` 触发重新编译，不要急于判定代码改动无效。
- 教训：`-batchmode -nographics` 的 PlayMode 中调用过 ES3 保存后，退出 PlayMode
  会冻结。原因是 `ES3.Editor.dll` 的 `playModeStateChanged`/`EditorApplication`
  钩子在无头环境行为异常，不是自有代码问题。处理：批处理验证必须由外部看门狗
  强杀收尾；交互式编辑器未复现（待用户确认）。若交互式编辑器在胜利后出现
  停止 Play 冻结，优先排查此条。
- 影响范围：仅验证流程经验，未改动任何运行时玩法代码。

## 记录格式

后续重大决策在文件末尾追加：日期、决策、理由、影响范围。若推翻旧决策，应明确
写出“替代哪一项”，不要删除历史记录。
