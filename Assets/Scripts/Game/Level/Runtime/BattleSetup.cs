using Game.Combat;
using Game.Content;
using Game.Core;
using Game.Base;
using Game.Tower;
using Game.UI;
using UnityEngine;

namespace Game.Level
{
    /// <summary>
    /// 场景入口：从 LevelDefinition 装配一场可玩的战斗。
    /// 职责：清理上一场战斗 -> 注入关卡配置 -> 生成大本营与防御塔 -> 绑定 HUD -> 开始战斗。
    /// </summary>
    public class BattleSetup : MonoBehaviour
    {
        [SerializeField] LevelDefinition levelDefinition;
        [SerializeField] BattleFlow battleFlow;
        [SerializeField] BattleHUD battleHUD;

        public LevelDefinition LevelDefinition => levelDefinition;

        void Start()
        {
            Time.timeScale = 1f;
            CombatRegistry.CloseAll();
            CombatPool.RecycleAll();
            battleFlow.Init(levelDefinition);
            BuildEnemies();
            battleHUD.Bind(levelDefinition, battleFlow);
            battleFlow.StartBattle();
        }

        void BuildEnemies()
        {
            // 大本营
            GameObject baseInstance = CombatPool.Spawn(levelDefinition.BaseDefinition.Prefab);
            BaseControl baseControl = baseInstance.GetComponent<BaseControl>();
            if (baseControl == null)
            {
                Debug.LogError("[Battle] 大本营预制体未挂载 Game.Base.BaseControl。", baseInstance);
                CombatPool.Recycle(baseInstance);
                return;
            }
            baseControl.Initialize(new BaseInit
            {
                Definition = levelDefinition.BaseDefinition,
                Position = levelDefinition.BasePosition
            });
            baseControl.Destroyed += _ => battleFlow.NotifyBaseDestroyed();
            baseControl.Open();

            // 防御塔
            foreach (LevelDefinition.TowerPlacement placement in levelDefinition.Towers)
            {
                if (placement == null || placement.definition == null)
                {
                    continue;
                }
                GameObject towerInstance = CombatPool.Spawn(placement.definition.Prefab);
                TowerBase towerControl = towerInstance.GetComponent<TowerBase>();
                if (towerControl == null)
                {
                    Debug.LogError($"[Battle] 防御塔预制体未挂载 TowerBase 具体子类：{placement.definition.Prefab.name}");
                    CombatPool.Recycle(towerInstance);
                    continue;
                }
                towerControl.Initialize(new TowerInit
                {
                    Definition = placement.definition,
                    Position = placement.position
                });
                towerControl.Open();
            }
        }

    }
}
