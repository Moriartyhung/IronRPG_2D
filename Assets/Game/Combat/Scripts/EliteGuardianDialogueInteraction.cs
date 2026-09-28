using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class EliteGuardianDialogueInteraction : MonoBehaviour
{
    [SerializeField] private EliteGuardianCombat guardian;
    [SerializeField] private string interactPrompt = "按 E 與精英怪對峙";
    [SerializeField] private string[] speakers =
    {
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪",
        "主角",
        "精英怪"
    };
    [TextArea(2, 4)]
    [SerializeField] private string[] dialogueLines =
    {
        "停下。",
        "你會說話？",
        "我還會殺人。你要先聽哪一個？",
        "我只想知道你守著那個箱子做什麼。",
        "人類總是這樣。看見光，就以為裡面的東西該屬於自己。",
        "森林裡的小怪是你派出去的？",
        "是。",
        "村民受傷也是因為你？",
        "弱者闖進狩獵場，被咬傷很奇怪嗎？",
        "這片森林不是你的。",
        "很快就是了。",
        "你是洞穴 Boss 的手下？",
        "曾經是。",
        "曾經？",
        "那頭老怪物躲在洞裡太久了。它曾經讓所有東西跪下，但現在，它只是一具靠恐懼支撐的殘軀。",
        "所以你背叛它。",
        "我稱之為取代。",
        "森林異常是你造成的？",
        "我只是拿回本該屬於強者的力量。",
        "你說的是雷痕符文？",
        "你知道它？",
        "聽說過。那是能把雷導入拳掌的古代符文。",
        "它原本封在洞穴深處，像一枚可笑的釘子，壓著那頭老怪物的力量。",
        "你拔走了符文。",
        "我拔走了它。洞穴因此震動，那老怪物因此醒來。",
        "所以不是那些年輕人驚醒 Boss。",
        "年輕人？那些在洞口尖叫的幼崽？",
        "你知道他們？",
        "他們吵醒不了真正的災厄。但他們讓我知道，人類又開始靠近禁地。",
        "你拔走符文後，Boss 追殺你？",
        "它反應很快。比我希望的快。",
        "你的手下呢？",
        "死了一半。",
        "剩下的呢？",
        "現在歸我。",
        "包括森林裡的小怪？",
        "它們害怕我，所以服從我。",
        "你得到雷痕符文了？",
        "我拿到了它的箱子，卻還沒有得到符文的承認。",
        "它拒絕了你。",
        "它會屈服。",
        "聽起來不像。",
        "閉嘴。",
        "你派小怪阻擋其他人靠近，是怕有人比你先被它承認？",
        "我是防止蠢貨送死。",
        "真體貼。",
        "你很強。比那些哭喊著逃跑的人類強。",
        "你想說什麼？",
        "臣服我。",
        "……",
        "跟我一起推翻洞穴裡那頭老怪物。等我殺了它，我會成為新的王。到時候，我可以讓你活著。",
        "聽起來條件不錯。只是有一點。",
        "什麼？",
        "我不喜歡當怪物的手下。",
        "那就用血證明你的骨氣。",
        "我會先把你打倒，再看雷痕符文願不願意理我。",
        "那就來。"
    };
    [SerializeField] private bool pauseEnemiesWhileOpen = true;
    [SerializeField] private float enemyResumeDelay = 0.75f;
    [SerializeField] private float playerInvincibleAfterClose = 1.2f;
    [SerializeField] private int fontSize = 20;
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.8f);
    [SerializeField] private Color speakerColor = new Color(1f, 0.68f, 0.38f, 1f);

    private bool playerInRange;
    private bool dialogueOpen;
    private bool dialogueCompleted;
    private bool holdingEnemyPause;
    private int lineIndex;
    private GUIStyle panelStyle;
    private GUIStyle speakerStyle;
    private GUIStyle lineStyle;
    private GUIStyle hintStyle;
    private Texture2D panelTexture;

    public bool DialogueCompleted => dialogueCompleted;

    private void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;

        if (guardian == null)
        {
            guardian = GetComponentInParent<EliteGuardianCombat>();
        }
    }

    private void Update()
    {
        if (!playerInRange || dialogueCompleted)
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
            CloseDialogue(false);
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
        if (IsPlayer(other))
        {
            playerInRange = false;
            CloseDialogue(false);
        }
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.GetComponent<PlayerMovement>() != null
            || other.GetComponentInParent<PlayerMovement>() != null
            || other.gameObject.name == "Player";
    }

    private void OpenDialogue()
    {
        dialogueOpen = true;
        lineIndex = 0;
        RequestEnemyPauseIfNeeded();
    }

    private void AdvanceDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0 || lineIndex >= dialogueLines.Length - 1)
        {
            CompleteDialogue();
            return;
        }

        lineIndex++;
    }

    private void CompleteDialogue()
    {
        dialogueCompleted = true;
        CloseDialogue(true);

        if (guardian != null)
        {
            guardian.BeginCombat();
        }

        CurrentObjectiveDisplay.SetObjective("擊敗森林深處的精英怪，取得寶箱中的雷痕符文");
    }

    private void CloseDialogue(bool completed)
    {
        if (!dialogueOpen && !holdingEnemyPause)
        {
            return;
        }

        ReleaseEnemyPauseIfNeeded();
        if (completed)
        {
            GameplayPauseState.PauseEnemiesFor(enemyResumeDelay);
            GrantPlayerInvincibilityAfterClose();
        }

        dialogueOpen = false;
        lineIndex = 0;
    }

    private void RequestEnemyPauseIfNeeded()
    {
        if (!pauseEnemiesWhileOpen || holdingEnemyPause)
        {
            return;
        }

        GameplayPauseState.RequestEnemyPause();
        holdingEnemyPause = true;
    }

    private void ReleaseEnemyPauseIfNeeded()
    {
        if (!holdingEnemyPause)
        {
            return;
        }

        GameplayPauseState.ReleaseEnemyPause();
        holdingEnemyPause = false;
    }

    private void GrantPlayerInvincibilityAfterClose()
    {
        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.GrantInvincibility(playerInvincibleAfterClose);
        }
    }

    private void OnGUI()
    {
        if (BackpackUI.IsOpen)
        {
            return;
        }

        if (!playerInRange || dialogueCompleted)
        {
            return;
        }

        EnsureStyles();
        if (!dialogueOpen)
        {
            float promptWidth = Mathf.Min(340f, Screen.width - 40f);
            Rect promptRect = new Rect((Screen.width - promptWidth) * 0.5f, Screen.height - 82f, promptWidth, 42f);
            GUI.Box(promptRect, interactPrompt, hintStyle);
            return;
        }

        float margin = Mathf.Max(24f, Screen.width * 0.08f);
        Rect panelRect = new Rect(margin, Screen.height - 178f, Screen.width - margin * 2f, 138f);
        GUI.Box(panelRect, GUIContent.none, panelStyle);

        Rect speakerRect = new Rect(panelRect.x + 22f, panelRect.y + 14f, panelRect.width - 44f, 28f);
        GUI.Label(speakerRect, GetCurrentSpeaker(), speakerStyle);

        Rect lineRect = new Rect(panelRect.x + 22f, panelRect.y + 48f, panelRect.width - 44f, 54f);
        GUI.Label(lineRect, GetCurrentLine(), lineStyle);

        Rect footerRect = new Rect(panelRect.x + 22f, panelRect.y + panelRect.height - 32f, panelRect.width - 44f, 24f);
        GUI.Label(footerRect, "E / Space 下一句，Esc 關閉", hintStyle);
    }

    private string GetCurrentSpeaker()
    {
        if (speakers == null || speakers.Length == 0)
        {
            return "精英怪";
        }

        return speakers[Mathf.Clamp(lineIndex, 0, speakers.Length - 1)];
    }

    private string GetCurrentLine()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            return string.Empty;
        }

        return dialogueLines[Mathf.Clamp(lineIndex, 0, dialogueLines.Length - 1)];
    }

    private void EnsureStyles()
    {
        if (panelStyle != null)
        {
            return;
        }

        panelTexture = new Texture2D(1, 1)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        panelTexture.SetPixel(0, 0, panelColor);
        panelTexture.Apply();

        panelStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = panelTexture },
            padding = new RectOffset(18, 18, 14, 14)
        };

        speakerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            normal = { textColor = speakerColor }
        };

        lineStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            wordWrap = true,
            normal = { textColor = Color.white }
        };

        hintStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = Mathf.Max(14, fontSize - 4),
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            normal = { textColor = Color.white }
        };
    }

    private void OnDestroy()
    {
        ReleaseEnemyPauseIfNeeded();

        if (panelTexture != null)
        {
            Destroy(panelTexture);
        }
    }
}
