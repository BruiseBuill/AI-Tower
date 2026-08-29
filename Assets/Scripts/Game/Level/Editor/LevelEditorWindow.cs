#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Game.Content;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Level.Editor
{
    /// <summary>
    /// 关卡数据编辑器：管理 LevelCatalog，并用 Scene 视图直接编辑塔位和大本营位置。
    /// 所有可运行数据最终写入 LevelDefinition ScriptableObject。
    /// </summary>
    public sealed class LevelEditorWindow : EditorWindow
    {
        const string LevelDirectory = "Assets/Game/Content/Definitions/Levels";
        const string CatalogPath = LevelDirectory + "/LevelCatalog.asset";

        LevelCatalog catalog;
        int selectedIndex = -1;
        Vector2 scrollPosition;
        string validationMessage;
        MessageType validationMessageType = MessageType.None;

        [MenuItem("AIOnly/关卡编辑器")]
        public static void Open()
        {
            GetWindow<LevelEditorWindow>("AIOnly 关卡编辑器");
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += DrawSceneHandles;
            RefreshCatalog();
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= DrawSceneHandles;
        }

        void OnGUI()
        {
            DrawToolbar();
            if (catalog == null)
            {
                EditorGUILayout.HelpBox("找不到关卡目录，请点击刷新。", MessageType.Warning);
                return;
            }

            DrawLevelSelector();
            LevelDefinition level = SelectedLevel;
            if (level == null)
            {
                EditorGUILayout.HelpBox("请选择一个关卡，或创建新的关卡资产。", MessageType.Info);
                return;
            }

            DrawLevelActions(level);
            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditorGUILayout.HelpBox(validationMessage, validationMessageType);
            }

            SerializedObject serialized = new SerializedObject(level);
            serialized.Update();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawLevelProperties(serialized);
            EditorGUILayout.EndScrollView();

            if (serialized.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(level);
                RepaintScene();
            }

            EditorGUILayout.Space(4f);
            if (GUILayout.Button("保存当前关卡数据"))
            {
                SaveAssets(level);
            }
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("刷新目录", EditorStyles.toolbarButton))
            {
                RefreshCatalog();
            }
            if (GUILayout.Button("创建关卡", EditorStyles.toolbarButton))
            {
                CreateLevel();
            }
            if (GUILayout.Button("保存全部", EditorStyles.toolbarButton))
            {
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawLevelActions(LevelDefinition level)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("打开运行场景"))
            {
                OpenScene(level);
            }
            if (GUILayout.Button("校验当前关卡"))
            {
                ValidateLevel(level);
            }
            EditorGUILayout.EndHorizontal();
        }

        void OpenScene(LevelDefinition level)
        {
            if (string.IsNullOrWhiteSpace(level.ScenePath) ||
                AssetDatabase.LoadAssetAtPath<SceneAsset>(level.ScenePath) == null)
            {
                validationMessage = $"无法打开场景，路径不存在：{level.ScenePath}";
                validationMessageType = MessageType.Error;
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(level.ScenePath, OpenSceneMode.Single);
            }
        }

        void ValidateLevel(LevelDefinition level)
        {
            if (LevelDefinitionValidator.TryValidate(level, out List<string> errors))
            {
                validationMessage = "当前关卡数据校验通过。";
                validationMessageType = MessageType.Info;
            }
            else
            {
                validationMessage = string.Join("\n", errors);
                validationMessageType = MessageType.Error;
            }
        }

        void DrawLevelSelector()
        {
            IReadOnlyList<LevelDefinition> levels = catalog.Levels;
            if (levels.Count == 0)
            {
                return;
            }

            string[] options = new string[levels.Count];
            for (int i = 0; i < levels.Count; i++)
            {
                LevelDefinition level = levels[i];
                options[i] = level == null ? "<空引用>" : $"{i + 1}. {level.DisplayName} [{level.ContentId}]";
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, levels.Count - 1);
            int nextIndex = EditorGUILayout.Popup("当前关卡", selectedIndex, options);
            if (nextIndex != selectedIndex)
            {
                selectedIndex = nextIndex;
                Selection.activeObject = SelectedLevel;
                RepaintScene();
            }
        }

        void DrawLevelProperties(SerializedObject serialized)
        {
            EditorGUILayout.LabelField("基础信息", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serialized.FindProperty("contentId"));
            EditorGUILayout.PropertyField(serialized.FindProperty("displayName"));
            EditorGUILayout.PropertyField(serialized.FindProperty("scenePath"));

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("战斗规则", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serialized.FindProperty("timeLimitSeconds"));
            EditorGUILayout.PropertyField(serialized.FindProperty("energyStart"));
            EditorGUILayout.PropertyField(serialized.FindProperty("energyMax"));
            EditorGUILayout.PropertyField(serialized.FindProperty("energyRegenPerSecond"));

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("玩家部署", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serialized.FindProperty("deployableSoldiers"), true);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("敌方布阵", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serialized.FindProperty("baseDefinition"));
            EditorGUILayout.PropertyField(serialized.FindProperty("basePosition"));
            EditorGUILayout.PropertyField(serialized.FindProperty("towers"), true);

            EditorGUILayout.HelpBox("士兵生成后直线前往大本营，进入塔吸引范围后优先转向防御塔。", MessageType.None);
        }

        void RefreshCatalog()
        {
            EnsureDirectory(LevelDirectory);
            catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = CreateInstance<LevelCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            List<LevelDefinition> discovered = FindLevelDefinitions();
            SyncCatalog(discovered);
            if (catalog.Levels.Count > 0)
            {
                selectedIndex = Mathf.Clamp(selectedIndex, 0, catalog.Levels.Count - 1);
            }
            else
            {
                selectedIndex = -1;
            }
            RepaintScene();
        }

        List<LevelDefinition> FindLevelDefinitions()
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelDefinition", new[] { LevelDirectory });
            var result = new List<LevelDefinition>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                LevelDefinition level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
                if (level != null)
                {
                    result.Add(level);
                }
            }
            result.Sort((left, right) => string.Compare(
                AssetDatabase.GetAssetPath(left),
                AssetDatabase.GetAssetPath(right),
                System.StringComparison.Ordinal));
            return result;
        }

        void SyncCatalog(List<LevelDefinition> discovered)
        {
            var discoveredSet = new HashSet<LevelDefinition>(discovered);
            var ordered = new List<LevelDefinition>();
            for (int i = 0; i < catalog.Levels.Count; i++)
            {
                LevelDefinition level = catalog.Levels[i];
                if (level != null && discoveredSet.Contains(level))
                {
                    ordered.Add(level);
                    discoveredSet.Remove(level);
                }
            }
            for (int i = 0; i < discovered.Count; i++)
            {
                if (discoveredSet.Contains(discovered[i]))
                {
                    ordered.Add(discovered[i]);
                }
            }

            SerializedObject serialized = new SerializedObject(catalog);
            SerializedProperty levels = serialized.FindProperty("levels");
            levels.arraySize = ordered.Count;
            for (int i = 0; i < ordered.Count; i++)
            {
                levels.GetArrayElementAtIndex(i).objectReferenceValue = ordered[i];
            }
            if (serialized.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
            }
        }

        void CreateLevel()
        {
            EnsureDirectory(LevelDirectory);
            string path = AssetDatabase.GenerateUniqueAssetPath(LevelDirectory + "/Def_Level_New.asset");
            LevelDefinition level = CreateInstance<LevelDefinition>();
            AssetDatabase.CreateAsset(level, path);

            string assetName = Path.GetFileNameWithoutExtension(path);
            SerializedObject serialized = new SerializedObject(level);
            serialized.FindProperty("contentId").stringValue = CreateLevelId();
            serialized.FindProperty("displayName").stringValue = assetName;
            serialized.FindProperty("scenePath").stringValue = "Assets/Scenes/" + assetName + ".unity";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();

            RefreshCatalog();
            selectedIndex = FindLevelIndex(level);
            Selection.activeObject = level;
            Repaint();
        }

        string CreateLevelId()
        {
            int suffix = 1;
            while (true)
            {
                string id = $"level_{suffix:000}";
                bool exists = false;
                for (int i = 0; i < catalog.Levels.Count; i++)
                {
                    if (catalog.Levels[i] != null && catalog.Levels[i].ContentId == id)
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    return id;
                }
                suffix++;
            }
        }

        void SaveAssets(LevelDefinition level)
        {
            EditorUtility.SetDirty(level);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            RepaintScene();
        }

        LevelDefinition SelectedLevel
        {
            get
            {
                if (catalog == null || selectedIndex < 0 || selectedIndex >= catalog.Levels.Count)
                {
                    return null;
                }
                return catalog.Levels[selectedIndex];
            }
        }

        int FindLevelIndex(LevelDefinition level)
        {
            for (int i = 0; i < catalog.Levels.Count; i++)
            {
                if (catalog.Levels[i] == level)
                {
                    return i;
                }
            }
            return -1;
        }

        void DrawSceneHandles(SceneView sceneView)
        {
            LevelDefinition level = SelectedLevel;
            if (level == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(level);
            serialized.Update();
            SerializedProperty towers = serialized.FindProperty("towers");
            SerializedProperty basePosition = serialized.FindProperty("basePosition");
            bool changed = false;
            bool undoRecorded = false;

            Handles.color = new Color(1f, 0.35f, 0.25f, 0.9f);
            for (int i = 0; i < towers.arraySize; i++)
            {
                SerializedProperty placement = towers.GetArrayElementAtIndex(i);
                SerializedProperty position = placement.FindPropertyRelative("position");
                Vector3 point = position.vector3Value;
                Handles.Label(point, $"塔 {i + 1}");
                EditorGUI.BeginChangeCheck();
                Vector3 moved = Handles.PositionHandle(point, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    RecordUndo(level, "移动防御塔", ref undoRecorded);
                    position.vector3Value = moved;
                    changed = true;
                }
            }

            Handles.color = new Color(0.95f, 0.75f, 0.2f, 0.9f);
            Handles.Label(basePosition.vector3Value, "大本营");
            EditorGUI.BeginChangeCheck();
            Vector3 movedBase = Handles.PositionHandle(basePosition.vector3Value, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                RecordUndo(level, "移动大本营", ref undoRecorded);
                basePosition.vector3Value = movedBase;
                changed = true;
            }

            if (changed)
            {
                serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(level);
                Repaint();
                SceneView.RepaintAll();
            }
        }

        static void RecordUndo(LevelDefinition level, string title, ref bool recorded)
        {
            if (!recorded)
            {
                Undo.RecordObject(level, title);
                recorded = true;
            }
        }

        static void RepaintScene()
        {
            SceneView.RepaintAll();
        }

        static void EnsureDirectory(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }
            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureDirectory(parent);
            }
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif
