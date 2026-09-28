using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Day25ForestCombatOrganizer
{
    private const string ForestScenePath = "Assets/Scenes/ForestScene.unity";
    private const string PlayerPrefabPath = "Assets/Game/Player/Prefabs/Player.prefab";
    private const string PigSpritePath = "Assets/ImportedAssets/NinjaAdventure/Characters/pig.png";
    private const string EquipmentFolder = "Assets/Game/Generated/Day26/GauntletsRunes";
    private const string MonsterFolder = "Assets/Game/Generated/Day25/Monsters";
    private static readonly Vector3 SmallMonsterScale = new Vector3(0.8f, 0.8f, 1f);
    private static readonly Vector2 MinimumMonsterColliderSize = new Vector2(0.28f, 0.22f);
    private static readonly Vector3 EliteMonsterScale = new Vector3(7.8f, 7.8f, 1f);

    private readonly struct WeaponDefinition
    {
        public WeaponDefinition(string weaponId, string displayName, EquipmentKind itemType, WeaponQuality quality, int damage, int buyPrice, int sellPrice, string iconFileName)
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

    private readonly struct MonsterDefinition
    {
        public MonsterDefinition(string slug, string displayName, string spriteFileName)
        {
            Slug = slug;
            DisplayName = displayName;
            SpriteFileName = spriteFileName;
        }

        public string Slug { get; }
        public string DisplayName { get; }
        public string SpriteFileName { get; }
        public string SpritePath => $"{MonsterFolder}/{SpriteFileName}";
    }

    private static readonly WeaponDefinition[] SmallMonsterDrops =
    {
        new("common_training_gauntlet", "練習拳套", EquipmentKind.Gauntlet, WeaponQuality.Common, 10, 35, 6, "common_training_gauntlet.png"),
        new("common_leather_gauntlet", "皮革拳套", EquipmentKind.Gauntlet, WeaponQuality.Common, 12, 55, 9, "common_leather_gauntlet.png"),
        new("common_cracked_rune", "裂紋符文", EquipmentKind.Rune, WeaponQuality.Common, 2, 40, 8, "common_cracked_rune.png"),
        new("common_spark_rune", "微光符文", EquipmentKind.Rune, WeaponQuality.Common, 5, 80, 14, "common_spark_rune.png"),
        new("elite_flame_rune", "焰紋符文", EquipmentKind.Rune, WeaponQuality.Elite, 18, 240, 38, "elite_flame_rune.png"),
        new("elite_iron_gauntlet", "精鐵拳套", EquipmentKind.Gauntlet, WeaponQuality.Elite, 30, 260, 42, "elite_iron_gauntlet.png")
    };

    private static readonly WeaponDefinition LegendaryChestRune =
        new("legendary_thunder_rune", "雷痕符文", EquipmentKind.Rune, WeaponQuality.Legendary, 300, 0, 0, "legendary_thunder_rune.png");

    private static readonly MonsterDefinition[] SmallMonsterTypes =
    {
        new("Boar", "森林野豬", "forest_boar.png"),
        new("Slime", "森林史萊姆", "forest_slime.png"),
        new("Bat", "暗翼蝙蝠", "forest_bat.png"),
        new("Mushroom", "毒菇怪", "forest_mushroom.png"),
        new("Rootling", "樹根怪", "forest_rootling.png")
    };

    [MenuItem("IronRPG/Day 25/1 Setup Forest Small Monster Combat And Drops")]
    public static void SetupForestSmallMonsterCombatAndDrops()
    {
        OpenScene(ForestScenePath);
        ConfigureGeneratedWeaponSprites();

        EnsurePlayerCombat();
        EnsureCombatMessageDisplay();
        CreateSmallMonstersFromMarkers();

        SaveSceneAndSelect(GameObject.Find("ForestSmallMonsters"));

        EditorUtility.DisplayDialog(
            "Day 25 Forest Combat",
            "Forest small monster combat and drops are ready.\n\nNormal article flow: keep the existing ForestScene layout, add PlayerAttack and PlayerHealth to the player, then spawn different small monsters on ForestMonsterMarkers. Press J to attack. Small monsters drop gold and sometimes common gauntlets or runes.",
            "OK");
    }

    [MenuItem("IronRPG/Day 25/Open Forest Scene")]
    public static void OpenForestScene()
    {
        OpenScene(ForestScenePath);
    }

    [MenuItem("IronRPG/Day 25/2 Align Existing Monster Colliders To Visuals")]
    public static void AlignExistingMonsterCollidersToVisuals()
    {
        OpenScene(ForestScenePath);
        int alignedCount = AlignMonsterCollidersToVisuals();
        SaveSceneAndSelect(GameObject.Find("ForestSmallMonsters"));

        EditorUtility.DisplayDialog(
            "Day 25 Forest Combat",
            $"Aligned {alignedCount} monster colliders to their Visual child positions and sprite sizes.\n\nNormal article flow: after manually moving each monster Visual in the scene, update the BoxCollider2D Offset and Size so the collider follows the visible monster body.",
            "OK");
    }

    [MenuItem("IronRPG/Day 25/3 Setup Elite Monster And Legendary Chest")]
    public static void SetupEliteMonsterAndLegendaryChest()
    {
        OpenScene(ForestScenePath);
        ConfigureGeneratedWeaponSprites();
        EnsurePlayerCombat();
        EnsureCombatMessageDisplay();
        CreateEliteMonsterAndChest();

        SaveSceneAndSelect(GameObject.Find("ForestEliteEncounter"));

        EditorUtility.DisplayDialog(
            "Day 25 Forest Elite",
            "Forest elite monster, pre-battle dialogue, small-monster clear gate, and legendary chest interaction are ready.\n\nNormal article flow: block the deep forest path until small monsters are cleared, talk to the elite monster before combat starts, make low-level equipment barely hurt it, then unlock the 雷痕符文 chest after the elite monster is defeated.",
            "OK");
    }

    private static void EnsurePlayerCombat()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            player = playerPrefab != null
                ? PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject
                : new GameObject("Player");

            if (player != null)
            {
                Undo.RegisterCreatedObjectUndo(player, "Create Player");
                player.name = "Player";
                player.transform.position = new Vector3(16.5f, 0f, 0f);
                player.transform.localScale = new Vector3(2f, 2f, 1f);
            }
        }

        if (player == null)
        {
            return;
        }

        if (player.GetComponent<PlayerAttack>() == null)
        {
            Undo.AddComponent<PlayerAttack>(player);
        }

        if (player.GetComponent<PlayerHealth>() == null)
        {
            Undo.AddComponent<PlayerHealth>(player);
        }

        PlayerInventory inventory = Object.FindAnyObjectByType<PlayerInventory>();
        if (inventory == null)
        {
            GameObject inventoryObject = new GameObject("PlayerInventory");
            Undo.RegisterCreatedObjectUndo(inventoryObject, "Create PlayerInventory");
            inventory = Undo.AddComponent<PlayerInventory>(inventoryObject);
        }

        ConfigureStartingInventory(inventory);
        EditorUtility.SetDirty(player);
    }

    private static void ConfigureStartingInventory(PlayerInventory inventory)
    {
        SerializedObject serializedInventory = new SerializedObject(inventory);
        serializedInventory.FindProperty("startingGold").intValue = 80;

        SerializedProperty startingWeapons = serializedInventory.FindProperty("startingWeapons");
        startingWeapons.arraySize = 2;
        AssignWeaponStack(startingWeapons.GetArrayElementAtIndex(0), SmallMonsterDrops[0], 1);
        AssignWeaponStack(startingWeapons.GetArrayElementAtIndex(1), SmallMonsterDrops[2], 1);

        serializedInventory.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(inventory);
    }

    private static void EnsureCombatMessageDisplay()
    {
        if (Object.FindAnyObjectByType<CombatMessageDisplay>() != null)
        {
            return;
        }

        GameObject displayObject = new GameObject("CombatMessageDisplay");
        Undo.RegisterCreatedObjectUndo(displayObject, "Create CombatMessageDisplay");
        Undo.AddComponent<CombatMessageDisplay>(displayObject);
    }

    private static void CreateSmallMonstersFromMarkers()
    {
        Transform monsterRoot = GetOrCreateGroup("ForestSmallMonsters");
        ClearChildren(monsterRoot);

        List<Transform> markers = FindSmallMonsterMarkers();
        for (int i = 0; i < markers.Count; i++)
        {
            MonsterDefinition monsterType = SmallMonsterTypes[i % SmallMonsterTypes.Length];
            GameObject monster = new GameObject($"SmallMonster_{i + 1:00}_{monsterType.Slug}");
            Undo.RegisterCreatedObjectUndo(monster, $"Create SmallMonster_{i + 1:00}");
            Undo.SetTransformParent(monster.transform, monsterRoot, "Move SmallMonster");
            monster.transform.position = markers[i].position;
            monster.transform.localScale = SmallMonsterScale;

            GameObject visual = new GameObject("Visual");
            Undo.RegisterCreatedObjectUndo(visual, "Create SmallMonster Visual");
            Undo.SetTransformParent(visual.transform, monster.transform, "Move SmallMonster Visual");
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadMonsterSprite(monsterType);
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
            ConfigureEnemyHealth(health, i, monsterType.DisplayName);

            SimpleEnemyChase chase = monster.AddComponent<SimpleEnemyChase>();
            ConfigureEnemyChase(chase, i);

            monster.AddComponent<EnemyVisualAnimator>();

            EnemyContactDamage contactDamage = monster.AddComponent<EnemyContactDamage>();
            ConfigureContactDamage(contactDamage, i);

            EnemyLootDrop lootDrop = monster.AddComponent<EnemyLootDrop>();
            ConfigureLootDrop(lootDrop, i);
        }
    }

    private static void CreateEliteMonsterAndChest()
    {
        Transform encounterRoot = GetOrCreateGroup("ForestEliteEncounter");
        ClearChildren(encounterRoot);

        Transform eliteSpawn = FindSceneObject("EliteMonsterSpawn");
        Transform chestPoint = FindSceneObject("LegendaryChestPoint");

        Vector3 elitePosition = eliteSpawn != null ? eliteSpawn.position : new Vector3(-26f, 0f, 0f);
        Vector3 chestPosition = chestPoint != null ? chestPoint.position : new Vector3(-30.2f, 0f, 0f);

        GameObject elite = new GameObject("EliteMonster_ForestGuardian");
        Undo.RegisterCreatedObjectUndo(elite, "Create Forest Elite Monster");
        Undo.SetTransformParent(elite.transform, encounterRoot, "Move Forest Elite Monster");
        elite.transform.position = elitePosition;
        elite.transform.localScale = EliteMonsterScale;

        GameObject visual = new GameObject("Visual");
        Undo.RegisterCreatedObjectUndo(visual, "Create EliteMonster Visual");
        Undo.SetTransformParent(visual.transform, elite.transform, "Move EliteMonster Visual");
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadMonsterSprite(SmallMonsterTypes[4]);
        renderer.sortingOrder = 28;
        renderer.color = new Color(0.75f, 0.58f, 1f, 1f);

        Rigidbody2D body = elite.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.linearDamping = 1.45f;

        BoxCollider2D collider = elite.AddComponent<BoxCollider2D>();
        collider.offset = new Vector2(0f, 0.08f);
        collider.size = new Vector2(0.52f, 0.36f);

        EnemyHealth health = elite.AddComponent<EnemyHealth>();
        ConfigureEliteHealth(health);

        SimpleEnemyChase chase = elite.AddComponent<SimpleEnemyChase>();
        ConfigureEliteChase(chase);
        chase.enabled = false;

        elite.AddComponent<EnemyVisualAnimator>();

        EnemyContactDamage contactDamage = elite.AddComponent<EnemyContactDamage>();
        ConfigureEliteContactDamage(contactDamage);
        contactDamage.enabled = false;

        EliteGuardianCombat eliteGuardian = elite.AddComponent<EliteGuardianCombat>();
        ConfigureEliteGuardianCombat(eliteGuardian);

        GameObject dialogueTrigger = new GameObject("EliteGuardianDialogueTrigger");
        Undo.RegisterCreatedObjectUndo(dialogueTrigger, "Create Elite Guardian Dialogue Trigger");
        Undo.SetTransformParent(dialogueTrigger.transform, encounterRoot, "Move Elite Guardian Dialogue Trigger");
        dialogueTrigger.transform.position = elitePosition;
        dialogueTrigger.transform.localScale = Vector3.one;

        BoxCollider2D dialogueCollider = dialogueTrigger.AddComponent<BoxCollider2D>();
        dialogueCollider.isTrigger = true;
        dialogueCollider.offset = new Vector2(4f, 0f);
        dialogueCollider.size = new Vector2(6f, 4f);

        EliteGuardianDialogueInteraction dialogue = dialogueTrigger.AddComponent<EliteGuardianDialogueInteraction>();
        ConfigureEliteDialogue(dialogue, eliteGuardian);

        GameObject clearGate = new GameObject("SmallMonsterClearAirWall");
        Undo.RegisterCreatedObjectUndo(clearGate, "Create Small Monster Clear Air Wall");
        Undo.SetTransformParent(clearGate.transform, encounterRoot, "Move Small Monster Clear Air Wall");
        clearGate.transform.position = elitePosition + new Vector3(4.2f, 0f, 0f);
        clearGate.transform.localScale = Vector3.one;

        BoxCollider2D clearGateCollider = clearGate.AddComponent<BoxCollider2D>();
        clearGateCollider.isTrigger = false;
        clearGateCollider.size = new Vector2(0.55f, 2.6f);

        clearGate.AddComponent<SmallMonsterClearGate>();

        GameObject chestInteraction = new GameObject("LegendaryChestInteraction");
        Undo.RegisterCreatedObjectUndo(chestInteraction, "Create Legendary Chest Interaction");
        Undo.SetTransformParent(chestInteraction.transform, encounterRoot, "Move Legendary Chest Interaction");
        chestInteraction.transform.position = chestPosition;
        chestInteraction.transform.localScale = Vector3.one;

        BoxCollider2D chestTrigger = chestInteraction.AddComponent<BoxCollider2D>();
        chestTrigger.isTrigger = true;
        chestTrigger.size = new Vector2(1.2f, 1.2f);

        LegendaryChestInteraction chest = chestInteraction.AddComponent<LegendaryChestInteraction>();
        ConfigureLegendaryChest(chest);
    }

    private static Transform FindSceneObject(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.transform : null;
    }

    private static int AlignMonsterCollidersToVisuals()
    {
        GameObject monsterRoot = GameObject.Find("ForestSmallMonsters");
        if (monsterRoot == null)
        {
            EditorUtility.DisplayDialog("Day 25 Forest Combat", "Could not find ForestSmallMonsters in ForestScene.", "OK");
            return 0;
        }

        int alignedCount = 0;
        for (int i = 0; i < monsterRoot.transform.childCount; i++)
        {
            Transform monster = monsterRoot.transform.GetChild(i);
            Transform visual = monster.Find("Visual");
            BoxCollider2D collider = monster.GetComponent<BoxCollider2D>();
            if (visual == null || collider == null)
            {
                continue;
            }

            Undo.RecordObject(collider, "Align monster collider to Visual");
            collider.offset = new Vector2(visual.localPosition.x, visual.localPosition.y);
            collider.size = EstimateColliderSizeFromVisual(visual, collider.size);
            EditorUtility.SetDirty(collider);
            alignedCount++;
        }

        return alignedCount;
    }

    private static Vector2 EstimateColliderSizeFromVisual(Transform visual, Vector2 fallbackSize)
    {
        SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
        if (renderer == null || renderer.sprite == null)
        {
            return fallbackSize;
        }

        Bounds spriteBounds = renderer.sprite.bounds;
        Vector3 visualScale = visual.localScale;
        float width = Mathf.Abs(spriteBounds.size.x * visualScale.x) * 0.9f;
        float height = Mathf.Abs(spriteBounds.size.y * visualScale.y) * 0.72f;

        return new Vector2(
            Mathf.Max(width, MinimumMonsterColliderSize.x),
            Mathf.Max(height, MinimumMonsterColliderSize.y));
    }

    private static List<Transform> FindSmallMonsterMarkers()
    {
        List<Transform> markers = new List<Transform>();
        GameObject markerRoot = GameObject.Find("ForestMonsterMarkers");
        if (markerRoot == null)
        {
            Debug.LogWarning("Day25ForestCombatOrganizer could not find ForestMonsterMarkers. Using fallback positions.");
            return CreateFallbackMarkerPositions();
        }

        for (int i = 0; i < markerRoot.transform.childCount; i++)
        {
            Transform child = markerRoot.transform.GetChild(i);
            if (child.name.StartsWith("SmallMonsterSpawn_"))
            {
                markers.Add(child);
            }
        }

        markers.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        if (markers.Count == 0)
        {
            Debug.LogWarning("Day25ForestCombatOrganizer found ForestMonsterMarkers but no SmallMonsterSpawn children. Using fallback positions.");
            return CreateFallbackMarkerPositions();
        }

        return markers;
    }

    private static List<Transform> CreateFallbackMarkerPositions()
    {
        Vector3[] positions =
        {
            new Vector3(15.8f, 1.6f, 0f),
            new Vector3(12.4f, -1.8f, 0f),
            new Vector3(8.4f, 1.6f, 0f),
            new Vector3(4.4f, -1.8f, 0f),
            new Vector3(0.2f, 1.7f, 0f),
            new Vector3(-4.8f, -1.7f, 0f),
            new Vector3(-9.8f, 1.4f, 0f),
            new Vector3(-15.8f, -1.3f, 0f)
        };

        GameObject fallbackRoot = new GameObject("Day25FallbackMonsterMarkers");
        Undo.RegisterCreatedObjectUndo(fallbackRoot, "Create Day25FallbackMonsterMarkers");
        List<Transform> markers = new List<Transform>();
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject marker = new GameObject($"SmallMonsterSpawn_Fallback_{i + 1:00}");
            Undo.RegisterCreatedObjectUndo(marker, "Create fallback monster marker");
            Undo.SetTransformParent(marker.transform, fallbackRoot.transform, "Move fallback marker");
            marker.transform.position = positions[i];
            markers.Add(marker.transform);
        }

        return markers;
    }

    private static void ConfigureEnemyHealth(EnemyHealth health, int index, string enemyName)
    {
        SerializedObject serializedHealth = new SerializedObject(health);
        serializedHealth.FindProperty("enemyName").stringValue = $"{enemyName} {index + 1:00}";
        serializedHealth.FindProperty("maxHealth").intValue = 300;
        serializedHealth.FindProperty("experienceReward").intValue = 10;
        serializedHealth.FindProperty("healthBarOffset").vector2Value = new Vector2(0f, 0.18f);
        serializedHealth.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(health);
    }

    private static void ConfigureEliteHealth(EnemyHealth health)
    {
        SerializedObject serializedHealth = new SerializedObject(health);
        serializedHealth.FindProperty("enemyName").stringValue = "精英怪";
        serializedHealth.FindProperty("maxHealth").intValue = 6000;
        serializedHealth.FindProperty("experienceReward").intValue = 0;
        serializedHealth.FindProperty("healthBarOffset").vector2Value = new Vector2(0f, 0.2f);
        serializedHealth.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(health);
    }

    private static void ConfigureEnemyChase(SimpleEnemyChase chase, int index)
    {
        SerializedObject serializedChase = new SerializedObject(chase);
        serializedChase.FindProperty("moveSpeed").floatValue = index % 4 == 0 ? 1.85f : 1.45f;
        serializedChase.FindProperty("aggroRange").floatValue = 6f;
        serializedChase.FindProperty("stopDistance").floatValue = 0.72f;
        serializedChase.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(chase);
    }

    private static void ConfigureEliteChase(SimpleEnemyChase chase)
    {
        SerializedObject serializedChase = new SerializedObject(chase);
        serializedChase.FindProperty("moveSpeed").floatValue = 1.15f;
        serializedChase.FindProperty("aggroRange").floatValue = 5.8f;
        serializedChase.FindProperty("stopDistance").floatValue = 0.95f;
        serializedChase.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(chase);
    }

    private static void ConfigureContactDamage(EnemyContactDamage contactDamage, int index)
    {
        SerializedObject serializedDamage = new SerializedObject(contactDamage);
        serializedDamage.FindProperty("damage").intValue = index < 10 ? 7 : 10;
        serializedDamage.FindProperty("damageInterval").floatValue = 0.65f;
        serializedDamage.FindProperty("maxAttackDistance").floatValue = 0f;
        serializedDamage.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(contactDamage);
    }

    private static void ConfigureEliteContactDamage(EnemyContactDamage contactDamage)
    {
        SerializedObject serializedDamage = new SerializedObject(contactDamage);
        serializedDamage.FindProperty("damage").intValue = 18;
        serializedDamage.FindProperty("damageInterval").floatValue = 0.55f;
        serializedDamage.FindProperty("maxAttackDistance").floatValue = 6f;
        serializedDamage.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(contactDamage);
    }

    private static void ConfigureEliteGuardianCombat(EliteGuardianCombat combat)
    {
        SerializedObject serializedCombat = new SerializedObject(combat);
        serializedCombat.FindProperty("eliteAggroRange").floatValue = 6f;
        serializedCombat.FindProperty("eliteAttackRange").floatValue = 6f;
        serializedCombat.FindProperty("eliteDamageInterval").floatValue = 0.55f;
        serializedCombat.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(combat);
    }

    private static void ConfigureEliteDialogue(EliteGuardianDialogueInteraction dialogue, EliteGuardianCombat guardian)
    {
        SerializedObject serializedDialogue = new SerializedObject(dialogue);
        serializedDialogue.FindProperty("guardian").objectReferenceValue = guardian;
        serializedDialogue.FindProperty("interactPrompt").stringValue = "按 E 與精英怪對峙";
        serializedDialogue.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(dialogue);
    }

    private static void ConfigureLegendaryChest(LegendaryChestInteraction chest)
    {
        SerializedObject serializedChest = new SerializedObject(chest);
        AssignWeaponItem(serializedChest.FindProperty("legendaryRune"), LegendaryChestRune);
        serializedChest.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(chest);
    }

    private static void ConfigureLootDrop(EnemyLootDrop lootDrop, int index)
    {
        SerializedObject serializedLoot = new SerializedObject(lootDrop);
        serializedLoot.FindProperty("minGold").intValue = 5;
        serializedLoot.FindProperty("maxGold").intValue = 8;

        SerializedProperty weaponDrops = serializedLoot.FindProperty("weaponDrops");
        weaponDrops.arraySize = SmallMonsterDrops.Length;
        for (int i = 0; i < SmallMonsterDrops.Length; i++)
        {
            SerializedProperty drop = weaponDrops.GetArrayElementAtIndex(i);
            AssignWeaponItem(drop.FindPropertyRelative("weapon"), SmallMonsterDrops[i]);
            drop.FindPropertyRelative("chance").floatValue = SmallMonsterDrops[i].Quality == WeaponQuality.Elite ? 0.005f : 0.015f;
        }

        serializedLoot.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(lootDrop);
    }

    private static void AssignWeaponItem(SerializedProperty weaponProperty, WeaponDefinition weapon)
    {
        weaponProperty.FindPropertyRelative("weaponId").stringValue = weapon.WeaponId;
        weaponProperty.FindPropertyRelative("displayName").stringValue = weapon.DisplayName;
        weaponProperty.FindPropertyRelative("itemType").enumValueIndex = (int)weapon.ItemType;
        weaponProperty.FindPropertyRelative("quality").enumValueIndex = (int)weapon.Quality;
        weaponProperty.FindPropertyRelative("icon").objectReferenceValue = LoadGeneratedWeaponSprite(weapon);
        weaponProperty.FindPropertyRelative("damage").intValue = weapon.Damage;
        weaponProperty.FindPropertyRelative("buyPrice").intValue = weapon.BuyPrice;
        weaponProperty.FindPropertyRelative("sellPrice").intValue = weapon.SellPrice;
    }

    private static void AssignWeaponStack(SerializedProperty stackProperty, WeaponDefinition weapon, int count)
    {
        AssignWeaponItem(stackProperty.FindPropertyRelative("weapon"), weapon);
        stackProperty.FindPropertyRelative("count").intValue = count;
    }

    private static Sprite LoadGeneratedWeaponSprite(WeaponDefinition weapon)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(weapon.IconPath);
        if (sprite == null)
        {
            Debug.LogWarning($"Day25ForestCombatOrganizer could not load weapon sprite: {weapon.IconPath}");
        }

        return sprite;
    }

    private static void ConfigureGeneratedWeaponSprites()
    {
        AssetDatabase.Refresh();

        for (int i = 0; i < SmallMonsterDrops.Length; i++)
        {
            ConfigureSpriteImporter(SmallMonsterDrops[i].IconPath);
        }

        ConfigureSpriteImporter(LegendaryChestRune.IconPath);

        for (int i = 0; i < SmallMonsterTypes.Length; i++)
        {
            ConfigureSpriteImporter(SmallMonsterTypes[i].SpritePath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ConfigureSpriteImporter(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"Day25ForestCombatOrganizer could not find texture importer: {assetPath}");
            return;
        }

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.alphaIsTransparency == false)
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

    private static Sprite LoadFirstSprite(string assetPath)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (Object asset in assets)
        {
            if (asset is Sprite sprite)
            {
                return sprite;
            }
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static Sprite LoadMonsterSprite(MonsterDefinition monster)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(monster.SpritePath);
        if (sprite != null)
        {
            return sprite;
        }

        Debug.LogWarning($"Day25ForestCombatOrganizer could not load generated monster sprite: {monster.SpritePath}. Falling back to pig sprite.");
        return LoadFirstSprite(PigSpritePath);
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
        return group.transform;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static void SaveSceneAndSelect(GameObject selectedObject)
    {
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Selection.activeGameObject = selectedObject;
        if (selectedObject != null)
        {
            EditorGUIUtility.PingObject(selectedObject);
        }
    }

    private static void OpenScene(string scenePath)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
        {
            EditorUtility.DisplayDialog("Day 25 Forest Combat", $"Could not find scene at {scenePath}.", "OK");
            return;
        }

        if (SceneManager.GetActiveScene().path == scenePath)
        {
            return;
        }

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }
}
