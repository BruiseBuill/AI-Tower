using System;
using BF;
using Game.Combat;
using Game.Content;
using UnityEngine;

namespace Game.Base
{
    /// <summary>
    /// 敌方大本营共享运行时状态。大本营被摧毁即关卡胜利。
    /// </summary>
    public class BaseData : BaseShareData, IDamagePresentation
    {
        [SerializeField] DataWithEvent<float> health = new DataWithEvent<float>();

        public BaseDefinition Definition { get; private set; }
        public float Health => health.Value;
        public float MaxHealth => Definition != null ? Definition.MaxHealth : 0f;

        public event Action<float> Damaged;
        public event Action<float, float> HealthChanged;

        public void Bind(BaseDefinition definition)
        {
            Definition = definition;
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
