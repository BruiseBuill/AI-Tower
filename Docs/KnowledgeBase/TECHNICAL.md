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
- `BaseObject`、`BaseControl`、`BaseComponent`、`BaseShareData`：Control + Component + Data
  对象组合与生命周期框架；支持强类型 Data、命令入口、内部事件和外部事件出口。
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
- Control + Component + Data 的核心生命周期已完成首轮修复，但现有业务对象尚未使用它，
  仍需在首个垂直切片中验证对象池复用、异常路径和 Unity 组件挂载方式。
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

## Control + Component + Data 对象架构

该模式用于一个运行时物体内部的组合，不作为跨系统全局消息总线。它更接近
“外观入口 + 共享状态/领域事件 + 行为组件”，不是传统 MVC。

### 固定职责与依赖方向

- `Control`：物体唯一公开入口。接收外部命令、校验调用时机、驱动 Data，并把少量
  领域结果转换成外部只读事件。外界不得直接修改 Data 或调用 Component。
- `Data`：保存该物体的运行时共享状态，提供有业务含义的方法，并发布物体内部的
  状态/领域事件。Data 不引用具体 Control 或 Component，也不执行表现、物理、寻路等行为。
- `Component`：实现移动、攻击、动画、受击表现等单一行为；只依赖具体 Data，监听
  Data 事件并通过 Data 的领域方法提交结果。Component 之间不得直接互相引用。
- 推荐编译期依赖为 `Control -> Data <- Component`。运行时事件回流可以是
  `Data -> Control` 和 `Data -> Component`，但 Data 不持有二者的实例引用。

### 通信流

1. 外部命令：`调用方 -> Control.命令方法 -> Data.领域方法 -> Data 内部事件 -> Component`。
2. 内部结果：`Component -> Data.领域方法 -> Data 内部事件 -> 其他 Component`。
3. 对外通知：`Component -> Data.领域方法/领域事件 -> Control 订阅并转发 -> 外部订阅者`。

不要在 Control 中为每个内部事件机械建立同名中转事件。只有跨越物体边界、确实属于
外部契约的结果（例如死亡、到达终点、部署完成）才由 Control 暴露。纯表现事件、缓存刷新
和组件协作事件留在 Data 内部。

### 生命周期契约

- 只允许外部调用 `Control.Open()` / `Control.Close()`；Data 的实际打开、关闭入口是
  `internal`，Component 的入口由框架调度。
- 打开时按 `priority` 从小到大调用 Component 的 `OnOpen()`；关闭时严格逆序调用
  `OnClose()`。重复打开/关闭是幂等操作。
- Component 在 `Awake()` 注册、`OnDestroy()` 注销。每次 `OnOpen()` 订阅的 Data 事件
  必须在 `OnClose()` 对称解绑，保证对象池重复使用不会累积监听器。
- Data 通过 `RequestClose()` 请求关闭，由 Control 执行最终生命周期；Data 不保存
  Control 引用。

### 推荐继承方式

```csharp
public readonly struct UnitInit : BF.ControlInit
{
    public readonly int Health;
    public UnitInit(int health) => Health = health;
}

public sealed class UnitData : BF.BaseShareData
{
    public readonly DataWithEvent<int> Health = new DataWithEvent<int>();

    public void ApplyDamage(int amount)
    {
        Health.Value = Mathf.Max(0, Health.Value - amount);
        if (Health.Value == 0)
        {
            RequestClose();
        }
    }
}

public sealed class UnitControl : BF.BaseControl<UnitData, UnitInit>
{
    public override void Initialize(UnitInit parameters) { /* 写入初始状态 */ }
    public void TakeDamage(int amount) => data.ApplyDamage(amount);
}

public sealed class UnitVisual : BF.BaseComponent<UnitData>
{
    protected override void OnOpen() => data.Health.onValueChange += Refresh;
    protected override void OnClose() => data.Health.onValueChange -= Refresh;
    void Refresh(int health) { /* 只更新表现 */ }
}
```

### 已规避与仍需控制的风险

- 已规避：Data 持有 Control/Component 导致的循环依赖；外界绕过 Control；组件关闭越界；
  同优先级/最大优先级组件未注册；重复开关和销毁时未解绑框架引用。
- 仍需控制：Data 容易继续膨胀成“上帝对象”。复杂对象应按领域拆分明确的方法和事件，
  纯配置放 ScriptableObject，跨对象协调交给更上层系统，不把全局服务塞入 Data。
- 事件是同步调用并存在重入风险。事件处理器应短小，不在监听回调中任意嵌套多次状态写入；
  需要严格顺序的业务流程应由 Control 的单个命令方法完成。
- 事件命名应表达事实（如 `Died`、`HealthChanged`），命令方法表达意图
  （如 `TakeDamage`），避免只按参数类型建立难以追踪的通用事件。

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
