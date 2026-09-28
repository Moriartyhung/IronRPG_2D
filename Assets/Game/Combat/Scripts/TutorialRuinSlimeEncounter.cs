using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TutorialRuinSlimeEncounter : MonoBehaviour
{
    private const string TutorialSceneName = "TutorialScene";
    private const string MonsterObjectName = "monster";
    private const string DialogObjectName = "dialog";

    private static bool sawSlimeDialogueShown;
    private static bool slimeDefeated;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneLoadedEvent()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != TutorialSceneName)
        {
            return;
        }

        ConfigureDialogTrigger();
        ConfigureSlime();
    }

    private static void ConfigureDialogTrigger()
    {
        GameObject triggerObject = FindObjectByExactName(DialogObjectName);
        if (triggerObject == null)
        {
            return;
        }

        Collider2D trigger = triggerObject.GetComponent<Collider2D>();
        if (trigger == null)
        {
            BoxCollider2D box = triggerObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1.3f, 1.1f);
            trigger = box;
        }

        trigger.isTrigger = true;

        RuinSlimeSightTrigger sightTrigger = triggerObject.GetComponent<RuinSlimeSightTrigger>();
        if (sightTrigger == null)
        {
            sightTrigger = triggerObject.AddComponent<RuinSlimeSightTrigger>();
        }
    }

    private static void ConfigureSlime()
    {
        GameObject monster = FindObjectByExactName(MonsterObjectName);
        if (monster == null)
        {
            return;
        }

        if (slimeDefeated)
        {
            monster.SetActive(false);
            return;
        }

        EnsureSlimeVisual(monster);

        Rigidbody2D body = monster.GetComponent<Rigidbody2D>();
        if (body == null)
        {
            body = monster.AddComponent<Rigidbody2D>();
        }

        body.gravityScale = 0f;
        body.freezeRotation = true;

        BoxCollider2D collider = monster.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = monster.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = false;
        collider.size = new Vector2(0.34f, 0.24f);
        collider.offset = new Vector2(0f, -0.04f);

        EnemyHealth health = monster.GetComponent<EnemyHealth>();
        if (health == null)
        {
            health = monster.AddComponent<EnemyHealth>();
        }

        health.ConfigureStats("遺跡史萊姆", 150, 5, new Vector2(0f, 0.24f));
        health.Defeated -= HandleSlimeDefeated;
        health.Defeated += HandleSlimeDefeated;

        EnemyLootDrop lootDrop = monster.GetComponent<EnemyLootDrop>();
        if (lootDrop == null)
        {
            lootDrop = monster.AddComponent<EnemyLootDrop>();
        }

        lootDrop.ConfigureBasicLoot(2, 4);

        EnemyContactDamage contactDamage = monster.GetComponent<EnemyContactDamage>();
        if (contactDamage == null)
        {
            contactDamage = monster.AddComponent<EnemyContactDamage>();
        }

        contactDamage.SetDamage(4);
        contactDamage.SetDamageInterval(0.85f);
        contactDamage.SetMaxAttackDistance(0.7f);

        SimpleEnemyChase chase = monster.GetComponent<SimpleEnemyChase>();
        if (chase == null)
        {
            chase = monster.AddComponent<SimpleEnemyChase>();
        }

        chase.SetMoveSpeed(0.72f);
        chase.SetAggroRange(2.9f);
        chase.SetStopDistance(0.72f);

        if (monster.GetComponent<EnemyVisualAnimator>() == null)
        {
            monster.AddComponent<EnemyVisualAnimator>();
        }
    }

    private static void EnsureSlimeVisual(GameObject monster)
    {
        Transform visualTransform = monster.transform.Find("Visual");
        if (visualTransform == null)
        {
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(monster.transform, false);
            visualTransform = visual.transform;
        }

        SpriteRenderer renderer = visualTransform.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
        }

        if (renderer.sprite == null)
        {
            renderer.sprite = CreateFallbackSlimeSprite();
        }

        visualTransform.localPosition = Vector3.zero;
        visualTransform.localScale = new Vector3(3.5f, 3.5f, 1f);
        renderer.sortingLayerName = "Layer 3";
        renderer.sortingOrder = 50;
    }

    private static Sprite CreateFallbackSlimeSprite()
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color outline = new Color(0.05f, 0.23f, 0.04f, 1f);
        Color body = new Color(0.36f, 0.9f, 0.22f, 1f);
        Color shine = new Color(0.72f, 1f, 0.55f, 1f);
        Color eye = new Color(0.02f, 0.08f, 0.02f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - 15.5f) / 13.5f;
                float ny = (y - 11.5f) / 10.5f;
                bool inside = nx * nx + ny * ny < 1f && y < 24 && y > 4;
                bool baseLip = y >= 9 && y <= 13 && x > 5 && x < 27;
                bool edge = nx * nx + ny * ny > 0.78f && inside;

                Color pixel = clear;
                if (inside || baseLip)
                {
                    pixel = edge || y < 7 ? outline : body;
                }

                if ((x == 11 || x == 21) && y >= 13 && y <= 15)
                {
                    pixel = eye;
                }
                else if (x >= 9 && x <= 13 && y >= 18 && y <= 21)
                {
                    pixel = shine;
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.35f), 32f);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private static void HandleSlimeDefeated(EnemyHealth enemy)
    {
        slimeDefeated = true;
        RuinInnerMonologuePanel.Show(
            "主角",
            new[]
            {
                "這種怪物以前從沒出現在遺跡附近。",
                "先祖的地方出了怪物……這可不是小事。",
                "得先回村告訴村長。"
            },
            "回村向村長報告遺跡小怪");
    }

    private static GameObject FindObjectByExactName(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject candidate = objects[i];
            if (candidate == null || candidate.name != objectName || !candidate.scene.IsValid())
            {
                continue;
            }

            return candidate;
        }

        return null;
    }

    private sealed class RuinSlimeSightTrigger : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (sawSlimeDialogueShown || slimeDefeated || !IsPlayer(other))
            {
                return;
            }

            sawSlimeDialogueShown = true;
            RuinInnerMonologuePanel.Show(
                "主角",
                new[]
                {
                    "嗯？那是什麼聲音？",
                    "遺跡附近怎麼會有這種東西？",
                    "看來不是普通野獸。正好，別擋我回村的路。"
                },
                "擊敗遺跡附近的小怪");
        }
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.GetComponent<PlayerMovement>() != null
            || other.GetComponentInParent<PlayerMovement>() != null
            || other.gameObject.name == "Player";
    }

    private sealed class RuinInnerMonologuePanel : MonoBehaviour
    {
        private static RuinInnerMonologuePanel instance;

        private string title;
        private string[] lines;
        private string objectiveOnClose;
        private int lineIndex;
        private bool panelOpen;
        private bool holdingEnemyPause;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle lineStyle;
        private GUIStyle hintStyle;
        private Texture2D panelTexture;

        public static void Show(string title, string[] lines, string objectiveOnClose)
        {
            Ensure().Open(title, lines, objectiveOnClose);
        }

        private static RuinInnerMonologuePanel Ensure()
        {
            if (instance != null)
            {
                return instance;
            }

            GameObject panelObject = new GameObject("RuinInnerMonologuePanel");
            instance = panelObject.AddComponent<RuinInnerMonologuePanel>();
            return instance;
        }

        private void Open(string nextTitle, string[] nextLines, string nextObjective)
        {
            title = nextTitle;
            lines = nextLines != null && nextLines.Length > 0 ? nextLines : new[] { string.Empty };
            objectiveOnClose = nextObjective;
            lineIndex = 0;
            panelOpen = true;
            RequestEnemyPause();
        }

        private void Update()
        {
            if (!panelOpen)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Close();
                return;
            }

            if (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                if (lineIndex >= lines.Length - 1)
                {
                    Close();
                }
                else
                {
                    lineIndex++;
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

        private void Close()
        {
            if (!panelOpen)
            {
                return;
            }

            panelOpen = false;
            if (holdingEnemyPause)
            {
                GameplayPauseState.ReleaseEnemyPause();
                holdingEnemyPause = false;
            }

            GameplayPauseState.PauseEnemiesFor(1.2f);
            PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.GrantInvincibility(1.2f);
            }

            if (!string.IsNullOrWhiteSpace(objectiveOnClose))
            {
                CurrentObjectiveDisplay.SetObjective(objectiveOnClose);
            }
        }

        private void OnGUI()
        {
            if (!panelOpen || BackpackUI.IsOpen)
            {
                return;
            }

            EnsureStyles();
            float margin = Mathf.Max(24f, Screen.width * 0.08f);
            Rect panelRect = new Rect(margin, Screen.height - 172f, Screen.width - margin * 2f, 132f);
            GUI.Box(panelRect, GUIContent.none, panelStyle);

            GUI.Label(new Rect(panelRect.x + 22f, panelRect.y + 14f, panelRect.width - 44f, 28f), title, titleStyle);
            string currentLine = lines[Mathf.Clamp(lineIndex, 0, lines.Length - 1)];
            GUI.Label(new Rect(panelRect.x + 22f, panelRect.y + 48f, panelRect.width - 44f, 48f), currentLine, lineStyle);
            GUI.Label(new Rect(panelRect.x + 22f, panelRect.y + panelRect.height - 32f, panelRect.width - 44f, 24f), "E / Space 下一句，Esc 關閉", hintStyle);
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
            panelTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.78f));
            panelTexture.Apply();

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = panelTexture },
                padding = new RectOffset(18, 18, 14, 14)
            };

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.82f, 0.35f, 1f) }
            };

            lineStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }

        private void OnDestroy()
        {
            if (holdingEnemyPause)
            {
                GameplayPauseState.ReleaseEnemyPause();
            }

            if (panelTexture != null)
            {
                Destroy(panelTexture);
            }
        }
    }
}
