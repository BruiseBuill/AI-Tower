# 技术基线与架构

## 当前工程基线

- Unity：`2022.2.15f1c1`。
- 渲染管线：URP `14.0.7`，2D Renderer。
- 默认分辨率配置：1920 x 1080；最终屏幕方向和适配范围待确认。
- 输入：当前项目设置使用旧 Input Manager；`BF.InputManager` 基于鼠标 API，并通过
  Android 上的触摸到鼠标模拟实现基础兼容。
- 场景：构建列表目前只有 `Assets/Scenes/SampleScene.unity`，内容基本为空。
- 自有代码：`Assets/Scripts/BaseFramework` 下约 43 个 C# 文件。
- 第三方：DOTween Pro、Odin Inspector、Easy Save 3。
- 测试：Unity Test Framework 包存在，但用户不要求自有自动化测试。

## BaseFramework 已有能力

- `BF.GameManager`：游戏/暂停状态及全局生命周期基础。
- `BF.InputManager`：点击、拖动、双击、长按和 Windows 键盘事件。
- `BF.PoolManager`：按预制体名称索引的对象池。
- `BF.AudioManager`：BGM、UI、SFX 与场景音源管理。
- `BF.MomentoManager`：基于 Easy Save 3 的存档雏形。
- `BF.TransitManager`：附加场景加载和淡入淡出。
- `BF.EventChannel` 系列：ScriptableObject 事件通道。
- `BaseObject`、`BaseControl`、`BaseComponent`、`BaseShareData`：对象和组件生命周期框架。
- `TweenAnimation` 系列：基于 DOTween 的 UI 动画组件。

这些代码属于“已有基础”，不等于已经验证可用于正式玩法。修改时保留框架归属，
逐项审查并修复，不另起一套重复系统。

## 已知技术风险

- 当前 Unity 版本不是 LTS；未经用户批准不得升级。
- 最近的编辑器日志曾包含 Odin 和 Easy Save 3 相关编译错误。日志也可能包含旧状态，
  因此下一次正式开发前需以当前 Unity Console/新日志重新确认。
- `BF.InputManager` 依赖 `Input.mousePosition`、固定 1920 x 1080 参考值和
  `EventSystem.current`，需要验证不同 Android 分辨率、刘海/安全区及无 EventSystem 场景。
- `BF.AudioManager` 运行时代码直接引用了 `UnityEditor` 命名空间，Android 构建前需修正。
- `BaseShareData.Close()` 的逆序循环边界、部分生命周期和事件解绑逻辑需要专项审查。
- 对象池以预制体名称作为键，重命名或同名资源可能破坏引用；长期应迁移到稳定 ID。
- `MomentoManager` 仍是未完成雏形，抽象基类直接反序列化和版本迁移策略尚未定义。
- 自有运行时代码缺少独立 Assembly Definition，第三方和游戏代码边界不清晰。

## 建议模块边界

保持 `BaseFramework` 作为通用基础层，在其外增加面向本游戏的模块：

- `Assets/Scripts/Game/Core`：游戏流程、关卡状态和共享契约。
- `Assets/Scripts/Game/Combat`：单位、防御塔、伤害、状态和目标选择。
- `Assets/Scripts/Game/Level`：关卡加载、地图、路径、部署区和胜负条件。
- `Assets/Scripts/Game/Progression`：解锁、奖励和成长。
- `Assets/Scripts/Game/Save`：具体存档模型、版本和迁移。
- `Assets/Scripts/Game/UI`：战斗 HUD、关卡选择、成长和设置界面。
- `Assets/Scripts/Game/Content`：ScriptableObject 配置与稳定内容 ID。
- `Assets/Scripts/Game/Editor`：内容校验和素材替换工具，仅编辑器使用。

目录只在相关系统开始实现时创建，不提前生成空架构。

## 运行时原则

- Android 优先，避免每帧分配、频繁反射和无界 `Update()` 数量。
- 高频生成的单位、弹道和特效使用对象池，但先基于实际规模保持简单。
- 静态配置与运行时状态分离；场景负责布局，不承担全局进度存储。
- UI 与战场输入统一仲裁，避免触摸穿透。
- 所有内容实体使用稳定字符串 ID 或等价稳定键，不以显示名称、路径或对象名称作为
  长期存档身份。
- 编辑器专用 API 必须放入 Editor 目录、Editor 程序集或条件编译块中。

## 验证策略

用户不要求自动化测试和截图。AI 每次交付仍需：

- 尽量确认当前脚本能编译且 Console 没有新错误。
- 为用户给出 3 至 6 步的 Unity 手动验收流程。
- 涉及 Android 特性的功能在结论中注明是否只在编辑器验证。
- 不声称已完成未实际运行或未由日志证实的验证。
