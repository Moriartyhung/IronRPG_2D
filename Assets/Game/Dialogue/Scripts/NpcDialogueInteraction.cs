using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class NpcDialogueInteraction : MonoBehaviour
{
    [System.Serializable]
    private class DialogueLineEntry
    {
        public string speakerName = "村長";
        [TextArea(2, 4)]
        public string text = string.Empty;
    }

    [System.Serializable]
    private class DialogueSequence
    {
        public string sequenceName = "Dialogue";
        public MainQuestStage requiredQuestStage = MainQuestStage.ReportToChief;
        public string speakerName = "村長";
        public string interactPrompt = "按 E 交談";
        public bool useSpeakerLines = true;
        public DialogueLineEntry[] speakerLines = new DialogueLineEntry[0];
        [TextArea(2, 4)]
        public string[] dialogueLines = new string[0];
        public bool setObjectiveOnComplete = false;
        public string objectiveTextOnComplete = string.Empty;
        public bool advanceQuestStageOnComplete = false;
        public MainQuestStage questStageOnComplete = MainQuestStage.TavernInvestigation;
    }

    [SerializeField] private string speakerName = "村長";
    [SerializeField] private string interactPrompt = "按 E 與村長交談";
    [Header("Quest Dialogue Sequences")]
    [SerializeField] private bool useDialogueSequences;
    [SerializeField] private DialogueSequence[] dialogueSequences;
    [Header("Multi Speaker Dialogue")]
    [SerializeField] private bool useSpeakerLines;
    [SerializeField] private DialogueLineEntry[] speakerLines;
    [Header("Legacy Dialogue")]
    [TextArea(2, 4)]
    [SerializeField] private string[] dialogueLines =
    {
        "你回來了。今天比平常晚些。",
        "你不是今天第一個帶來壞消息的人。",
        "村裡幾個年輕人，今天早上在森林外圍遇到大量小怪。",
        "他們現在都在酒館養傷和壓驚。",
        "先去酒館問清楚，不要自己一頭鑽進森林深處。"
    };
    [SerializeField] private int fontSize = 20;
    [SerializeField] private Color dialogueBackgroundColor = new Color(0f, 0f, 0f, 0.78f);
    [SerializeField] private Color speakerColor = new Color(1f, 0.86f, 0.45f, 1f);
    [Header("Objective")]
    [SerializeField] private bool setObjectiveOnComplete;
    [SerializeField] private string objectiveTextOnComplete = "前往酒館，詢問受傷的年輕人";

    private bool playerInRange;
    private bool dialogueOpen;
    private int lineIndex;
    private GUIStyle boxStyle;
    private GUIStyle speakerStyle;
    private GUIStyle lineStyle;
    private GUIStyle hintStyle;
    private Texture2D backgroundTexture;
    private DialogueSequence activeSequence;
    private DialogueSequence legendaryRuneSequence;
    private DialogueSequence postLegendaryRuneSequence;
    private DialogueSequence postBossBlacksmithSequence;
    private DialogueSequence bossDefeatedChiefSequence;
    private DialogueSequence postBossChiefSequence;
    private Collider2D interactionCollider;

    private void Awake()
    {
        interactionCollider = GetComponent<Collider2D>();
        interactionCollider.isTrigger = true;
    }

    private void Update()
    {
        RefreshPlayerInRange();
        if (!playerInRange)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (dialogueOpen && keyboard.escapeKey.wasPressedThisFrame)
        {
            CloseDialogue();
            return;
        }

        if (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
        {
            if (dialogueOpen)
            {
                AdvanceDialogue();
            }
            else
            {
                OpenDialogue();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other) && !IsPlayerNearInteractionArea())
        {
            playerInRange = false;
            CloseDialogue();
        }
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.GetComponent<PlayerMovement>() != null
            || other.GetComponentInParent<PlayerMovement>() != null
            || other.gameObject.name == "Player";
    }

    private void RefreshPlayerInRange()
    {
        if (IsPlayerNearInteractionArea())
        {
            playerInRange = true;
            return;
        }

        if (!dialogueOpen)
        {
            playerInRange = false;
        }
    }

    private bool IsPlayerNearInteractionArea()
    {
        Transform player = GetClosestPlayerTransform();
        if (player == null)
        {
            return false;
        }

        Vector2 playerPosition = player.position;
        if (interactionCollider != null)
        {
            Vector2 closestPoint = interactionCollider.ClosestPoint(playerPosition);
            if (Vector2.Distance(playerPosition, closestPoint) <= 0.6f)
            {
                return true;
            }

            float colliderRange = Mathf.Clamp(interactionCollider.bounds.extents.magnitude + 1f, 2.2f, 6f);
            return Vector2.Distance(transform.position, playerPosition) <= colliderRange;
        }

        return Vector2.Distance(transform.position, playerPosition) <= 2.2f;
    }

    private Transform GetClosestPlayerTransform()
    {
        Transform closestPlayer = null;
        float closestDistance = float.MaxValue;

        PlayerMovement[] playerMovements = FindObjectsByType<PlayerMovement>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < playerMovements.Length; i++)
        {
            CheckClosestPlayer(playerMovements[i].transform, ref closestPlayer, ref closestDistance);
        }

        GameObject[] taggedPlayers = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < taggedPlayers.Length; i++)
        {
            CheckClosestPlayer(taggedPlayers[i].transform, ref closestPlayer, ref closestDistance);
        }

        GameObject namedPlayer = GameObject.Find("Player");
        if (namedPlayer != null)
        {
            CheckClosestPlayer(namedPlayer.transform, ref closestPlayer, ref closestDistance);
        }

        return closestPlayer;
    }

    private void CheckClosestPlayer(Transform candidate, ref Transform closestPlayer, ref float closestDistance)
    {
        if (candidate == null || !candidate.gameObject.activeInHierarchy)
        {
            return;
        }

        Vector2 candidatePosition = candidate.position;
        Vector2 referencePoint = interactionCollider != null
            ? interactionCollider.ClosestPoint(candidatePosition)
            : (Vector2)transform.position;
        float distance = Vector2.Distance(candidatePosition, referencePoint);
        if (distance < closestDistance)
        {
            closestDistance = distance;
            closestPlayer = candidate;
        }
    }

    private void OpenDialogue()
    {
        activeSequence = GetActiveSequence();
        if (UsingDialogueSequences() && activeSequence == null)
        {
            return;
        }

        if (GetLineCount() == 0)
        {
            activeSequence = null;
            return;
        }

        dialogueOpen = true;
        lineIndex = 0;
    }

    private void AdvanceDialogue()
    {
        int lineCount = GetLineCount();
        if (lineCount == 0 || lineIndex >= lineCount - 1)
        {
            CompleteDialogue();
            return;
        }

        lineIndex++;
    }

    private void CompleteDialogue()
    {
        if (activeSequence != null)
        {
            if (activeSequence.setObjectiveOnComplete && !string.IsNullOrWhiteSpace(activeSequence.objectiveTextOnComplete))
            {
                CurrentObjectiveDisplay.SetObjective(activeSequence.objectiveTextOnComplete);
            }

            if (activeSequence.advanceQuestStageOnComplete)
            {
                MainQuestState.SetStage(activeSequence.questStageOnComplete);
            }
        }
        else if (setObjectiveOnComplete && !string.IsNullOrWhiteSpace(objectiveTextOnComplete))
        {
            CurrentObjectiveDisplay.SetObjective(objectiveTextOnComplete);
        }

        CloseDialogue();
    }

    private void CloseDialogue()
    {
        dialogueOpen = false;
        lineIndex = 0;
        activeSequence = null;
    }

    private void OnGUI()
    {
        if (BackpackUI.IsOpen)
        {
            return;
        }

        if (!playerInRange)
        {
            return;
        }

        EnsureStyles();

        if (!dialogueOpen)
        {
            DrawInteractPrompt();
            return;
        }

        float margin = Mathf.Max(24f, Screen.width * 0.08f);
        Rect boxRect = new Rect(margin, Screen.height - 172f, Screen.width - margin * 2f, 132f);
        GUI.Box(boxRect, GUIContent.none, boxStyle);

        Rect speakerRect = new Rect(boxRect.x + 22f, boxRect.y + 14f, boxRect.width - 44f, 28f);
        GUI.Label(speakerRect, GetCurrentSpeakerName(), speakerStyle);

        Rect lineRect = new Rect(boxRect.x + 22f, boxRect.y + 48f, boxRect.width - 44f, 48f);
        GUI.Label(lineRect, GetCurrentLineText(), lineStyle);

        Rect footerRect = new Rect(boxRect.x + 22f, boxRect.y + boxRect.height - 32f, boxRect.width - 44f, 24f);
        GUI.Label(footerRect, "E / Space 下一句，Esc 關閉", hintStyle);
    }

    private int GetLineCount()
    {
        if (activeSequence != null)
        {
            if (activeSequence.useSpeakerLines && activeSequence.speakerLines != null && activeSequence.speakerLines.Length > 0)
            {
                return activeSequence.speakerLines.Length;
            }

            return activeSequence.dialogueLines != null ? activeSequence.dialogueLines.Length : 0;
        }

        if (useSpeakerLines && speakerLines != null && speakerLines.Length > 0)
        {
            return speakerLines.Length;
        }

        return dialogueLines != null ? dialogueLines.Length : 0;
    }

    private string GetCurrentSpeakerName()
    {
        if (activeSequence != null)
        {
            if (activeSequence.useSpeakerLines && activeSequence.speakerLines != null && activeSequence.speakerLines.Length > 0)
            {
                DialogueLineEntry entry = activeSequence.speakerLines[Mathf.Clamp(lineIndex, 0, activeSequence.speakerLines.Length - 1)];
                return !string.IsNullOrWhiteSpace(entry.speakerName) ? entry.speakerName : activeSequence.speakerName;
            }

            return activeSequence.speakerName;
        }

        if (useSpeakerLines && speakerLines != null && speakerLines.Length > 0)
        {
            DialogueLineEntry entry = speakerLines[Mathf.Clamp(lineIndex, 0, speakerLines.Length - 1)];
            return !string.IsNullOrWhiteSpace(entry.speakerName) ? entry.speakerName : speakerName;
        }

        return speakerName;
    }

    private string GetCurrentLineText()
    {
        if (activeSequence != null)
        {
            if (activeSequence.useSpeakerLines && activeSequence.speakerLines != null && activeSequence.speakerLines.Length > 0)
            {
                DialogueLineEntry entry = activeSequence.speakerLines[Mathf.Clamp(lineIndex, 0, activeSequence.speakerLines.Length - 1)];
                return entry.text ?? string.Empty;
            }

            if (activeSequence.dialogueLines != null && activeSequence.dialogueLines.Length > 0)
            {
                return activeSequence.dialogueLines[Mathf.Clamp(lineIndex, 0, activeSequence.dialogueLines.Length - 1)];
            }

            return string.Empty;
        }

        if (useSpeakerLines && speakerLines != null && speakerLines.Length > 0)
        {
            DialogueLineEntry entry = speakerLines[Mathf.Clamp(lineIndex, 0, speakerLines.Length - 1)];
            return entry.text ?? string.Empty;
        }

        if (dialogueLines != null && dialogueLines.Length > 0)
        {
            return dialogueLines[Mathf.Clamp(lineIndex, 0, dialogueLines.Length - 1)];
        }

        return string.Empty;
    }

    private string GetCurrentInteractPrompt()
    {
        DialogueSequence sequence = GetActiveSequence();
        if (UsingDialogueSequences())
        {
            return sequence != null ? sequence.interactPrompt : string.Empty;
        }

        return interactPrompt;
    }

    private DialogueSequence GetActiveSequence()
    {
        DialogueSequence bossResolutionSequence = GetRuntimeBossResolutionChiefSequence();
        if (bossResolutionSequence != null)
        {
            return bossResolutionSequence;
        }

        DialogueSequence runtimeSequence = GetRuntimeLegendaryRuneSequence();
        if (runtimeSequence != null)
        {
            return runtimeSequence;
        }

        if (ShouldBlockPreForestDialogue())
        {
            return null;
        }

        if (!UsingDialogueSequences())
        {
            return null;
        }

        MainQuestStage currentStage = MainQuestState.CurrentStage;
        for (int i = 0; i < dialogueSequences.Length; i++)
        {
            DialogueSequence sequence = dialogueSequences[i];
            if (sequence != null && sequence.requiredQuestStage == currentStage)
            {
                return sequence;
            }
        }

        if (dialogueSequences.Length == 1 && dialogueSequences[0] != null && !ShouldSuppressSingleSequenceFallback())
        {
            return dialogueSequences[0];
        }

        return null;
    }

    private DialogueSequence GetRuntimeBossResolutionChiefSequence()
    {
        if (!IsChiefDialogue())
        {
            return null;
        }

        MainQuestStage currentStage = MainQuestState.CurrentStage;
        if (currentStage == MainQuestStage.BossDefeated)
        {
            if (bossDefeatedChiefSequence != null)
            {
                return bossDefeatedChiefSequence;
            }

            bossDefeatedChiefSequence = new DialogueSequence
            {
                sequenceName = "Boss戰後回報村長",
                requiredQuestStage = MainQuestStage.BossDefeated,
                speakerName = "村長",
                interactPrompt = "按 E 向村長回報 Boss 已被擊敗",
                useSpeakerLines = true,
                setObjectiveOnComplete = true,
                objectiveTextOnComplete = "村子暫時安全了，主線完成",
                advanceQuestStageOnComplete = true,
                questStageOnComplete = MainQuestStage.BossReported,
                speakerLines = new[]
                {
                    new DialogueLineEntry { speakerName = "村長", text = "你回來了。" },
                    new DialogueLineEntry { speakerName = "主角", text = "Boss 已經死了。" },
                    new DialogueLineEntry { speakerName = "村長", text = "……先祖們若能看見，應該會安心。" },
                    new DialogueLineEntry { speakerName = "主角", text = "這次不是只有先祖。酒館那些年輕人提供了線索，武器匠也幫我確認了雷痕符文。" },
                    new DialogueLineEntry { speakerName = "村長", text = "你倒是學會替他們說話了。" },
                    new DialogueLineEntry { speakerName = "主角", text = "他們還是該修柵欄。" },
                    new DialogueLineEntry { speakerName = "村長", text = "至少三個月。" },
                    new DialogueLineEntry { speakerName = "主角", text = "洛克會哭。" },
                    new DialogueLineEntry { speakerName = "村長", text = "那就讓他哭著修。" },
                    new DialogueLineEntry { speakerName = "村長", text = "我年輕時，以為先祖的故事只是提醒我們不要靠近洞穴。" },
                    new DialogueLineEntry { speakerName = "主角", text = "現在呢？" },
                    new DialogueLineEntry { speakerName = "村長", text = "現在我覺得，那也是提醒我們，不要把所有責任都留給死去的人。" },
                    new DialogueLineEntry { speakerName = "主角", text = "你之前也說過，這次是活著的人做選擇。" },
                    new DialogueLineEntry { speakerName = "村長", text = "村子暫時安全了。森林的小怪失去控制者後，數量應該會慢慢減少，但還需要巡邏。" },
                    new DialogueLineEntry { speakerName = "主角", text = "我知道。" },
                    new DialogueLineEntry { speakerName = "村長", text = "很好。那明天開始巡邏。" },
                    new DialogueLineEntry { speakerName = "主角", text = "……我就知道。" },
                    new DialogueLineEntry { speakerName = "村長", text = "英雄也要工作。" }
                }
            };

            return bossDefeatedChiefSequence;
        }

        if (currentStage == MainQuestStage.BossReported)
        {
            if (postBossChiefSequence != null)
            {
                return postBossChiefSequence;
            }

            postBossChiefSequence = new DialogueSequence
            {
                sequenceName = "Boss戰後村長閒談",
                requiredQuestStage = MainQuestStage.BossReported,
                speakerName = "村長",
                interactPrompt = "按 E 與村長交談",
                useSpeakerLines = true,
                setObjectiveOnComplete = true,
                objectiveTextOnComplete = "村子暫時安全了，主線完成",
                advanceQuestStageOnComplete = false,
                speakerLines = new[]
                {
                    new DialogueLineEntry { speakerName = "村長", text = "洞穴的事暫時告一段落，但村子還是需要人巡邏。" },
                    new DialogueLineEntry { speakerName = "主角", text = "我才剛回來。" },
                    new DialogueLineEntry { speakerName = "村長", text = "所以今天休息，明天開始。" },
                    new DialogueLineEntry { speakerName = "主角", text = "英雄也要工作，對吧？" },
                    new DialogueLineEntry { speakerName = "村長", text = "你記得就好。" }
                }
            };

            return postBossChiefSequence;
        }

        return null;
    }

    private DialogueSequence GetRuntimeLegendaryRuneSequence()
    {
        MainQuestStage currentStage = MainQuestState.CurrentStage;
        if (!IsBlacksmithDialogue() || !PlayerHasLegendaryRune())
        {
            return null;
        }

        if ((int)currentStage >= (int)MainQuestStage.BossDefeated)
        {
            return GetPostBossBlacksmithSequence();
        }

        if (currentStage == MainQuestStage.PrepareForCave)
        {
            CurrentObjectiveDisplay.SetObjective("前往洞穴，準備挑戰最終 Boss");
            return GetPostLegendaryRuneSequence();
        }

        SyncLegendaryRuneReturnState(currentStage);

        if (legendaryRuneSequence != null)
        {
            return legendaryRuneSequence;
        }

        legendaryRuneSequence = new DialogueSequence
        {
            sequenceName = "帶雷痕符文回武器匠",
            requiredQuestStage = MainQuestStage.ForestInvestigation,
            speakerName = "武器匠",
            interactPrompt = "按 E 讓武器匠確認雷痕符文",
            useSpeakerLines = true,
            setObjectiveOnComplete = true,
            objectiveTextOnComplete = "前往洞穴，準備挑戰最終 Boss",
            advanceQuestStageOnComplete = true,
            questStageOnComplete = MainQuestStage.PrepareForCave,
            speakerLines = new[]
            {
                new DialogueLineEntry { speakerName = "武器匠", text = "……等一下。你身上那股焦味，不是普通火燒出來的。" },
                new DialogueLineEntry { speakerName = "主角", text = "我在森林深處打倒了精英怪，寶箱裡有這枚符文。" },
                new DialogueLineEntry { speakerName = "武器匠", text = "雷痕符文。它真的重新選了主人。" },
                new DialogueLineEntry { speakerName = "主角", text = "所以它能傷到洞穴裡的 Boss？" },
                new DialogueLineEntry { speakerName = "武器匠", text = "能，但你別把它當普通增傷符文。它對那種厚甲魔皮有特別強的撕裂效果。" },
                new DialogueLineEntry { speakerName = "武器匠", text = "記住：赤鋒符文能幫你打倒精英怪，雷痕符文才是面對洞穴 Boss 的鑰匙。" },
                new DialogueLineEntry { speakerName = "主角", text = "我會帶著它去洞穴。" },
                new DialogueLineEntry { speakerName = "武器匠", text = "活著回來。我的店裡可不收英雄遺物，只收金幣。" }
            }
        };

        return legendaryRuneSequence;
    }

    private DialogueSequence GetPostLegendaryRuneSequence()
    {
        if (postLegendaryRuneSequence != null)
        {
            return postLegendaryRuneSequence;
        }

        postLegendaryRuneSequence = new DialogueSequence
        {
            sequenceName = "雷痕符文確認後",
            requiredQuestStage = MainQuestStage.PrepareForCave,
            speakerName = "武器匠",
            interactPrompt = "按 E 與武器匠交談",
            useSpeakerLines = true,
            setObjectiveOnComplete = true,
            objectiveTextOnComplete = "前往洞穴，準備挑戰最終 Boss",
            advanceQuestStageOnComplete = false,
            speakerLines = new[]
            {
                new DialogueLineEntry { speakerName = "武器匠", text = "雷痕符文已經確認過了。別再回森林繞圈，下一步是洞穴。" },
                new DialogueLineEntry { speakerName = "主角", text = "我會準備好再出發。" },
                new DialogueLineEntry { speakerName = "武器匠", text = "帶好藥草。那地方可不會因為你是新手就手下留情。" }
            }
        };

        return postLegendaryRuneSequence;
    }

    private DialogueSequence GetPostBossBlacksmithSequence()
    {
        if (postBossBlacksmithSequence != null)
        {
            return postBossBlacksmithSequence;
        }

        postBossBlacksmithSequence = new DialogueSequence
        {
            sequenceName = "Boss戰後武器匠閒談",
            requiredQuestStage = MainQuestStage.BossDefeated,
            speakerName = "武器匠",
            interactPrompt = "按 E 與武器匠交談",
            useSpeakerLines = true,
            setObjectiveOnComplete = false,
            advanceQuestStageOnComplete = false,
            speakerLines = new[]
            {
                new DialogueLineEntry { speakerName = "武器匠", text = "回來了？看來雷痕符文沒有白白認主。" },
                new DialogueLineEntry { speakerName = "主角", text = "Boss 已經倒下了。" },
                new DialogueLineEntry { speakerName = "武器匠", text = "那我祖先欠它的記錄，總算能寫完了。" },
                new DialogueLineEntry { speakerName = "主角", text = "你不是只收金幣嗎？" },
                new DialogueLineEntry { speakerName = "武器匠", text = "歷史也收。只是不能拿來付帳。" }
            }
        };

        return postBossBlacksmithSequence;
    }

    private static void SyncLegendaryRuneReturnState(MainQuestStage currentStage)
    {
        if ((int)currentStage <= (int)MainQuestStage.AskBlacksmith)
        {
            MainQuestState.SetStage(MainQuestStage.ForestInvestigation);
        }

        CurrentObjectiveDisplay.SetObjective("回到武器鋪，請武器匠確認雷痕符文");
    }

    private bool ShouldBlockPreForestDialogue()
    {
        if (!UsingDialogueSequences() || !PlayerHasLegendaryRune())
        {
            return false;
        }

        MainQuestStage currentStage = MainQuestState.CurrentStage;
        return (int)currentStage <= (int)MainQuestStage.AskBlacksmith;
    }

    private bool ShouldSuppressSingleSequenceFallback()
    {
        return IsBlacksmithDialogue() && PlayerHasLegendaryRune();
    }

    private static bool PlayerHasLegendaryRune()
    {
        PlayerInventory inventory = PlayerInventory.Ensure();
        return inventory != null && inventory.HasLegendaryRune();
    }

    private bool IsBlacksmithDialogue()
    {
        return speakerName.Contains("武器匠");
    }

    private bool IsChiefDialogue()
    {
        return speakerName.Contains("村長");
    }

    private void DrawInteractPrompt()
    {
        string prompt = GetCurrentInteractPrompt();
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return;
        }

        float width = Mathf.Min(360f, Screen.width - 40f);
        Rect promptRect = new Rect((Screen.width - width) * 0.5f, Screen.height - 82f, width, 42f);
        GUI.Box(promptRect, prompt, hintStyle);
    }

    private bool UsingDialogueSequences()
    {
        return useDialogueSequences && dialogueSequences != null && dialogueSequences.Length > 0;
    }

    private void EnsureStyles()
    {
        if (backgroundTexture == null)
        {
            backgroundTexture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            backgroundTexture.SetPixel(0, 0, dialogueBackgroundColor);
            backgroundTexture.Apply();
        }

        boxStyle ??= new GUIStyle(GUI.skin.box)
        {
            normal = { background = backgroundTexture },
            padding = new RectOffset(18, 18, 14, 14)
        };

        speakerStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = speakerColor }
        };

        lineStyle ??= new GUIStyle(GUI.skin.label)
        {
            wordWrap = true,
            normal = { textColor = Color.white }
        };

        hintStyle ??= new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,
            normal = { textColor = Color.white }
        };

        speakerStyle.fontSize = fontSize;
        lineStyle.fontSize = fontSize;
        hintStyle.fontSize = Mathf.Max(14, fontSize - 4);
    }

    private void OnDestroy()
    {
        if (backgroundTexture != null)
        {
            Destroy(backgroundTexture);
        }
    }
}

public enum MainQuestStage
{
    ReportToChief = 0,
    TavernInvestigation = 1,
    ReturnToChief = 2,
    AskBlacksmith = 3,
    ForestInvestigation = 4,
    PrepareForCave = 5,
    BossDefeated = 6,
    BossReported = 7
}

[DisallowMultipleComponent]
public class MainQuestState : MonoBehaviour
{
    [SerializeField] private MainQuestStage currentStage = MainQuestStage.ReportToChief;

    private static MainQuestState instance;

    public static MainQuestStage CurrentStage
    {
        get
        {
            EnsureInstance();
            return instance.currentStage;
        }
    }

    public static void SetStage(MainQuestStage stage)
    {
        EnsureInstance();
        instance.currentStage = stage;
    }

    public static void ResetForNewGame()
    {
        SetStage(MainQuestStage.ReportToChief);
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        MainQuestState existing = FindAnyObjectByType<MainQuestState>();
        if (existing != null)
        {
            instance = existing;
            return;
        }

        GameObject questObject = new GameObject("MainQuestState");
        instance = questObject.AddComponent<MainQuestState>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
