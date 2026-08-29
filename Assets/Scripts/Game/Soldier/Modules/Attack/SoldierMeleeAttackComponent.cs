using BF;
using Game.Combat;
using Game.Core;
using UnityEngine;

namespace Game.Soldier
{
    /// <summary>只负责近战索敌和造成伤害，移动由 SoldierMovementComponent 独立处理。</summary>
    public class SoldierMeleeAttackComponent : BaseComponent<SoldierRuntimeData>
    {
        bool running;
        float cooldown;

        protected override void OnOpen()
        {
            running = true;
            cooldown = 0f;
        }

        protected override void OnClose() => running = false;

        void Update()
        {
            if (!running || !BattleFlow.IsActive || data.Definition == null)
            {
                return;
            }

            cooldown -= Time.deltaTime;
            ICombatEntity target = CombatRegistry.NearestOpponent(
                Team.Player,
                transform.position,
                data.Definition.AttackRange,
                CombatEntityKind.Tower);
            if (target == null)
            {
                target = CombatRegistry.NearestOpponent(
                    Team.Player,
                    transform.position,
                    data.Definition.AttackRange,
                    CombatEntityKind.Base);
            }
            data.SetCurrentTarget(target);
            if (target == null || cooldown > 0f)
            {
                return;
            }

            cooldown = data.Definition.AttackInterval;
            target.TakeDamage(data.Definition.AttackDamage);
        }
    }
}
