using UnityEngine;

namespace Game.Content
{
    /// <summary>
    /// 进攻单位（士兵）定义。所有数值数据驱动：新增兵种只需新建资产并加入关卡的可部署列表。
    /// prefab 必须指向包含 SoldierControl 的运行时预制体；对象池键 = 预制体名称。
    /// </summary>
    [CreateAssetMenu(menuName = "AIOnly/Soldier Definition", fileName = "SoldierDefinition")]
    public class SoldierDefinition : ContentDefinition
    {
        [Header("部署")]
        [SerializeField, Min(0f), Tooltip("部署一次消耗的能量")]
        float energyCost = 50f;

        [Header("战斗")]
        [SerializeField, Min(1f)] float maxHealth = 100f;
        [SerializeField, Min(0.1f)] float moveSpeed = 1.6f;
        [SerializeField, Min(0.1f), Tooltip("攻击距离。进入该距离的敌方目标会被锁定并停下攻击")]
        float attackRange = 0.9f;
        [SerializeField, Min(0.1f)] float attackDamage = 20f;
        [SerializeField, Min(0.05f)] float attackInterval = 1f;

        [Header("表现")]
        [SerializeField, Tooltip("主体颜色覆盖。Alpha 为 0 时使用预制体自身颜色")]
        Color tint = new Color(0f, 0f, 0f, 0f);

        [Header("运行时")]
        [SerializeField, Tooltip("运行时预制体（需含 SoldierControl）。预制体名称即对象池键")]
        GameObject prefab;

        public float EnergyCost => energyCost;
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float AttackRange => attackRange;
        public float AttackDamage => attackDamage;
        public float AttackInterval => attackInterval;
        public Color Tint => tint;
        public GameObject Prefab => prefab;
    }
}
