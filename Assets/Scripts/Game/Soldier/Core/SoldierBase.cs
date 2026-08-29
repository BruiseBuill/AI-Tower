using System;
using BF;
using Game.Combat;
using Game.Content;
using UnityEngine;

using SoldierConfig = Game.Content.SoldierData;

namespace Game.Soldier
{
    /// <summary>
    /// 士兵部署参数，由 BattleFlow 传入。
    /// </summary>
    public sealed class SoldierInit : ControlInit
    {
        public SoldierConfig Definition;
        public Vector3 SpawnPosition;
    }

    /// <summary>
    /// 士兵抽象运行时基类。具体兵种通过独立的 MonoBehaviour 子类挂载到各自 Prefab。
    /// </summary>
    public abstract class SoldierBase : BaseControl<SoldierRuntimeData, SoldierInit>, ICombatEntity
    {
        [SerializeField] SpriteRenderer bodyRenderer;

        /// <summary>士兵死亡（Close 完成）时触发。</summary>
        public event Action<SoldierBase> Died;

        public Team Team => Team.Player;
        public CombatEntityKind Kind => CombatEntityKind.Soldier;
        public Vector3 Position => transform.position;

        public override void Initialize(SoldierInit parameters)
        {
            data.Bind(parameters.Definition);
            transform.position = parameters.SpawnPosition;
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

        protected virtual void ApplyTint(SoldierConfig definition)
        {
            if (bodyRenderer == null || definition.Tint.a <= 0f)
            {
                return;
            }
            bodyRenderer.color = definition.Tint;
        }
    }
}
