using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenuScene";
    private const string FirstGameSceneName = "TutorialScene";
    private const string LastSceneKey = "IronRPG.Save.LastScene";
    private const string HasSaveKey = "IronRPG.Save.HasSave";
    private const string BackgroundResourcePath = "Menu/main_menu_background";

    private static MainMenuController instance;

    private Texture2D background;
    private GUIStyle titleStyle;
    private GUIStyle subtitleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle disabledButtonStyle;
    private GUIStyle panelStyle;
    private Texture2D panelTexture;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= TrackSceneLoaded;
        SceneManager.sceneLoaded += TrackSceneLoaded;
        EnsureInstanceIfNeeded(SceneManager.GetActiveScene());
    }

    private static void TrackSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureInstanceIfNeeded(scene);
        if (scene.name == MainMenuSceneName)
        {
            return;
        }

        PlayerPrefs.SetString(LastSceneKey, scene.name);
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.Save();
    }

    private static void EnsureInstanceIfNeeded(Scene scene)
    {
        if (scene.name != MainMenuSceneName)
        {
            return;
        }

        if (instance != null)
        {
            return;
        }

        GameObject menuObject = new GameObject(nameof(MainMenuController));
        instance = menuObject.AddComponent<MainMenuController>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        background = Resources.Load<Texture2D>(BackgroundResourcePath);
        Time.timeScale = 1f;
        GameAudioManager.SetMusicPausedBySettings(false);
    }

    private void OnGUI()
    {
        if (SceneManager.GetActiveScene().name != MainMenuSceneName)
        {
            return;
        }

        EnsureStyles();
        DrawBackground();
        DrawTitle();
        DrawButtons();
    }

    private void DrawBackground()
    {
        if (background != null)
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), background, ScaleMode.ScaleAndCrop);
            return;
        }

        GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
    }

    private void DrawTitle()
    {
        float titleX = Mathf.Max(36f, Screen.width * 0.08f);
        float titleY = Mathf.Max(36f, Screen.height * 0.08f);
        GUI.Label(new Rect(titleX, titleY, 520f, 58f), "IronRPG", titleStyle);
        GUI.Label(new Rect(titleX + 4f, titleY + 58f, 560f, 34f), "雷痕符文與洞穴古樹", subtitleStyle);
    }

    private void DrawButtons()
    {
        float panelWidth = Mathf.Min(360f, Screen.width - 48f);
        float panelHeight = 250f;
        float panelX = Mathf.Clamp(Screen.width * 0.66f, 24f, Screen.width - panelWidth - 24f);
        float panelY = Mathf.Max(120f, (Screen.height - panelHeight) * 0.5f);
        Rect panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);

        GUI.Box(panelRect, GUIContent.none, panelStyle);
        GUILayout.BeginArea(new Rect(panelRect.x + 28f, panelRect.y + 24f, panelRect.width - 56f, panelRect.height - 48f));

        bool hasSave = PlayerPrefs.GetInt(HasSaveKey, 0) == 1 && !string.IsNullOrWhiteSpace(PlayerPrefs.GetString(LastSceneKey, string.Empty));
        GUI.enabled = hasSave;
        if (GUILayout.Button("繼續遊戲", hasSave ? buttonStyle : disabledButtonStyle, GUILayout.Height(46f)))
        {
            ContinueGame();
        }
        GUI.enabled = true;

        GUILayout.Space(14f);
        if (GUILayout.Button("新遊戲", buttonStyle, GUILayout.Height(46f)))
        {
            StartNewGame();
        }

        GUILayout.Space(14f);
        if (GUILayout.Button("退出遊戲", buttonStyle, GUILayout.Height(46f)))
        {
            QuitGame();
        }

        GUILayout.EndArea();
    }

    private static void ContinueGame()
    {
        string sceneName = PlayerPrefs.GetString(LastSceneKey, FirstGameSceneName);
        if (string.IsNullOrWhiteSpace(sceneName) || sceneName == MainMenuSceneName)
        {
            sceneName = FirstGameSceneName;
        }

        SceneManager.LoadScene(sceneName);
    }

    private static void StartNewGame()
    {
        PlayerPrefs.SetString(LastSceneKey, FirstGameSceneName);
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.Save();

        MainQuestState.ResetForNewGame();
        PlayerInventory.ResetForNewGame();
        PlayerProgress.ResetForNewGame();
        BossCoreBattleController.ResetForNewGame();
        GameplayPauseState.ClearEnemyPauses();
        SceneManager.LoadScene(FirstGameSceneName);
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
        if (titleStyle != null)
        {
            return;
        }

        panelTexture = MakeTexture(new Color(0.03f, 0.025f, 0.02f, 0.72f));
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.085f), 42, 78),
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.83f, 0.42f, 1f) }
        };

        subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.028f), 18, 28),
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.92f, 0.86f, 0.72f, 1f) }
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };

        disabledButtonStyle = new GUIStyle(buttonStyle)
        {
            normal = { textColor = new Color(0.55f, 0.55f, 0.55f, 1f) }
        };

        panelStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(18, 18, 18, 18),
            normal = { background = panelTexture }
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
            instance = null;
        }

        if (panelTexture != null)
        {
            Destroy(panelTexture);
        }
    }
}
