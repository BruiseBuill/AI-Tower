using System;
using System.Collections.Generic;
using BF;
using Game.Combat;
using Game.Content;
using Game.Core;
using Game.Level;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Game.Editor
{
    /// <summary>
    /// 垂直切片一键构建器：生成定义资产、运行时预制体、战斗场景与构建设置。
    /// 幂等：重复执行会复用已有资产并覆盖数值，GUID 与稳定 ID 保持不变。
    /// 入口：菜单 “AIOnly/构建垂直切片场景”，或批处理
    ///   -executeMethod Game.Editor.BattleSceneBuilder.BuildAll
    /// UI 构建部分见 BattleSceneBuilderUI.cs。
    /// </summary>
    public static partial class BattleSceneBuilder
    {
        const string ContentDirectory = "Assets/Game/Content/Definitions";
        const string PrefabDirectory = "Assets/Game/Prefabs";
        const string ScenePath = "Assets/Scenes/Level001.unity";

        const string BoxSpritePath = "Assets/Art/PolySprite/Poly/Box.png";
        const string CircleSpritePath = "Assets/Art/PolySprite/Poly/Circle.png";
        const string BoxLineSpritePath = "Assets/Art/PolySprite/Poly/BoxLine.png";

        static readonly Color BackgroundColor = new Color32(0x16, 0x16, 0x1E, 0xFF);
        static readonly Color SoldierColor = new Color32(0x7F, 0xD4, 0xFF, 0xFF);
        static readonly Color HeavySoldierColor = new Color32(0x3E, 0x8E, 0xE0, 0xFF);
        static readonly Color TowerColor = new Color32(0x7A, 0x1F, 0x1F, 0xFF);
        static readonly Color BaseColor = new Color32(0x4A, 0x10, 0x10, 0xFF);
        static readonly Color ProjectileColor = new Color32(0xFF, 0xE0, 0x8A, 0xFF);
        static readonly Color PathColor = new Color(1f, 1f, 1f, 0.10f);

        class Definitions
        {
            public SoldierDefinition soldierBasic;
            public SoldierDefinition soldierHeavy;
            public ProjectileDefinition projectile;
            public TowerDefinition tower;
            public BaseDefinition baseDef;
            public LevelDefinition level;
        }

        class Prefabs
        {
            public GameObject soldier;
            public GameObject tower;
            public GameObject basePrefab;
            public GameObject projectile;
        }

        [MenuItem("AIOnly/构建垂直切片场景")]
        public static void BuildAll()
        {
            EnsureDirectory(ContentDirectory);
            EnsureDirectory(PrefabDirectory);

            Sprite boxSprite = LoadSprite(BoxSpritePath);
            Sprite circleSprite = LoadSprite(CircleSpritePath);
            Sprite boxLineSprite = LoadSprite(BoxLineSpritePath);

            Prefabs prefabs = CreatePrefabs(boxSprite, circleSprite);
            Definitions definitions = CreateDefinitions(prefabs);
            BuildScene(boxSprite, boxLineSprite, definitions, prefabs);
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            Debug.Log($"[Builder] 垂直切片构建完成。入口场景：{ScenePath}");
        }

        // ---------- 定义资产 ----------

        static Definitions CreateDefinitions(Prefabs prefabs)
        {
            var definitions = new Definitions();

            definitions.soldierBasic = CreateOrLoadAsset<SoldierDefinition>("Def_Soldier_Basic");
            EditAsset(definitions.soldierBasic, so =>
            {
                so.FindProperty("contentId").stringValue = "soldier_basic";
                so.FindProperty("displayName").stringValue = "突击兵";
                so.FindProperty("energyCost").floatValue = 50f;
                so.FindProperty("maxHealth").floatValue = 120f;
                so.FindProperty("moveSpeed").floatValue = 1.6f;
                so.FindProperty("attackRange").floatValue = 0.9f;
                so.FindProperty("attackDamage").floatValue = 20f;
                so.FindProperty("attackInterval").floatValue = 1f;
                so.FindProperty("tint").colorValue = new Color(0f, 0f, 0f, 0f);
                so.FindProperty("prefab").objectReferenceValue = prefabs.soldier;
            });

            definitions.soldierHeavy = CreateOrLoadAsset<SoldierDefinition>("Def_Soldier_Heavy");
            EditAsset(definitions.soldierHeavy, so =>
            {
                so.FindProperty("contentId").stringValue = "soldier_heavy";
                so.FindProperty("displayName").stringValue = "重装兵";
                so.FindProperty("energyCost").floatValue = 120f;
                so.FindProperty("maxHealth").floatValue = 360f;
                so.FindProperty("moveSpeed").floatValue = 1.1f;
                so.FindProperty("attackRange").floatValue = 1.0f;
                so.FindProperty("attackDamage").floatValue = 50f;
                so.FindProperty("attackInterval").floatValue = 1.3f;
                so.FindProperty("tint").colorValue = HeavySoldierColor;
                so.FindProperty("prefab").objectReferenceValue = prefabs.soldier;
            });

            definitions.projectile = CreateOrLoadAsset<ProjectileDefinition>("Def_Projectile_Basic");
            EditAsset(definitions.projectile, so =>
            {
                so.FindProperty("contentId").stringValue = "proj_basic";
                so.FindProperty("displayName").stringValue = "防御塔炮弹";
                so.FindProperty("moveSpeed").floatValue = 6f;
                so.FindProperty("hitRadius").floatValue = 0.25f;
                so.FindProperty("lifeTime").floatValue = 5f;
                so.FindProperty("prefab").objectReferenceValue = prefabs.projectile;
            });

            definitions.tower = CreateOrLoadAsset<TowerDefinition>("Def_Tower_Basic");
            EditAsset(definitions.tower, so =>
            {
                so.FindProperty("contentId").stringValue = "tower_basic";
                so.FindProperty("displayName").stringValue = "防御塔";
                so.FindProperty("maxHealth").floatValue = 150f;
                so.FindProperty("attackRange").floatValue = 2.6f;
                so.FindProperty("attackInterval").floatValue = 1.4f;
                so.FindProperty("attackDamage").floatValue = 10f;
                so.FindProperty("projectile").objectReferenceValue = definitions.projectile;
                so.FindProperty("prefab").objectReferenceValue = prefabs.tower;
            });

            definitions.baseDef = CreateOrLoadAsset<BaseDefinition>("Def_Base_Basic");
            EditAsset(definitions.baseDef, so =>
            {
                so.FindProperty("contentId").stringValue = "base_basic";
                so.FindProperty("displayName").stringValue = "敌方大本营";
                so.FindProperty("maxHealth").floatValue = 400f;
                so.FindProperty("prefab").objectReferenceValue = prefabs.basePrefab;
            });

            definitions.level = CreateOrLoadAsset<LevelDefinition>("Def_Level_001");
            EditAsset(definitions.level, so =>
            {
                so.FindProperty("contentId").stringValue = "level_001";
                so.FindProperty("displayName").stringValue = "第 1 关：突破演练";
                so.FindProperty("timeLimitSeconds").floatValue = 99f;
                so.FindProperty("energyStart").floatValue = 100f;
                so.FindProperty("energyMax").floatValue = 200f;
                so.FindProperty("energyRegenPerSecond").floatValue = 8f;

                SerializedProperty soldiers = so.FindProperty("deployableSoldiers");
                soldiers.arraySize = 2;
                soldiers.GetArrayElementAtIndex(0).objectReferenceValue = definitions.soldierBasic;
                soldiers.GetArrayElementAtIndex(1).objectReferenceValue = definitions.soldierHeavy;

                so.FindProperty("baseDefinition").objectReferenceValue = definitions.baseDef;
                so.FindProperty("basePosition").vector3Value = new Vector3(7.2f, 0f, 0f);

                SerializedProperty towers = so.FindProperty("towers");
                towers.arraySize = 3;
                SetTowerPlacement(towers.GetArrayElementAtIndex(0), definitions.tower, new Vector3(-1.2f, 1.4f, 0f));
                // 第二座塔贴近行军路线，士兵会在其攻击距离内停下并反击
                SetTowerPlacement(towers.GetArrayElementAtIndex(1), definitions.tower, new Vector3(1.8f, -0.7f, 0f));
                SetTowerPlacement(towers.GetArrayElementAtIndex(2), definitions.tower, new Vector3(4.6f, 1.2f, 0f));

                SerializedProperty path = so.FindProperty("soldierPath");
                path.arraySize = 5;
                path.GetArrayElementAtIndex(0).vector3Value = new Vector3(-7.6f, -0.6f, 0f);
                path.GetArrayElementAtIndex(1).vector3Value = new Vector3(-3.5f, -0.6f, 0f);
                path.GetArrayElementAtIndex(2).vector3Value = new Vector3(0.5f, 0.2f, 0f);
                path.GetArrayElementAtIndex(3).vector3Value = new Vector3(4f, 0f, 0f);
                path.GetArrayElementAtIndex(4).vector3Value = new Vector3(6.6f, 0f, 0f);
            });

            return definitions;
        }

        static void SetTowerPlacement(SerializedProperty element, TowerDefinition definition, Vector3 position)
        {
            element.FindPropertyRelative("definition").objectReferenceValue = definition;
            element.FindPropertyRelative("position").vector3Value = position;
        }

        // ---------- 预制体 ----------

        static Prefabs CreatePrefabs(Sprite boxSprite, Sprite circleSprite)
        {
            var prefabs = new Prefabs();

            // 士兵
            {
                var root = new GameObject("Prefab_Soldier");
                SpriteRenderer body = AddVisual(root, boxSprite, SoldierColor, new Vector3(0.55f, 0.55f, 1f), 10);
                root.AddComponent<SoldierData>();
                var control = root.AddComponent<SoldierControl>();
                root.AddComponent<SoldierMoveComponent>();
                root.AddComponent<SoldierAttackComponent>();
                var flash = root.AddComponent<HitFlashComponent>();
                SetSerialized(control, so => so.FindProperty("bodyRenderer").objectReferenceValue = body);
                SetSerialized(flash, so => so.FindProperty("bodyRenderer").objectReferenceValue = body);
                prefabs.soldier = SavePrefab(root, "Prefab_Soldier");
            }

            // 防御塔
            {
                var root = new GameObject("Prefab_Tower");
                SpriteRenderer body = AddVisual(root, boxSprite, TowerColor, new Vector3(0.8f, 0.8f, 1f), 10);
                root.AddComponent<TowerData>();
                root.AddComponent<TowerControl>();
                root.AddComponent<TowerAttackComponent>();
                var flash = root.AddComponent<HitFlashComponent>();
                SetSerialized(flash, so => so.FindProperty("bodyRenderer").objectReferenceValue = body);
                prefabs.tower = SavePrefab(root, "Prefab_Tower");
            }

            // 大本营
            {
                var root = new GameObject("Prefab_Base");
                SpriteRenderer body = AddVisual(root, boxSprite, BaseColor, new Vector3(1.7f, 1.7f, 1f), 9);
                root.AddComponent<BaseData>();
                var control = root.AddComponent<Game.Combat.BaseControl>();
                var flash = root.AddComponent<HitFlashComponent>();
                SetSerialized(flash, so => so.FindProperty("bodyRenderer").objectReferenceValue = body);
                prefabs.basePrefab = SavePrefab(root, "Prefab_Base");
            }

            // 子弹
            {
                var root = new GameObject("Prefab_Projectile");
                AddVisual(root, circleSprite, ProjectileColor, new Vector3(0.2f, 0.2f, 1f), 20);
                root.AddComponent<Projectile>();
                prefabs.projectile = SavePrefab(root, "Prefab_Projectile");
            }

            return prefabs;
        }

        /// <summary>按表现契约添加 VisualRoot 子节点与主体 SpriteRenderer。</summary>
        static SpriteRenderer AddVisual(GameObject root, Sprite sprite, Color color, Vector3 scale, int sortingOrder)
        {
            var visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(root.transform, false);
            visualRoot.transform.localScale = scale;
            var renderer = visualRoot.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        static GameObject SavePrefab(GameObject sceneInstance, string prefabName)
        {
            string path = $"{PrefabDirectory}/{prefabName}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(sceneInstance, path);
            UnityEngine.Object.DestroyImmediate(sceneInstance);
            return prefab;
        }

        // ---------- 场景 ----------

        static void BuildScene(Sprite boxSprite, Sprite boxLineSprite, Definitions definitions, Prefabs prefabs)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateGlobalLight();

            var managers = new GameObject("GameManagers");
            managers.AddComponent<GameManager>();
            managers.AddComponent<InputManager>();
            PoolManager pool = managers.AddComponent<PoolManager>();

            var battleObject = new GameObject("Battle");
            BattleFlow flow = battleObject.AddComponent<BattleFlow>();
            BattleSetup setup = battleObject.AddComponent<BattleSetup>();

            CreatePathVisual(boxSprite, definitions.level);
            CreateSpawnMarker(boxLineSprite, definitions.level);

            var hud = CreateHud();

            SetSerialized(setup, so =>
            {
                so.FindProperty("levelDefinition").objectReferenceValue = definitions.level;
                so.FindProperty("battleFlow").objectReferenceValue = flow;
                so.FindProperty("battleHUD").objectReferenceValue = hud;
            });

            SetSerialized(pool, so =>
            {
                AddPoolEntry(so.FindProperty("characterPool"), prefabs.soldier, 8);
                AddPoolEntry(so.FindProperty("characterPool"), prefabs.tower, 4);
                AddPoolEntry(so.FindProperty("characterPool"), prefabs.basePrefab, 1);
                AddPoolEntry(so.FindProperty("itemPool"), prefabs.projectile, 16);
            });

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
        }

        static void CreateGlobalLight()
        {
            var lightObject = new GameObject("Global Light 2D");
            var light = lightObject.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
            light.color = Color.white;
            light.intensity = 1f;
        }

        /// <summary>用拉伸方块画出行军路线（低透明度，仅辅助观察）。</summary>
        static void CreatePathVisual(Sprite boxSprite, LevelDefinition level)
        {
            var root = new GameObject("PathVisual");
            IReadOnlyList<Vector3> path = level.SoldierPath;
            for (int i = 0; i + 1 < path.Count; i++)
            {
                Vector3 from = path[i];
                Vector3 to = path[i + 1];
                Vector3 delta = to - from;

                var segment = new GameObject($"Segment_{i}");
                segment.transform.SetParent(root.transform, false);
                segment.transform.position = (from + to) * 0.5f;
                segment.transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, Vector3.forward);
                segment.transform.localScale = new Vector3(delta.magnitude, 0.06f, 1f);

                var renderer = segment.AddComponent<SpriteRenderer>();
                renderer.sprite = boxSprite;
                renderer.color = PathColor;
                renderer.sortingOrder = 1;
            }
        }

        static void CreateSpawnMarker(Sprite boxLineSprite, LevelDefinition level)
        {
            if (level.SoldierPath.Count == 0)
            {
                return;
            }
            var marker = new GameObject("SpawnMarker");
            marker.transform.position = level.SoldierPath[0];
            marker.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            var renderer = marker.AddComponent<SpriteRenderer>();
            renderer.sprite = boxLineSprite;
            renderer.color = new Color(SoldierColor.r, SoldierColor.g, SoldierColor.b, 0.6f);
            renderer.sortingOrder = 2;
        }

        static void AddPoolEntry(SerializedProperty arrayProperty, GameObject prefab, int size)
        {
            int index = arrayProperty.arraySize;
            arrayProperty.InsertArrayElementAtIndex(index);
            SerializedProperty element = arrayProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            element.FindPropertyRelative("size").intValue = size;
        }

        static void UpdateBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (existing.path != ScenePath && !scenes.Exists(scene => scene.path == existing.path))
                {
                    scenes.Add(existing);
                }
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ---------- 通用工具 ----------

        static T CreateOrLoadAsset<T>(string assetName) where T : ScriptableObject
        {
            string path = $"{ContentDirectory}/{assetName}.asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        static void EditAsset(UnityEngine.Object asset, Action<SerializedObject> edit)
        {
            var serialized = new SerializedObject(asset);
            edit(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        static void SetSerialized(Component component, Action<SerializedObject> edit)
        {
            var serialized = new SerializedObject(component);
            edit(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new Exception($"[Builder] 找不到精灵：{path}");
            }
            return sprite;
        }

        static void EnsureDirectory(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }
            string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = System.IO.Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureDirectory(parent);
            }
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
