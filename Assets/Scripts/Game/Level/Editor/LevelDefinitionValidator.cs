#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Game.Content;
using UnityEditor;
using UnityEngine;

namespace Game.Level.Editor
{
    /// <summary>
    /// 关卡数据编辑器的轻量校验器。它只检查数据引用和结构，不修改资产。
    /// </summary>
    public static class LevelDefinitionValidator
    {
        public static bool TryValidate(LevelDefinition level, out List<string> errors)
        {
            errors = new List<string>();
            if (level == null)
            {
                errors.Add("关卡资产为空。");
                return false;
            }

            if (string.IsNullOrWhiteSpace(level.ContentId))
            {
                errors.Add("缺少稳定 Content ID。");
            }
            if (string.IsNullOrWhiteSpace(level.ScenePath))
            {
                errors.Add("缺少运行场景路径。");
            }
            else if (AssetDatabase.LoadAssetAtPath<SceneAsset>(level.ScenePath) == null)
            {
                errors.Add($"运行场景不存在：{level.ScenePath}");
            }

            if (level.BaseDefinition == null)
            {
                errors.Add("未配置敌方大本营数据。");
            }
            else if (level.BaseDefinition.Prefab == null)
            {
                errors.Add($"大本营数据未配置 Prefab：{level.BaseDefinition.ContentId}");
            }

            if (level.DeployableSoldiers == null || level.DeployableSoldiers.Count == 0)
            {
                errors.Add("至少配置一种可部署士兵。");
            }
            else
            {
                for (int i = 0; i < level.DeployableSoldiers.Count; i++)
                {
                    SoldierData soldier = level.DeployableSoldiers[i];
                    if (soldier == null)
                    {
                        errors.Add($"可部署士兵列表第 {i + 1} 项为空。");
                    }
                    else if (soldier.Prefab == null)
                    {
                        errors.Add($"士兵数据未配置 Prefab：{soldier.ContentId}");
                    }
                }
            }

            if (level.Towers == null || level.Towers.Count == 0)
            {
                errors.Add("至少配置一座防御塔。");
            }
            else
            {
                for (int i = 0; i < level.Towers.Count; i++)
                {
                    LevelDefinition.TowerPlacement placement = level.Towers[i];
                    if (placement == null || placement.definition == null)
                    {
                        errors.Add($"防御塔列表第 {i + 1} 项缺少塔数据。");
                    }
                    else
                    {
                        if (placement.definition.Prefab == null)
                        {
                            errors.Add($"防御塔数据未配置 Prefab：{placement.definition.ContentId}");
                        }
                        if (placement.definition.Projectile == null || placement.definition.Projectile.Prefab == null)
                        {
                            errors.Add($"防御塔数据缺少有效子弹 Prefab：{placement.definition.ContentId}");
                        }
                    }
                }
            }

            return errors.Count == 0;
        }

        [MenuItem("AIOnly/校验全部关卡")]
        static void ValidateAll()
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelDefinition", new[] { "Assets/Game/Content/Definitions/Levels" });
            int errorCount = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                LevelDefinition level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (!TryValidate(level, out List<string> errors))
                {
                    errorCount += errors.Count;
                    for (int j = 0; j < errors.Count; j++)
                    {
                        Debug.LogError($"[LevelEditor] {level.name}: {errors[j]}", level);
                    }
                }
            }

            if (errorCount == 0)
            {
                Debug.Log("[LevelEditor] 全部关卡数据校验通过。");
            }
            else
            {
                Debug.LogError($"[LevelEditor] 关卡数据校验失败，共 {errorCount} 个问题。");
            }
        }
    }
}
#endif
