using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class Day24WeaponShopOrganizer
{
    private const string VillageSceneName = "VillageScene";
    private const string VillageScenePath = "Assets/Scenes/VillageScene.unity";
    private const string WeaponShopSceneName = "WeaponShopInteriorScene";
    private const string WeaponShopScenePath = "Assets/Scenes/WeaponShopInteriorScene.unity";
    private const string PlayerPrefabPath = "Assets/Game/Player/Prefabs/Player.prefab";
    private const string FloorTilePath = "Assets/ImportedAssets/NinjaAdventure/myown_test/test/tileset_floor_0.asset";
    private const string FloorAltTilePath = "Assets/ImportedAssets/NinjaAdventure/myown_test/test/tileset_floor_1.asset";
    private const string WallTilePath = "Assets/ImportedAssets/Anokolisa_TopdownRPG/VillageTiles/WallTiles_16x16/Wall_Tiles_0.asset";
    private const string WallAltTilePath = "Assets/ImportedAssets/Anokolisa_TopdownRPG/VillageTiles/WallTiles_16x16/Wall_Tiles_1.asset";
    private const string WorkbenchSpritePath = "Assets/ImportedAssets/Anokolisa_TopdownRPG/Props/Workbench/Workbench.png";
    private const string BlacksmithSpritePath = "Assets/ImportedAssets/Anokolisa_TopdownRPG/NPCs/Knight/Idle/Idle-Sheet.png";
    private const string SwordSpritePath = "Assets/ImportedAssets/NinjaAdventure/Weapons/big_sword/sprite.png";
    private const string AxeSpritePath = "Assets/ImportedAssets/NinjaAdventure/Weapons/axe/sprite.png";
    private const string ClubSpritePath = "Assets/ImportedAssets/NinjaAdventure/Weapons/club/sprite.png";
    private const string GeneratedFolder = "Assets/Game/Generated/Day24";
    private const string WeaponShopBackgroundPath = GeneratedFolder + "/WeaponShopInterior_Background.png";
    private const string GeneratedWeaponFolder = "Assets/Game/Generated/Day26/GauntletsRunes";
    private const string ForestObjective = "前往森林，調查五色寶光";

    private readonly struct DialogueLine
    {
        public DialogueLine(string speakerName, string text)
        {
            SpeakerName = speakerName;
            Text = text;
        }

        public string SpeakerName { get; }
        public string Text { get; }
    }

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
        public string IconPath => $"{GeneratedWeaponFolder}/{IconFileName}";
    }

    private static readonly WeaponDefinition[] WeaponCatalog =
    {
        new("common_training_gauntlet", "練習拳套", EquipmentKind.Gauntlet, WeaponQuality.Common, 10, 35, 6, "common_training_gauntlet.png"),
        new("common_leather_gauntlet", "皮革拳套", EquipmentKind.Gauntlet, WeaponQuality.Common, 12, 55, 9, "common_leather_gauntlet.png"),
        new("elite_iron_gauntlet", "精鐵拳套", EquipmentKind.Gauntlet, WeaponQuality.Elite, 30, 260, 42, "elite_iron_gauntlet.png"),
        new("common_cracked_rune", "裂紋符文", EquipmentKind.Rune, WeaponQuality.Common, 2, 40, 8, "common_cracked_rune.png"),
        new("common_spark_rune", "微光符文", EquipmentKind.Rune, WeaponQuality.Common, 5, 80, 14, "common_spark_rune.png"),
        new("elite_flame_rune", "焰紋符文", EquipmentKind.Rune, WeaponQuality.Elite, 18, 240, 38, "elite_flame_rune.png"),
        new("elite_guard_rune", "守心符文", EquipmentKind.Rune, WeaponQuality.Elite, 22, 320, 48, "elite_guard_rune.png"),
        new("epic_red_edge_rune", "赤鋒符文", EquipmentKind.Rune, WeaponQuality.Epic, 200, 1080, 220, "epic_red_edge_rune.png"),
        new("legendary_thunder_rune", "雷痕符文", EquipmentKind.Rune, WeaponQuality.Legendary, 300, 0, 0, "legendary_thunder_rune.png")
    };

    private static readonly DialogueLine[] BlacksmithLines =
    {
        new("武器匠", "喲，今天吹什麼風？獵人不去森林，跑來看鐵？"),
        new("主角", "村長讓我來問你一些事。"),
        new("武器匠", "如果是修柵欄，我不去。上次我修完，他們說我把門做得太重，推不開。"),
        new("主角", "不是柵欄，是拳套和符文。"),
        new("武器匠", "那倒像句人話。"),
        new("主角", "村長說，洞穴裡的 Boss 皮糙肉厚，普通攻擊傷不了它。"),
        new("武器匠", "村長終於把那老故事翻出來了？"),
        new("主角", "那有沒有辦法傷到那個 Boss？"),
        new("武器匠", "普通拳套不行。低階符文也只是讓它覺得癢。"),
        new("主角", "史詩符文呢？"),
        new("武器匠", "能打得動強怪，但要說對付洞穴裡那東西，還差得遠。"),
        new("主角", "那什麼才行？"),
        new("武器匠", "雷痕符文。"),
        new("主角", "你真的知道這種東西？"),
        new("武器匠", "知道，因為我們家曾經打造過能承受符文的拳套。"),
        new("武器匠", "雷痕符文裡留下了雷。不是普通雷光，是能撕開厚甲和魔皮的天雷。"),
        new("主角", "它現在在哪？"),
        new("武器匠", "失蹤了很多年。傳說那枚符文會認主。它不服懦弱的人，也不服貪婪的人。"),
        new("主角", "怎麼找到它？"),
        new("武器匠", "它會定期發出五色霞光，吸引人去征服它。"),
        new("主角", "酒館的年輕人說，森林深處有寶光。他們說那光像寶石，又像霧，有很多顏色。"),
        new("武器匠", "那不是普通寶物，很可能就是雷痕符文。"),
        new("主角", "如果那枚符文能傷 Boss，我必須找到它。"),
        new("武器匠", "你可以去找，但別以為看到光就等於它屬於你。"),
        new("主角", "森林裡有大量小怪，深處可能還有更強的東西。"),
        new("武器匠", "那你手上的普通拳套和低階符文不夠。"),
        new("主角", "你有更好的裝備？"),
        new("武器匠", "有一枚史詩符文，名字叫『赤鋒符文』。"),
        new("主角", "它能打精英怪？"),
        new("武器匠", "如果配上精鐵拳套，它能震碎硬骨。當然，想拿它打洞穴 Boss，還是不夠。"),
        new("主角", "我現在可能買不起。"),
        new("武器匠", "那就去森林清小怪，撿金幣，賣掉掉落裝備。你不是獵人嗎？賺錢這種事也要我教？"),
        new("主角", "我先去森林看看。"),
        new("武器匠", "如果你真的看到雷痕符文，不要急著伸手。"),
        new("主角", "為什麼？"),
        new("武器匠", "不被承認的人，碰它不是取得力量，是招雷。"),
        new("主角", "你這話很有說服力，畢竟你祖先就是這樣死的。"),
        new("武器匠", "……滾去森林吧。")
    };

    [MenuItem("IronRPG/Day 24/1 Create Weapon Shop Interior Scene")]
    public static void CreateWeaponShopInteriorScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(WeaponShopScenePath) == null)
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(newScene, WeaponShopScenePath);
        }

        AddOrEnableScene(WeaponShopScenePath);
        EditorSceneManager.OpenScene(WeaponShopScenePath, OpenSceneMode.Single);

        ClearWeaponShopPrototypeObjects();
        CreateWeaponShopRoomBase();
        CreateWeaponShopProps();
        CreateWeaponShopCollision();
        CreateWeaponShopMarkers();
        EnsurePlayerAndCamera();
        EnsureMainQuestState(MainQuestStage.AskBlacksmith);

        SaveSceneAndSelect(GameObject.Find("WeaponShopInteriorRoot"));

        EditorUtility.DisplayDialog(
            "Day 24 Weapon Shop",
            "WeaponShopInteriorScene is ready.\n\nNormal article flow: create a new weapon shop interior scene, add it to the Scene List, build a weapon shop room background, place workbench and weapon display props, then add Player and Main Camera.",
            "OK");
    }

    [MenuItem("IronRPG/Day 24/2 Setup Village To Weapon Shop Transition")]
    public static void SetupVillageToWeaponShopTransition()
    {
        OpenScene(VillageScenePath);

        Transform markers = GetOrCreateGroup("VillageSceneMarkers", null);
        Vector3 doorPosition = GetVillageWeaponShopDoorPosition();
        Vector3 returnPosition = doorPosition + new Vector3(0f, -0.75f, 0f);

        GameObject exit = GetOrCreateMarker("Exit_ToWeaponShop", markers, doorPosition);
        ConfigureTrigger(exit, new Vector2(0.9f, 0.5f));
        ConfigureTransition(exit, WeaponShopSceneName, "Entry_FromVillage");

        GetOrCreateMarker("Entry_FromWeaponShop", markers, returnPosition);

        SaveSceneAndSelect(exit);

        EditorUtility.DisplayDialog(
            "Day 24 Weapon Shop Transition",
            "VillageScene now has Exit_ToWeaponShop and Entry_FromWeaponShop.\n\nNormal article flow: place a trigger at the weapon shop door, set Target Scene Name to WeaponShopInteriorScene, and create a return spawn point outside the shop.",
            "OK");
    }

    [MenuItem("IronRPG/Day 24/3 Setup Blacksmith NPC And Dialogue")]
    public static void SetupBlacksmithNpcAndDialogue()
    {
        OpenScene(WeaponShopScenePath);
        EnsureMainQuestState(MainQuestStage.AskBlacksmith);

        Transform npcRoot = GetOrCreateGroup("WeaponShopNPCs", null);
        GameObject blacksmith = FindOrCreateChild(npcRoot, "BlacksmithNPC");
        blacksmith.transform.position = new Vector3(0f, -4.15f, 0f);
        blacksmith.transform.localScale = new Vector3(2.2f, 2.2f, 1f);

        SpriteRenderer renderer = blacksmith.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(blacksmith);
        }

        renderer.sprite = LoadFirstSprite(BlacksmithSpritePath);
        renderer.sortingOrder = 22;

        BoxCollider2D bodyCollider = blacksmith.GetComponent<BoxCollider2D>();
        if (bodyCollider == null)
        {
            bodyCollider = Undo.AddComponent<BoxCollider2D>(blacksmith);
        }

        bodyCollider.isTrigger = false;
        bodyCollider.offset = new Vector2(0f, -0.05f);
        bodyCollider.size = new Vector2(0.45f, 0.55f);

        GameObject interactionArea = FindOrCreateChild(npcRoot, "BlacksmithInteractionArea");
        interactionArea.transform.position = new Vector3(0f, -5.05f, 0f);
        interactionArea.transform.localScale = Vector3.one;
        ConfigureTrigger(interactionArea, new Vector2(1.5f, 1.25f));

        NpcDialogueInteraction dialogue = interactionArea.GetComponent<NpcDialogueInteraction>();
        if (dialogue == null)
        {
            dialogue = Undo.AddComponent<NpcDialogueInteraction>(interactionArea);
        }

        ApplyBlacksmithDialogueSequence(dialogue);

        SaveSceneAndSelect(interactionArea);

        EditorUtility.DisplayDialog(
            "Day 24 Blacksmith Dialogue",
            "BlacksmithNPC and BlacksmithInteractionArea are ready.\n\nNormal article flow: place the blacksmith NPC in the shop, add a solid body collider, add a trigger interaction area, and configure the dialogue to require AskBlacksmith before advancing to ForestInvestigation.",
            "OK");
    }

    [MenuItem("IronRPG/Day 24/4 Prepare AskBlacksmith Test Stage")]
    public static void PrepareAskBlacksmithTestStage()
    {
        EnsureMainQuestState(MainQuestStage.AskBlacksmith);
        SaveSceneAndSelect(GameObject.Find("MainQuestState"));

        EditorUtility.DisplayDialog(
            "Day 24 Test Stage",
            "MainQuestState is set to AskBlacksmith for direct Play Mode testing.\n\nNormal article flow: this is only a testing helper. The real game reaches AskBlacksmith after the second chief dialogue.",
            "OK");
    }

    [MenuItem("IronRPG/Day 24/5 Setup Manual Weapon Shop Collision And Dialogue")]
    public static void SetupManualWeaponShopCollisionAndDialogue()
    {
        OpenScene(WeaponShopScenePath);

        Bounds roomBounds = GetManualWeaponShopBackgroundBounds();

        RemoveGeneratedWeaponShopHelpers();
        CreateManualWeaponShopCollision(roomBounds);
        CreateManualWeaponShopMarkers(roomBounds);
        EnsurePlayerAndCameraForManualShop(roomBounds);
        EnsureMainQuestState(MainQuestStage.AskBlacksmith);
        CreateManualBlacksmithNpcAndDialogue(roomBounds);

        SaveSceneAndSelect(GameObject.Find("BlacksmithInteractionArea"));

        EditorUtility.DisplayDialog(
            "Day 24 Manual Weapon Shop",
            "Manual weapon shop collision, markers, blacksmith NPC, and AskBlacksmith dialogue are ready.\n\nThis tool preserves the user's weapon_scene background and only rebuilds helper objects.",
            "OK");
    }

    [MenuItem("IronRPG/Day 24/6 Setup Weapon Shop Buy Sell System")]
    public static void SetupWeaponShopBuySellSystem()
    {
        OpenScene(WeaponShopScenePath);

        Bounds roomBounds = GetManualWeaponShopBackgroundBounds();
        EnsurePlayerInventoryForShop();
        CreateWeaponShopTradingArea(roomBounds);

        SaveSceneAndSelect(GameObject.Find("WeaponShopTradingArea"));

        EditorUtility.DisplayDialog(
            "Day 24 Weapon Shop Trading",
            "Weapon shop buy/sell interaction is ready.\n\nNormal article flow: create a PlayerInventory object for gold and owned weapons, then add a trigger area near the blacksmith counter with WeaponShopInteraction. The shop sells the epic weapon Red Edge and lets the player sell dropped weapons.",
            "OK");
    }

    [MenuItem("IronRPG/Day 24/Open Weapon Shop Interior Scene")]
    public static void OpenWeaponShopInteriorScene()
    {
        OpenScene(WeaponShopScenePath);
    }

    private static void CreateWeaponShopRoomBase()
    {
        GameObject root = new GameObject("WeaponShopInteriorRoot");
        Undo.RegisterCreatedObjectUndo(root, "Create WeaponShopInteriorRoot");

        Sprite backgroundSprite = GenerateWeaponShopBackgroundSprite();
        GameObject backgroundObject = new GameObject("WeaponShopRoom_Background");
        Undo.RegisterCreatedObjectUndo(backgroundObject, "Create WeaponShopRoom_Background");
        Undo.SetTransformParent(backgroundObject.transform, root.transform, "Move WeaponShopRoom_Background");
        backgroundObject.transform.position = Vector3.zero;

        SpriteRenderer renderer = backgroundObject.AddComponent<SpriteRenderer>();
        renderer.sprite = backgroundSprite;
        renderer.sortingOrder = -5;

        if (backgroundSprite != null)
        {
            const float targetWidthUnits = 36f;
            const float targetHeightUnits = 25.2f;
            Vector2 spriteSize = backgroundSprite.bounds.size;
            backgroundObject.transform.localScale = new Vector3(
                targetWidthUnits / spriteSize.x,
                targetHeightUnits / spriteSize.y,
                1f);
        }
    }

    private static Tilemap CreateTilemap(string objectName, Transform parent, int sortingOrder)
    {
        GameObject tilemapObject = new GameObject(objectName);
        Undo.RegisterCreatedObjectUndo(tilemapObject, $"Create {objectName}");
        Undo.SetTransformParent(tilemapObject.transform, parent, $"Move {objectName}");

        Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;
        return tilemap;
    }

    private static void CreateWeaponShopProps()
    {
        Transform propRoot = GetOrCreateGroup("WeaponShopProps", null);

        CreateSpriteProp(propRoot, "BlacksmithWorkbench", WorkbenchSpritePath, 0, new Vector3(11.75f, -1.95f, 0f), new Vector3(2.6f, 2.6f, 1f), 10);
        CreateSpriteProp(propRoot, "EpicWeapon_Display_RedBlade", SwordSpritePath, 0, new Vector3(-6.8f, 6.95f, 0f), new Vector3(2.8f, 2.8f, 1f), 14);
        CreateSpriteProp(propRoot, "CommonWeapon_Display_Axe", AxeSpritePath, 0, new Vector3(3.8f, 6.95f, 0f), new Vector3(2.7f, 2.7f, 1f), 14);
        CreateSpriteProp(propRoot, "CommonWeapon_Display_Club", ClubSpritePath, 0, new Vector3(13.7f, -8.65f, 0f), new Vector3(2.7f, 2.7f, 1f), 14);
        CreateSpriteProp(propRoot, "LegendaryWeapon_InfoPlaceholder", SwordSpritePath, 0, new Vector3(10.85f, 6.45f, 0f), new Vector3(3.1f, 3.1f, 1f), 14);
    }

    private static GameObject CreateSpriteProp(Transform parent, string objectName, string spritePath, int spriteIndex, Vector3 position, Vector3 scale, int sortingOrder)
    {
        return CreateSpriteProp(parent, objectName, spritePath, spriteIndex, position, scale, sortingOrder, 0f);
    }

    private static GameObject CreateSpriteProp(Transform parent, string objectName, string spritePath, int spriteIndex, Vector3 position, Vector3 scale, int sortingOrder, float rotationZ)
    {
        GameObject prop = FindOrCreateChild(parent, objectName);
        prop.transform.position = position;
        prop.transform.localScale = scale;
        prop.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);

        SpriteRenderer renderer = prop.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(prop);
        }

        renderer.sprite = LoadSprite(spritePath, spriteIndex);
        renderer.sortingOrder = sortingOrder;
        EditorUtility.SetDirty(prop);
        EditorUtility.SetDirty(renderer);
        return prop;
    }

    private static void CreateWeaponShopCollision()
    {
        Transform collisionRoot = GetOrCreateGroup("WeaponShopCollision", null);
        ClearChildren(collisionRoot);

        CreateCollider(collisionRoot, "WallCollider_Top", new Vector3(0f, 12.95f, 0f), new Vector2(36.5f, 1.0f));
        CreateCollider(collisionRoot, "WallCollider_Left", new Vector3(-18.25f, 0f, 0f), new Vector2(1.0f, 25.8f));
        CreateCollider(collisionRoot, "WallCollider_Right", new Vector3(18.25f, 0f, 0f), new Vector2(1.0f, 25.8f));
        CreateCollider(collisionRoot, "WallCollider_BottomLeft", new Vector3(-10.3f, -12.95f, 0f), new Vector2(15.9f, 1.0f));
        CreateCollider(collisionRoot, "WallCollider_BottomRight", new Vector3(10.3f, -12.95f, 0f), new Vector2(15.9f, 1.0f));
        CreateCollider(collisionRoot, "ForgeCollider", new Vector3(12.8f, 8.05f, 0f), new Vector2(5.0f, 4.7f));
        CreateCollider(collisionRoot, "AnvilCollider", new Vector3(9.45f, 5.05f, 0f), new Vector2(2.6f, 1.8f));
        CreateCollider(collisionRoot, "ShopCounterCollider", new Vector3(0f, -4.75f, 0f), new Vector2(11.2f, 2.0f));
        CreateCollider(collisionRoot, "WeaponRackCollider_TopLeft", new Vector3(-8.3f, 8.6f, 0f), new Vector2(4.6f, 1.7f));
        CreateCollider(collisionRoot, "WeaponRackCollider_TopCenter", new Vector3(1.7f, 8.6f, 0f), new Vector2(4.9f, 1.7f));
        CreateCollider(collisionRoot, "WeaponRackCollider_Left", new Vector3(-15.55f, 1.2f, 0f), new Vector2(2.0f, 8.9f));
        CreateCollider(collisionRoot, "WeaponRackCollider_BottomRight", new Vector3(12.85f, -9.25f, 0f), new Vector2(3.6f, 1.9f));
        CreateCollider(collisionRoot, "MaterialCratesCollider_Left", new Vector3(-14.2f, -9.2f, 0f), new Vector2(5.7f, 2.8f));
        CreateCollider(collisionRoot, "MaterialCratesCollider_Right", new Vector3(15.0f, -8.2f, 0f), new Vector2(4.7f, 3.3f));
        CreateCollider(collisionRoot, "RightWorkbenchCollider", new Vector3(14.45f, 1.0f, 0f), new Vector2(3.4f, 2.4f));
    }

    private static void CreateWeaponShopMarkers()
    {
        Transform markers = GetOrCreateGroup("WeaponShopMarkers", null);
        GetOrCreateMarker("Entry_FromVillage", markers, new Vector3(0f, -11.05f, 0f));
        GameObject exit = GetOrCreateMarker("Exit_ToVillage", markers, new Vector3(0f, -12.35f, 0f));
        ConfigureTrigger(exit, new Vector2(2.2f, 0.85f));
        ConfigureTransition(exit, VillageSceneName, "Entry_FromWeaponShop");
    }

    private static void CreateManualWeaponShopCollision(Bounds roomBounds)
    {
        Transform collisionRoot = GetOrCreateGroup("WeaponShopCollision", null);
        ClearChildren(collisionRoot);

        CreateBoundsCollider(collisionRoot, "WallCollider_Top", roomBounds, new Vector2(0.5f, 0.975f), new Vector2(0.96f, 0.05f));
        CreateBoundsCollider(collisionRoot, "WallCollider_Left", roomBounds, new Vector2(0.02f, 0.5f), new Vector2(0.04f, 0.92f));
        CreateBoundsCollider(collisionRoot, "WallCollider_Right", roomBounds, new Vector2(0.98f, 0.5f), new Vector2(0.04f, 0.92f));
        CreateBoundsCollider(collisionRoot, "WallCollider_BottomLeft", roomBounds, new Vector2(0.25f, 0.025f), new Vector2(0.42f, 0.05f));
        CreateBoundsCollider(collisionRoot, "WallCollider_BottomRight", roomBounds, new Vector2(0.75f, 0.025f), new Vector2(0.42f, 0.05f));

        CreateBoundsCollider(collisionRoot, "CounterCollider_BottomLeft", roomBounds, new Vector2(0.2f, 0.56f), new Vector2(0.18f, 0.16f));
        CreateBoundsCollider(collisionRoot, "CounterCollider_LeftMiddle", roomBounds, new Vector2(0.12f, 0.50f), new Vector2(0.17f, 0.20f));
        CreateBoundsCollider(collisionRoot, "ForgeCollider_TopRight", roomBounds, new Vector2(0.78f, 0.80f), new Vector2(0.19f, 0.25f));
        CreateBoundsCollider(collisionRoot, "WorkbenchCollider_Right", roomBounds, new Vector2(0.88f, 0.44f), new Vector2(0.18f, 0.20f));
        CreateBoundsCollider(collisionRoot, "WeaponRackCollider_CenterLeft", roomBounds, new Vector2(0.40f, 0.52f), new Vector2(0.18f, 0.18f));
        CreateBoundsCollider(collisionRoot, "WeaponRackCollider_CenterRight", roomBounds, new Vector2(0.58f, 0.52f), new Vector2(0.18f, 0.18f));
        CreateBoundsCollider(collisionRoot, "WeaponRackCollider_LowerCenterLeft", roomBounds, new Vector2(0.42f, 0.31f), new Vector2(0.18f, 0.17f));
        CreateBoundsCollider(collisionRoot, "WeaponRackCollider_LowerCenterRight", roomBounds, new Vector2(0.61f, 0.31f), new Vector2(0.18f, 0.17f));
        CreateBoundsCollider(collisionRoot, "DisplayShelvesCollider_Top", roomBounds, new Vector2(0.47f, 0.88f), new Vector2(0.70f, 0.15f));
        CreateBoundsCollider(collisionRoot, "DisplayShelvesCollider_Right", roomBounds, new Vector2(0.91f, 0.27f), new Vector2(0.14f, 0.34f));
        CreateBoundsCollider(collisionRoot, "BarrelsAndCratesCollider_Bottom", roomBounds, new Vector2(0.50f, 0.10f), new Vector2(0.72f, 0.10f));
    }

    private static void CreateManualWeaponShopMarkers(Bounds roomBounds)
    {
        Transform markers = GetOrCreateGroup("WeaponShopMarkers", null);
        ClearChildren(markers);

        GetOrCreateMarker("Entry_FromVillage", markers, BoundsPoint(roomBounds, new Vector2(0.5f, 0.12f)));
        GameObject exit = GetOrCreateMarker("Exit_ToVillage", markers, BoundsPoint(roomBounds, new Vector2(0.5f, 0.055f)));
        ConfigureTrigger(exit, new Vector2(roomBounds.size.x * 0.14f, roomBounds.size.y * 0.07f));
        ConfigureTransition(exit, VillageSceneName, "Entry_FromWeaponShop");
    }

    private static void EnsurePlayerAndCameraForManualShop(Bounds roomBounds)
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
            }
        }

        if (player != null)
        {
            player.transform.position = BoundsPoint(roomBounds, new Vector2(0.5f, 0.12f));
            player.transform.localScale = new Vector3(2f, 2f, 1f);
            EditorUtility.SetDirty(player);
        }

        GameObject camera = GameObject.Find("Main Camera");
        if (camera == null)
        {
            camera = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(camera, "Create Main Camera");
            camera.tag = "MainCamera";
            camera.AddComponent<AudioListener>();
        }

        camera.transform.position = new Vector3(roomBounds.center.x, roomBounds.center.y, -10f);

        Camera cameraComponent = camera.GetComponent<Camera>();
        if (cameraComponent == null)
        {
            cameraComponent = Undo.AddComponent<Camera>(camera);
        }

        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = Mathf.Max(roomBounds.size.y * 0.55f, 3f);
        cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        cameraComponent.backgroundColor = new Color(0.09f, 0.08f, 0.07f, 1f);

        if (camera.GetComponent<CameraFollow>() == null)
        {
            Undo.AddComponent<CameraFollow>(camera);
        }

        EditorUtility.SetDirty(camera);
    }

    private static void CreateManualBlacksmithNpcAndDialogue(Bounds roomBounds)
    {
        Transform npcRoot = GetOrCreateGroup("WeaponShopNPCs", null);
        ClearChildren(npcRoot);

        GameObject blacksmith = FindOrCreateChild(npcRoot, "BlacksmithNPC");
        blacksmith.transform.position = BoundsPoint(roomBounds, new Vector2(0.22f, 0.58f));
        blacksmith.transform.localScale = new Vector3(2.2f, 2.2f, 1f);

        SpriteRenderer renderer = blacksmith.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(blacksmith);
        }

        renderer.sprite = LoadFirstSprite(BlacksmithSpritePath);
        renderer.sortingOrder = 22;

        BoxCollider2D bodyCollider = blacksmith.GetComponent<BoxCollider2D>();
        if (bodyCollider == null)
        {
            bodyCollider = Undo.AddComponent<BoxCollider2D>(blacksmith);
        }

        bodyCollider.isTrigger = false;
        bodyCollider.offset = new Vector2(0f, -0.05f);
        bodyCollider.size = new Vector2(0.45f, 0.55f);

        GameObject interactionArea = FindOrCreateChild(npcRoot, "BlacksmithInteractionArea");
        interactionArea.transform.position = BoundsPoint(roomBounds, new Vector2(0.22f, 0.45f));
        interactionArea.transform.localScale = Vector3.one;
        ConfigureTrigger(interactionArea, new Vector2(roomBounds.size.x * 0.18f, roomBounds.size.y * 0.16f));

        NpcDialogueInteraction dialogue = interactionArea.GetComponent<NpcDialogueInteraction>();
        if (dialogue == null)
        {
            dialogue = Undo.AddComponent<NpcDialogueInteraction>(interactionArea);
        }

        ApplyBlacksmithDialogueSequence(dialogue);
    }

    private static void EnsurePlayerInventoryForShop()
    {
        ConfigureGeneratedWeaponSprites();

        GameObject inventoryObject = GameObject.Find("PlayerInventory");
        if (inventoryObject == null)
        {
            inventoryObject = new GameObject("PlayerInventory");
            Undo.RegisterCreatedObjectUndo(inventoryObject, "Create PlayerInventory");
        }

        PlayerInventory inventory = inventoryObject.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            inventory = Undo.AddComponent<PlayerInventory>(inventoryObject);
        }

        SerializedObject serializedInventory = new SerializedObject(inventory);
        serializedInventory.FindProperty("startingGold").intValue = 80;

        SerializedProperty startingWeapons = serializedInventory.FindProperty("startingWeapons");
        startingWeapons.arraySize = 2;
        AssignWeaponStack(startingWeapons.GetArrayElementAtIndex(0), WeaponCatalog[0], 1);
        AssignWeaponStack(startingWeapons.GetArrayElementAtIndex(1), WeaponCatalog[3], 1);

        serializedInventory.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(inventory);
    }

    private static void CreateWeaponShopTradingArea(Bounds roomBounds)
    {
        Transform tradingRoot = GetOrCreateGroup("WeaponShopTrading", null);
        ClearChildren(tradingRoot);

        GameObject tradingArea = FindOrCreateChild(tradingRoot, "WeaponShopTradingArea");
        tradingArea.transform.position = BoundsPoint(roomBounds, new Vector2(0.22f, 0.45f));
        tradingArea.transform.localScale = Vector3.one;
        ConfigureTrigger(tradingArea, new Vector2(roomBounds.size.x * 0.22f, roomBounds.size.y * 0.16f));

        WeaponShopInteraction interaction = tradingArea.GetComponent<WeaponShopInteraction>();
        if (interaction == null)
        {
            interaction = Undo.AddComponent<WeaponShopInteraction>(tradingArea);
        }

        SerializedObject serializedInteraction = new SerializedObject(interaction);
        serializedInteraction.FindProperty("interactPrompt").stringValue = "按 T 打開拳套與符文交易";

        SerializedProperty weaponsForSale = serializedInteraction.FindProperty("weaponsForSale");
        weaponsForSale.arraySize = 8;
        for (int i = 0; i < 8; i++)
        {
            AssignWeaponItem(weaponsForSale.GetArrayElementAtIndex(i), WeaponCatalog[i]);
        }

        serializedInteraction.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(interaction);
    }

    private static void AssignWeaponStack(SerializedProperty stackProperty, WeaponDefinition weapon, int count)
    {
        AssignWeaponItem(stackProperty.FindPropertyRelative("weapon"), weapon);
        stackProperty.FindPropertyRelative("count").intValue = count;
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

    private static Sprite LoadGeneratedWeaponSprite(WeaponDefinition weapon)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(weapon.IconPath);
        if (sprite == null)
        {
            Debug.LogWarning($"Day24WeaponShopOrganizer could not load generated weapon sprite: {weapon.IconPath}");
        }

        return sprite;
    }

    private static void ConfigureGeneratedWeaponSprites()
    {
        for (int i = 0; i < WeaponCatalog.Length; i++)
        {
            ConfigureSpriteImporter(WeaponCatalog[i].IconPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ConfigureSpriteImporter(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"Day24WeaponShopOrganizer could not find texture importer: {assetPath}");
            return;
        }

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
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

        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

    private static void EnsurePlayerAndCamera()
    {
        if (GameObject.Find("Player") == null)
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            GameObject player = playerPrefab != null
                ? PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject
                : new GameObject("Player");

            if (player != null)
            {
                Undo.RegisterCreatedObjectUndo(player, "Create Player");
                player.name = "Player";
                player.transform.position = new Vector3(0f, -11.05f, 0f);
                player.transform.localScale = new Vector3(2f, 2f, 1f);
            }
        }

        GameObject camera = GameObject.Find("Main Camera");
        if (camera == null)
        {
            camera = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(camera, "Create Main Camera");
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 0f, -10f);

            Camera cameraComponent = camera.AddComponent<Camera>();
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = 13.8f;
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = new Color(0.09f, 0.08f, 0.07f, 1f);

            camera.AddComponent<AudioListener>();
        }

        if (camera.GetComponent<CameraFollow>() == null)
        {
            Undo.AddComponent<CameraFollow>(camera);
        }

        EditorUtility.SetDirty(camera);
    }

    private static void ApplyBlacksmithDialogueSequence(NpcDialogueInteraction dialogue)
    {
        SerializedObject serializedDialogue = new SerializedObject(dialogue);
        serializedDialogue.FindProperty("speakerName").stringValue = "武器匠";
        serializedDialogue.FindProperty("interactPrompt").stringValue = "按 E 與武器匠交談";
        serializedDialogue.FindProperty("useDialogueSequences").boolValue = true;

        SerializedProperty sequences = serializedDialogue.FindProperty("dialogueSequences");
        sequences.arraySize = 1;

        SerializedProperty sequence = sequences.GetArrayElementAtIndex(0);
        sequence.FindPropertyRelative("sequenceName").stringValue = "第一次找武器匠";
        sequence.FindPropertyRelative("requiredQuestStage").enumValueIndex = (int)MainQuestStage.AskBlacksmith;
        sequence.FindPropertyRelative("speakerName").stringValue = "武器匠";
        sequence.FindPropertyRelative("interactPrompt").stringValue = "按 E 與武器匠交談";
        sequence.FindPropertyRelative("useSpeakerLines").boolValue = true;
        sequence.FindPropertyRelative("setObjectiveOnComplete").boolValue = true;
        sequence.FindPropertyRelative("objectiveTextOnComplete").stringValue = ForestObjective;
        sequence.FindPropertyRelative("advanceQuestStageOnComplete").boolValue = true;
        sequence.FindPropertyRelative("questStageOnComplete").enumValueIndex = (int)MainQuestStage.ForestInvestigation;

        SerializedProperty speakerLines = sequence.FindPropertyRelative("speakerLines");
        speakerLines.arraySize = BlacksmithLines.Length;
        for (int i = 0; i < BlacksmithLines.Length; i++)
        {
            SerializedProperty entry = speakerLines.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("speakerName").stringValue = BlacksmithLines[i].SpeakerName;
            entry.FindPropertyRelative("text").stringValue = BlacksmithLines[i].Text;
        }

        serializedDialogue.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(dialogue);
    }

    private static void EnsureMainQuestState(MainQuestStage stage)
    {
        GameObject stateObject = GameObject.Find("MainQuestState");
        if (stateObject == null)
        {
            stateObject = new GameObject("MainQuestState");
            Undo.RegisterCreatedObjectUndo(stateObject, "Create MainQuestState");
        }

        MainQuestState state = stateObject.GetComponent<MainQuestState>();
        if (state == null)
        {
            state = Undo.AddComponent<MainQuestState>(stateObject);
        }

        SerializedObject serializedState = new SerializedObject(state);
        serializedState.FindProperty("currentStage").enumValueIndex = (int)stage;
        serializedState.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(state);
    }

    private static Vector3 GetVillageWeaponShopDoorPosition()
    {
        GameObject existingExit = GameObject.Find("Exit_ToWeaponShop");
        if (existingExit != null)
        {
            return existingExit.transform.position;
        }

        GameObject point = GameObject.Find("EquipmentShopPoint");
        if (point != null)
        {
            return point.transform.position;
        }

        GameObject equipmentShop = GameObject.Find("EquipmentShop");
        if (equipmentShop != null)
        {
            return equipmentShop.transform.position + new Vector3(0f, -3.3f, 0f);
        }

        Debug.LogWarning("Day24WeaponShopOrganizer could not find EquipmentShop or EquipmentShopPoint. Using fallback position.");
        return new Vector3(14.8f, -6.4f, 0f);
    }

    private static Bounds GetManualWeaponShopBackgroundBounds()
    {
        SpriteRenderer renderer = null;
        GameObject exactBackground = GameObject.Find("weapon_scene_0");
        if (exactBackground != null)
        {
            renderer = exactBackground.GetComponent<SpriteRenderer>();
        }

        if (renderer == null)
        {
            SpriteRenderer[] renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (SpriteRenderer candidate in renderers)
            {
                if (candidate.gameObject.name.Contains("weapon_scene")
                    || candidate.gameObject.name.Contains("WeaponShopRoom_Background"))
                {
                    renderer = candidate;
                    break;
                }
            }
        }

        if (renderer != null)
        {
            return renderer.bounds;
        }

        Debug.LogWarning("Day24WeaponShopOrganizer could not find weapon_scene_0 SpriteRenderer. Using fallback room bounds.");
        return new Bounds(Vector3.zero, new Vector3(6.2f, 4.1f, 0f));
    }

    private static Vector3 BoundsPoint(Bounds bounds, Vector2 normalizedPoint)
    {
        return new Vector3(
            Mathf.Lerp(bounds.min.x, bounds.max.x, normalizedPoint.x),
            Mathf.Lerp(bounds.min.y, bounds.max.y, normalizedPoint.y),
            0f);
    }

    private static Vector2 BoundsSize(Bounds bounds, Vector2 normalizedSize)
    {
        return new Vector2(bounds.size.x * normalizedSize.x, bounds.size.y * normalizedSize.y);
    }

    private static void CreateBoundsCollider(Transform parent, string objectName, Bounds bounds, Vector2 normalizedCenter, Vector2 normalizedSize)
    {
        CreateCollider(parent, objectName, BoundsPoint(bounds, normalizedCenter), BoundsSize(bounds, normalizedSize));
    }

    private static void RemoveGeneratedWeaponShopHelpers()
    {
        string[] generatedNames =
        {
            "WeaponShopInteriorRoot",
            "WeaponShopProps",
            "WeaponShopCollision",
            "WeaponShopMarkers",
            "WeaponShopNPCs"
        };

        foreach (string objectName in generatedNames)
        {
            GameObject existing = GameObject.Find(objectName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }
        }
    }

    private static void ClearWeaponShopPrototypeObjects()
    {
        string[] names =
        {
            "WeaponShopInteriorRoot",
            "Grid",
            "WeaponShopProps",
            "WeaponShopCollision",
            "WeaponShopMarkers",
            "WeaponShopNPCs",
            "Player",
            "Main Camera",
            "MainQuestState"
        };

        foreach (string objectName in names)
        {
            GameObject existing = GameObject.Find(objectName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }
        }
    }

    private static TileBase LoadTile(string assetPath)
    {
        TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
        if (tile == null)
        {
            Debug.LogWarning($"Day24WeaponShopOrganizer could not load tile: {assetPath}");
        }

        return tile;
    }

    private static Sprite GenerateWeaponShopBackgroundSprite()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Game/Generated"))
        {
            AssetDatabase.CreateFolder("Assets/Game", "Generated");
        }

        if (!AssetDatabase.IsValidFolder(GeneratedFolder))
        {
            AssetDatabase.CreateFolder("Assets/Game/Generated", "Day24");
        }

        const int width = 640;
        const int height = 448;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };

        Color32 transparent = new Color32(0, 0, 0, 0);
        Color32 shadow = new Color32(27, 23, 22, 255);
        Color32 stoneDark = new Color32(62, 62, 59, 255);
        Color32 stoneMid = new Color32(88, 86, 80, 255);
        Color32 stoneLight = new Color32(126, 122, 110, 255);
        Color32 floorDark = new Color32(82, 50, 31, 255);
        Color32 floorMid = new Color32(112, 68, 39, 255);
        Color32 floorLight = new Color32(143, 88, 47, 255);
        Color32 wallBase = new Color32(116, 86, 58, 255);
        Color32 wallTrim = new Color32(71, 44, 31, 255);
        Color32 woodDark = new Color32(65, 40, 27, 255);
        Color32 woodMid = new Color32(112, 67, 38, 255);
        Color32 woodLight = new Color32(163, 104, 55, 255);
        Color32 metal = new Color32(174, 178, 172, 255);
        Color32 metalDark = new Color32(98, 104, 103, 255);
        Color32 coal = new Color32(25, 22, 21, 255);
        Color32 fire = new Color32(238, 85, 35, 255);
        Color32 ember = new Color32(255, 185, 67, 255);
        Color32 cloth = new Color32(98, 35, 35, 255);

        Fill(texture, 0, 0, width, height, transparent);
        Fill(texture, 28, 24, 584, 392, shadow);
        DrawBrickPattern(texture, 36, 32, 568, 376, stoneDark, stoneMid);
        Fill(texture, 56, 56, 528, 328, floorMid);

        // Thick stone border with a bottom doorway.
        DrawBrickPattern(texture, 36, 384, 568, 24, stoneDark, stoneLight);
        DrawBrickPattern(texture, 36, 32, 26, 376, stoneDark, stoneMid);
        DrawBrickPattern(texture, 578, 32, 26, 376, stoneDark, stoneMid);
        DrawBrickPattern(texture, 36, 32, 252, 24, stoneDark, stoneMid);
        DrawBrickPattern(texture, 352, 32, 252, 24, stoneDark, stoneMid);

        // Back wall and floor.
        Fill(texture, 56, 300, 528, 84, wallBase);
        DrawBrickPattern(texture, 56, 300, 528, 84, wallBase, wallTrim);
        Fill(texture, 56, 288, 528, 12, wallTrim);
        Fill(texture, 56, 56, 528, 232, floorMid);

        for (int y = 58; y < 288; y += 16)
        {
            Color32 plank = ((y / 16) % 2 == 0) ? floorMid : floorDark;
            Fill(texture, 58, y, 524, 15, plank);
            DrawLine(texture, 58, y, 581, y, wallTrim);
        }

        for (int x = 70; x < 582; x += 36)
        {
            DrawLine(texture, x, 58, x, 287, new Color32(88, 52, 33, 255));
        }

        DrawWindow(texture, 178, 346, woodDark, woodLight, ember);
        DrawWindow(texture, 292, 346, woodDark, woodLight, ember);
        DrawWindow(texture, 410, 346, woodDark, woodLight, ember);
        DrawTorch(texture, 250, 328, fire, ember, woodDark);
        DrawTorch(texture, 500, 328, fire, ember, woodDark);

        DrawWallWeaponShelf(texture, 94, 318, metal, metalDark, woodDark, woodLight);
        DrawWallWeaponShelf(texture, 238, 318, metal, metalDark, woodDark, woodLight);
        DrawWallWeaponShelf(texture, 362, 318, metal, metalDark, woodDark, woodLight);
        DrawStandingRack(texture, 72, 176, metal, metalDark, woodDark, woodLight);
        DrawStandingRack(texture, 72, 94, metal, metalDark, woodDark, woodLight);
        DrawStandingRack(texture, 494, 72, metal, metalDark, woodDark, woodLight);

        DrawForge(texture, 470, 286, stoneDark, stoneMid, coal, fire, ember);
        DrawAnvil(texture, 438, 248, metalDark, metal, woodDark);
        DrawOrePile(texture, 544, 242, stoneDark, stoneMid, stoneLight);
        DrawCounter(texture, 206, 126, 228, 38, woodDark, woodMid, woodLight);
        Fill(texture, 220, 140, 92, 12, cloth);
        Fill(texture, 342, 136, 10, 10, metal);
        Fill(texture, 362, 136, 12, 12, metalDark);
        DrawCarpet(texture, 250, 198, 140, 80, cloth, woodLight);
        DrawCarpet(texture, 296, 54, 48, 28, cloth, woodLight);

        DrawCrate(texture, 76, 62, woodDark, woodLight);
        DrawCrate(texture, 108, 62, woodDark, woodLight);
        DrawCrate(texture, 148, 70, woodDark, woodLight);
        DrawCrate(texture, 542, 74, woodDark, woodLight);
        DrawCrate(texture, 512, 58, woodDark, woodLight);
        DrawBarrel(texture, 72, 332, woodDark, woodLight);
        DrawBarrel(texture, 100, 332, woodDark, woodLight);
        DrawBarrel(texture, 94, 58, woodDark, woodLight);
        DrawBarrel(texture, 556, 60, woodDark, woodLight);
        DrawPottedPlant(texture, 128, 292, woodDark, new Color32(51, 106, 55, 255));
        DrawPottedPlant(texture, 528, 132, woodDark, new Color32(51, 106, 55, 255));
        DrawShield(texture, 154, 326, cloth, metal);
        DrawShield(texture, 174, 326, new Color32(44, 69, 96, 255), metal);

        Fill(texture, 296, 32, 56, 34, new Color32(58, 37, 28, 255));
        Fill(texture, 306, 32, 36, 58, new Color32(92, 58, 35, 255));
        DrawLine(texture, 324, 34, 324, 88, woodDark);
        Fill(texture, 300, 88, 48, 8, woodLight);

        byte[] png = texture.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(), WeaponShopBackgroundPath), png);
        Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(WeaponShopBackgroundPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(WeaponShopBackgroundPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 16f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(WeaponShopBackgroundPath);
    }

    private static void DrawWindow(Texture2D texture, int x, int y, Color32 woodDark, Color32 woodLight, Color32 glow)
    {
        Fill(texture, x, y, 28, 36, woodDark);
        Fill(texture, x + 4, y + 5, 20, 26, new Color32(228, 211, 164, 255));
        Fill(texture, x + 6, y + 7, 16, 22, glow);
        DrawLine(texture, x + 14, y + 5, x + 14, y + 30, woodDark);
        DrawLine(texture, x + 4, y + 18, x + 23, y + 18, woodDark);
        Fill(texture, x - 3, y - 4, 34, 5, woodLight);
    }

    private static void DrawTorch(Texture2D texture, int x, int y, Color32 fire, Color32 ember, Color32 woodDark)
    {
        Fill(texture, x + 5, y, 4, 20, woodDark);
        Fill(texture, x + 1, y + 14, 12, 6, woodDark);
        Fill(texture, x + 4, y + 20, 6, 12, fire);
        Fill(texture, x + 2, y + 24, 10, 9, ember);
        Fill(texture, x + 5, y + 29, 4, 8, new Color32(255, 225, 91, 255));
    }

    private static void DrawCarpet(Texture2D texture, int x, int y, int width, int height, Color32 cloth, Color32 trim)
    {
        Fill(texture, x, y, width, height, new Color32(72, 29, 29, 255));
        Fill(texture, x + 5, y + 5, width - 10, height - 10, cloth);
        Fill(texture, x + 8, y + 8, width - 16, height - 16, new Color32(105, 45, 39, 255));
        DrawLine(texture, x + 4, y + 4, x + width - 5, y + 4, trim);
        DrawLine(texture, x + 4, y + height - 5, x + width - 5, y + height - 5, trim);
        DrawLine(texture, x + 4, y + 4, x + 4, y + height - 5, trim);
        DrawLine(texture, x + width - 5, y + 4, x + width - 5, y + height - 5, trim);

        for (int xx = x + 18; xx < x + width - 18; xx += 22)
        {
            Fill(texture, xx, y + height / 2 - 2, 10, 4, new Color32(128, 57, 47, 255));
        }
    }

    private static void DrawPottedPlant(Texture2D texture, int x, int y, Color32 pot, Color32 leaf)
    {
        Fill(texture, x + 8, y, 16, 14, pot);
        Fill(texture, x + 4, y + 11, 24, 5, new Color32(151, 91, 47, 255));
        Fill(texture, x + 10, y + 15, 6, 20, leaf);
        Fill(texture, x + 2, y + 23, 14, 10, leaf);
        Fill(texture, x + 16, y + 24, 15, 10, new Color32(42, 91, 48, 255));
        Fill(texture, x + 8, y + 32, 18, 8, new Color32(59, 121, 61, 255));
    }

    private static void DrawForge(Texture2D texture, int x, int y, Color32 stoneDark, Color32 stoneMid, Color32 coal, Color32 fire, Color32 ember)
    {
        Fill(texture, x, y, 70, 44, stoneDark);
        Fill(texture, x + 8, y + 8, 54, 28, stoneMid);
        Fill(texture, x + 18, y + 14, 34, 14, coal);
        Fill(texture, x + 22, y + 16, 8, 10, fire);
        Fill(texture, x + 32, y + 15, 10, 12, ember);
        Fill(texture, x + 44, y + 17, 6, 8, fire);
        Fill(texture, x + 24, y + 30, 26, 4, new Color32(190, 92, 47, 255));
        Fill(texture, x + 28, y + 44, 18, 24, stoneDark);
    }

    private static void DrawCounter(Texture2D texture, int x, int y, int width, int height, Color32 woodDark, Color32 woodMid, Color32 woodLight)
    {
        Fill(texture, x, y, width, height, woodDark);
        Fill(texture, x + 4, y + 5, width - 8, height - 8, woodMid);
        DrawLine(texture, x + 4, y + height - 9, x + width - 5, y + height - 9, woodLight);
        for (int xx = x + 24; xx < x + width - 8; xx += 28)
        {
            DrawLine(texture, xx, y + 4, xx, y + height - 6, new Color32(78, 48, 31, 255));
        }

        DrawStraightSword(texture, x + 14, y + 15, false, new Color32(176, 181, 176, 255), woodLight);
        DrawStraightSword(texture, x + width - 52, y + 8, false, new Color32(176, 181, 176, 255), woodLight);
        DrawAxeIcon(texture, x + width - 26, y + 14, new Color32(170, 174, 169, 255), woodLight);
    }

    private static void DrawWallWeaponShelf(Texture2D texture, int x, int y, Color32 metal, Color32 metalDark, Color32 woodDark, Color32 woodLight)
    {
        Fill(texture, x, y, 84, 8, woodDark);
        Fill(texture, x, y + 32, 84, 8, woodDark);
        Fill(texture, x + 4, y + 4, 76, 3, woodLight);
        Fill(texture, x + 4, y + 36, 76, 3, woodLight);

        DrawStraightSword(texture, x + 8, y + 18, false, metal, woodLight);
        DrawStraightSword(texture, x + 34, y + 18, false, metal, woodLight);
        DrawStraightSword(texture, x + 60, y + 18, false, metal, woodLight);
        DrawAxeIcon(texture, x + 8, y + 46, metal, woodLight);
        DrawAxeIcon(texture, x + 36, y + 46, metal, woodLight);
        Fill(texture, x + 62, y + 44, 18, 12, metalDark);
        Fill(texture, x + 66, y + 48, 10, 4, metal);
    }

    private static void DrawStandingRack(Texture2D texture, int x, int y, Color32 metal, Color32 metalDark, Color32 woodDark, Color32 woodLight)
    {
        Fill(texture, x, y, 54, 82, woodDark);
        Fill(texture, x + 5, y + 6, 44, 70, new Color32(84, 52, 34, 255));
        Fill(texture, x + 5, y + 18, 44, 5, woodLight);
        Fill(texture, x + 5, y + 54, 44, 5, woodLight);
        DrawStraightSword(texture, x + 12, y + 28, true, metal, woodLight);
        DrawStraightSword(texture, x + 27, y + 28, true, metal, woodLight);
        Fill(texture, x + 40, y + 28, 5, 42, metalDark);
        Fill(texture, x + 36, y + 64, 13, 8, metal);
        Fill(texture, x + 37, y + 23, 11, 5, woodLight);
    }

    private static void DrawStraightSword(Texture2D texture, int x, int y, bool vertical, Color32 metal, Color32 wood)
    {
        if (vertical)
        {
            Fill(texture, x + 3, y + 9, 4, 34, metal);
            Fill(texture, x + 1, y + 43, 8, 6, metal);
            Fill(texture, x, y + 7, 10, 3, wood);
            Fill(texture, x + 4, y, 2, 8, wood);
            return;
        }

        Fill(texture, x + 10, y + 3, 36, 4, metal);
        Fill(texture, x + 46, y + 1, 7, 8, metal);
        Fill(texture, x + 7, y, 3, 10, wood);
        Fill(texture, x, y + 4, 8, 2, wood);
    }

    private static void DrawAxeIcon(Texture2D texture, int x, int y, Color32 metal, Color32 wood)
    {
        Fill(texture, x + 8, y, 4, 28, wood);
        Fill(texture, x + 2, y + 18, 18, 8, metal);
        Fill(texture, x + 16, y + 14, 6, 10, metal);
    }

    private static void DrawShield(Texture2D texture, int x, int y, Color32 cloth, Color32 metal)
    {
        Fill(texture, x, y + 5, 22, 18, metal);
        Fill(texture, x + 3, y + 3, 16, 22, cloth);
        Fill(texture, x + 7, y, 8, 5, cloth);
        DrawLine(texture, x + 11, y + 3, x + 11, y + 23, metal);
    }

    private static void DrawAnvil(Texture2D texture, int x, int y, Color32 metalDark, Color32 metal, Color32 woodDark)
    {
        Fill(texture, x, y + 16, 46, 10, metalDark);
        Fill(texture, x + 8, y + 8, 30, 10, metal);
        Fill(texture, x + 16, y, 14, 8, woodDark);
        Fill(texture, x + 2, y + 22, 13, 5, metal);
        Fill(texture, x + 31, y + 22, 13, 5, metal);
    }

    private static void DrawCrate(Texture2D texture, int x, int y, Color32 woodDark, Color32 woodLight)
    {
        Fill(texture, x, y, 28, 24, woodDark);
        Fill(texture, x + 3, y + 3, 22, 18, new Color32(105, 64, 38, 255));
        DrawLine(texture, x + 3, y + 12, x + 24, y + 12, woodLight);
        DrawLine(texture, x + 14, y + 3, x + 14, y + 20, woodLight);
    }

    private static void DrawBarrel(Texture2D texture, int x, int y, Color32 woodDark, Color32 woodLight)
    {
        Fill(texture, x + 4, y, 20, 28, woodDark);
        Fill(texture, x, y + 5, 28, 18, new Color32(113, 70, 39, 255));
        DrawLine(texture, x + 3, y + 8, x + 24, y + 8, woodLight);
        DrawLine(texture, x + 3, y + 20, x + 24, y + 20, woodLight);
        DrawLine(texture, x + 9, y + 3, x + 9, y + 25, woodDark);
        DrawLine(texture, x + 19, y + 3, x + 19, y + 25, woodDark);
    }

    private static void DrawOrePile(Texture2D texture, int x, int y, Color32 stoneDark, Color32 stoneMid, Color32 stoneLight)
    {
        Fill(texture, x + 4, y, 14, 10, stoneDark);
        Fill(texture, x + 17, y + 2, 18, 13, stoneMid);
        Fill(texture, x + 36, y + 1, 12, 9, stoneDark);
        Fill(texture, x + 9, y + 12, 16, 12, stoneLight);
        Fill(texture, x + 28, y + 14, 18, 10, stoneMid);
    }

    private static void DrawWeaponRack(Texture2D texture, int x, int y, bool leanRight, Color32 metal, Color32 wood)
    {
        Fill(texture, x, y, 6, 64, wood);
        Fill(texture, x + 38, y, 6, 64, wood);
        Fill(texture, x, y + 14, 44, 5, wood);
        Fill(texture, x, y + 40, 44, 5, wood);

        for (int i = 0; i < 3; i++)
        {
            int baseX = x + 10 + i * 11;
            Fill(texture, baseX, y + 12, 4, 38, metal);
            Fill(texture, baseX - 2, y + 50, 8, 6, metal);
            Fill(texture, baseX - 3, y + 8, 10, 4, wood);
        }
    }

    private static void DrawBrickPattern(Texture2D texture, int x, int y, int width, int height, Color32 fillColor, Color32 lineColor)
    {
        Fill(texture, x, y, width, height, fillColor);
        for (int row = 0; row < height; row += 8)
        {
            DrawLine(texture, x, y + row, x + width - 1, y + row, lineColor);
            int offset = (row / 8) % 2 == 0 ? 0 : 8;
            for (int col = offset; col < width; col += 16)
            {
                DrawLine(texture, x + col, y + row, x + col, Mathf.Min(y + row + 7, y + height - 1), lineColor);
            }
        }
    }

    private static void Fill(Texture2D texture, int x, int y, int width, int height, Color32 color)
    {
        for (int yy = Mathf.Max(0, y); yy < Mathf.Min(texture.height, y + height); yy++)
        {
            for (int xx = Mathf.Max(0, x); xx < Mathf.Min(texture.width, x + width); xx++)
            {
                texture.SetPixel(xx, yy, color);
            }
        }
    }

    private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color32 color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = -Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int error = dx + dy;

        while (true)
        {
            if (x0 >= 0 && x0 < texture.width && y0 >= 0 && y0 < texture.height)
            {
                texture.SetPixel(x0, y0, color);
            }

            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = 2 * error;
            if (e2 >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    private static Sprite LoadFirstSprite(string assetPath)
    {
        return LoadSprite(assetPath, 0);
    }

    private static Sprite LoadSprite(string assetPath, int index)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        int spriteIndex = 0;
        foreach (Object asset in assets)
        {
            if (asset is not Sprite sprite)
            {
                continue;
            }

            if (spriteIndex == index)
            {
                return sprite;
            }

            spriteIndex++;
        }

        Debug.LogWarning($"Day24WeaponShopOrganizer could not load sprite index {index}: {assetPath}");
        return null;
    }

    private static void OpenScene(string scenePath)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path == scenePath)
        {
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
        {
            EditorUtility.DisplayDialog("Day 24 Weapon Shop", $"Could not find scene at {scenePath}.", "OK");
            return;
        }

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }

    private static bool AddOrEnableScene(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path != scenePath)
            {
                continue;
            }

            if (!scenes[i].enabled)
            {
                scenes[i].enabled = true;
                EditorBuildSettings.scenes = scenes.ToArray();
                return true;
            }

            return false;
        }

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        return true;
    }

    private static Transform GetOrCreateGroup(string groupName, Transform parent)
    {
        GameObject existing = GameObject.Find(groupName);
        if (existing != null)
        {
            if (parent != null && existing.transform.parent != parent)
            {
                Undo.SetTransformParent(existing.transform, parent, $"Move {groupName}");
            }

            return existing.transform;
        }

        GameObject group = new GameObject(groupName);
        Undo.RegisterCreatedObjectUndo(group, $"Create {groupName}");
        if (parent != null)
        {
            Undo.SetTransformParent(group.transform, parent, $"Move {groupName}");
        }

        return group.transform;
    }

    private static GameObject FindOrCreateChild(Transform parent, string childName)
    {
        Transform existing = parent.Find(childName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject child = new GameObject(childName);
        Undo.RegisterCreatedObjectUndo(child, $"Create {childName}");
        Undo.SetTransformParent(child.transform, parent, $"Move {childName}");
        child.transform.localPosition = Vector3.zero;
        child.transform.localScale = Vector3.one;
        return child;
    }

    private static GameObject GetOrCreateMarker(string markerName, Transform parent, Vector3 position)
    {
        GameObject marker = GameObject.Find(markerName);
        if (marker == null)
        {
            marker = new GameObject(markerName);
            Undo.RegisterCreatedObjectUndo(marker, $"Create {markerName}");
        }
        else
        {
            Undo.RecordObject(marker.transform, $"Update {markerName}");
        }

        Undo.SetTransformParent(marker.transform, parent, $"Move {markerName}");
        marker.transform.position = position;
        EditorUtility.SetDirty(marker);
        return marker;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static void CreateCollider(Transform parent, string objectName, Vector3 position, Vector2 size)
    {
        GameObject colliderObject = new GameObject(objectName);
        Undo.RegisterCreatedObjectUndo(colliderObject, $"Create {objectName}");
        Undo.SetTransformParent(colliderObject.transform, parent, $"Move {objectName}");
        colliderObject.transform.position = position;

        BoxCollider2D collider = colliderObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = false;
        collider.offset = Vector2.zero;
        collider.size = size;
    }

    private static void ConfigureTrigger(GameObject target, Vector2 size)
    {
        BoxCollider2D collider = target.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = Undo.AddComponent<BoxCollider2D>(target);
        }
        else
        {
            Undo.RecordObject(collider, $"Update {target.name} trigger");
        }

        collider.isTrigger = true;
        collider.offset = Vector2.zero;
        collider.size = size;
        EditorUtility.SetDirty(collider);
    }

    private static void ConfigureTransition(GameObject target, string targetSceneName, string targetSpawnPointName)
    {
        SceneTransitionPoint transition = target.GetComponent<SceneTransitionPoint>();
        if (transition == null)
        {
            transition = Undo.AddComponent<SceneTransitionPoint>(target);
        }

        SerializedObject serializedTransition = new SerializedObject(transition);
        serializedTransition.FindProperty("targetSceneName").stringValue = targetSceneName;
        serializedTransition.FindProperty("targetSpawnPointName").stringValue = targetSpawnPointName;
        serializedTransition.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(transition);
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
}
