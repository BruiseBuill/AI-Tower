using UnityEngine;

namespace Game.Content
{
    /// <summary>
    /// 子弹定义。当前为追踪弹：锁定目标后跟踪，命中造成伤害，目标死亡或超时则回收。
    /// </summary>
    [CreateAssetMenu(menuName = "AIOnly/Projectile Definition", fileName = "ProjectileDefinition")]
    public class ProjectileDefinition : ContentDefinition
    {
        [SerializeField, Min(0.1f)] float moveSpeed = 6f;
        [SerializeField, Min(0.05f), Tooltip("与目标的该距离内判定命中")]
        float hitRadius = 0.25f;
        [SerializeField, Min(0.1f), Tooltip("未命中时的自动回收时长（秒）")]
        float lifeTime = 5f;

        [Header("运行时")]
        [SerializeField, Tooltip("运行时预制体（需含 Projectile）。预制体名称即对象池键")]
        GameObject prefab;

        public float MoveSpeed => moveSpeed;
        public float HitRadius => hitRadius;
        public float LifeTime => lifeTime;
        public GameObject Prefab => prefab;
    }
}
