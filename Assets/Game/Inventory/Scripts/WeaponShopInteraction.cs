using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider2D))]
public class WeaponShopInteraction : MonoBehaviour
{
    private const string HerbIconResourcePath = "Game/Generated/Day26/Consumables/herb";

    [SerializeField] private string interactPrompt = "按 T 打開拳套與符文交易";
    [SerializeField] private WeaponItem[] weaponsForSale =
    {
        new WeaponItem("common_training_gauntlet", "練習拳套", WeaponQuality.Common, 10, 35, 6, null, EquipmentKind.Gauntlet),
        new WeaponItem("common_leather_gauntlet", "皮革拳套", WeaponQuality.Common, 12, 55, 9, null, EquipmentKind.Gauntlet),
        new WeaponItem("elite_iron_gauntlet", "精鐵拳套", WeaponQuality.Elite, 30, 260, 42, null, EquipmentKind.Gauntlet),
        new WeaponItem("common_cracked_rune", "裂紋符文", WeaponQuality.Common, 2, 40, 8, null, EquipmentKind.Rune),
        new WeaponItem("common_spark_rune", "微光符文", WeaponQuality.Common, 5, 80, 14, null, EquipmentKind.Rune),
        new WeaponItem("elite_flame_rune", "焰紋符文", WeaponQuality.Elite, 18, 240, 38, null, EquipmentKind.Rune),
        new WeaponItem("elite_guard_rune", "守心符文", WeaponQuality.Elite, 22, 320, 48, null, EquipmentKind.Rune),
        new WeaponItem("epic_red_edge_rune", "赤鋒符文", WeaponQuality.Epic, 200, 1080, 220, null, EquipmentKind.Rune)
    };

    private bool playerInRange;
    private bool shopOpen;
    private string statusMessage = "E 交談，T 買賣。赤鋒符文很貴，森林掉落金幣後再回來也可以。";
    private Vector2 saleScrollPosition;
    private Vector2 inventoryScrollPosition;
    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;
    private BoxCollider2D interactionCollider;
    private Sprite herbIcon;
    private string SerializedInteractPrompt => interactPrompt;

    private void Awake()
    {
        interactionCollider = GetComponent<BoxCollider2D>();
        interactionCollider.isTrigger = true;
        NormalizeSaleItems();
        PlayerInventory.Ensure();
    }

    private void Update()
    {
        RefreshPlayerInRange();
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (playerInRange && keyboard.tKey.wasPressedThisFrame)
        {
            shopOpen = !shopOpen;
        }

        if (shopOpen && keyboard.escapeKey.wasPressedThisFrame)
        {
            shopOpen = false;
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
        if (IsPlayer(other) && !IsPlayerNearTradingArea())
        {
            playerInRange = false;
            shopOpen = false;
        }
    }

    private void OnGUI()
    {
        EnsureStyles();

        if (BackpackUI.IsOpen)
        {
            return;
        }

        if (!shopOpen)
        {
            return;
        }

        PlayerInventory inventory = PlayerInventory.Ensure();
        Rect panelRect = new Rect(
            Mathf.Max(18f, (Screen.width - 920f) * 0.5f),
            56f,
            Mathf.Min(920f, Screen.width - 36f),
            Mathf.Min(Screen.height - 96f, 620f));
        GUI.Box(panelRect, GUIContent.none, panelStyle);

        GUILayout.BeginArea(new Rect(panelRect.x + 18f, panelRect.y + 14f, panelRect.width - 36f, panelRect.height - 28f));
        GUILayout.BeginHorizontal();
        GUILayout.Label("拳套與符文", titleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label($"金幣：{inventory.Gold}  藥草：{inventory.Herbs}", labelStyle, GUILayout.Width(230f));
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
        DrawHerbRow(inventory);
        GUILayout.Space(8f);

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width((panelRect.width - 48f) * 0.5f));
        GUILayout.Label("武器匠販售", labelStyle);
        saleScrollPosition = GUILayout.BeginScrollView(saleScrollPosition, GUILayout.Height(panelRect.height - 168f));
        for (int i = 0; i < weaponsForSale.Length; i++)
        {
            DrawSaleRow(inventory, weaponsForSale[i]);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.Space(12f);

        GUILayout.BeginVertical(GUILayout.Width((panelRect.width - 48f) * 0.5f));
        GUILayout.Label("玩家背包", labelStyle);
        inventoryScrollPosition = GUILayout.BeginScrollView(inventoryScrollPosition, GUILayout.Height(panelRect.height - 168f));
        var ownedWeapons = inventory.GetWeapons();
        for (int i = 0; i < ownedWeapons.Count; i++)
        {
            DrawInventoryRow(inventory, ownedWeapons[i], i);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.Label(statusMessage, labelStyle);
        GUILayout.Label("T：關閉商店 / Esc：關閉商店", labelStyle);
        GUILayout.EndArea();
    }

    private void DrawSaleRow(PlayerInventory inventory, WeaponItem weapon)
    {
        weapon.NormalizeForCurrentStory();
        GUILayout.BeginHorizontal();
        DrawWeaponIcon(weapon);
        string traitText = weapon.GetTraitText();
        GUILayout.Label(string.IsNullOrWhiteSpace(traitText)
            ? $"{weapon.displayName}\n{KindText(weapon.itemType)} / {QualityText(weapon.quality)} / {PowerText(weapon)} / 價格 {weapon.buyPrice}"
            : $"{weapon.displayName}\n{KindText(weapon.itemType)} / {QualityText(weapon.quality)} / {PowerText(weapon)} / 價格 {weapon.buyPrice}\n{traitText}", labelStyle, GUILayout.Width(250f));
        if (GUILayout.Button("購買", buttonStyle, GUILayout.Width(86f), GUILayout.Height(28f)))
        {
            inventory.TryBuyWeapon(weapon, out statusMessage);
        }
        GUILayout.EndHorizontal();
    }

    private void DrawHerbRow(PlayerInventory inventory)
    {
        GUILayout.BeginHorizontal();
        DrawHerbIcon();
        GUILayout.Label("藥草\n消耗品 / 使用後回滿生命 / 價格 1", labelStyle, GUILayout.Width(258f));
        if (GUILayout.Button("購買", buttonStyle, GUILayout.Width(86f), GUILayout.Height(28f)))
        {
            inventory.TryBuyHerb(out statusMessage);
        }

        GUILayout.EndHorizontal();
    }

    private void DrawHerbIcon()
    {
        EnsureHerbIcon();
        Rect iconRect = GUILayoutUtility.GetRect(42f, 42f, GUILayout.Width(42f), GUILayout.Height(42f));
        if (herbIcon == null)
        {
            GUI.Box(iconRect, string.Empty);
            return;
        }

        Rect textureRect = GetTextureCoords(herbIcon);
        GUI.DrawTextureWithTexCoords(iconRect, herbIcon.texture, textureRect, true);
    }

    private void DrawInventoryRow(PlayerInventory inventory, PlayerInventory.WeaponStack stack, int slotIndex)
    {
        WeaponItem weapon = stack.weapon;
        weapon.NormalizeForCurrentStory();
        bool equipped = inventory.IsSlotEquipped(slotIndex);

        GUILayout.BeginHorizontal();
        DrawWeaponIcon(weapon);
        string traitText = weapon.GetTraitText();
        GUILayout.Label(string.IsNullOrWhiteSpace(traitText)
            ? $"#{slotIndex + 1} {weapon.displayName}\n{KindText(weapon.itemType)} / {QualityText(weapon.quality)} / {PowerText(weapon)} / 售價 {weapon.sellPrice}"
            : $"#{slotIndex + 1} {weapon.displayName}\n{KindText(weapon.itemType)} / {QualityText(weapon.quality)} / {PowerText(weapon)} / 售價 {weapon.sellPrice}\n{traitText}", labelStyle, GUILayout.Width(220f));

        GUI.enabled = !equipped;
        if (GUILayout.Button(equipped ? EquippedText(weapon.itemType) : EquipActionText(weapon.itemType), buttonStyle, GUILayout.Width(70f), GUILayout.Height(28f)))
        {
            inventory.TryEquipWeaponAt(slotIndex, out statusMessage);
        }

        GUI.enabled = true;
        if (GUILayout.Button("出售", buttonStyle, GUILayout.Width(70f), GUILayout.Height(28f)))
        {
            inventory.TrySellWeaponAt(slotIndex, out statusMessage);
        }

        GUILayout.EndHorizontal();
    }

    private static void DrawWeaponIcon(WeaponItem weapon)
    {
        weapon.EnsureIcon();
        Rect iconRect = GUILayoutUtility.GetRect(42f, 42f, GUILayout.Width(42f), GUILayout.Height(42f));
        if (weapon.icon == null)
        {
            GUI.Box(iconRect, string.Empty);
            return;
        }

        Rect textureRect = GetTextureCoords(weapon.icon);
        GUI.DrawTextureWithTexCoords(iconRect, weapon.icon.texture, textureRect, true);
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.GetComponent<PlayerMovement>() != null
            || other.GetComponentInParent<PlayerMovement>() != null
            || other.gameObject.name == "Player";
    }

    private void RefreshPlayerInRange()
    {
        if (IsPlayerNearTradingArea())
        {
            playerInRange = true;
            return;
        }

        playerInRange = false;
        shopOpen = false;
    }

    private bool IsPlayerNearTradingArea()
    {
        Transform player = GetClosestPlayerTransform();
        if (player == null)
        {
            return false;
        }

        Vector2 playerPosition = player.position;
        if (interactionCollider != null)
        {
            Vector2 closestPoint = interactionCollider.ClosestPoint(playerPosition);
            if (Vector2.Distance(playerPosition, closestPoint) <= 0.75f)
            {
                return true;
            }

            float colliderRange = Mathf.Clamp(interactionCollider.bounds.extents.magnitude + 1.2f, 2.5f, 8f);
            return Vector2.Distance(transform.position, playerPosition) <= colliderRange;
        }

        return Vector2.Distance(transform.position, playerPosition) <= 3f;
    }

    private Transform GetClosestPlayerTransform()
    {
        Transform closestPlayer = null;
        float closestDistance = float.MaxValue;

        PlayerMovement[] playerMovements = FindObjectsByType<PlayerMovement>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < playerMovements.Length; i++)
        {
            CheckClosestPlayer(playerMovements[i].transform, ref closestPlayer, ref closestDistance);
        }

        GameObject[] taggedPlayers = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < taggedPlayers.Length; i++)
        {
            CheckClosestPlayer(taggedPlayers[i].transform, ref closestPlayer, ref closestDistance);
        }

        GameObject namedPlayer = GameObject.Find("Player");
        if (namedPlayer != null)
        {
            CheckClosestPlayer(namedPlayer.transform, ref closestPlayer, ref closestDistance);
        }

        return closestPlayer;
    }

    private void CheckClosestPlayer(Transform candidate, ref Transform closestPlayer, ref float closestDistance)
    {
        if (candidate == null || !candidate.gameObject.activeInHierarchy)
        {
            return;
        }

        Vector2 candidatePosition = candidate.position;
        Vector2 referencePoint = interactionCollider != null
            ? interactionCollider.ClosestPoint(candidatePosition)
            : (Vector2)transform.position;
        float distance = Vector2.Distance(candidatePosition, referencePoint);
        if (distance < closestDistance)
        {
            closestDistance = distance;
            closestPlayer = candidate;
        }
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

    private static string PowerText(WeaponItem item)
    {
        return item.itemType == EquipmentKind.Rune ? $"加成 {item.damage}" : $"傷害 {item.damage}";
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
        panelStyle ??= new GUIStyle(GUI.skin.box);
        if (panelStyle.normal.background == null)
        {
            panelStyle.normal.background = MakeTexture(new Color(0.08f, 0.07f, 0.06f, 0.92f));
        }

        titleStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.88f, 0.58f, 1f) }
        };

        labelStyle ??= new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = Color.white },
            wordWrap = true
        };

        buttonStyle ??= new GUIStyle(GUI.skin.button);

        titleStyle.fontSize = 24;
        labelStyle.fontSize = 17;
        buttonStyle.fontSize = 16;
    }

    private static Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private void NormalizeSaleItems()
    {
        if (weaponsForSale == null)
        {
            return;
        }

        for (int i = 0; i < weaponsForSale.Length; i++)
        {
            weaponsForSale[i]?.NormalizeForCurrentStory();
        }
    }

    private void EnsureHerbIcon()
    {
        if (herbIcon != null)
        {
            return;
        }

        Texture2D texture = Resources.Load<Texture2D>(HerbIconResourcePath);
        if (texture == null)
        {
            return;
        }

        herbIcon = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            64f);
    }
}
