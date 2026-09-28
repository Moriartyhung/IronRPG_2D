using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsMenuUI : MonoBehaviour
{
    private const float PanelWidth = 420f;
    private const float PanelHeight = 330f;

    private static SettingsMenuUI instance;

    private GUIStyle overlayStyle;
    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;
    private Texture2D overlayTexture;
    private Texture2D panelTexture;
    private float previousTimeScale = 1f;

    public static bool IsOpen { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        EnsureInstance();
    }

    private static SettingsMenuUI EnsureInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject settingsObject = new GameObject(nameof(SettingsMenuUI));
        instance = settingsObject.AddComponent<SettingsMenuUI>();
        DontDestroyOnLoad(settingsObject);
        return instance;
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

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (IsOpen)
            {
                CloseSettings();
            }
            else if (CanOpenSettings())
            {
                OpenSettings();
            }
        }
    }

    private static bool CanOpenSettings()
    {
        return !BackpackUI.IsOpen && !GameplayPauseState.EnemiesPaused;
    }

    private void OpenSettings()
    {
        if (IsOpen)
        {
            return;
        }

        previousTimeScale = Time.timeScale <= 0f ? 1f : Time.timeScale;
        Time.timeScale = 0f;
        GameAudioManager.SetMusicPausedBySettings(true);
        IsOpen = true;
    }

    private void CloseSettings()
    {
        if (!IsOpen)
        {
            return;
        }

        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        GameAudioManager.SetMusicPausedBySettings(false);
        IsOpen = false;
    }

    private void OnGUI()
    {
        if (!IsOpen)
        {
            return;
        }

        EnsureStyles();

        int previousDepth = GUI.depth;
        GUI.depth = -1000;

        GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none, overlayStyle);

        Rect panelRect = new Rect(
            (Screen.width - PanelWidth) * 0.5f,
            Mathf.Max(42f, (Screen.height - PanelHeight) * 0.5f),
            PanelWidth,
            PanelHeight);

        GUI.Box(panelRect, GUIContent.none, panelStyle);
        GUILayout.BeginArea(new Rect(panelRect.x + 24f, panelRect.y + 20f, panelRect.width - 48f, panelRect.height - 40f));

        GUILayout.Label("遊戲設定", titleStyle);
        GUILayout.Space(16f);

        DrawVolumeSlider("音樂音量", GameAudioManager.MusicVolume, GameAudioManager.SetMusicVolume);
        GUILayout.Space(10f);
        DrawVolumeSlider("音效音量", GameAudioManager.SfxVolume, GameAudioManager.SetSfxVolume);

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("繼續遊戲", buttonStyle, GUILayout.Height(42f)))
        {
            CloseSettings();
        }

        GUILayout.Space(8f);
        if (GUILayout.Button("退出遊戲", buttonStyle, GUILayout.Height(42f)))
        {
            QuitGame();
        }

        GUILayout.Space(8f);
        GUILayout.Label("Esc：關閉設定並繼續遊戲", labelStyle);
        GUILayout.EndArea();

        GUI.depth = previousDepth;
    }

    private void DrawVolumeSlider(string label, float value, System.Action<float> setter)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, labelStyle, GUILayout.Width(86f));
        float nextValue = GUILayout.HorizontalSlider(value, 0f, 1f, GUILayout.Width(210f));
        GUILayout.Label($"{Mathf.RoundToInt(nextValue * 100f)}%", labelStyle, GUILayout.Width(52f));
        GUILayout.EndHorizontal();

        if (!Mathf.Approximately(nextValue, value))
        {
            setter(nextValue);
        }
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EnsureStyles()
    {
        if (panelStyle != null)
        {
            return;
        }

        overlayTexture = MakeTexture(new Color(0f, 0f, 0f, 0.68f));
        panelTexture = MakeTexture(new Color(0.07f, 0.06f, 0.05f, 0.96f));

        overlayStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = overlayTexture }
        };

        panelStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(18, 18, 16, 16),
            normal = { background = panelTexture }
        };

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 26,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(1f, 0.86f, 0.48f, 1f) }
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            normal = { textColor = Color.white }
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
    }

    private static Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            CloseSettings();
            instance = null;
        }

        if (overlayTexture != null)
        {
            Destroy(overlayTexture);
        }

        if (panelTexture != null)
        {
            Destroy(panelTexture);
        }
    }
}
