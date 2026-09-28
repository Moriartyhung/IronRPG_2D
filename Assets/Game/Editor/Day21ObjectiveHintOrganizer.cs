using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Day21ObjectiveHintOrganizer
{
    private const string ChiefScenePath = "Assets/Scenes/ChiefHouseInteriorScene.unity";
    private const string ObjectiveText = "前往酒館，詢問受傷的年輕人";

    private static readonly string[] ChiefStoryDialogue =
    {
        "你回來了。今天比平常晚些。",
        "你不是今天第一個帶來壞消息的人。",
        "村裡幾個年輕人，今天早上去森林採集木材和藥草，結果在森林外圍遇到大量小怪。",
        "不是一兩隻，是成群出現。有人被抓傷，還有人摔斷了腿。",
        "我不敢斷定遺跡和森林的異常有關，但我不喜歡巧合。",
        "先不要急著進森林深處。你需要更多情報。",
        "去酒館找那些回來的年輕人，他們現在都在那裡養傷和壓驚。",
        "問清楚他們看見的寶光和聽見的吼聲，再回來向我報告。"
    };

    [MenuItem("IronRPG/Day 21/1 Add Current Objective Display")]
    public static void AddCurrentObjectiveDisplay()
    {
        OpenChiefScene();
        GameObject displayObject = EnsureCurrentObjectiveDisplay();
        SaveSceneAndSelect(displayObject);

        EditorUtility.DisplayDialog(
            "Day 21 Objective Display",
            "CurrentObjectiveDisplay has been added to ChiefHouseInteriorScene.\n\nNormal article flow: create an empty GameObject named CurrentObjectiveDisplay, add the CurrentObjectiveDisplay script, and keep the objective text empty until the chief dialogue finishes.",
            "OK");
    }

    [MenuItem("IronRPG/Day 21/2 Connect Chief Dialogue To Objective")]
    public static void ConnectChiefDialogueToObjective()
    {
        OpenChiefScene();
        EnsureCurrentObjectiveDisplay();

        GameObject interactionArea = GameObject.Find("ChiefInteractionArea");
        if (interactionArea == null)
        {
            EditorUtility.DisplayDialog(
                "Day 21 Objective",
                "ChiefInteractionArea was not found. Run Day 19/3 Setup Chief Dialogue Interaction first.",
                "OK");
            return;
        }

        NpcDialogueInteraction dialogue = interactionArea.GetComponent<NpcDialogueInteraction>();
        if (dialogue == null)
        {
            dialogue = interactionArea.AddComponent<NpcDialogueInteraction>();
        }

        SerializedObject serializedDialogue = new SerializedObject(dialogue);
        serializedDialogue.FindProperty("setObjectiveOnComplete").boolValue = true;
        serializedDialogue.FindProperty("objectiveTextOnComplete").stringValue = ObjectiveText;

        SerializedProperty lines = serializedDialogue.FindProperty("dialogueLines");
        lines.arraySize = ChiefStoryDialogue.Length;
        for (int i = 0; i < ChiefStoryDialogue.Length; i++)
        {
            lines.GetArrayElementAtIndex(i).stringValue = ChiefStoryDialogue[i];
        }

        serializedDialogue.ApplyModifiedPropertiesWithoutUndo();

        SaveSceneAndSelect(interactionArea);
        EditorUtility.DisplayDialog(
            "Day 21 Chief Objective",
            $"Chief dialogue now follows the story script and sets the objective when the final dialogue line is completed:\n{ObjectiveText}",
            "OK");
    }

    [MenuItem("IronRPG/Day 21/Open Chief House Interior Scene")]
    public static void OpenChiefHouseInteriorScene()
    {
        OpenChiefScene();
    }

    private static void OpenChiefScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path == ChiefScenePath)
        {
            return;
        }

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(ChiefScenePath, OpenSceneMode.Single);
    }

    private static GameObject EnsureCurrentObjectiveDisplay()
    {
        GameObject displayObject = FindOrCreateRoot("CurrentObjectiveDisplay");
        CurrentObjectiveDisplay display = displayObject.GetComponent<CurrentObjectiveDisplay>();
        if (display == null)
        {
            display = displayObject.AddComponent<CurrentObjectiveDisplay>();
        }

        SerializedObject serializedDisplay = new SerializedObject(display);
        serializedDisplay.FindProperty("currentObjective").stringValue = string.Empty;
        serializedDisplay.FindProperty("title").stringValue = "目前目標";
        serializedDisplay.FindProperty("fontSize").intValue = 18;
        serializedDisplay.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(displayObject);
        EditorUtility.SetDirty(display);

        return displayObject;
    }

    private static GameObject FindOrCreateRoot(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing != null)
        {
            return existing;
        }

        GameObject created = new GameObject(objectName);
        Undo.RegisterCreatedObjectUndo(created, $"Create {objectName}");
        return created;
    }

    private static void SaveSceneAndSelect(GameObject selectedObject)
    {
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Selection.activeGameObject = selectedObject;
        EditorGUIUtility.PingObject(selectedObject);
    }
}
