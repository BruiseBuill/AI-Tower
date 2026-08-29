using BF;
using Game.Combat;
using Game.Content;
using Game.Core;
using UnityEngine;

namespace Game.Projectile
{
    /// <summary>
    /// 防御塔子弹：追踪锁定目标，命中造成伤害；目标死亡、被对象池复用或超时则回收。
    /// 遵循 SimpleObject 约定：Open(pos) 启用，Close() 回池。
    /// </summary>
    public class Projectile : SimpleObject
    {
        ICombatEntity target;
        int targetIdentity;
        float speed;
        float damage;
        float hitRadius;
        float maxLifeTime;
        float age;
        bool flying;

        /// <summary>
        /// 从对象池取出一颗子弹并发射。由 TowerAttackComponent 调用。
        /// </summary>
        public static Projectile Spawn(ProjectileDefinition definition, Vector3 origin, ICombatEntity newTarget, float newDamage)
        {
            GameObject instance = CombatPool.Spawn(definition.Prefab);
            Projectile projectile = instance.GetComponent<Projectile>();
            projectile.Initialize(definition, origin, newTarget, newDamage);
            projectile.Open(origin);
            return projectile;
        }

        void Initialize(ProjectileDefinition definition, Vector3 origin, ICombatEntity newTarget, float newDamage)
        {
            target = newTarget;
            targetIdentity = newTarget.IdentityCode;
            speed = definition.MoveSpeed;
            damage = newDamage;
            hitRadius = definition.HitRadius;
            maxLifeTime = definition.LifeTime;
            age = 0f;
            flying = true;
            transform.position = origin;
        }

        void Update()
        {
            if (!flying || !BattleFlow.IsActive)
            {
                return;
            }

            age += Time.deltaTime;
            if (age >= maxLifeTime || target == null || !target.IsAlive || target.IdentityCode != targetIdentity)
            {
                Close();
                return;
            }

            Vector3 position = transform.position;
            Vector3 delta = target.Position - position;
            float step = speed * Time.deltaTime;
            float hitDistance = Mathf.Max(step, hitRadius);
            if (delta.sqrMagnitude <= hitDistance * hitDistance)
            {
                ICombatEntity hitTarget = target;
                Close();
                hitTarget.TakeDamage(damage);
                return;
            }
            transform.position = position + delta.normalized * step;
        }

        public override void Close()
        {
            if (!flying && !gameObject.activeSelf)
            {
                return;
            }
            flying = false;
            target = null;
            base.Close();
        }
    }
}
