using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class BossIntroDialogueTrigger : MonoBehaviour
{
    public static event Action BossIntroCompleted;
    public static bool HasCompleted { get; private set; }

    [SerializeField] private Transform dialogueMark;
    [SerializeField] private float autoMoveSpeed = 4.8f;
    [SerializeField] private float arriveDistance = 0.05f;
    [SerializeField] private float introCameraOrthographicSize = 6f;
    [SerializeField] private float introCameraOffsetY = 4.2f;
    [SerializeField] private float enemyResumeDelay = 0.8f;
    [SerializeField] private int fontSize = 20;
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.82f);
    [SerializeField] private Color speakerColor = new Color(1f, 0.68f, 0.38f, 1f);
    [SerializeField] private string objectiveAfterDialogue = "擊敗洞穴深處的 Boss";
    [SerializeField] private string[] speakers =
    {
        "Boss", "主角", "Boss", "主角", "Boss", "Boss", "主角", "Boss", "主角", "Boss",
        "主角", "Boss", "主角", "Boss", "主角", "Boss", "主角", "Boss", "主角", "Boss",
        "主角", "Boss", "主角", "Boss", "主角", "Boss", "主角", "Boss", "主角", "Boss",
        "主角", "Boss", "主角", "Boss", "主角", "Boss", "主角"
    };

    [TextArea(2, 4)]
    [SerializeField] private string[] dialogueLines =
    {
        "雷……",
        "醒了？",
        "那枚符文……竟又回到人類手中。",
        "你認得雷痕符文。",
        "我怎會忘記。",
        "那道雷劈開我的甲，那群螻蟻用屍體堵住我的路。你們的先祖，把死亡堆成牆，才換來我退回此地。",
        "聽起來你記得很清楚。",
        "我在黑暗中記了很多年。",
        "那你應該也記得自己為什麼被討伐。",
        "因為我強。",
        "因為你屠村。",
        "弱者把強者的進食稱為屠殺。",
        "你還真是一點都沒變。",
        "人類倒是變得更脆弱了。當年那些人，至少敢用身體擋在我面前。",
        "今天也有人站在你面前。",
        "你？",
        "我。",
        "一個村莊獵人，帶著不屬於自己的雷。",
        "它承認我了。",
        "雷會選錯人。",
        "那你可以問它。",
        "狂妄。",
        "這句話從你嘴裡說出來，很有教育意義。",
        "你知道那叛徒做了什麼嗎？",
        "精英怪？",
        "它拔走了壓制我的符文，以為自己能掌控雷。可笑。",
        "它想殺你稱王。",
        "所有弱者都夢想坐上王座。",
        "你不也是？",
        "我不需要夢想。我生來就該站在所有生命之上。",
        "那你今天會摔得很重。",
        "先祖的血脈……",
        "你感覺到了？",
        "你身上有那群人的味道。",
        "那你也該知道，我不是第一個來阻止你的人。",
        "也不會是第一個死在我面前的人。",
        "那就讓我看看，雷是否還像當年一樣疼。"
    };

    private PlayerMovement playerMovement;
    private Rigidbody2D playerBody;
    private PlayerAttack playerAttack;
    private bool sequenceStarted;
    private bool movingToMark;
    private bool dialogueOpen;
    private bool dialogueCompleted;
    private bool holdingEnemyPause;
    private int lineIndex;
    private GUIStyle panelStyle;
    private GUIStyle speakerStyle;
    private GUIStyle lineStyle;
    private GUIStyle hintStyle;
    private Texture2D panelTexture;

    public void Configure(Transform targetDialogueMark)
    {
        dialogueMark = targetDialogueMark;
    }

    private void Awake()
    {
        HasCompleted = false;
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    private void Update()
    {
        if (movingToMark)
        {
            MovePlayerToDialogueMark();
            return;
        }

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (sequenceStarted || dialogueCompleted || !IsPlayer(other))
        {
            return;
        }

        playerMovement = other.GetComponentInParent<PlayerMovement>() ?? other.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            return;
        }

        playerBody = playerMovement.GetComponent<Rigidbody2D>();
        playerAttack = playerMovement.GetComponent<PlayerAttack>();
        BeginSequence();
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.GetComponent<PlayerMovement>() != null
            || other.GetComponentInParent<PlayerMovement>() != null
            || other.gameObject.name == "Player";
    }

    private void BeginSequence()
    {
        sequenceStarted = true;
        movingToMark = dialogueMark != null;
        RequestEnemyPause();
        ShiftCameraForBossIntro();
        playerMovement.SetInputLocked(true);
        SetPlayerAttackEnabled(false);

        if (!movingToMark)
        {
            StartDialogue();
        }
    }

    private void MovePlayerToDialogueMark()
    {
        if (playerMovement == null || dialogueMark == null)
        {
            StartDialogue();
            return;
        }

        Vector2 current = playerBody != null ? playerBody.position : (Vector2)playerMovement.transform.position;
        Vector2 target = dialogueMark.position;
        Vector2 toTarget = target - current;
        if (toTarget.magnitude <= arriveDistance)
        {
            if (playerBody != null)
            {
                playerBody.MovePosition(target);
            }
            else
            {
                playerMovement.transform.position = new Vector3(target.x, target.y, playerMovement.transform.position.z);
            }

            playerMovement.SetScriptedMoveInput(Vector2.zero, false);
            StartDialogue();
            return;
        }

        Vector2 direction = toTarget.normalized;
        Vector2 next = Vector2.MoveTowards(current, target, autoMoveSpeed * Time.deltaTime);
        playerMovement.SetScriptedMoveInput(direction, true);
        if (playerBody != null)
        {
            playerBody.MovePosition(next);
        }
        else
        {
            playerMovement.transform.position = new Vector3(next.x, next.y, playerMovement.transform.position.z);
        }
    }

    private void StartDialogue()
    {
        movingToMark = false;
        dialogueOpen = true;
        lineIndex = 0;
        if (playerMovement != null)
        {
            playerMovement.SetScriptedMoveInput(Vector2.zero, false);
            playerMovement.FaceDirection(Vector2.up);
        }
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
        dialogueCompleted = true;
        movingToMark = false;

        if (playerMovement != null)
        {
            playerMovement.SetInputLocked(false);
            playerMovement.FaceDirection(Vector2.up);
        }

        SetPlayerAttackEnabled(true);
        ReleaseEnemyPause();
        GameplayPauseState.PauseEnemiesFor(enemyResumeDelay);

        if (!string.IsNullOrWhiteSpace(objectiveAfterDialogue))
        {
            CurrentObjectiveDisplay.SetObjective(objectiveAfterDialogue);
        }

        HasCompleted = true;
        BossIntroCompleted?.Invoke();
        enabled = false;
    }

    private void SetPlayerAttackEnabled(bool enabledValue)
    {
        if (playerAttack != null)
        {
            playerAttack.enabled = enabledValue;
        }
    }

    private void ShiftCameraForBossIntro()
    {
        Camera targetCamera = Camera.main ?? FindAnyObjectByType<Camera>();
        if (targetCamera != null && targetCamera.orthographic)
        {
            targetCamera.orthographicSize = introCameraOrthographicSize;
            CameraFollow cameraFollow = targetCamera.GetComponent<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.SetOffsetY(introCameraOffsetY);
            }
        }
    }

    private void RequestEnemyPause()
    {
        if (holdingEnemyPause)
        {
            return;
        }

        GameplayPauseState.RequestEnemyPause();
        holdingEnemyPause = true;
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

        SetPlayerAttackEnabled(true);

        if (panelTexture != null)
        {
            Destroy(panelTexture);
        }
    }
}
