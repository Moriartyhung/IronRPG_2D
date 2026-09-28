using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Day27CaveMonsterPrefabBuilder
{
    private const string CaveScenePath = "Assets/Scenes/CaveScene.unity";
    private const string MonsterFolder = "Assets/Game/Generated/Day27/Monsters";
    private const string PrefabFolder = MonsterFolder + "/Prefabs";
    private const string EquipmentFolder = "Assets/Game/Generated/Day26/GauntletsRunes";
    private const int CaveMonsterHealth = 1200;
    private const int CaveMonsterExperience = 40;
    private const int CaveMonsterMinGold = 15;
    private const int CaveMonsterMaxGold = 22;
    private const int CaveMonsterGuaranteedHerbs = 1;
    private static readonly Vector3 CaveMonsterScale = new Vector3(3.5f, 3.5f, 1f);

    private readonly struct EquipmentDefinition
    {
        public EquipmentDefinition(string weaponId, string displayName, EquipmentKind itemType, WeaponQuality quality, int damage, int buyPrice, int sellPrice, string iconFileName)
        {
            WeaponId = weaponId;
            DisplayName = displayName;
            ItemType = itemType;
            Quality = quality;
            Damage = damage;
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
            IconFileName = iconFileName;
        }

        public string WeaponId { get; }
        public string DisplayName { get; }
        public EquipmentKind ItemType { get; }
        public WeaponQuality Quality { get; }
        public int Damage { get; }
        public int BuyPrice { get; }
        public int SellPrice { get; }
        public string IconFileName { get; }
        public string IconPath => $"{EquipmentFolder}/{IconFileName}";
    }

    private readonly struct CaveMonsterDefinition
    {
        public CaveMonsterDefinition(string prefabName, string displayName, string spriteFileName, float speed)
        {
            PrefabName = prefabName;
            DisplayName = displayName;
            SpriteFileName = spriteFileName;
            Speed = speed;
        }

        public string PrefabName { get; }
        public string DisplayName { get; }
        public string SpriteFileName { get; }
        public float Speed { get; }
        public string SpritePath => $"{MonsterFolder}/{SpriteFileName}";
        public string PrefabPath => $"{PrefabFolder}/{PrefabName}.prefab";
    }

    private static readonly EquipmentDefinition[] SmallMonsterDrops =
    {
        new("common_training_gauntlet", "練習拳套", EquipmentKind.Gauntlet, WeaponQuality.Common, 10, 35, 6, "common_training_gauntlet.png"),
        new("common_leather_gauntlet", "皮革拳套", EquipmentKind.Gauntlet, WeaponQuality.Common, 12, 55, 9, "common_leather_gauntlet.png"),
        new("common_cracked_rune", "裂紋符文", EquipmentKind.Rune, WeaponQuality.Common, 2, 40, 8, "common_cracked_rune.png"),
        new("common_spark_rune", "微光符文", EquipmentKind.Rune, WeaponQuality.Common, 5, 80, 14, "common_spark_rune.png"),
        new("elite_flame_rune", "焰紋符文", EquipmentKind.Rune, WeaponQuality.Elite, 18, 240, 38, "elite_flame_rune.png"),
        new("elite_iron_gauntlet", "精鐵拳套", EquipmentKind.Gauntlet, WeaponQuality.Elite, 30, 260, 42, "elite_iron_gauntlet.png")
    };

    private static readonly CaveMonsterDefinition[] CaveMonsters =
    {
        new("CaveCrystalSlime", "洞穴晶史萊姆", "cave_crystal_slime.png", 1.35f),
        new("CaveStoneBeetle", "岩甲蟲", "cave_stone_beetle.png", 1.55f),
        new("CaveRootImp", "根骨小鬼", "cave_root_imp.png", 1.45f)
    };

    private static readonly Vector3[] CaveMonsterPositions =
    {
        new(-19.8f, -6.9f, 0f),
        new(-17.4f, -4.8f, 0f),
        new(-14.8f, -7.2f, 0f),
        new(-12.2f, -5.0f, 0f),
        new(-10.3f, -8.6f, 0f),
        new(-8.0f, -6.2f, 0f),
        new(-5.8f, -9.4f, 0f),
        new(-3.4f, -6.8f, 0f),
        new(-1.2f, -10.1f, 0f),
        new(1.3f, -7.5f, 0f),
        new(3.4f, -4.7f, 0f),
        new(5.7f, -7.9f, 0f),
        new(8.1f, -5.4f, 0f),
        new(10.2f, -8.8f, 0f),
        new(12.6f, -6.2f, 0f),
        new(15.0f, -8.1f, 0f),
        new(17.4f, -5.3f, 0f),
        new(19.7f, -7.5f, 0f),
        new(20.4f, -2.8f, 0f),
        new(17.8f, 0.3f, 0f),
        new(14.8f, 2.5f, 0f),
        new(11.7f, 0.9f, 0f),
        new(8.5f, 3.1f, 0f),
        new(5.5f, 0.5f, 0f),
        new(2.3f, 2.4f, 0f),
        new(-0.8f, 0.1f, 0f),
        new(-4.0f, 2.7f, 0f),
        new(-7.1f, 0.6f, 0f),
        new(-10.0f, 3.4f, 0f),
        new(-13.2f, 1.1f, 0f)
    };

    [MenuItem("IronRPG/Day 27/Create Cave Small Monster Prefabs Only")]
    public static void CreateCaveSmallMonsterPrefabsOnly()
    {
        CreatePrefabAssets();

        EditorUtility.DisplayDialog(
            "Day 27 Cave Monsters",
            "已建立 3 種洞穴小怪 prefab。\n\n本工具只產生 prefab，不會放置任何怪物到洞穴場景。後續要鋪 30 隻小怪時，再從這三種 prefab 分散放置。",
            "OK");
    }

    [MenuItem("IronRPG/Day 27/Place 30 Cave Small Monsters")]
    public static void PlaceThirtyCaveSmallMonsters()
    {
        CreatePrefabAssets();
        OpenScene(CaveScenePath);

        Transform root = GetOrCreateGroup("CaveSmallMonsters");
        ClearChildren(root);

        for (int i = 0; i < CaveMonsterPositions.Length; i++)
        {
            CaveMonsterDefinition monsterType = CaveMonsters[i % CaveMonsters.Length];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(monsterType.PrefabPath);
            GameObject monster = prefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(prefab)
                : new GameObject(monsterType.PrefabName);

            Undo.RegisterCreatedObjectUndo(monster, $"Create CaveMonster_{i + 1:00}");
            monster.name = $"CaveMonster_{i + 1:00}_{monsterType.PrefabName}";
            Undo.SetTransformParent(monster.transform, root, "Move Cave Monster");
            monster.transform.position = CaveMonsterPositions[i];
            monster.transform.localRotation = Quaternion.identity;
            monster.transform.localScale = CaveMonsterScale;

            EnemyHealth health = monster.GetComponent<EnemyHealth>();
            if (health != null)
            {
                ConfigureEnemyHealth(health, monsterType.DisplayName, i);
            }
        }

        Selection.activeObject = root.gameObject;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Day 27 Cave Monsters",
            "已在 CaveScene 放置 30 隻洞穴小怪。\n\n比例依照森林主角 1.2 對小怪 7 的視覺比值換算：洞穴主角 0.6 時，小怪使用 3.5。洞穴小怪血量 1200、經驗 40、金幣 15-22、必掉 1 株藥草。工具只重建 CaveSmallMonsters，不會修改背景或碰撞箱。",
            "OK");
    }

    [MenuItem("IronRPG/Day 27/Update Cave Small Monster Stats Only")]
    public static void UpdateCaveSmallMonsterStatsOnly()
    {
        CreatePrefabAssets();
        OpenScene(CaveScenePath);

        GameObject root = GameObject.Find("CaveSmallMonsters");
        if (root == null)
        {
            EditorUtility.DisplayDialog(
                "Day 27 Cave Monsters",
                "找不到 CaveSmallMonsters。請先放置洞穴小怪。",
                "OK");
            return;
        }

        int updatedCount = 0;
        for (int i = 0; i < root.transform.childCount; i++)
        {
            Transform monster = root.transform.GetChild(i);
            int monsterTypeIndex = GetMonsterTypeIndex(monster.gameObject.name, i);
            CaveMonsterDefinition monsterType = CaveMonsters[monsterTypeIndex];
            UpdateExistingMonsterStats(monster.gameObject, monsterType.DisplayName, i, monsterTypeIndex);
            updatedCount++;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Day 27 Cave Monsters",
            $"已更新 {updatedCount} 隻洞穴小怪數值。\n\n本工具只更新數值，不會改怪物位置、比例、背景或碰撞箱。",
            "OK");
    }

    private static void CreatePrefabAssets()
    {
        Directory.CreateDirectory(ToFullPath(PrefabFolder));
        AssetDatabase.Refresh();

        for (int i = 0; i < CaveMonsters.Length; i++)
        {
            ConfigureSpriteImporter(CaveMonsters[i].SpritePath);
        }

        for (int i = 0; i < SmallMonsterDrops.Length; i++)
        {
            ConfigureSpriteImporter(SmallMonsterDrops[i].IconPath);
        }

        for (int i = 0; i < CaveMonsters.Length; i++)
        {
            CreateMonsterPrefab(CaveMonsters[i], i);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateMonsterPrefab(CaveMonsterDefinition monsterType, int index)
    {
        GameObject monster = new GameObject(monsterType.PrefabName);
        monster.transform.localScale = CaveMonsterScale;

        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(monster.transform, false);

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(monsterType.SpritePath);
        renderer.sortingOrder = 24;
        renderer.color = Color.white;

        Rigidbody2D body = monster.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.linearDamping = 1.25f;

        BoxCollider2D collider = monster.AddComponent<BoxCollider2D>();
        collider.offset = new Vector2(0f, 0.05f);
        collider.size = new Vector2(0.42f, 0.28f);

        EnemyHealth health = monster.AddComponent<EnemyHealth>();
        ConfigureEnemyHealth(health, monsterType.DisplayName, index);

        SimpleEnemyChase chase = monster.AddComponent<SimpleEnemyChase>();
        ConfigureEnemyChase(chase, monsterType.Speed);

        monster.AddComponent<EnemyVisualAnimator>();

        EnemyContactDamage contactDamage = monster.AddComponent<EnemyContactDamage>();
        ConfigureContactDamage(contactDamage, index);

        EnemyLootDrop lootDrop = monster.AddComponent<EnemyLootDrop>();
        ConfigureLootDrop(lootDrop);

        PrefabUtility.SaveAsPrefabAsset(monster, monsterType.PrefabPath);
        Object.DestroyImmediate(monster);
    }

    private static void ConfigureEnemyHealth(EnemyHealth health, string enemyName, int index)
    {
        SerializedObject serializedHealth = new SerializedObject(health);
        serializedHealth.FindProperty("enemyName").stringValue = $"{enemyName} {index + 1:00}";
        serializedHealth.FindProperty("maxHealth").intValue = CaveMonsterHealth;
        serializedHealth.FindProperty("experienceReward").intValue = CaveMonsterExperience;
        serializedHealth.FindProperty("healthBarOffset").vector2Value = new Vector2(0f, 0.18f);
        serializedHealth.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(health);
    }

    private static void ConfigureEnemyChase(SimpleEnemyChase chase, float moveSpeed)
    {
        SerializedObject serializedChase = new SerializedObject(chase);
        serializedChase.FindProperty("moveSpeed").floatValue = moveSpeed;
        serializedChase.FindProperty("aggroRange").floatValue = 6f;
        serializedChase.FindProperty("stopDistance").floatValue = 0.72f;
        serializedChase.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(chase);
    }

    private static void ConfigureContactDamage(EnemyContactDamage contactDamage, int index)
    {
        SerializedObject serializedDamage = new SerializedObject(contactDamage);
        serializedDamage.FindProperty("damage").intValue = index == 1 ? 40 : 28;
        serializedDamage.FindProperty("damageInterval").floatValue = 0.65f;
        serializedDamage.FindProperty("maxAttackDistance").floatValue = 0f;
        serializedDamage.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(contactDamage);
    }

    private static void ConfigureLootDrop(EnemyLootDrop lootDrop)
    {
        SerializedObject serializedLoot = new SerializedObject(lootDrop);
        serializedLoot.FindProperty("minGold").intValue = CaveMonsterMinGold;
        serializedLoot.FindProperty("maxGold").intValue = CaveMonsterMaxGold;
        serializedLoot.FindProperty("minHerbs").intValue = CaveMonsterGuaranteedHerbs;
        serializedLoot.FindProperty("maxHerbs").intValue = CaveMonsterGuaranteedHerbs;

        SerializedProperty weaponDrops = serializedLoot.FindProperty("weaponDrops");
        weaponDrops.arraySize = SmallMonsterDrops.Length;
        for (int i = 0; i < SmallMonsterDrops.Length; i++)
        {
            SerializedProperty drop = weaponDrops.GetArrayElementAtIndex(i);
            AssignEquipmentItem(drop.FindPropertyRelative("weapon"), SmallMonsterDrops[i]);
            drop.FindPropertyRelative("chance").floatValue = SmallMonsterDrops[i].Quality == WeaponQuality.Elite ? 0.005f : 0.015f;
        }

        serializedLoot.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(lootDrop);
    }

    private static void UpdateExistingMonsterStats(GameObject monster, string enemyName, int index, int monsterTypeIndex)
    {
        EnemyHealth health = monster.GetComponent<EnemyHealth>();
        if (health != null)
        {
            ConfigureEnemyHealth(health, enemyName, index);
        }

        EnemyContactDamage contactDamage = monster.GetComponent<EnemyContactDamage>();
        if (contactDamage != null)
        {
            ConfigureContactDamage(contactDamage, monsterTypeIndex);
        }

        EnemyLootDrop lootDrop = monster.GetComponent<EnemyLootDrop>();
        if (lootDrop != null)
        {
            ConfigureLootDrop(lootDrop);
        }

        SimpleEnemyChase chase = monster.GetComponent<SimpleEnemyChase>();
        if (chase != null)
        {
            ConfigureEnemyChase(chase, CaveMonsters[monsterTypeIndex].Speed);
        }
    }

    private static int GetMonsterTypeIndex(string monsterName, int fallbackIndex)
    {
        for (int i = 0; i < CaveMonsters.Length; i++)
        {
            if (monsterName.Contains(CaveMonsters[i].PrefabName))
            {
                return i;
            }
        }

        return fallbackIndex % CaveMonsters.Length;
    }

    private static void AssignEquipmentItem(SerializedProperty weaponProperty, EquipmentDefinition equipment)
    {
        weaponProperty.FindPropertyRelative("weaponId").stringValue = equipment.WeaponId;
        weaponProperty.FindPropertyRelative("displayName").stringValue = equipment.DisplayName;
        weaponProperty.FindPropertyRelative("itemType").enumValueIndex = (int)equipment.ItemType;
        weaponProperty.FindPropertyRelative("quality").enumValueIndex = (int)equipment.Quality;
        weaponProperty.FindPropertyRelative("icon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(equipment.IconPath);
        weaponProperty.FindPropertyRelative("damage").intValue = equipment.Damage;
        weaponProperty.FindPropertyRelative("buyPrice").intValue = equipment.BuyPrice;
        weaponProperty.FindPropertyRelative("sellPrice").intValue = equipment.SellPrice;
    }

    private static void ConfigureSpriteImporter(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"Day27CaveMonsterPrefabBuilder could not find texture importer: {assetPath}");
            return;
        }

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            changed = true;
        }

        if (importer.filterMode != FilterMode.Point)
        {
            importer.filterMode = FilterMode.Point;
            changed = true;
        }

        if (Mathf.Abs(importer.spritePixelsPerUnit - 100f) > 0.01f)
        {
            importer.spritePixelsPerUnit = 100f;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

    private static string ToFullPath(string assetPath)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), assetPath);
    }

    private static void OpenScene(string scenePath)
    {
        if (SceneManager.GetActiveScene().path == scenePath)
        {
            return;
        }

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }

    private static Transform GetOrCreateGroup(string groupName)
    {
        GameObject existing = GameObject.Find(groupName);
        if (existing != null)
        {
            return existing.transform;
        }

        GameObject group = new GameObject(groupName);
        Undo.RegisterCreatedObjectUndo(group, $"Create {groupName}");

        GameObject sceneRoot = GameObject.Find("CaveSceneRoot");
        if (sceneRoot != null)
        {
            Undo.SetTransformParent(group.transform, sceneRoot.transform, $"Move {groupName}");
        }

        group.transform.localPosition = Vector3.zero;
        group.transform.localRotation = Quaternion.identity;
        group.transform.localScale = Vector3.one;
        return group.transform;
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(root.GetChild(i).gameObject);
        }
    }
}
