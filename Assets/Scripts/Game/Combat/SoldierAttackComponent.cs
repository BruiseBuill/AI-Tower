using BF;
using Game.Content;
using Game.Core;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 士兵攻击：搜索射程内最近的敌方实体（防御塔或大本营）并停下近战攻击。
    /// 找到的目标会写入 data.CurrentTarget，供移动组件驻步。
    /// </summary>
    public class SoldierAttackComponent : BaseComponent<SoldierData>
    {
        bool running;
        float cooldown;

        protected override void OnOpen()
        {
            running = true;
            cooldown = 0f;
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
            cooldown -= Time.deltaTime;

            SoldierDefinition definition = data.Definition;
            ICombatEntity target = CombatRegistry.NearestOpponent(Team.Player, transform.position, definition.AttackRange);
            data.CurrentTarget = target;
            if (target == null || cooldown > 0f)
            {
                return;
            }

            cooldown = definition.AttackInterval;
            target.TakeDamage(definition.AttackDamage);
        }
    }
}
