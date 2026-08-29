using BF;
using Game.Combat;
using Game.Core;

namespace Game.Tower
{
    /// <summary>只负责维护防御塔当前目标，不执行攻击和伤害。</summary>
    public class TowerTargetingComponent : BaseComponent<TowerRuntimeData>
    {
        bool running;

        protected override void OnOpen() => running = true;

        protected override void OnClose()
        {
            running = false;
            data.ClearCurrentTarget();
        }

        void Update()
        {
            if (!running || !BattleFlow.IsActive || data.Definition == null)
            {
                return;
            }

            data.SetCurrentTarget(CombatRegistry.NearestOpponent(
                Team.Enemy,
                transform.position,
                data.Definition.AttackRange));
        }
    }
}
