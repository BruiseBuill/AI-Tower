# 素材替换工作流

## 目标

正式美术、音乐和语音由用户提供。AI 应能先用方块、圆形、纯色 Sprite 或其他简单
内容完成玩法，同时保证正式素材到来后无需修改战斗和关卡逻辑即可替换。

## 默认方案

使用“稳定内容 ID + ScriptableObject 定义 + 统一资源目录 + 编辑器校验”的方式：

1. 每个单位、塔、特效、UI 图标和音频定义拥有不可随显示名称变化的稳定 ID。
2. 定义资产保存玩法参数，并引用独立的表现资源集合。
3. 玩法代码只请求定义或稳定 ID，不直接依赖 `Assets/...` 路径和预制体名称。
4. 占位表现与正式表现遵守相同预制体契约，可在 Inspector 中替换引用。
5. 提供编辑器校验工具，检查缺失引用、重复 ID、错误组件和不合规资源设置。
6. 内容规模扩大后，再评估是否需要 Addressables；首个垂直切片不默认引入它。

## 建议的表现资源契约

单位或塔的根预制体负责稳定的逻辑组件，其下设置可替换的 `VisualRoot`。正式素材替换
优先发生在 `VisualRoot` 下，不改变碰撞体、寻路锚点、射击点和逻辑脚本。

每类内容按需要声明明确锚点，例如：

- `VisualRoot`：所有纯表现对象的父节点。
- `BodyAnchor`：主体视觉位置。
- `ProjectileOrigin`：弹道起点。
- `HealthBarAnchor`：血条挂点。
- `VfxAnchor`：通用特效挂点。

没有实际需求的锚点不提前创建。

## 目录建议

- `Assets/Game/Content/Definitions`：单位、塔、关卡、成长等定义资产。
- `Assets/Game/Art/Placeholders`：AI 使用的简单占位资源。
- `Assets/Game/Art/Production`：用户提供的正式美术。
- `Assets/Game/Audio/Placeholders`：占位音频，通常可以为空或使用静音。
- `Assets/Game/Audio/Production`：用户提供的正式音频。
- `Assets/Game/Prefabs`：遵守统一契约的运行时预制体。

目录在首次产生对应内容时建立，不创建大量空目录。

## 已落地现状（垂直切片，2026-08-24）

- 定义资产目录：`Assets/Game/Content/Definitions/`，全部由
  `Game.Editor.BattleSceneBuilder` 幂等生成，持稳定 `contentId`：

  | contentId | 类型 | 说明 |
  | --- | --- | --- |
  | `soldier_basic` | `SoldierDefinition` | 突击兵（低费近战） |
  | `soldier_heavy` | `SoldierDefinition` | 重装兵（高费高耐久） |
  | `tower_basic` | `TowerDefinition` | 基础防御塔 |
  | `proj_basic` | `ProjectileDefinition` | 塔用追踪弹 |
  | `base_basic` | `BaseDefinition` | 敌方大本营 |
  | `level_001` | `LevelDefinition` | 首关：时限/能量/塔位/路径 |

- 运行时预制体：`Assets/Game/Prefabs/`（`Prefab_Soldier`、`Prefab_Tower`、
  `Prefab_Base`、`Prefab_Projectile`），均含 `VisualRoot` 表现契约节点；
  士兵预制体按 `SoldierDefinition.Tint` 着色。
- 改值入口：直接编辑定义资产的 Inspector 字段（能量、血量、速度、伤害、时限等），
  不需要改代码；关卡结构变化（塔位、路径点）也在 `level_001` 资产内。
- 一键重建：菜单 `AIOnly/构建垂直切片场景`（或批处理
  `-executeMethod Game.Editor.BattleSceneBuilder.BuildAll`）重建定义资产、
  预制体、场景与对象池注册，会覆盖手工修改。
- 素材替换现状：占位表现为程序生成的纯色 Sprite/方块；正式素材到来后，
  替换 `VisualRoot` 下的表现对象或定义资产上的 Sprite 引用即可，
  `contentId` 与存档保持不变。

## 替换操作的验收标准

- 替换 Sprite、动画、音频或特效引用后，玩法代码无需改动。
- 替换单位视觉后，路径、碰撞、选中、血条和攻击锚点仍正确。
- 资源缺失时显示明确占位内容或警告，不以空引用导致流程崩溃。
- 稳定 ID 不因文件改名、显示名称改变或美术版本更新而改变。
- 已写入存档的内容 ID 在替换素材后仍可正常读取。

## 用户交付素材时建议附带的信息

- 对应内容 ID 或用途。
- 图像尺寸、切图方式、Pivot 和 Pixels Per Unit（如适用）。
- 动画片段名称、循环规则和期望帧率。
- 特效或音频的触发时机。
- 是否替换现有内容、是否需要保留旧版本。

具体导入参数应在视觉规格确定后追加到本文件。
