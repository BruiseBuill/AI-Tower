using System;
using BF;
using Game.Combat;
using Game.Content;
using UnityEngine;

using TowerConfig = Game.Content.TowerData;

namespace Game.Tower
{
    /// <summary>
    /// 防御塔摆放参数，由 BattleSetup 传入。
    /// </summary>
    public sealed class TowerInit : ControlInit
    {
        public TowerConfig Definition;
        public Vector3 Position;
    }

    /// <summary>
    /// 防御塔抽象运行时基类。具体塔型通过独立的 MonoBehaviour 子类挂载到各自 Prefab。
    /// </summary>
    public abstract class TowerBase : BaseControl<TowerRuntimeData, TowerInit>, ICombatEntity
    {
        /// <summary>防御塔被摧毁（Close 完成）时触发。</summary>
        public event Action<TowerBase> Died;

        public Team Team => Team.Enemy;
        public CombatEntityKind Kind => CombatEntityKind.Tower;
        public Vector3 Position => transform.position;

        public override void Initialize(TowerInit parameters)
        {
            data.Bind(parameters.Definition);
            transform.position = parameters.Position;
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
    }
}
