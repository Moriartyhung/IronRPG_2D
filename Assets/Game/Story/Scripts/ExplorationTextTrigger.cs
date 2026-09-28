using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class ExplorationTextTrigger : MonoBehaviour
{
    [SerializeField] private string title = "森林入口";
    [SerializeField] private string promptText = "按 E 調查";
    [TextArea(2, 4)]
    [SerializeField] private string[] storyLines =
    {
        "這裡的空氣比村莊冷很多。",
        "村長說的異常，應該就是從森林深處傳出來的。",
        "先不要太深入，附近可能有危險。"
    };
    [SerializeField] private bool openWhenPlayerEnters = true;
    [SerializeField] private bool showOnlyOnce = false;
    [SerializeField] private bool pauseEnemiesWhilePanelOpen = true;
    [SerializeField] private float enemyResumeDelay = 1.5f;
    [SerializeField] private float playerInvincibleAfterClose = 1.5f;
    [SerializeField] private int fontSize = 20;
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.78f);
    [SerializeField] private Color titleColor = new Color(0.65f, 0.95f, 0.72f, 1f);

    private bool playerInRange;
    private bool panelOpen;
    private bool hasShown;
    private int lineIndex;
    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle lineStyle;
    private GUIStyle hintStyle;
    private Texture2D panelTexture;
    private bool holdingEnemyPause;

    private void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    private void Update()
    {
        if (!playerInRange)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (panelOpen && keyboard.escapeKey.wasPressedThisFrame)
        {
            ClosePanel();
            return;
        }

        if (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
        {
            if (panelOpen)
            {
                AdvanceLine();
            }
            else if (!showOnlyOnce || !hasShown)
            {
                OpenPanel();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        playerInRange = true;
        if (openWhenPlayerEnters && (!showOnlyOnce || !hasShown))
        {
            OpenPanel();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerInRange = false;
            ClosePanel();
        }
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.GetComponent<PlayerMovement>() != null
            || other.GetComponentInParent<PlayerMovement>() != null
            || other.gameObject.name == "Player";
    }

    private void OpenPanel()
    {
        if (panelOpen)
        {
            return;
        }

        panelOpen = true;
        hasShown = true;
        lineIndex = 0;
        RequestEnemyPauseIfNeeded();
    }

    private void AdvanceLine()
    {
        if (storyLines == null || storyLines.Length == 0 || lineIndex >= storyLines.Length - 1)
        {
            ClosePanel();
            return;
        }

        lineIndex++;
    }

    private void ClosePanel()
    {
        if (!panelOpen && !holdingEnemyPause)
        {
            return;
        }

        ReleaseEnemyPauseIfNeeded();
        PauseEnemiesBrieflyAfterClose();
        GrantPlayerInvincibilityAfterClose();
        panelOpen = false;
        lineIndex = 0;
    }

    private void RequestEnemyPauseIfNeeded()
    {
        if (!pauseEnemiesWhilePanelOpen || holdingEnemyPause)
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

    private void PauseEnemiesBrieflyAfterClose()
    {
        if (!pauseEnemiesWhilePanelOpen)
        {
            return;
        }

        GameplayPauseState.PauseEnemiesFor(enemyResumeDelay);
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

        if (!playerInRange || (showOnlyOnce && hasShown && !panelOpen))
        {
            return;
        }

        EnsureStyles();

        if (!panelOpen)
        {
            float promptWidth = Mathf.Min(320f, Screen.width - 40f);
            Rect promptRect = new Rect((Screen.width - promptWidth) * 0.5f, Screen.height - 82f, promptWidth, 42f);
            GUI.Box(promptRect, promptText, hintStyle);
            return;
        }

        float margin = Mathf.Max(24f, Screen.width * 0.08f);
        Rect panelRect = new Rect(margin, Screen.height - 172f, Screen.width - margin * 2f, 132f);
        GUI.Box(panelRect, GUIContent.none, panelStyle);

        Rect titleRect = new Rect(panelRect.x + 22f, panelRect.y + 14f, panelRect.width - 44f, 28f);
        GUI.Label(titleRect, title, titleStyle);

        string currentLine = storyLines != null && storyLines.Length > 0
            ? storyLines[Mathf.Clamp(lineIndex, 0, storyLines.Length - 1)]
            : string.Empty;

        Rect lineRect = new Rect(panelRect.x + 22f, panelRect.y + 48f, panelRect.width - 44f, 48f);
        GUI.Label(lineRect, currentLine, lineStyle);

        Rect footerRect = new Rect(panelRect.x + 22f, panelRect.y + panelRect.height - 32f, panelRect.width - 44f, 24f);
        GUI.Label(footerRect, "E / Space 下一句，Esc 關閉", hintStyle);
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

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            normal = { textColor = titleColor }
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
