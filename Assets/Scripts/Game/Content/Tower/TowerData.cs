using UnityEngine;

namespace Game.Content
{
    /// <summary>
    /// 防御塔静态配置。它是 ScriptableObject，索敌和发射组件只读取这里的配置。
    /// </summary>
    [CreateAssetMenu(menuName = "AIOnly/Tower Data", fileName = "TowerData")]
    public class TowerData : ContentDefinition
    {
        [Header("战斗")]
        [SerializeField, Min(1f)] float maxHealth = 150f;
        [SerializeField, Min(0.1f)] float attackRange = 2.6f;
        [SerializeField, Min(0.05f)] float attackInterval = 1.2f;
        [SerializeField, Min(0.1f)] float attackDamage = 12f;

        [Header("子弹")]
        [SerializeField] ProjectileDefinition projectile;

        [Header("运行时")]
        [SerializeField, Tooltip("包含对应 TowerBase 具体子类和模块的运行时预制体")]
        GameObject prefab;

        public float MaxHealth => maxHealth;
        public float AttackRange => attackRange;
        public float AttackInterval => attackInterval;
        public float AttackDamage => attackDamage;
        public ProjectileDefinition Projectile => projectile;
        public GameObject Prefab => prefab;
    }
}
