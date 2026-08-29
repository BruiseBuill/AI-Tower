using System;
using BF;
using Game.Combat;
using Game.Content;
using UnityEngine;

namespace Game.Base
{
    /// <summary>
    /// 大本营摆放参数，由 BattleSetup 传入。
    /// </summary>
    public sealed class BaseInit : ControlInit
    {
        public BaseDefinition Definition;
        public Vector3 Position;
    }

    /// <summary>
    /// 敌方大本营唯一对外入口。被摧毁时触发 Destroyed，由 BattleFlow 判定胜利。
    /// </summary>
    public sealed class BaseControl : BF.BaseControl<BaseData, BaseInit>, ICombatEntity
    {
        /// <summary>大本营被摧毁（Close 完成）时触发。</summary>
        public event Action<BaseControl> Destroyed;

        public Team Team => Team.Enemy;
        public CombatEntityKind Kind => CombatEntityKind.Base;
        public Vector3 Position => transform.position;

        public override void Initialize(BaseInit parameters)
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
            Destroyed?.Invoke(this);
            CombatPool.Recycle(gameObject);
        }
    }
}
