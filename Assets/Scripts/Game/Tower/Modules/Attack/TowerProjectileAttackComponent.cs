using BF;
using Game.Combat;
using Game.Core;
using Game.Projectile;
using UnityEngine;

using ProjectileBehaviour = Game.Projectile.Projectile;

namespace Game.Tower
{
    /// <summary>只负责按冷却发射追踪子弹，索敌由 TowerTargetingComponent 独立提供。</summary>
    public class TowerProjectileAttackComponent : BaseComponent<TowerRuntimeData>
    {
        [SerializeField, Tooltip("子弹发射点。留空时使用塔自身位置")]
        Transform projectileOrigin;

        bool running;
        float cooldown;

        protected override void OnOpen()
        {
            running = true;
            cooldown = data.Definition == null
                ? 0.1f
                : data.Definition.AttackInterval * UnityEngine.Random.Range(0.25f, 0.75f);
        }

        protected override void OnClose() => running = false;

        void Update()
        {
            if (!running || !BattleFlow.IsActive || data.Definition == null)
            {
                return;
            }

            cooldown -= Time.deltaTime;
            if (cooldown > 0f)
            {
                return;
            }

            ICombatEntity target = data.CurrentTarget;
            if (target == null || !target.IsAlive)
            {
                cooldown = 0.1f;
                return;
            }

            if (data.Definition.Projectile == null || data.Definition.Projectile.Prefab == null)
            {
                Debug.LogError($"[Battle] 防御塔数据缺少 Projectile 或 Projectile Prefab：{data.Definition.ContentId}", this);
                cooldown = 0.5f;
                return;
            }

            cooldown = data.Definition.AttackInterval;
            Vector3 origin = projectileOrigin != null ? projectileOrigin.position : transform.position;
            ProjectileBehaviour.Spawn(data.Definition.Projectile, origin, target, data.Definition.AttackDamage);
        }
    }
}
