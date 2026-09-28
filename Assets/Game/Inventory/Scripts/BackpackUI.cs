using UnityEngine;
using UnityEngine.InputSystem;

public class BackpackUI : MonoBehaviour
{
    [SerializeField] private int columns = 5;
    [SerializeField] private int visibleRows = 3;

    private bool backpackOpen;
    private Vector2 scrollPosition;
    private string statusMessage = "按 B 打開背包，點選拳套或符文可以切換裝備。";
    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle labelStyle;
    private GUIStyle slotStyle;
    private GUIStyle equippedSlotStyle;
    private GUIStyle buttonStyle;
    private bool holdingEnemyPause;

    public static bool IsOpen { get; private set; }

    private void Awake()
    {
        PlayerInventory.Ensure();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.bKey.wasPressedThisFrame)
        {
            SetBackpackOpen(!backpackOpen);
        }

        if (backpackOpen && keyboard.escapeKey.wasPressedThisFrame)
        {
            SetBackpackOpen(false);
        }
    }

    private void OnDisable()
    {
        SetBackpackOpen(false);
    }

    private void SetBackpackOpen(bool open)
    {
        if (backpackOpen == open)
        {
            IsOpen = open;
            return;
        }

        backpackOpen = open;
        IsOpen = open;

        if (open)
        {
            RequestEnemyPauseIfNeeded();
        }
        else
        {
            ReleaseEnemyPauseIfNeeded();
        }
    }

    private void RequestEnemyPauseIfNeeded()
    {
        if (holdingEnemyPause)
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

    private void OnGUI()
    {
        EnsureStyles();

        if (!backpackOpen)
        {
            return;
        }

        PlayerInventory inventory = PlayerInventory.Ensure();
        Rect panelRect = new Rect(
            Mathf.Max(18f, (Screen.width - 620f) * 0.5f),
            56f,
            Mathf.Min(620f, Screen.width - 36f),
            Mathf.Min(520f, Screen.height - 96f));

        GUI.Box(panelRect, GUIContent.none, panelStyle);
        GUILayout.BeginArea(new Rect(panelRect.x + 18f, panelRect.y + 14f, panelRect.width - 36f, panelRect.height - 28f));

        GUILayout.BeginHorizontal();
        GUILayout.Label("背包", titleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label($"金幣：{inventory.Gold}  藥草：{inventory.Herbs}", labelStyle, GUILayout.Width(210f));
        GUILayout.EndHorizontal();

        WeaponItem equippedGauntlet = inventory.GetEquippedGauntlet();
        WeaponItem equippedRune = inventory.GetEquippedRune();
        GUILayout.Label(equippedGauntlet != null
            ? $"目前拳套：{equippedGauntlet.displayName} / {QualityText(equippedGauntlet.quality)} / 傷害 {equippedGauntlet.damage}"
            : "目前拳套：無", labelStyle);
        GUILayout.Label(equippedRune != null
            ? $"目前符文：{equippedRune.displayName} / {QualityText(equippedRune.quality)} / 加成 {equippedRune.damage}"
            : "目前符文：無", labelStyle);
        GUILayout.Label($"拳擊總傷害：{inventory.GetAttackDamage()}", labelStyle);

        GUILayout.Space(8f);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(visibleRows * 118f + 18f));
        DrawWeaponGrid(inventory);
        GUILayout.EndScrollView();

        GUILayout.FlexibleSpace();
        GUILayout.Label(statusMessage, labelStyle);
        GUILayout.Label("B / Esc：關閉背包，U：使用藥草", labelStyle);
        GUILayout.EndArea();
    }

    private void DrawWeaponGrid(PlayerInventory inventory)
    {
        var weapons = inventory.GetWeapons();
        int totalSlots = Mathf.Max(columns * visibleRows, Mathf.CeilToInt(weapons.Count / (float)columns) * columns);

        for (int slotIndex = 0; slotIndex < totalSlots; slotIndex++)
        {
            if (slotIndex % columns == 0)
            {
                GUILayout.BeginHorizontal();
            }

            if (slotIndex < weapons.Count)
            {
                DrawWeaponSlot(inventory, weapons[slotIndex].weapon, slotIndex);
            }
            else
            {
                GUILayout.Box(string.Empty, slotStyle, GUILayout.Width(108f), GUILayout.Height(108f));
            }

            if (slotIndex % columns == columns - 1)
            {
                GUILayout.EndHorizontal();
            }
        }
    }

    private void DrawWeaponSlot(PlayerInventory inventory, WeaponItem weapon, int slotIndex)
    {
        weapon.NormalizeForCurrentStory();
        weapon.EnsureIcon();
        bool equipped = inventory.IsSlotEquipped(slotIndex);
        GUILayout.BeginVertical(equipped ? equippedSlotStyle : slotStyle, GUILayout.Width(108f), GUILayout.Height(108f));

        Rect iconRect = GUILayoutUtility.GetRect(42f, 42f, GUILayout.Width(42f), GUILayout.Height(42f));
        if (weapon.icon != null)
        {
            Rect textureRect = GetTextureCoords(weapon.icon);
            GUI.DrawTextureWithTexCoords(iconRect, weapon.icon.texture, textureRect, true);
        }
        else
        {
            GUI.Box(iconRect, string.Empty);
        }

        GUILayout.Label(weapon.displayName, labelStyle, GUILayout.Height(22f));
        string traitText = weapon.GetTraitText();
        GUILayout.Label(string.IsNullOrWhiteSpace(traitText)
            ? $"{KindText(weapon.itemType)} / {QualityText(weapon.quality)}"
            : $"{KindText(weapon.itemType)} / {QualityText(weapon.quality)}\n{traitText}", labelStyle, GUILayout.Height(34f));

        GUI.enabled = !equipped;
        if (GUILayout.Button(equipped ? EquippedText(weapon.itemType) : EquipActionText(weapon.itemType), buttonStyle, GUILayout.Height(24f)))
        {
            inventory.TryEquipWeaponAt(slotIndex, out statusMessage);
        }
        GUI.enabled = true;

        GUILayout.EndVertical();
    }

    private static string QualityText(WeaponQuality quality)
    {
        return quality switch
        {
            WeaponQuality.Common => "普通",
            WeaponQuality.Elite => "精英",
            WeaponQuality.Epic => "史詩",
            WeaponQuality.Legendary => "傳說",
            _ => quality.ToString()
        };
    }

    private static string KindText(EquipmentKind kind)
    {
        return kind == EquipmentKind.Rune ? "符文" : "拳套";
    }

    private static string EquipActionText(EquipmentKind kind)
    {
        return kind == EquipmentKind.Rune ? "鑲嵌" : "裝備";
    }

    private static string EquippedText(EquipmentKind kind)
    {
        return kind == EquipmentKind.Rune ? "鑲嵌中" : "裝備中";
    }

    private static Rect GetTextureCoords(Sprite sprite)
    {
        Rect rect = sprite.textureRect;
        Texture texture = sprite.texture;
        return new Rect(
            rect.x / texture.width,
            rect.y / texture.height,
            rect.width / texture.width,
            rect.height / texture.height);
    }

    private void EnsureStyles()
    {
        if (panelStyle != null)
        {
            return;
        }

        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.normal.background = MakeTexture(new Color(0.07f, 0.06f, 0.05f, 0.94f));

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.88f, 0.58f, 1f) }
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 15,
            normal = { textColor = Color.white },
            wordWrap = true
        };

        slotStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(6, 6, 5, 5)
        };

        equippedSlotStyle = new GUIStyle(slotStyle);
        equippedSlotStyle.normal.textColor = Color.white;
        equippedSlotStyle.normal.background = MakeTexture(new Color(0.28f, 0.22f, 0.08f, 0.92f));

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14
        };
    }

    private static Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
