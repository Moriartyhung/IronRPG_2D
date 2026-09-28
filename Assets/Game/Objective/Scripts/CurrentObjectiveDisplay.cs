using UnityEngine;

[DisallowMultipleComponent]
public class CurrentObjectiveDisplay : MonoBehaviour
{
    [SerializeField] private string currentObjective;
    [SerializeField] private string title = "目前目標";
    [SerializeField] private int fontSize = 18;
    [SerializeField] private Vector2 screenOffset = new Vector2(24f, 24f);
    [SerializeField] private Vector2 boxSize = new Vector2(360f, 78f);
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.72f);
    [SerializeField] private Color titleColor = new Color(1f, 0.86f, 0.45f, 1f);

    private static CurrentObjectiveDisplay instance;
    private GUIStyle boxStyle;
    private GUIStyle titleStyle;
    private GUIStyle objectiveStyle;
    private Texture2D backgroundTexture;

    public static string CurrentObjective => instance != null ? instance.currentObjective : string.Empty;

    public static void SetObjective(string objective)
    {
        if (string.IsNullOrWhiteSpace(objective))
        {
            return;
        }

        EnsureInstance();
        instance.currentObjective = objective;
    }

    public static void ClearObjective()
    {
        if (instance != null)
        {
            instance.currentObjective = string.Empty;
        }
    }

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        CurrentObjectiveDisplay existing = FindAnyObjectByType<CurrentObjectiveDisplay>();
        if (existing != null)
        {
            instance = existing;
            return;
        }

        GameObject objectiveObject = new GameObject("CurrentObjectiveDisplay");
        instance = objectiveObject.AddComponent<CurrentObjectiveDisplay>();
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

    private void OnGUI()
    {
        if (BackpackUI.IsOpen)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(currentObjective))
        {
            return;
        }

        EnsureStyles();

        float width = Mathf.Min(boxSize.x, Screen.width - screenOffset.x * 2f);
        Rect boxRect = new Rect(screenOffset.x, screenOffset.y, width, boxSize.y);
        GUI.Box(boxRect, GUIContent.none, boxStyle);

        Rect titleRect = new Rect(boxRect.x + 16f, boxRect.y + 10f, boxRect.width - 32f, 24f);
        GUI.Label(titleRect, title, titleStyle);

        Rect objectiveRect = new Rect(boxRect.x + 16f, boxRect.y + 38f, boxRect.width - 32f, boxRect.height - 46f);
        GUI.Label(objectiveRect, currentObjective, objectiveStyle);
    }

    private void EnsureStyles()
    {
        if (backgroundTexture == null)
        {
            backgroundTexture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            backgroundTexture.SetPixel(0, 0, backgroundColor);
            backgroundTexture.Apply();
        }

        boxStyle ??= new GUIStyle(GUI.skin.box)
        {
            normal = { background = backgroundTexture },
            padding = new RectOffset(14, 14, 10, 10)
        };

        titleStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = titleColor }
        };

        objectiveStyle ??= new GUIStyle(GUI.skin.label)
        {
            wordWrap = true,
            normal = { textColor = Color.white }
        };

        titleStyle.fontSize = Mathf.Max(14, fontSize - 2);
        objectiveStyle.fontSize = fontSize;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        if (backgroundTexture != null)
        {
            Destroy(backgroundTexture);
        }
    }
}

public static class RuntimeHudLayout
{
    private const float Margin = 20f;
    private const float Gap = 4f;

    private static int frame = -1;
    private static EventType eventType;
    private static float nextTop;
    private static float nextTopCenter;

    public static Rect ReserveTopLeft(float requestedWidth, float requestedHeight)
    {
        Event currentEvent = Event.current;
        EventType currentType = currentEvent != null ? currentEvent.type : EventType.Repaint;
        if (frame != Time.frameCount || eventType != currentType)
        {
            frame = Time.frameCount;
            eventType = currentType;
            nextTop = Margin;
            nextTopCenter = Margin;
        }

        float width = ResponsiveWidth(requestedWidth);
        float height = Mathf.Max(22f, requestedHeight);
        Rect rect = new Rect(Margin, nextTop, width, height);
        nextTop += height + Gap;
        return rect;
    }

    public static Rect ReserveTopLeftText(string text, GUIStyle style, float requestedWidth, float minHeight = 24f, float verticalPadding = 6f)
    {
        float width = ResponsiveWidth(requestedWidth);
        float contentWidth = Mathf.Max(24f, width - style.padding.horizontal);
        float textHeight = style.CalcHeight(new GUIContent(text), contentWidth);
        float height = Mathf.Max(minHeight, Mathf.Ceil(textHeight + verticalPadding));
        return ReserveTopLeft(width, height);
    }

    public static Rect ReserveTopCenter(float requestedWidth, float requestedHeight)
    {
        ResetStacksIfNeeded();

        float width = ResponsiveWidth(requestedWidth);
        float height = Mathf.Max(22f, requestedHeight);
        Rect rect = new Rect((Screen.width - width) * 0.5f, nextTopCenter, width, height);
        nextTopCenter += height + Gap;
        return rect;
    }

    public static Rect ReserveTopCenterText(string text, GUIStyle style, float requestedWidth, float minHeight = 24f, float verticalPadding = 6f)
    {
        float width = ResponsiveWidth(requestedWidth);
        float contentWidth = Mathf.Max(24f, width - style.padding.horizontal);
        float textHeight = style.CalcHeight(new GUIContent(text), contentWidth);
        float height = Mathf.Max(minHeight, Mathf.Ceil(textHeight + verticalPadding));
        return ReserveTopCenter(width, height);
    }

    public static int FontSize(int desired, int min = 12, int max = 20)
    {
        int screenBasedMax = Screen.width < 360 || Screen.height < 260 ? 13 : Screen.width < 480 || Screen.height < 320 ? 14 : max;
        return Mathf.Clamp(desired, min, screenBasedMax);
    }

    public static float PanelScale()
    {
        float widthScale = Screen.width / 960f;
        float heightScale = Screen.height / 540f;
        return Mathf.Clamp(Mathf.Min(widthScale, heightScale), 0.62f, 1f);
    }

    private static float ResponsiveWidth(float requestedWidth)
    {
        float availableWidth = Mathf.Max(80f, Screen.width - Margin * 2f);
        float minWidth = Mathf.Min(120f, availableWidth);
        return Mathf.Clamp(requestedWidth, minWidth, availableWidth);
    }

    private static void ResetStacksIfNeeded()
    {
        Event currentEvent = Event.current;
        EventType currentType = currentEvent != null ? currentEvent.type : EventType.Repaint;
        if (frame == Time.frameCount && eventType == currentType)
        {
            return;
        }

        frame = Time.frameCount;
        eventType = currentType;
        nextTop = Margin;
        nextTopCenter = Margin;
    }
}
