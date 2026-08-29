using BF;
using Game.Combat;
using Game.Core;
using UnityEngine;

namespace Game.Soldier
{
    /// <summary>负责朝大本营直线移动，并在靠近防御塔时转向追塔。</summary>
    public class SoldierMovementComponent : BaseComponent<SoldierRuntimeData>
    {
        const float ReleaseTargetRangeFactor = 1.2f;
        bool running;

        protected override void OnOpen() => running = true;
        protected override void OnClose() => running = false;

        void Update()
        {
            if (!running || !BattleFlow.IsActive || data.Definition == null)
            {
                return;
            }

            ICombatEntity tower = CombatRegistry.NearestOpponent(
                Team.Player,
                transform.position,
                data.Definition.TowerDetectionRange,
                CombatEntityKind.Tower);
            if (tower != null)
            {
                // 塔的吸引优先级高于当前的大本营目标，进入范围后才改变方向。
                if (data.CurrentTarget != null && data.CurrentTarget.Kind != CombatEntityKind.Tower)
                {
                    data.ClearCurrentTarget();
                }
                if (!HoldsForCombat())
                {
                    MoveTowards(tower);
                }
                return;
            }

            if (HoldsForCombat())
            {
                return;
            }

            ICombatEntity baseTarget = CombatRegistry.NearestOpponent(
                Team.Player,
                transform.position,
                float.PositiveInfinity,
                CombatEntityKind.Base);
            if (baseTarget != null)
            {
                MoveTowards(baseTarget);
            }
        }

        void MoveTowards(ICombatEntity target)
        {
            Vector3 delta = target.Position - transform.position;
            float step = data.Definition.MoveSpeed * Time.deltaTime;
            if (delta.sqrMagnitude <= step * step)
            {
                transform.position = target.Position;
                return;
            }

            Vector3 position = transform.position;
            transform.position = position + delta.normalized * step;
        }

        bool HoldsForCombat()
        {
            ICombatEntity target = data.CurrentTarget;
            if (target == null)
            {
                return false;
            }
            if (!target.IsAlive || data.Definition == null)
            {
                data.ClearCurrentTarget();
                return false;
            }

            float releaseRange = data.Definition.AttackRange * ReleaseTargetRangeFactor;
            if ((target.Position - transform.position).sqrMagnitude > releaseRange * releaseRange)
            {
                data.ClearCurrentTarget();
                return false;
            }
            return true;
        }
    }
}
