using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class BossDefeatDialogueDisplay : MonoBehaviour
{
    private static BossDefeatDialogueDisplay instance;

    [SerializeField] private int fontSize = 20;
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.82f);
    [SerializeField] private Color speakerColor = new Color(1f, 0.68f, 0.38f, 1f);
    [SerializeField] private string objectiveAfterDialogue = "離開洞穴，回村莊向村長回報";
    [SerializeField] private string[] speakers =
    {
        "Boss",
        "主角",
        "Boss",
        "主角",
        "旁白",
        "主角"
    };

    [TextArea(2, 4)]
    [SerializeField] private string[] dialogueLines =
    {
        "雷……還在……",
        "雷痕符文撕開了你的魔甲。結束了。",
        "人類……又一次……把我逼回黑暗……",
        "這一次不是封印，是終止。",
        "洞穴深處的根鬚停止扭動，雷痕符文上的白色雷紋也慢慢沉入拳套。",
        "我要回村，把這裡發生的事告訴村長。"
    };

    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;
    private bool dialogueOpen;
    private bool holdingEnemyPause;
    private int lineIndex;
    private GUIStyle panelStyle;
    private GUIStyle speakerStyle;
    private GUIStyle lineStyle;
    private GUIStyle hintStyle;
    private Texture2D panelTexture;

    public static void ShowDialogue()
    {
        EnsureInstance();
        instance.Open();
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        BossDefeatDialogueDisplay existing = FindAnyObjectByType<BossDefeatDialogueDisplay>();
        if (existing != null)
        {
            instance = existing;
            return;
        }

        GameObject displayObject = new GameObject("BossDefeatDialogueDisplay");
        instance = displayObject.AddComponent<BossDefeatDialogueDisplay>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Update()
    {
        if (!dialogueOpen)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
        {
            AdvanceDialogue();
        }
    }

    private void Open()
    {
        if (dialogueOpen)
        {
            return;
        }

        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerAttack = playerMovement != null ? playerMovement.GetComponent<PlayerAttack>() : null;
        if (playerMovement != null)
        {
            playerMovement.SetInputLocked(true);
            playerMovement.SetScriptedMoveInput(Vector2.zero, false);
            playerMovement.FaceDirection(Vector2.up);
        }

        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }

        GameplayPauseState.RequestEnemyPause();
        holdingEnemyPause = true;
        lineIndex = 0;
        dialogueOpen = true;
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
        dialogueOpen = false;
        if (playerMovement != null)
        {
            playerMovement.SetInputLocked(false);
            playerMovement.FaceDirection(Vector2.up);
        }

        if (playerAttack != null)
        {
            playerAttack.enabled = true;
        }

        ReleaseEnemyPause();
        if (!string.IsNullOrWhiteSpace(objectiveAfterDialogue))
        {
            CurrentObjectiveDisplay.SetObjective(objectiveAfterDialogue);
        }
    }

    private void ReleaseEnemyPause()
    {
        if (!holdingEnemyPause)
        {
            return;
        }

        GameplayPauseState.ReleaseEnemyPause();
        holdingEnemyPause = false;
    }

    private void OnGUI()
    {
        if (!dialogueOpen || BackpackUI.IsOpen)
        {
            return;
        }

        EnsureStyles();

        float margin = Mathf.Max(24f, Screen.width * 0.08f);
        Rect panelRect = new Rect(margin, Screen.height - 178f, Screen.width - margin * 2f, 138f);
        GUI.Box(panelRect, GUIContent.none, panelStyle);

        Rect speakerRect = new Rect(panelRect.x + 22f, panelRect.y + 14f, panelRect.width - 44f, 28f);
        GUI.Label(speakerRect, GetCurrentSpeaker(), speakerStyle);

        Rect lineRect = new Rect(panelRect.x + 22f, panelRect.y + 48f, panelRect.width - 44f, 54f);
        GUI.Label(lineRect, GetCurrentLine(), lineStyle);

        Rect footerRect = new Rect(panelRect.x + 22f, panelRect.y + panelRect.height - 32f, panelRect.width - 44f, 24f);
        GUI.Label(footerRect, "E / Space 下一句", hintStyle);
    }

    private string GetCurrentSpeaker()
    {
        if (speakers == null || speakers.Length == 0)
        {
            return "Boss";
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

        hintStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Max(14, fontSize - 4),
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,
            normal = { textColor = Color.white }
        };
    }

    private void OnDestroy()
    {
        ReleaseEnemyPause();
        if (playerMovement != null)
        {
            playerMovement.SetInputLocked(false);
        }

        if (playerAttack != null)
        {
            playerAttack.enabled = true;
        }

        if (instance == this)
        {
            instance = null;
        }

        if (panelTexture != null)
        {
            Destroy(panelTexture);
        }
    }
}
