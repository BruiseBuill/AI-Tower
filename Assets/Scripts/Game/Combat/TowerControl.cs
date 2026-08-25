using System;
using BF;
using Game.Content;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 防御塔摆放参数，由 BattleSetup 传入。
    /// </summary>
    public class TowerInit : ControlInit
    {
        public TowerDefinition Definition;
        public Vector3 Position;
    }

    /// <summary>
    /// 防御塔唯一对外入口：固定位置，自动攻击（见 TowerAttackComponent）。
    /// </summary>
    public class TowerControl : BaseControl<TowerData, TowerInit>, ICombatEntity
    {
        /// <summary>防御塔被摧毁（Close 完成）时触发。</summary>
        public event Action<TowerControl> Died;

        public Team Team => Team.Enemy;
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
