using System;
using System.Collections.Generic;
using BF;
using Game.Content;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 士兵部署参数，由 BattleFlow 传入。
    /// </summary>
    public class SoldierInit : ControlInit
    {
        public SoldierDefinition Definition;
        public List<Vector3> Path;
        /// <summary>出生点附近的轻微偏移，避免多个士兵完全重叠。</summary>
        public Vector3 SpawnOffset;
    }

    /// <summary>
    /// 士兵唯一对外入口：接收部署参数、驱动生命周期、暴露战斗接口与死亡事件。
    /// </summary>
    public class SoldierControl : BaseControl<SoldierData, SoldierInit>, ICombatEntity
    {
        [SerializeField] SpriteRenderer bodyRenderer;

        /// <summary>士兵死亡（Close 完成）时触发。</summary>
        public event Action<SoldierControl> Died;

        public Team Team => Team.Player;
        public Vector3 Position => transform.position;

        public override void Initialize(SoldierInit parameters)
        {
            data.Bind(parameters.Definition, parameters.Path);
            if (parameters.Path != null && parameters.Path.Count > 0)
            {
                transform.position = parameters.Path[0] + parameters.SpawnOffset;
            }
            ApplyTint(parameters.Definition);
        }

        public void TakeDamage(float amount)
        {
            data.ApplyDamage(amount);
        }

        protected override void OnOpened()
        {
            CombatRegistry.Register(this);
        }

        protected override void OnClosed()
        {
            CombatRegistry.Unregister(this);
            Died?.Invoke(this);
            CombatPool.Recycle(gameObject);
        }

        void ApplyTint(SoldierDefinition definition)
        {
            if (bodyRenderer == null || definition.Tint.a <= 0f)
            {
                return;
            }
            bodyRenderer.color = definition.Tint;
        }
    }
}
