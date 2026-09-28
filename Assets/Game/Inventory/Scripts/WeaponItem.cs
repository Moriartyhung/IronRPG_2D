using System;
using UnityEngine;

public enum EquipmentKind
{
    Gauntlet,
    Rune
}

[Serializable]
public class WeaponItem
{
    private const string EquipmentIconResourceRoot = "Game/Generated/Day26/GauntletsRunes";

    public string weaponId = "weapon";
    public string displayName = "裝備";
    public EquipmentKind itemType = EquipmentKind.Gauntlet;
    public WeaponQuality quality = WeaponQuality.Common;
    public Sprite icon;
    public int damage = 1;
    public int buyPrice = 0;
    public int sellPrice = 0;

    public WeaponItem()
    {
    }

    public WeaponItem(string weaponId, string displayName, WeaponQuality quality, int damage, int buyPrice, int sellPrice, Sprite icon = null, EquipmentKind itemType = EquipmentKind.Gauntlet)
    {
        this.weaponId = weaponId;
        this.displayName = displayName;
        this.itemType = itemType;
        this.quality = quality;
        this.icon = icon;
        this.damage = damage;
        this.buyPrice = buyPrice;
        this.sellPrice = sellPrice;
    }

    public WeaponItem Clone()
    {
        NormalizeForCurrentStory();
        EnsureIcon();
        return new WeaponItem(weaponId, displayName, quality, damage, buyPrice, sellPrice, icon, itemType);
    }

    public void NormalizeForCurrentStory()
    {
        switch (weaponId)
        {
            case "starter_gauntlet":
            case "starter_knife":
            case "common_wooden_club":
                ApplyEquipment("common_training_gauntlet", "練習拳套", EquipmentKind.Gauntlet, WeaponQuality.Common, 10, 35, 6);
                break;
            case "rusty_axe":
            case "common_bone_dagger":
                ApplyEquipment("common_leather_gauntlet", "皮革拳套", EquipmentKind.Gauntlet, WeaponQuality.Common, 12, 55, 9);
                break;
            case "cracked_rune":
            case "common_iron_knife":
                ApplyEquipment("common_cracked_rune", "裂紋符文", EquipmentKind.Rune, WeaponQuality.Common, 2, 40, 8);
                break;
            case "common_short_spear":
                ApplyEquipment("common_spark_rune", "微光符文", EquipmentKind.Rune, WeaponQuality.Common, 5, 80, 14);
                break;
            case "common_hunter_axe":
            case "elite_steel_axe":
                ApplyEquipment("elite_iron_gauntlet", "精鐵拳套", EquipmentKind.Gauntlet, WeaponQuality.Elite, 30, 260, 42);
                break;
            case "elite_rune_book":
                ApplyEquipment("elite_flame_rune", "焰紋符文", EquipmentKind.Rune, WeaponQuality.Elite, 18, 240, 38);
                break;
            case "elite_war_hammer":
                ApplyEquipment("elite_guard_rune", "守心符文", EquipmentKind.Rune, WeaponQuality.Elite, 22, 320, 48);
                break;
            case "epic_red_edge":
                ApplyEquipment("epic_red_edge_rune", "赤鋒符文", EquipmentKind.Rune, WeaponQuality.Epic, 200, 1080, 220);
                break;
            case "legendary_thunder_sword":
                ApplyEquipment("legendary_thunder_rune", "雷痕符文", EquipmentKind.Rune, WeaponQuality.Legendary, 300, 0, 0);
                break;
        }

        EnsureIcon();
    }

    public void EnsureIcon()
    {
        if (icon != null || string.IsNullOrWhiteSpace(weaponId))
        {
            return;
        }

        Texture2D texture = Resources.Load<Texture2D>($"{EquipmentIconResourceRoot}/{weaponId}");
        if (texture == null)
        {
            return;
        }

        icon = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            64f);
    }

    private void ApplyEquipment(string id, string itemName, EquipmentKind kind, WeaponQuality itemQuality, int itemDamage, int itemBuyPrice, int itemSellPrice)
    {
        weaponId = id;
        displayName = itemName;
        itemType = kind;
        quality = itemQuality;
        damage = itemDamage;
        buyPrice = itemBuyPrice;
        sellPrice = itemSellPrice;
        icon = null;
    }

    public string GetTraitText()
    {
        return weaponId == "legendary_thunder_rune" ? "詞條：對 Boss 增傷 300%" : string.Empty;
    }
}
