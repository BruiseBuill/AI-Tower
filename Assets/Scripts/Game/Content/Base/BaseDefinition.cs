using UnityEngine;

namespace Game.Content
{
    /// <summary>
    /// 敌方大本营定义。大本营被摧毁即关卡胜利。
    /// </summary>
    [CreateAssetMenu(menuName = "AIOnly/Base Definition", fileName = "BaseDefinition")]
    public class BaseDefinition : ContentDefinition
    {
        [SerializeField, Min(1f)] float maxHealth = 400f;

        [Header("运行时")]
        [SerializeField, Tooltip("运行时预制体（需含 Game.Base.BaseControl）。预制体名称即对象池键")]
        GameObject prefab;

        public float MaxHealth => maxHealth;
        public GameObject Prefab => prefab;
    }
}
