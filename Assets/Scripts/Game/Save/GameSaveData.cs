using System;
using System.Collections.Generic;

namespace Game.Save
{
    /// <summary>
    /// 单关进度记录。只保存稳定关卡 ID 与纯数值，不引用任何场景对象。
    /// </summary>
    [Serializable]
    public class LevelRecord
    {
        public string levelId;
        public bool completed;
        public int winCount;
        public float bestTimeLeftSeconds;
    }

    /// <summary>
    /// 存档纯数据模型（版本 1）。
    /// 字段变更时必须递增 schemaVersion，并在 SaveService.Load 中补充迁移逻辑。
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public const int CurrentSchemaVersion = 1;

        public int schemaVersion = CurrentSchemaVersion;
        public List<LevelRecord> levels = new List<LevelRecord>();

        public LevelRecord FindLevel(string levelId)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].levelId == levelId)
                {
                    return levels[i];
                }
            }
            return null;
        }

        public bool IsLevelCompleted(string levelId)
        {
            LevelRecord record = FindLevel(levelId);
            return record != null && record.completed;
        }
    }
}
