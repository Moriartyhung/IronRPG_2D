using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Day23MultiSpeakerDialogueOrganizer
{
    private const string TavernScenePath = "Assets/Scenes/TavernInteriorScene.unity";
    private const string ChiefScenePath = "Assets/Scenes/ChiefHouseInteriorScene.unity";
    private const string ReturnObjective = "回村長家，回報洞穴線索";
    private const string FirstChiefObjective = "前往酒館，詢問受傷的年輕人";
    private const string SecondChiefObjective = "前往武器鋪，詢問拳套與符文情報";

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

    private readonly struct DialogueSequenceConfig
    {
        public DialogueSequenceConfig(
            string sequenceName,
            MainQuestStage requiredStage,
            string defaultSpeakerName,
            string interactPrompt,
            DialogueLine[] lines,
            bool setObjectiveOnComplete,
            string objectiveTextOnComplete,
            bool advanceQuestStageOnComplete,
            MainQuestStage questStageOnComplete)
        {
            SequenceName = sequenceName;
            RequiredStage = requiredStage;
            DefaultSpeakerName = defaultSpeakerName;
            InteractPrompt = interactPrompt;
            Lines = lines;
            SetObjectiveOnComplete = setObjectiveOnComplete;
            ObjectiveTextOnComplete = objectiveTextOnComplete;
            AdvanceQuestStageOnComplete = advanceQuestStageOnComplete;
            QuestStageOnComplete = questStageOnComplete;
        }

        public string SequenceName { get; }
        public MainQuestStage RequiredStage { get; }
        public string DefaultSpeakerName { get; }
        public string InteractPrompt { get; }
        public DialogueLine[] Lines { get; }
        public bool SetObjectiveOnComplete { get; }
        public string ObjectiveTextOnComplete { get; }
        public bool AdvanceQuestStageOnComplete { get; }
        public MainQuestStage QuestStageOnComplete { get; }
    }

    private static readonly DialogueLine[] TavernOwnerLines =
    {
        new("酒館老闆", "哎，你來得正好。那幾個孩子從森林回來後，就一直坐在角落裡。"),
        new("主角", "傷得嚴重嗎？"),
        new("酒館老闆", "皮肉傷居多，但嚇得不輕。洛克那小子到現在杯子都拿不穩。"),
        new("主角", "村長讓我來問他們森林的事。"),
        new("酒館老闆", "那你可得有耐心。他們今天說話像嘴裡塞了濕布。"),
        new("主角", "我會試試。"),
        new("酒館老闆", "還有，別把桌子砸壞。上次你們比腕力，那張桌子到現在還歪著。"),
        new("主角", "那不是我砸的。"),
        new("酒館老闆", "所有砸桌子的人都這麼說。")
    };

    private static readonly DialogueLine[] AsenLines =
    {
        new("阿森", "喲，是你啊。村長派你來的？"),
        new("主角", "他讓我問問森林發生了什麼。"),
        new("阿森", "沒什麼好問的，就是倒霉。"),
        new("主角", "倒霉到整群小怪突然出現？"),
        new("阿森", "森林嘛，出現怪物很奇怪嗎？"),
        new("主角", "以前沒出現過這麼多。你今天說話很像在藏東西。"),
        new("阿森", "我藏什麼？我身上連兩枚金幣都沒有。"),
        new("主角", "我問的是情報，不是你的窮。"),
        new("阿森", "好吧，我們是比平常採集點再靠深處一點，但還沒真的進深林。"),
        new("主角", "阿森。"),
        new("阿森", "好吧，是米娜先看到那邊有光，像寶石，又像火，五顏六色地閃。"),
        new("主角", "然後怪物就出現了？"),
        new("阿森", "先是吼聲。很低，很沉，像從地底下滾上來。"),
        new("主角", "你們有看到帶頭的怪物嗎？"),
        new("阿森", "沒有。也可能看到了，但我們當時只顧著跑。"),
        new("主角", "真難得，你也會承認自己跑了。"),
        new("阿森", "會跑才能活著坐在這裡，被你審問。")
    };

    private static readonly DialogueLine[] LockeLines =
    {
        new("洛克", "我什麼都不知道。"),
        new("主角", "我還沒問。"),
        new("洛克", "那你不要問。"),
        new("主角", "你們去過洞穴門口，對吧？"),
        new("洛克", "誰說的？"),
        new("主角", "你們三個每個人都露了一點。"),
        new("洛克", "我就知道阿森那張嘴靠不住。"),
        new("主角", "所以是真的。"),
        new("洛克", "我們沒有進去！只是去洞穴門口打鬧。"),
        new("主角", "你們在禁地門口大喊大叫，這叫打鬧？"),
        new("洛克", "阿森說只要在洞口喊一聲，如果沒有東西回應，就證明村長只是嚇唬人。"),
        new("主角", "然後呢？"),
        new("洛克", "他喊了三聲。第三聲之後，洞裡傳來聲音。"),
        new("主角", "什麼聲音？"),
        new("洛克", "像很大的東西在翻身。"),
        new("主角", "所以村長不知道洞穴的事情？"),
        new("洛克", "不知道。你不能告訴他，他會殺了我們。"),
        new("主角", "不會。但他可能會讓你們修三個月柵欄。"),
        new("洛克", "那還不如殺了我。"),
        new("主角", "我會跟他說實情。"),
        new("洛克", "完了。")
    };

    private static readonly DialogueLine[] FirstChiefLines =
    {
        new("村長", "你回來了。今天比平常晚些。"),
        new("主角", "我在遺跡遇到了小怪。"),
        new("村長", "小怪？在遺跡？"),
        new("主角", "是。不是野獸，也不像普通迷路的魔物。"),
        new("村長", "你確定是在祭壇附近？"),
        new("主角", "確定。我剛祭拜完先祖，準備回村，它就從石牆後面鑽出來。"),
        new("村長", "……"),
        new("主角", "村長，你臉色不太好。"),
        new("村長", "因為你不是今天第一個帶來壞消息的人。"),
        new("主角", "還有誰遇到怪物？"),
        new("村長", "村裡幾個年輕人，今天早上去森林採集木材和藥草。結果他們在森林外圍遇到大量小怪。"),
        new("主角", "大量？"),
        new("村長", "不是一兩隻。是成群出現。"),
        new("主角", "他們受傷了？"),
        new("村長", "有幾個被抓傷，還有人摔斷了腿。幸好他們跑得快。"),
        new("主角", "森林外圍以前頂多有野狼和蛇，不會有小怪群。"),
        new("村長", "所以這十分不對勁。"),
        new("主角", "你覺得遺跡和森林的異常有關？"),
        new("村長", "我不敢斷定，但我不喜歡巧合。尤其是這種會傷人的巧合。"),
        new("主角", "需要我去森林看看？"),
        new("村長", "先不要急著進森林深處。"),
        new("主角", "為什麼？"),
        new("村長", "你需要更多情報。去酒館找那些回來的年輕人，他們現在都在那裡養傷和壓驚。"),
        new("主角", "你沒問過他們？"),
        new("村長", "問過，但他們說話吞吞吐吐。有人說看見寶光，有人說聽見吼聲，還有人一提到森林深處就低頭。"),
        new("主角", "聽起來他們隱瞞了什麼。"),
        new("村長", "所以我讓你去問。"),
        new("主角", "我去問，他們就會說？"),
        new("村長", "你跟他們年紀近。他們怕我，但未必怕你。"),
        new("主角", "這話聽起來不像誇獎。"),
        new("村長", "這不是誇獎，是任務。"),
        new("主角", "明白了。我去酒館。"),
        new("村長", "記住，先問清楚。不要自己一頭鑽進森林深處。"),
        new("主角", "我會小心。")
    };

    private static readonly DialogueLine[] ChiefReportLines =
    {
        new("村長", "你回來了。問出什麼了？"),
        new("主角", "森林深處有寶光，接著傳來巨吼，然後小怪從四面八方出現。"),
        new("村長", "寶光……巨吼……"),
        new("主角", "還有一件事。"),
        new("村長", "說。"),
        new("主角", "那些年輕人上個月去過洞穴門口。"),
        new("村長", "……你再說一次。"),
        new("主角", "他們沒有進洞穴，但在洞口打鬧，還朝裡面喊叫。"),
        new("村長", "胡鬧！"),
        new("主角", "他們說洞裡有回應，像是有巨大的東西在翻身。"),
        new("村長", "我早就說過，任何人不得靠近那座洞穴！"),
        new("主角", "他們怕你生氣，所以沒敢說。"),
        new("村長", "生氣？他們以為我只是生氣？"),
        new("村長", "那座洞穴不是用來考驗膽量的地方，不是年輕人展示愚蠢的舞台！"),
        new("主角", "村長，洞穴裡到底有什麼？"),
        new("村長", "……"),
        new("主角", "現在森林和遺跡都出現怪物。如果洞穴也有問題，我需要知道真相。"),
        new("村長", "你說得對。"),
        new("村長", "那座洞穴，是 Boss 的巢穴。"),
        new("主角", "Boss？"),
        new("村長", "很久以前，它從洞穴深處出現，襲擊周圍村莊。它燒毀糧倉，吞食牲畜，殺死外出的人。那時候，這附近沒有一個村子能安睡。"),
        new("主角", "我們的先祖也參與了討伐？"),
        new("村長", "不只是參與。你的先祖是召集者之一。"),
        new("主角", "我只聽過先祖守護村子的傳說。"),
        new("村長", "傳說總會把血腥的部分省略。真實情況是，附近幾個村子幾乎把所有能拿武器的人都派了出去。"),
        new("主角", "結果呢？"),
        new("村長", "他們重傷了 Boss，逼它退回洞穴。"),
        new("主角", "殺不死？"),
        new("村長", "殺不死。"),
        new("主角", "為什麼？"),
        new("村長", "據說那個 Boss 皮糙肉厚，就算是打磨過的利刃都無法傷其分毫。"),
        new("主角", "那先祖是怎麼重傷它的？"),
        new("村長", "沒人知道。那一戰活下來的人很少。留下來的紀錄也殘缺不全。"),
        new("主角", "所以你才禁止任何人靠近洞穴。"),
        new("村長", "因為有些危險不是勇敢就能解決。"),
        new("主角", "但現在它可能正在醒來。"),
        new("村長", "也可能已經被什麼東西驚動。"),
        new("主角", "那些年輕人？"),
        new("村長", "他們愚蠢，但未必有那麼大本事。"),
        new("主角", "森林深處的寶光呢？"),
        new("村長", "我不知道。但寶光、怪物、洞穴聲響，同時出現，絕不正常。"),
        new("主角", "如果普通刀劍傷不了 Boss，那我們需要能把力量導入拳掌的東西。"),
        new("村長", "傳下來的說法很少。只知道當年討伐隊靠的不是鐵刃，而是一種能引動雷電的古老符文。"),
        new("主角", "提到符文和鍛造，就不得不想到村口的那位武器匠。"),
        new("村長", "你是說鐵砧那個老頑固？"),
        new("主角", "他們家世世代代都在打造拳套和刻印符文。也許他知道些什麼。"),
        new("村長", "他說自己祖上連天都敢敲。那時我以為他只是喝多了。"),
        new("主角", "也許不是。"),
        new("村長", "去問問他。"),
        new("主角", "如果他知道什麼，我會回來告訴你。"),
        new("村長", "還有一件事。"),
        new("主角", "什麼？"),
        new("村長", "如果你真的要進森林，先活著回來。"),
        new("主角", "我會。"),
        new("村長", "這不是玩笑。"),
        new("主角", "我知道。")
    };

    [MenuItem("IronRPG/Day 23/1 Apply Tavern Multi Speaker Dialogues")]
    public static void ApplyTavernMultiSpeakerDialogues()
    {
        OpenScene(TavernScenePath);
        EnsureMainQuestState();

        int updatedCount = 0;
        updatedCount += ApplyDialogueSequences(
            "BackgroundCharacter_01_Bar",
            new[]
            {
                new DialogueSequenceConfig("酒館老闆開場", MainQuestStage.TavernInvestigation, "酒館老闆", "按 E 與酒館老闆交談", TavernOwnerLines, false, string.Empty, false, MainQuestStage.TavernInvestigation)
            });
        updatedCount += ApplyDialogueSequences(
            "BackgroundCharacter_02_Pillar",
            new[]
            {
                new DialogueSequenceConfig("與阿森對話", MainQuestStage.TavernInvestigation, "阿森", "按 E 與阿森交談", AsenLines, false, string.Empty, false, MainQuestStage.TavernInvestigation)
            });
        updatedCount += ApplyDialogueSequences(
            "BackgroundCharacter_03_LowerHall",
            new[]
            {
                new DialogueSequenceConfig("與洛克對話", MainQuestStage.TavernInvestigation, "洛克", "按 E 與洛克交談", LockeLines, true, ReturnObjective, true, MainQuestStage.ReturnToChief)
            });

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        GameObject selected = GameObject.Find("BackgroundCharacter_01_Bar");
        Selection.activeGameObject = selected;
        EditorGUIUtility.PingObject(selected);

        EditorUtility.DisplayDialog(
            "Day 23 Multi Speaker Dialogues",
            $"Updated {updatedCount} tavern dialogue interaction points.\n\nNormal article flow: fill the tavern character interactions with alternating NPC / player lines, and make the final Locke dialogue advance the quest stage back to the chief.",
            "OK");
    }

    [MenuItem("IronRPG/Day 23/2 Apply Chief Multi Speaker Dialogue")]
    public static void ApplyChiefMultiSpeakerDialogue()
    {
        OpenScene(ChiefScenePath);
        EnsureMainQuestState();

        int updatedCount = ApplyChiefDialogueFlow();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        GameObject selected = GameObject.Find("ChiefInteractionArea");
        Selection.activeGameObject = selected;
        EditorGUIUtility.PingObject(selected);

        EditorUtility.DisplayDialog(
            "Day 23 Chief Multi Speaker Dialogue",
            $"Updated {updatedCount} chief dialogue interaction point.\n\nNormal article flow: configure ChiefInteractionArea with quest-stage dialogue sequences. The first chief dialogue leads to the tavern; the report dialogue only appears after the tavern clues are collected.",
            "OK");
    }

    [MenuItem("IronRPG/Day 23/3 Apply Chief Report Dialogue")]
    public static void ApplyChiefReportDialogue()
    {
        OpenScene(ChiefScenePath);
        EnsureMainQuestState();

        int updatedCount = ApplyChiefDialogueFlow();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        GameObject selected = GameObject.Find("ChiefInteractionArea");
        Selection.activeGameObject = selected;
        EditorGUIUtility.PingObject(selected);

        EditorUtility.DisplayDialog(
            "Day 23 Chief Report Dialogue",
            $"Updated {updatedCount} chief dialogue interaction point.\n\nNormal article flow: after gathering tavern clues, return to ChiefHouseInteriorScene. ChiefInteractionArea will show the report dialogue only when the quest stage is ReturnToChief.",
            "OK");
    }

    [MenuItem("IronRPG/Day 23/4 Apply Quest Stage Dialogue Flow")]
    public static void ApplyQuestStageDialogueFlow()
    {
        ApplyChiefMultiSpeakerDialogue();
        ApplyTavernMultiSpeakerDialogues();
    }

    private static int ApplyDialogue(
        string objectName,
        string defaultSpeakerName,
        string interactPrompt,
        DialogueLine[] dialogueLines,
        bool setObjectiveOnComplete,
        string objectiveTextOnComplete)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
        {
            Debug.LogWarning($"Day23MultiSpeakerDialogueOrganizer could not find {objectName}.");
            return 0;
        }

        NpcDialogueInteraction interaction = target.GetComponent<NpcDialogueInteraction>();
        if (interaction == null)
        {
            interaction = Undo.AddComponent<NpcDialogueInteraction>(target);
        }

        SerializedObject serializedInteraction = new SerializedObject(interaction);
        serializedInteraction.FindProperty("speakerName").stringValue = defaultSpeakerName;
        serializedInteraction.FindProperty("interactPrompt").stringValue = interactPrompt;
        serializedInteraction.FindProperty("useSpeakerLines").boolValue = true;
        serializedInteraction.FindProperty("setObjectiveOnComplete").boolValue = setObjectiveOnComplete;
        serializedInteraction.FindProperty("objectiveTextOnComplete").stringValue = objectiveTextOnComplete;

        SerializedProperty speakerLines = serializedInteraction.FindProperty("speakerLines");
        speakerLines.arraySize = dialogueLines.Length;

        for (int i = 0; i < dialogueLines.Length; i++)
        {
            SerializedProperty entry = speakerLines.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("speakerName").stringValue = dialogueLines[i].SpeakerName;
            entry.FindPropertyRelative("text").stringValue = dialogueLines[i].Text;
        }

        serializedInteraction.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(interaction);
        return 1;
    }

    private static int ApplyChiefDialogueFlow()
    {
        return ApplyDialogueSequences(
            "ChiefInteractionArea",
            new[]
            {
                new DialogueSequenceConfig("第一次找村長", MainQuestStage.ReportToChief, "村長", "按 E 與村長交談", FirstChiefLines, true, FirstChiefObjective, true, MainQuestStage.TavernInvestigation),
                new DialogueSequenceConfig("回報洞穴線索", MainQuestStage.ReturnToChief, "村長", "按 E 向村長回報", ChiefReportLines, true, SecondChiefObjective, true, MainQuestStage.AskBlacksmith)
            });
    }

    private static int ApplyDialogueSequences(string objectName, DialogueSequenceConfig[] sequenceConfigs)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
        {
            Debug.LogWarning($"Day23MultiSpeakerDialogueOrganizer could not find {objectName}.");
            return 0;
        }

        NpcDialogueInteraction interaction = target.GetComponent<NpcDialogueInteraction>();
        if (interaction == null)
        {
            interaction = Undo.AddComponent<NpcDialogueInteraction>(target);
        }

        SerializedObject serializedInteraction = new SerializedObject(interaction);
        serializedInteraction.FindProperty("speakerName").stringValue = sequenceConfigs[0].DefaultSpeakerName;
        serializedInteraction.FindProperty("interactPrompt").stringValue = sequenceConfigs[0].InteractPrompt;
        serializedInteraction.FindProperty("useDialogueSequences").boolValue = true;

        SerializedProperty sequences = serializedInteraction.FindProperty("dialogueSequences");
        sequences.arraySize = sequenceConfigs.Length;

        for (int i = 0; i < sequenceConfigs.Length; i++)
        {
            DialogueSequenceConfig config = sequenceConfigs[i];
            SerializedProperty sequence = sequences.GetArrayElementAtIndex(i);
            sequence.FindPropertyRelative("sequenceName").stringValue = config.SequenceName;
            sequence.FindPropertyRelative("requiredQuestStage").enumValueIndex = (int)config.RequiredStage;
            sequence.FindPropertyRelative("speakerName").stringValue = config.DefaultSpeakerName;
            sequence.FindPropertyRelative("interactPrompt").stringValue = config.InteractPrompt;
            sequence.FindPropertyRelative("useSpeakerLines").boolValue = true;
            sequence.FindPropertyRelative("setObjectiveOnComplete").boolValue = config.SetObjectiveOnComplete;
            sequence.FindPropertyRelative("objectiveTextOnComplete").stringValue = config.ObjectiveTextOnComplete;
            sequence.FindPropertyRelative("advanceQuestStageOnComplete").boolValue = config.AdvanceQuestStageOnComplete;
            sequence.FindPropertyRelative("questStageOnComplete").enumValueIndex = (int)config.QuestStageOnComplete;

            SerializedProperty lines = sequence.FindPropertyRelative("speakerLines");
            lines.arraySize = config.Lines.Length;

            for (int lineIndex = 0; lineIndex < config.Lines.Length; lineIndex++)
            {
                SerializedProperty entry = lines.GetArrayElementAtIndex(lineIndex);
                entry.FindPropertyRelative("speakerName").stringValue = config.Lines[lineIndex].SpeakerName;
                entry.FindPropertyRelative("text").stringValue = config.Lines[lineIndex].Text;
            }
        }

        serializedInteraction.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(interaction);
        return 1;
    }

    private static void EnsureMainQuestState()
    {
        GameObject existing = GameObject.Find("MainQuestState");
        if (existing != null)
        {
            if (existing.GetComponent<MainQuestState>() == null)
            {
                existing.AddComponent<MainQuestState>();
            }

            return;
        }

        GameObject questObject = new GameObject("MainQuestState");
        Undo.RegisterCreatedObjectUndo(questObject, "Create MainQuestState");
        questObject.AddComponent<MainQuestState>();
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
            EditorUtility.DisplayDialog(
                "Day 23 Multi Speaker Dialogues",
                $"Could not find scene at {scenePath}.",
                "OK");
            return;
        }

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }
}
