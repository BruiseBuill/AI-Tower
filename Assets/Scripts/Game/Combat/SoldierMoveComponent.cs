using BF;
using Game.Core;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 士兵移动：沿关卡路径点推进；当前目标存活且在攻击范围内时驻步。
    /// </summary>
    public class SoldierMoveComponent : BaseComponent<SoldierData>
    {
        /// <summary>目标脱离攻击距离超过该倍率时放弃锁定，避免边界抖动。</summary>
        const float ReleaseTargetRangeFactor = 1.2f;

        bool running;

        protected override void OnOpen()
        {
            running = true;
        }

        protected override void OnClose()
        {
            running = false;
        }

        void Update()
        {
            if (!running || !BattleFlow.IsActive)
            {
                return;
            }
            if (HoldsForCombat())
            {
                return;
            }

            var path = data.Path;
            if (path == null || data.PathIndex >= path.Count)
            {
                return;
            }

            Vector3 waypoint = path[data.PathIndex];
            Vector3 position = transform.position;
            Vector3 delta = waypoint - position;
            float step = data.Definition.MoveSpeed * Time.deltaTime;
            if (delta.sqrMagnitude <= step * step)
            {
                transform.position = waypoint;
                data.PathIndex++;
            }
            else
            {
                transform.position = position + delta.normalized * step;
            }
        }

        bool HoldsForCombat()
        {
            ICombatEntity target = data.CurrentTarget;
            if (target == null)
            {
                return false;
            }
            if (!target.IsAlive)
            {
                data.CurrentTarget = null;
                return false;
            }
            float releaseRange = data.Definition.AttackRange * ReleaseTargetRangeFactor;
            if ((target.Position - transform.position).sqrMagnitude > releaseRange * releaseRange)
            {
                data.CurrentTarget = null;
                return false;
            }
            return true;
        }
    }
}
