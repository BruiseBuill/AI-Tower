using BF;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// 战斗层对 BF.PoolManager 的薄封装。
    /// 对象池键 = 预制体名称（PoolManager 约定），定义资产中的 prefab 引用决定键。
    /// </summary>
    public static class CombatPool
    {
        /// <summary>从对象池取出实例并激活。调用方负责随后 Initialize 与 Open。</summary>
        public static GameObject Spawn(GameObject prefab)
        {
            GameObject instance = PoolManager.Instance().Release(prefab.name);
            instance.SetActive(true);
            return instance;
        }

        /// <summary>回收实例到对象池。对象池不存在时（如场景正在销毁）退化为直接销毁。</summary>
        public static void Recycle(GameObject instance)
        {
            PoolManager pool = PoolManager.Instance();
            if (pool != null)
            {
                pool.Recycle(instance);
            }
            else
            {
                Object.Destroy(instance);
            }
        }
    }
}
