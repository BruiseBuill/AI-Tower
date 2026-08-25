using UnityEngine;

namespace Game.Save
{
    /// <summary>
    /// 存档读写服务。直接构建在 Easy Save 3 之上（与 BF.MomentoManager 同一存储底座；
    /// MomentoManager 的 BaseMomento 多态加载尚不成熟，玩法侧暂不经过它）。
    /// 存档文件：Application.persistentDataPath/GameSave.es3。
    /// 读取失败时保留原文件并返回默认数据，绝不覆盖疑似损坏的存档。
    /// </summary>
    public static class SaveService
    {
        const string SaveKey = "game_save";

        static string FilePath => System.IO.Path.Combine(Application.persistentDataPath, "GameSave.es3");

        public static GameSaveData Load()
        {
            try
            {
                if (!ES3.FileExists(FilePath) || !ES3.KeyExists(SaveKey, FilePath))
                {
                    return new GameSaveData();
                }
                GameSaveData data = ES3.Load<GameSaveData>(SaveKey, new GameSaveData(), new ES3Settings(FilePath));
                if (data == null)
                {
                    return new GameSaveData();
                }
                if (data.schemaVersion != GameSaveData.CurrentSchemaVersion)
                {
                    // 未来版本迁移在此实现；当前策略：使用默认数据但保留旧文件。
                    Debug.LogWarning($"[Save] 存档版本 {data.schemaVersion} 与当前 {GameSaveData.CurrentSchemaVersion} 不一致，使用默认数据（旧文件保留）。");
                    return new GameSaveData();
                }
                return data;
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[Save] 读取失败，保留原文件：{exception}");
                return new GameSaveData();
            }
        }

        public static void Save(GameSaveData data)
        {
            try
            {
                ES3.Save(SaveKey, data, FilePath);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[Save] 写入失败：{exception}");
            }
        }

        /// <summary>胜利结算时调用：记录通关、累计胜场并刷新最佳剩余时间。</summary>
        public static void MarkLevelCompleted(string levelId, float timeLeftSeconds)
        {
            GameSaveData data = Load();
            LevelRecord record = data.FindLevel(levelId);
            if (record == null)
            {
                record = new LevelRecord { levelId = levelId };
                data.levels.Add(record);
            }
            record.completed = true;
            record.winCount++;
            record.bestTimeLeftSeconds = Mathf.Max(record.bestTimeLeftSeconds, timeLeftSeconds);
            Save(data);
            Debug.Log($"[Save] 关卡 {levelId} 通关进度已写入：{FilePath}");
        }
    }
}
