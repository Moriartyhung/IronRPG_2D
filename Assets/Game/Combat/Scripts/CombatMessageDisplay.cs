using System.Collections.Generic;
using UnityEngine;

public class CombatMessageDisplay : MonoBehaviour
{
    private struct MessageEntry
    {
        public string Text;
        public float EndTime;
    }

    private static readonly List<MessageEntry> Messages = new List<MessageEntry>();
    private GUIStyle messageStyle;

    public static void Show(string text, float duration = 2.2f)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        Ensure();
        Messages.Add(new MessageEntry
        {
            Text = text,
            EndTime = Time.time + duration
        });

        if (Messages.Count > 5)
        {
            Messages.RemoveAt(0);
        }
    }

    public static CombatMessageDisplay Ensure()
    {
        CombatMessageDisplay existing = FindAnyObjectByType<CombatMessageDisplay>();
        if (existing != null)
        {
            return existing;
        }

        GameObject displayObject = new GameObject("CombatMessageDisplay");
        return displayObject.AddComponent<CombatMessageDisplay>();
    }

    private void OnGUI()
    {
        EnsureStyle();

        for (int i = Messages.Count - 1; i >= 0; i--)
        {
            if (Time.time > Messages[i].EndTime)
            {
                Messages.RemoveAt(i);
            }
        }

        if (BackpackUI.IsOpen)
        {
            return;
        }

        float scale = RuntimeHudLayout.PanelScale();
        float margin = Mathf.Max(18f, 24f * scale);
        float width = Mathf.Clamp(380f * scale, 220f, Mathf.Max(220f, Screen.width - margin * 2f));
        float y = Mathf.Max(86f, 96f * scale);
        for (int i = 0; i < Messages.Count; i++)
        {
            float textHeight = messageStyle.CalcHeight(new GUIContent(Messages[i].Text), width);
            Rect rect = new Rect(
                Screen.width - width - margin,
                y,
                width,
                Mathf.Max(24f, Mathf.Ceil(textHeight + 6f)));
            GUI.Label(rect, Messages[i].Text, messageStyle);
            y += rect.height + 4f;
        }
    }

    private void EnsureStyle()
    {
        messageStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleRight,
            wordWrap = true,
            normal = { textColor = new Color(1f, 0.92f, 0.58f, 1f) }
        };
        messageStyle.fontSize = RuntimeHudLayout.FontSize(18, 12, 18);
    }
}
