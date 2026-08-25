using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 可参与战斗的实体统一接口，用于塔索敌、士兵攻击与子弹命中。
    /// 实现者：SoldierControl、TowerControl、BaseControl。
    /// </summary>
    public interface ICombatEntity
    {
        Team Team { get; }

        /// <summary>是否存活。Close（含回池）之后为 false。</summary>
        bool IsAlive { get; }

        /// <summary>
        /// BaseShareData 的身份码，每次 Open 自增。
        /// 子弹用它区分“发射时锁定的实体”与“后来复用了同一对象池实例的新实体”。
        /// </summary>
        int IdentityCode { get; }

        Vector3 Position { get; }

        void TakeDamage(float amount);
    }
}
