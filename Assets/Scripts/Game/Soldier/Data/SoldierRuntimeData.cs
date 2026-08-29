using System;
using BF;
using Game.Combat;
using Game.Content;
using UnityEngine;

using SoldierConfig = Game.Content.SoldierData;

namespace Game.Soldier
{
    /// <summary>
    /// 士兵运行时共享状态。与 SoldierData ScriptableObject 分离，实例可安全地被对象池复用。
    /// </summary>
    public class SoldierRuntimeData : BaseShareData, IDamagePresentation
    {
        [SerializeField] DataWithEvent<float> health = new DataWithEvent<float>();

        public SoldierConfig Definition { get; private set; }
        public ICombatEntity CurrentTarget { get; private set; }
        public float Health => health.Value;
        public float MaxHealth => Definition != null ? Definition.MaxHealth : 0f;

        public event Action<float> Damaged;
        public event Action<float, float> HealthChanged;

        public void Bind(SoldierConfig definition)
        {
            Definition = definition;
            CurrentTarget = null;
            health.ResetData(definition.MaxHealth);
        }

        public void SetCurrentTarget(ICombatEntity target) => CurrentTarget = target;
        public void ClearCurrentTarget() => CurrentTarget = null;

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
