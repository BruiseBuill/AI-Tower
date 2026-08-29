using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 存活实体的全局注册表。实体在 Open 时注册、Close 时注销，战斗开始时清空一次防残留。
    /// 未来的索敌规则（优先级、属性过滤等）在此扩展独立查询方法即可。
    /// </summary>
    public static class CombatRegistry
    {
        static readonly List<ICombatEntity> playerEntities = new List<ICombatEntity>();
        static readonly List<ICombatEntity> enemyEntities = new List<ICombatEntity>();

        /// <summary>当前注册的玩家实体数（调试与验证用）。</summary>
        public static int PlayerCount => playerEntities.Count;

        /// <summary>当前注册的敌方实体数（调试与验证用）。</summary>
        public static int EnemyCount => enemyEntities.Count;

        public static void Clear()
        {
            playerEntities.Clear();
            enemyEntities.Clear();
        }

        /// <summary>关闭所有仍在注册表中的战斗实体，并清空注册表。</summary>
        public static void CloseAll()
        {
            CloseAll(playerEntities);
            CloseAll(enemyEntities);
            Clear();
        }

        static void CloseAll(List<ICombatEntity> entities)
        {
            ICombatEntity[] snapshot = entities.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                if (snapshot[i] != null && snapshot[i].IsAlive)
                {
                    snapshot[i].Close();
                }
            }
        }

        public static void Register(ICombatEntity entity)
        {
            if (entity == null)
            {
                return;
            }
            List<ICombatEntity> list = entity.Team == Team.Player ? playerEntities : enemyEntities;
            if (!list.Contains(entity))
            {
                list.Add(entity);
            }
        }

        public static void Unregister(ICombatEntity entity)
        {
            if (entity == null)
            {
                return;
            }
            playerEntities.Remove(entity);
            enemyEntities.Remove(entity);
        }

        /// <summary>
        /// 返回 position 附近 maxRange 内最近的存活敌对阵营实体；没有则返回 null。
        /// </summary>
        public static ICombatEntity NearestOpponent(Team team, Vector3 position, float maxRange)
        {
            return NearestOpponent(team, position, maxRange, null);
        }

        /// <summary>按阵营和实体类型返回范围内最近的存活敌对实体。</summary>
        public static ICombatEntity NearestOpponent(
            Team team,
            Vector3 position,
            float maxRange,
            CombatEntityKind? kind)
        {
            List<ICombatEntity> list = team == Team.Player ? enemyEntities : playerEntities;
            float bestSqrDistance = maxRange * maxRange;
            ICombatEntity best = null;
            for (int i = 0; i < list.Count; i++)
            {
                ICombatEntity entity = list[i];
                if (entity == null || !entity.IsAlive)
                {
                    continue;
                }
                if (kind.HasValue && entity.Kind != kind.Value)
                {
                    continue;
                }
                float sqrDistance = (entity.Position - position).sqrMagnitude;
                if (sqrDistance <= bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    best = entity;
                }
            }
            return best;
        }
    }
}
