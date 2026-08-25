using UnityEngine;

namespace Game.Content
{
    /// <summary>
    /// 防御塔定义。塔自动索敌并向范围内玩家士兵发射子弹。
    /// </summary>
    [CreateAssetMenu(menuName = "AIOnly/Tower Definition", fileName = "TowerDefinition")]
    public class TowerDefinition : ContentDefinition
    {
        [Header("战斗")]
        [SerializeField, Min(1f)] float maxHealth = 150f;
        [SerializeField, Min(0.1f)] float attackRange = 2.6f;
        [SerializeField, Min(0.05f)] float attackInterval = 1.2f;
        [SerializeField, Min(0.1f)] float attackDamage = 12f;

        [Header("子弹")]
        [SerializeField] ProjectileDefinition projectile;

        [Header("运行时")]
        [SerializeField, Tooltip("运行时预制体（需含 TowerControl）。预制体名称即对象池键")]
        GameObject prefab;

        public float MaxHealth => maxHealth;
        public float AttackRange => attackRange;
        public float AttackInterval => attackInterval;
        public float AttackDamage => attackDamage;
        public ProjectileDefinition Projectile => projectile;
        public GameObject Prefab => prefab;
    }
}
