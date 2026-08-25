using Game.Combat;
using Game.Content;
using Game.Core;
using Game.UI;
using UnityEngine;

namespace Game.Level
{
    /// <summary>
    /// 场景入口：从 LevelDefinition 装配一场可玩的战斗。
    /// 职责：清空战斗注册表 -> 注入关卡配置 -> 生成大本营与防御塔 -> 绑定 HUD -> 开始战斗。
    /// </summary>
    public class BattleSetup : MonoBehaviour
    {
        [SerializeField] LevelDefinition levelDefinition;
        [SerializeField] BattleFlow battleFlow;
        [SerializeField] BattleHUD battleHUD;
        [SerializeField, Tooltip("仅在编辑器显示行军路线辅助线")]
        bool drawPathGizmos = true;

        public LevelDefinition LevelDefinition => levelDefinition;

        void Start()
        {
            Time.timeScale = 1f;
            CombatRegistry.Clear();
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
                TowerControl towerControl = towerInstance.GetComponent<TowerControl>();
                towerControl.Initialize(new TowerInit
                {
                    Definition = placement.definition,
                    Position = placement.position
                });
                towerControl.Open();
            }
        }

        void OnDrawGizmos()
        {
            if (!drawPathGizmos || levelDefinition == null)
            {
                return;
            }
            var path = levelDefinition.SoldierPath;
            if (path == null || path.Count < 2)
            {
                return;
            }
            Gizmos.color = new Color(0.5f, 0.83f, 1f, 0.5f);
            for (int i = 0; i + 1 < path.Count; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }
    }
}
