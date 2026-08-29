using UnityEngine;

namespace Game.Content
{
    /// <summary>
    /// 所有内容定义(士兵/防御塔/子弹/大本营/关卡)的基类。
    /// contentId 是稳定标识：关卡数据与存档数据都引用它，
    /// 一经设定不得随资产改名、显示名称或美术迭代而变化。
    /// </summary>
    public abstract class ContentDefinition : ScriptableObject
    {
        [SerializeField, Tooltip("稳定内容 ID。关卡与存档引用它，一经设定禁止修改。")]
        string contentId;

        [SerializeField, Tooltip("中文显示名。仅用于 UI 展示，逻辑不得引用。")]
        string displayName;

        public string ContentId => contentId;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    }
}
