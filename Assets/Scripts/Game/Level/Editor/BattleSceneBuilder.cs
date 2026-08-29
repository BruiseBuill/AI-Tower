using System;
using System.Collections.Generic;
using BF;
using Game.Base;
using Game.Combat;
using Game.Content;
using Game.Core;
using Game.Input;
using Game.Projectile;
using Game.Soldier;
using Game.Tower;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

using ProjectileBehaviour = Game.Projectile.Projectile;
using SoldierConfig = Game.Content.SoldierData;
using TowerConfig = Game.Content.TowerData;

namespace Game.Level.Editor
{
    /// <summary>
    /// 垂直切片一键构建器：生成定义资产、运行时预制体、战斗场景与构建设置。
    /// 幂等：重复执行会复用已有资产；已有 LevelDefinition 的布阵数据由关卡编辑器维护，
    /// 不会被本工具重置。
    /// 入口：菜单 “AIOnly/构建垂直切片场景”，或批处理
    ///   -executeMethod Game.Level.Editor.BattleSceneBuilder.BuildAll
    /// UI 构建部分见 BattleSceneBuilderUI.cs。
    /// </summary>
    public static partial class BattleSceneBuilder
    {
        const string ContentDirectory = "Assets/Game/Content/Definitions";
        const string LevelDirectory = ContentDirectory + "/Levels";
        const string PrefabDirectory = "Assets/Game/Prefabs";
        const string ScenePath = "Assets/Scenes/Level001.unity";

        const string BoxSpritePath = "Assets/Art/PolySprite/Poly/Box.png";
        const string CircleSpritePath = "Assets/Art/PolySprite/Poly/Circle.png";

        static readonly Color BackgroundColor = new Color32(0x16, 0x16, 0x1E, 0xFF);
        static readonly Color SoldierColor = new Color32(0x7F, 0xD4, 0xFF, 0xFF);
        static readonly Color HeavySoldierColor = new Color32(0x3E, 0x8E, 0xE0, 0xFF);
        static readonly Color EliteSoldierColor = new Color32(0xFF, 0x7A, 0x66, 0xFF);
        static readonly Color BasicTierColor = new Color32(0x3F, 0xA7, 0xFF, 0xFF);
        static readonly Color HeavyTierColor = new Color32(0xFF, 0xC8, 0x4A, 0xFF);
        static readonly Color EliteTierColor = new Color32(0xFF, 0x6B, 0x6B, 0xFF);
        static readonly Color TowerColor = new Color32(0x7A, 0x1F, 0x1F, 0xFF);
        static readonly Color BaseColor = new Color32(0x4A, 0x10, 0x10, 0xFF);
        static readonly Color ProjectileColor = new Color32(0xFF, 0xE0, 0x8A, 0xFF);

        class Definitions
        {
            public SoldierConfig soldierBasic;
            public SoldierConfig soldierHeavy;
            public SoldierConfig soldierElite;
            public ProjectileDefinition projectile;
            public TowerConfig tower;
            public BaseDefinition baseDef;
            public LevelDefinition level;
        }

        class Prefabs
        {
            public GameObject soldierBasic;
            public GameObject soldierHeavy;
            public GameObject soldierElite;
            public GameObject towerBasic;
            public GameObject basePrefab;
            public GameObject projectile;
        }

        [MenuItem("AIOnly/构建垂直切片场景")]
        public static void BuildAll()
        {
            EnsureDirectory(ContentDirectory);
            EnsureDirectory(LevelDirectory);
            EnsureDirectory(PrefabDirectory);

            Sprite boxSprite = LoadSprite(BoxSpritePath);
            Sprite circleSprite = LoadSprite(CircleSpritePath);

            Prefabs prefabs = CreatePrefabs(boxSprite, circleSprite);
            Definitions definitions = CreateDefinitions(prefabs);
            BuildScene(definitions, prefabs, definitions.level.ScenePath);
            UpdateBuildSettings(definitions.level.ScenePath);

            AssetDatabase.SaveAssets();
            Debug.Log($"[Builder] 垂直切片构建完成。入口场景：{definitions.level.ScenePath}");
        }

        // ---------- 定义资产 ----------

        static Definitions CreateDefinitions(Prefabs prefabs)
        {
            var definitions = new Definitions();

            definitions.soldierBasic = CreateOrLoadAsset<SoldierConfig>("Def_Soldier_Basic_Data");
            EditAsset(definitions.soldierBasic, so =>
            {
                so.FindProperty("contentId").stringValue = "soldier_basic";
                so.FindProperty("displayName").stringValue = "突击兵";
                so.FindProperty("energyCost").floatValue = 50f;
                so.FindProperty("maxHealth").floatValue = 120f;
                so.FindProperty("moveSpeed").floatValue = 1.6f;
                so.FindProperty("attackRange").floatValue = 0.9f;
                so.FindProperty("towerDetectionRange").floatValue = 2.5f;
                so.FindProperty("attackDamage").floatValue = 20f;
                so.FindProperty("attackInterval").floatValue = 1f;
                so.FindProperty("tierColor").colorValue = BasicTierColor;
                so.FindProperty("tint").colorValue = new Color(0f, 0f, 0f, 0f);
                so.FindProperty("prefab").objectReferenceValue = prefabs.soldierBasic;
            });

            definitions.soldierHeavy = CreateOrLoadAsset<SoldierConfig>("Def_Soldier_Heavy_Data");
            EditAsset(definitions.soldierHeavy, so =>
            {
                so.FindProperty("contentId").stringValue = "soldier_heavy";
                so.FindProperty("displayName").stringValue = "重装兵";
                so.FindProperty("energyCost").floatValue = 120f;
                so.FindProperty("maxHealth").floatValue = 360f;
                so.FindProperty("moveSpeed").floatValue = 1.1f;
                so.FindProperty("attackRange").floatValue = 1.0f;
                so.FindProperty("towerDetectionRange").floatValue = 2.5f;
                so.FindProperty("attackDamage").floatValue = 50f;
                so.FindProperty("attackInterval").floatValue = 1.3f;
                so.FindProperty("tierColor").colorValue = HeavyTierColor;
                so.FindProperty("tint").colorValue = HeavySoldierColor;
                so.FindProperty("prefab").objectReferenceValue = prefabs.soldierHeavy;
            });

            definitions.soldierElite = CreateOrLoadAsset<SoldierConfig>("Def_Soldier_Elite_Data");
            EditAsset(definitions.soldierElite, so =>
            {
                so.FindProperty("contentId").stringValue = "soldier_elite";
                so.FindProperty("displayName").stringValue = "精英兵";
                so.FindProperty("energyCost").floatValue = 180f;
                so.FindProperty("maxHealth").floatValue = 560f;
                so.FindProperty("moveSpeed").floatValue = 1.0f;
                so.FindProperty("attackRange").floatValue = 1.1f;
                so.FindProperty("towerDetectionRange").floatValue = 2.5f;
                so.FindProperty("attackDamage").floatValue = 85f;
                so.FindProperty("attackInterval").floatValue = 1.5f;
                so.FindProperty("tierColor").colorValue = EliteTierColor;
                so.FindProperty("tint").colorValue = EliteSoldierColor;
                so.FindProperty("prefab").objectReferenceValue = prefabs.soldierElite;
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

            definitions.tower = CreateOrLoadAsset<TowerConfig>("Def_Tower_Basic_Data");
            EditAsset(definitions.tower, so =>
            {
                so.FindProperty("contentId").stringValue = "tower_basic";
                so.FindProperty("displayName").stringValue = "防御塔";
                so.FindProperty("maxHealth").floatValue = 150f;
                so.FindProperty("attackRange").floatValue = 2.6f;
                so.FindProperty("attackInterval").floatValue = 1.4f;
                so.FindProperty("attackDamage").floatValue = 10f;
                so.FindProperty("projectile").objectReferenceValue = definitions.projectile;
                so.FindProperty("prefab").objectReferenceValue = prefabs.towerBasic;
            });

            definitions.baseDef = CreateOrLoadAsset<BaseDefinition>("Def_Base_Basic");
            EditAsset(definitions.baseDef, so =>
            {
                so.FindProperty("contentId").stringValue = "base_basic";
                so.FindProperty("displayName").stringValue = "敌方大本营";
                so.FindProperty("maxHealth").floatValue = 400f;
                so.FindProperty("prefab").objectReferenceValue = prefabs.basePrefab;
            });

            bool isNewLevel;
            definitions.level = CreateOrLoadLevelAsset("Def_Level_001", out isNewLevel);
            if (isNewLevel)
            {
                EditAsset(definitions.level, so =>
                {
                    so.FindProperty("contentId").stringValue = "level_001";
                    so.FindProperty("displayName").stringValue = "第 1 关：突破演练";
                    so.FindProperty("scenePath").stringValue = ScenePath;
                    so.FindProperty("timeLimitSeconds").floatValue = 99f;
                    so.FindProperty("energyStart").floatValue = 100f;
                    so.FindProperty("energyMax").floatValue = 200f;
                    so.FindProperty("energyRegenPerSecond").floatValue = 8f;

                    SerializedProperty soldiers = so.FindProperty("deployableSoldiers");
                    soldiers.arraySize = 3;
                    soldiers.GetArrayElementAtIndex(0).objectReferenceValue = definitions.soldierBasic;
                    soldiers.GetArrayElementAtIndex(1).objectReferenceValue = definitions.soldierHeavy;
                    soldiers.GetArrayElementAtIndex(2).objectReferenceValue = definitions.soldierElite;

                    so.FindProperty("baseDefinition").objectReferenceValue = definitions.baseDef;
                    so.FindProperty("basePosition").vector3Value = new Vector3(7.2f, 0f, 0f);

                    SerializedProperty towers = so.FindProperty("towers");
                    towers.arraySize = 3;
                    SetTowerPlacement(towers.GetArrayElementAtIndex(0), definitions.tower, new Vector3(-1.2f, 1.4f, 0f));
                    // 第二座塔贴近行军路线，士兵会在其攻击距离内停下并反击
                    SetTowerPlacement(towers.GetArrayElementAtIndex(1), definitions.tower, new Vector3(1.8f, -0.7f, 0f));
                    SetTowerPlacement(towers.GetArrayElementAtIndex(2), definitions.tower, new Vector3(4.6f, 1.2f, 0f));

                });
            }
            else
            {
                // 只补齐入口字段并迁移本工具曾创建的旧数据引用，不触碰关卡编辑器维护的布阵。
                MigrateLevelDefinitionReferences(definitions.level, definitions);
            }

            return definitions;
        }

        static void SetTowerPlacement(SerializedProperty element, TowerConfig definition, Vector3 position)
        {
            element.FindPropertyRelative("definition").objectReferenceValue = definition;
            element.FindPropertyRelative("position").vector3Value = position;
        }

        static void MigrateLevelDefinitionReferences(LevelDefinition level, Definitions definitions)
        {
            SerializedObject serialized = new SerializedObject(level);
            bool changed = false;

            SerializedProperty scenePath = serialized.FindProperty("scenePath");
            if (string.IsNullOrEmpty(scenePath.stringValue))
            {
                scenePath.stringValue = ScenePath;
                changed = true;
            }

            SerializedProperty soldiers = serialized.FindProperty("deployableSoldiers");
            bool hasElite = false;
            for (int i = 0; i < soldiers.arraySize; i++)
            {
                SerializedProperty element = soldiers.GetArrayElementAtIndex(i);
                SoldierConfig current = element.objectReferenceValue as SoldierConfig;
                if (current == null)
                {
                    continue;
                }

                if (current.ContentId == definitions.soldierBasic.ContentId)
                {
                    element.objectReferenceValue = definitions.soldierBasic;
                    changed = true;
                }
                else if (current.ContentId == definitions.soldierHeavy.ContentId)
                {
                    element.objectReferenceValue = definitions.soldierHeavy;
                    changed = true;
                }
                else if (current.ContentId == definitions.soldierElite.ContentId)
                {
                    element.objectReferenceValue = definitions.soldierElite;
                    hasElite = true;
                    changed = true;
                }
            }

            if (!hasElite)
            {
                int index = soldiers.arraySize;
                soldiers.InsertArrayElementAtIndex(index);
                soldiers.GetArrayElementAtIndex(index).objectReferenceValue = definitions.soldierElite;
                changed = true;
            }

            SerializedProperty towers = serialized.FindProperty("towers");
            for (int i = 0; i < towers.arraySize; i++)
            {
                SerializedProperty definition = towers.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("definition");
                TowerConfig current = definition.objectReferenceValue as TowerConfig;
                if (current != null && current.ContentId == definitions.tower.ContentId)
                {
                    definition.objectReferenceValue = definitions.tower;
                    changed = true;
                }
            }

            if (changed)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(level);
            }
        }

        // ---------- 预制体 ----------

        static Prefabs CreatePrefabs(Sprite boxSprite, Sprite circleSprite)
        {
            var prefabs = new Prefabs();

            // 基础突击兵
            {
                var root = new GameObject("Prefab_Soldier_Basic");
                SpriteRenderer body = AddVisual(root, boxSprite, SoldierColor, new Vector3(0.55f, 0.55f, 1f), 10);
                root.AddComponent<SoldierRuntimeData>();
                var control = root.AddComponent<BasicSoldier>();
                root.AddComponent<SoldierMovementComponent>();
                var attack = root.AddComponent<SoldierMeleeAttackComponent>();
                var flash = root.AddComponent<HitFlashComponent>();
                SetSerialized(attack, so => so.FindProperty("priority").intValue = -10);
                SetSerialized(control, so => so.FindProperty("bodyRenderer").objectReferenceValue = body);
                SetSerialized(flash, so => so.FindProperty("bodyRenderer").objectReferenceValue = body);
                prefabs.soldierBasic = SavePrefab(root, "Prefab_Soldier_Basic");
            }

            // 重装兵
            {
                var root = new GameObject("Prefab_Soldier_Heavy");
                SpriteRenderer body = AddVisual(root, boxSprite, HeavySoldierColor, new Vector3(0.68f, 0.68f, 1f), 10);
                root.AddComponent<SoldierRuntimeData>();
                var control = root.AddComponent<HeavySoldier>();
                root.AddComponent<SoldierMovementComponent>();
                var attack = root.AddComponent<SoldierMeleeAttackComponent>();
                var flash = root.AddComponent<HitFlashComponent>();
                SetSerialized(attack, so => so.FindProperty("priority").intValue = -10);
                SetSerialized(control, so => so.FindProperty("bodyRenderer").objectReferenceValue = body);
                SetSerialized(flash, so => so.FindProperty("bodyRenderer").objectReferenceValue = body);
                prefabs.soldierHeavy = SavePrefab(root, "Prefab_Soldier_Heavy");
            }

            // 精英兵
            {
                var root = new GameObject("Prefab_Soldier_Elite");
                SpriteRenderer body = AddVisual(root, boxSprite, EliteSoldierColor, new Vector3(0.78f, 0.78f, 1f), 10);
                root.AddComponent<SoldierRuntimeData>();
                var control = root.AddComponent<EliteSoldier>();
                root.AddComponent<SoldierMovementComponent>();
                var attack = root.AddComponent<SoldierMeleeAttackComponent>();
                var flash = root.AddComponent<HitFlashComponent>();
                SetSerialized(attack, so => so.FindProperty("priority").intValue = -10);
                SetSerialized(control, so => so.FindProperty("bodyRenderer").objectReferenceValue = body);
                SetSerialized(flash, so => so.FindProperty("bodyRenderer").objectReferenceValue = body);
                prefabs.soldierElite = SavePrefab(root, "Prefab_Soldier_Elite");
            }

            // 基础防御塔
            {
                var root = new GameObject("Prefab_Tower_Basic");
                SpriteRenderer body = AddVisual(root, boxSprite, TowerColor, new Vector3(0.8f, 0.8f, 1f), 10);
                root.AddComponent<TowerRuntimeData>();
                root.AddComponent<BasicTower>();
                var targeting = root.AddComponent<TowerTargetingComponent>();
                var attack = root.AddComponent<TowerProjectileAttackComponent>();
                var flash = root.AddComponent<HitFlashComponent>();
                SetSerialized(targeting, so => so.FindProperty("priority").intValue = -10);
                SetSerialized(attack, so => so.FindProperty("priority").intValue = 0);
                SetSerialized(flash, so => so.FindProperty("bodyRenderer").objectReferenceValue = body);
                prefabs.towerBasic = SavePrefab(root, "Prefab_Tower_Basic");
            }

            // 大本营
            {
                var root = new GameObject("Prefab_Base");
                SpriteRenderer body = AddVisual(root, boxSprite, BaseColor, new Vector3(1.7f, 1.7f, 1f), 9);
                root.AddComponent<BaseData>();
                var control = root.AddComponent<Game.Base.BaseControl>();
                var flash = root.AddComponent<HitFlashComponent>();
                SetSerialized(flash, so => so.FindProperty("bodyRenderer").objectReferenceValue = body);
                prefabs.basePrefab = SavePrefab(root, "Prefab_Base");
            }

            // 子弹
            {
                var root = new GameObject("Prefab_Projectile");
                AddVisual(root, circleSprite, ProjectileColor, new Vector3(0.2f, 0.2f, 1f), 20);
                root.AddComponent<ProjectileBehaviour>();
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

        static void BuildScene(Definitions definitions, Prefabs prefabs, string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateGlobalLight();

            var managers = new GameObject("GameManagers");
            managers.AddComponent<GameManager>();
            managers.AddComponent<BattleInputManager>();
            PoolManager pool = managers.AddComponent<PoolManager>();

            var battleObject = new GameObject("Battle");
            BattleFlow flow = battleObject.AddComponent<BattleFlow>();
            BattleSetup setup = battleObject.AddComponent<BattleSetup>();

            var hud = CreateHud();

            SetSerialized(setup, so =>
            {
                so.FindProperty("levelDefinition").objectReferenceValue = definitions.level;
                so.FindProperty("battleFlow").objectReferenceValue = flow;
                so.FindProperty("battleHUD").objectReferenceValue = hud;
            });

            SetSerialized(pool, so =>
            {
                AddPoolEntry(so.FindProperty("characterPool"), prefabs.soldierBasic, 8);
                AddPoolEntry(so.FindProperty("characterPool"), prefabs.soldierHeavy, 8);
                AddPoolEntry(so.FindProperty("characterPool"), prefabs.soldierElite, 8);
                AddPoolEntry(so.FindProperty("characterPool"), prefabs.towerBasic, 4);
                AddPoolEntry(so.FindProperty("characterPool"), prefabs.basePrefab, 1);
                AddPoolEntry(so.FindProperty("itemPool"), prefabs.projectile, 16);
            });

            EditorSceneManager.SaveScene(scene, string.IsNullOrEmpty(scenePath) ? ScenePath : scenePath);
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

        static void AddPoolEntry(SerializedProperty arrayProperty, GameObject prefab, int size)
        {
            int index = arrayProperty.arraySize;
            arrayProperty.InsertArrayElementAtIndex(index);
            SerializedProperty element = arrayProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            element.FindPropertyRelative("size").intValue = size;
        }

        static void UpdateBuildSettings(string scenePath)
        {
            scenePath = string.IsNullOrEmpty(scenePath) ? ScenePath : scenePath;
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (existing.path != scenePath && !scenes.Exists(scene => scene.path == existing.path))
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

        static LevelDefinition CreateOrLoadLevelAsset(string assetName, out bool isNew)
        {
            string path = $"{LevelDirectory}/{assetName}.asset";
            LevelDefinition asset = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
            isNew = asset == null;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<LevelDefinition>();
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
