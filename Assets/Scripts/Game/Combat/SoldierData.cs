using System;
using System.Collections.Generic;
using BF;
using Game.Content;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 士兵共享运行时状态：生命、定义、进军路线、当前目标。
    /// 不引用 Control / Component；组件通过本类的领域方法与事件读写状态。
    /// </summary>
    public class SoldierData : BaseShareData, IDamagePresentation
    {
        [SerializeField] DataWithEvent<float> health = new DataWithEvent<float>();

        public SoldierDefinition Definition { get; private set; }
        public IReadOnlyList<Vector3> Path { get; private set; }
        public int PathIndex { get; set; }

        /// <summary>由攻击组件在目标进入射程时写入；移动组件据此驻步。</summary>
        public ICombatEntity CurrentTarget { get; set; }

        public float Health => health.Value;
        public float MaxHealth => Definition != null ? Definition.MaxHealth : 0f;

        /// <summary>受到伤害时触发（参数为本次伤害量）。</summary>
        public event Action<float> Damaged;

        /// <summary>生命变化（当前值, 上限）。预留给血条等表现。</summary>
        public event Action<float, float> HealthChanged;

        /// <summary>部署时重置运行时状态（对象池复用安全）。</summary>
        public void Bind(SoldierDefinition definition, IReadOnlyList<Vector3> path)
        {
            Definition = definition;
            Path = path;
            PathIndex = 0;
            CurrentTarget = null;
            health.ResetData(definition.MaxHealth);
        }

        public void ApplyDamage(float amount)
        {
            if (!IsOpen || amount <= 0f)
            {
                return;
            }
            float newHealth = Mathf.Max(0f, health.Value - amount);
            health.Value = newHealth;
            HealthChanged?.Invoke(newHealth, MaxHealth);
            Damaged?.Invoke(amount);
            if (newHealth <= 0f)
            {
                RequestClose();
            }
        }
    }
}
