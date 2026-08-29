using System.Collections.Generic;
using UnityEngine;

namespace Game.Content
{
    /// <summary>
    /// 所有关卡定义的稳定目录。关卡编辑器通过它管理关卡顺序和关卡资产引用，
    /// 运行时只读取单关 LevelDefinition，不依赖编辑器 API。
    /// </summary>
    [CreateAssetMenu(menuName = "AIOnly/Level Catalog", fileName = "LevelCatalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField] List<LevelDefinition> levels = new List<LevelDefinition>();

        public IReadOnlyList<LevelDefinition> Levels => levels;

        public LevelDefinition Find(string contentId)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                LevelDefinition level = levels[i];
                if (level != null && level.ContentId == contentId)
                {
                    return level;
                }
            }
            return null;
        }
    }
}
