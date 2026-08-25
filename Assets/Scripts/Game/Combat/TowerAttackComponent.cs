using BF;
using Game.Content;
using Game.Core;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 防御塔攻击：搜索射程内最近的玩家士兵并发射子弹。
    /// 未来索敌策略（最前 / 最强等）可在此替换或做成可配置策略。
    /// </summary>
    public class TowerAttackComponent : BaseComponent<TowerData>
    {
        [SerializeField, Tooltip("子弹发射点。留空时使用塔自身位置")]
        Transform projectileOrigin;

        bool running;
        float cooldown;

        protected override void OnOpen()
        {
            running = true;
            // 错开首次射击时机，避免同射程多塔同步开火
            cooldown = data.Definition.AttackInterval * UnityEngine.Random.Range(0.25f, 0.75f);
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
            if (cooldown > 0f)
            {
                return;
            }

            TowerDefinition definition = data.Definition;
            ICombatEntity target = CombatRegistry.NearestOpponent(Team.Enemy, transform.position, definition.AttackRange);
            if (target == null)
            {
                cooldown = 0.1f;
                return;
            }

            cooldown = definition.AttackInterval;
            Vector3 origin = projectileOrigin != null ? projectileOrigin.position : transform.position;
            Projectile.Spawn(definition.Projectile, origin, target, definition.AttackDamage);
        }
    }
}
