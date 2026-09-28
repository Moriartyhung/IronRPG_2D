using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Day28BossAttackOrganizer
{
    private const string CaveScenePath = "Assets/Scenes/CaveScene.unity";
    private const string ToCaveScenePath = "Assets/Scenes/ToCaveScene.unity";
    private const string StabSpritePath = "Assets/Game/Generated/Day28/BossAttacks/boss_vine_stab.png";
    private const string SlamSpritePath = "Assets/Game/Generated/Day28/BossAttacks/boss_vine_slam.png";
    private const string SweepSpritePath = "Assets/Game/Generated/Day28/BossAttacks/boss_vine_sweep.png";
    private const string CaveBossExitName = "Exit";
    private const string ToCaveBossReturnSpawnName = "Exitformcave";
    private const string ToCaveBossReturnBlockerName = "special box";
    private const string ToRuinsMarkerName = "toruins";

    [MenuItem("IronRPG/Day 28/1 Setup Boss Attack System")]
    public static void SetupBossAttackSystem()
    {
        ConfigureSpriteImporter(StabSpritePath);
        ConfigureSpriteImporter(SlamSpritePath);
        ConfigureSpriteImporter(SweepSpritePath);
        AssetDatabase.Refresh();

        OpenScene(CaveScenePath);

        BossCoreBattleController battleController = Object.FindAnyObjectByType<BossCoreBattleController>();
        if (battleController == null)
        {
            EditorUtility.DisplayDialog("Day 28 Boss Attack", "找不到 BossCoreBattleController，請確認目前是 CaveScene。", "OK");
            return;
        }

        BossVineAttackController attackController = battleController.GetComponent<BossVineAttackController>();
        if (attackController == null)
        {
            attackController = Undo.AddComponent<BossVineAttackController>(battleController.gameObject);
        }

        Sprite stabSprite = AssetDatabase.LoadAssetAtPath<Sprite>(StabSpritePath);
        Sprite slamSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SlamSpritePath);
        Sprite sweepSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SweepSpritePath);

        BossCoreNode[] nodes = Object.FindObjectsByType<BossCoreNode>(FindObjectsInactive.Include);
        Vector2 bossOrigin = EstimateBossOrigin(nodes);
        Vector2 sweepCenter = EstimateSweepCenter(nodes, bossOrigin);
        float sweepWidth = EstimateSweepWidth(nodes);

        SerializedObject serializedAttack = new SerializedObject(attackController);
        serializedAttack.FindProperty("battleController").objectReferenceValue = battleController;
        serializedAttack.FindProperty("stabSprite").objectReferenceValue = stabSprite;
        serializedAttack.FindProperty("slamSprite").objectReferenceValue = slamSprite;
        serializedAttack.FindProperty("sweepSprite").objectReferenceValue = sweepSprite;
        serializedAttack.FindProperty("bossOrigin").vector2Value = bossOrigin;
        serializedAttack.FindProperty("sweepCenter").vector2Value = sweepCenter;
        serializedAttack.FindProperty("sweepWidth").floatValue = sweepWidth;
        serializedAttack.FindProperty("stabDamage").intValue = 70;
        serializedAttack.FindProperty("stabDamageInterval").floatValue = 0.5f;
        serializedAttack.FindProperty("stabMinCooldown").floatValue = 0.35f;
        serializedAttack.FindProperty("stabMaxCooldown").floatValue = 0.65f;
        serializedAttack.FindProperty("slamDamage").intValue = 150;
        serializedAttack.FindProperty("slamSize").vector2Value = new Vector2(4.2f, 12f);
        serializedAttack.FindProperty("sweepDamage").intValue = 300;
        serializedAttack.FindProperty("sweepHeight").floatValue = 2.1f;
        serializedAttack.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(attackController);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        Selection.activeObject = attackController.gameObject;
        EditorUtility.DisplayDialog(
            "Day 28 Boss Attack",
            "Boss 攻擊系統已掛上 CaveScene。\n\n對話結束後開始攻擊：普通直刺會持續進行，70 傷害且 0.5 秒一跳；重砸藤蔓會放大 X 軸並覆蓋 Boss 場景 Y 軸；橫掃改為直立藤蔓左右雨刷式掃擊，造成 300 傷害。工具只新增/更新 BossVineAttackController，不會修改你調好的洞穴碰撞箱與觸發器大小。",
            "OK");
    }

    [MenuItem("IronRPG/Day 28/2 Setup Boss Defeat Exit")]
    public static void SetupBossDefeatExit()
    {
        OpenScene(ToCaveScenePath);
        bool hasReturnSpawn = GameObject.Find(ToCaveBossReturnSpawnName) != null;
        bool hasBlocker = GameObject.Find(ToCaveBossReturnBlockerName) != null;
        bool hasToRuinsMarker = GameObject.Find(ToRuinsMarkerName) != null;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        OpenScene(CaveScenePath);
        GameObject exit = GameObject.Find(CaveBossExitName);
        if (exit == null)
        {
            EditorUtility.DisplayDialog(
                "Day 28 Boss Exit",
                "找不到 CaveScene 裡名為 Exit 的物件。請先確認你放的出口物件名稱是 Exit。",
                "OK");
            return;
        }

        ConfigureBossExitTransition(exit);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        Selection.activeObject = exit;
        EditorUtility.DisplayDialog(
            "Day 28 Boss Exit",
            $"Boss 戰後出口已設定。\n\nCaveScene 的 Exit 會在 Boss 被擊敗後啟用轉場，目標場景是 ToCaveScene，出生點是 Exitformcave。抵達後會隱藏 special box，讓回到遺跡方向的小路打開。\n\n檢查結果：Exitformcave {(hasReturnSpawn ? "已存在" : "不存在")}，special box {(hasBlocker ? "已存在" : "不存在")}，toruins {(hasToRuinsMarker ? "已存在" : "不存在")}。",
            "OK");
    }

    private static void ConfigureSpriteImporter(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 64f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }

    private static void OpenScene(string scenePath)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path == scenePath)
        {
            return;
        }

        if (activeScene.isDirty)
        {
            EditorSceneManager.SaveScene(activeScene);
        }

        EditorSceneManager.OpenScene(scenePath);
    }

    private static void ConfigureBossExitTransition(GameObject exit)
    {
        BoxCollider2D trigger = exit.GetComponent<BoxCollider2D>();
        if (trigger == null)
        {
            trigger = Undo.AddComponent<BoxCollider2D>(exit);
            trigger.size = new Vector2(1.2f, 1.2f);
        }

        trigger.isTrigger = true;

        SceneTransitionPoint transition = exit.GetComponent<SceneTransitionPoint>();
        if (transition == null)
        {
            transition = Undo.AddComponent<SceneTransitionPoint>(exit);
        }

        SerializedObject serializedTransition = new SerializedObject(transition);
        serializedTransition.FindProperty("targetSceneName").stringValue = "ToCaveScene";
        serializedTransition.FindProperty("targetSpawnPointName").stringValue = ToCaveBossReturnSpawnName;
        serializedTransition.FindProperty("requireBossDefeatedBeforeUse").boolValue = true;
        serializedTransition.FindProperty("sceneObjectToDisableAfterArrival").stringValue = ToCaveBossReturnBlockerName;
        serializedTransition.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(exit);
        EditorUtility.SetDirty(trigger);
        EditorUtility.SetDirty(transition);
    }

    private static Vector2 EstimateBossOrigin(BossCoreNode[] nodes)
    {
        if (nodes == null || nodes.Length == 0)
        {
            return new Vector2(17.1f, 8.6f);
        }

        Bounds bounds = BuildBounds(nodes);
        return new Vector2(bounds.center.x, bounds.max.y + 1.8f);
    }

    private static Vector2 EstimateSweepCenter(BossCoreNode[] nodes, Vector2 bossOrigin)
    {
        if (nodes == null || nodes.Length == 0)
        {
            return bossOrigin + Vector2.down * 5.8f;
        }

        Bounds bounds = BuildBounds(nodes);
        return new Vector2(bounds.center.x, bounds.min.y - 3.7f);
    }

    private static float EstimateSweepWidth(BossCoreNode[] nodes)
    {
        if (nodes == null || nodes.Length == 0)
        {
            return 19f;
        }

        Bounds bounds = BuildBounds(nodes);
        return Mathf.Max(19f, bounds.size.x + 10f);
    }

    private static Bounds BuildBounds(BossCoreNode[] nodes)
    {
        Bounds bounds = new Bounds(nodes[0].transform.position, Vector3.zero);
        for (int i = 1; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                bounds.Encapsulate(nodes[i].transform.position);
            }
        }

        return bounds;
    }
}
