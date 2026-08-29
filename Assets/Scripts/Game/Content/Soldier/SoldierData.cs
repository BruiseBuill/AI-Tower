using UnityEngine;

namespace Game.Content
{
    /// <summary>
    /// 士兵静态配置。它是 ScriptableObject，不参与场景生命周期，也不保存当前生命等运行时状态。
    /// </summary>
    [CreateAssetMenu(menuName = "AIOnly/Soldier Data", fileName = "SoldierData")]
    public class SoldierData : ContentDefinition
    {
        [Header("部署")]
        [SerializeField, Min(0f)] float energyCost = 50f;
        [SerializeField, Tooltip("该兵种对应的能量条档位颜色")]
        Color tierColor = Color.white;

        [Header("战斗")]
        [SerializeField, Min(1f)] float maxHealth = 100f;
        [SerializeField, Min(0.1f)] float moveSpeed = 1.6f;
        [SerializeField, Min(0.1f)] float attackRange = 0.9f;
        [SerializeField, Min(0.1f), Tooltip("进入该范围后会转向并优先攻击防御塔")]
        float towerDetectionRange = 2.5f;
        [SerializeField, Min(0.1f)] float attackDamage = 20f;
        [SerializeField, Min(0.05f)] float attackInterval = 1f;

        [Header("表现")]
        [SerializeField] Color tint = new Color(0f, 0f, 0f, 0f);

        [Header("运行时")]
        [SerializeField, Tooltip("包含对应 SoldierBase 具体子类和模块的运行时预制体")]
        GameObject prefab;

        public float EnergyCost => energyCost;
        public Color TierColor => tierColor;
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float AttackRange => attackRange;
        public float TowerDetectionRange => towerDetectionRange;
        public float AttackDamage => attackDamage;
        public float AttackInterval => attackInterval;
        public Color Tint => tint;
        public GameObject Prefab => prefab;
    }
}
