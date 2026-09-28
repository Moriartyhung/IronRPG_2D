using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [Serializable]
    public class WeaponStack
    {
        public WeaponItem weapon = new WeaponItem();
        public int count = 1;
    }

    [SerializeField] private int startingGold = 80;
    [SerializeField] private int startingHerbs;
    [SerializeField] private List<WeaponStack> startingWeapons = new List<WeaponStack>();

    private readonly List<WeaponStack> weapons = new List<WeaponStack>();
    private int gold;
    private int herbs;
    private string equippedWeaponId;
    private string equippedRuneId;
    private int equippedGauntletIndex = -1;
    private int equippedRuneIndex = -1;

    public static PlayerInventory Instance { get; private set; }
    public int Gold => gold;
    public int Herbs => herbs;
    public int EquippedWeaponIndex => equippedGauntletIndex;
    public int EquippedGauntletIndex => equippedGauntletIndex;
    public int EquippedRuneIndex => equippedRuneIndex;
    public string EquippedWeaponId => GetEquippedGauntlet()?.weaponId ?? equippedWeaponId;
    public string EquippedRuneId => GetEquippedRune()?.weaponId ?? equippedRuneId;
    public int EquippedDamage => GetAttackDamage();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureBackpackUI();
        ResetRuntimeState();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.uKey.wasPressedThisFrame)
        {
            UseHerb();
        }
    }

    public static PlayerInventory Ensure()
    {
        if (Instance != null)
        {
            Instance.EnsureBackpackUI();
            return Instance;
        }

        PlayerInventory existing = FindAnyObjectByType<PlayerInventory>();
        if (existing != null)
        {
            Instance = existing;
            existing.EnsureBackpackUI();
            return existing;
        }

        GameObject inventoryObject = new GameObject("PlayerInventory");
        PlayerInventory inventory = inventoryObject.AddComponent<PlayerInventory>();
        inventory.EnsureBackpackUI();
        return inventory;
    }

    public static void AddGold(int amount)
    {
        Ensure().AddGoldInternal(amount);
    }

    public static void ClearGold()
    {
        Ensure().gold = 0;
    }

    public static void ResetForNewGame()
    {
        Ensure().ResetRuntimeState();
    }

    public static void AddHerbs(int amount)
    {
        Ensure().AddHerbsInternal(amount);
    }

    public static void AddWeapon(WeaponItem weapon)
    {
        Ensure().AddWeaponInternal(weapon);
    }

    public bool TryBuyWeapon(WeaponItem weapon, out string message)
    {
        if (weapon == null)
        {
            message = "沒有可購買的裝備。";
            return false;
        }

        weapon.NormalizeForCurrentStory();
        if (gold < weapon.buyPrice)
        {
            message = $"金幣不足，還需要 {weapon.buyPrice - gold} 枚金幣。";
            return false;
        }

        gold -= weapon.buyPrice;
        AddWeaponInternal(weapon);
        message = $"買下 {weapon.displayName}。";
        return true;
    }

    public bool TryBuyHerb(out string message)
    {
        if (gold < 1)
        {
            message = "金幣不足，無法購買藥草。";
            return false;
        }

        gold -= 1;
        AddHerbsInternal(1);
        message = "購買藥草。";
        return true;
    }

    public bool UseHerb()
    {
        if (herbs <= 0)
        {
            CombatMessageDisplay.Show("背包裡沒有藥草，無法使用。");
            return false;
        }

        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            CombatMessageDisplay.Show("找不到主角，無法使用藥草。");
            return false;
        }

        herbs--;
        playerHealth.HealToFull();
        CombatMessageDisplay.Show($"使用藥草，生命回滿。剩餘藥草：{herbs}");
        return true;
    }

    public bool TrySellWeapon(string weaponId, out string message)
    {
        int slotIndex = FindFirstSlotIndex(weaponId, false);
        if (slotIndex < 0)
        {
            message = "背包裡沒有這件裝備。";
            return false;
        }

        return TrySellWeaponAt(slotIndex, out message);
    }

    public bool TrySellWeaponAt(int slotIndex, out string message)
    {
        if (!IsValidSlot(slotIndex))
        {
            message = "這個背包格沒有裝備。";
            return false;
        }

        if (IsSlotEquipped(slotIndex))
        {
            message = "已裝備的拳套或符文不能出售，請先切換其他裝備。";
            return false;
        }

        WeaponStack stack = weapons[slotIndex];
        string soldName = stack.weapon.displayName;
        int sellPrice = stack.weapon.sellPrice;
        gold += sellPrice;
        weapons.RemoveAt(slotIndex);

        if (slotIndex < equippedGauntletIndex)
        {
            equippedGauntletIndex--;
        }

        if (slotIndex < equippedRuneIndex)
        {
            equippedRuneIndex--;
        }

        message = $"出售 {soldName}，獲得 {sellPrice} 枚金幣。";
        return true;
    }

    public bool TryEquipWeapon(string weaponId, out string message)
    {
        int slotIndex = FindFirstSlotIndex(weaponId, true);
        if (slotIndex < 0)
        {
            message = "背包裡沒有這件裝備。";
            return false;
        }

        return TryEquipWeaponAt(slotIndex, out message);
    }

    public bool TryEquipWeaponAt(int slotIndex, out string message)
    {
        if (!IsValidSlot(slotIndex))
        {
            message = "這個背包格沒有裝備。";
            return false;
        }

        WeaponStack stack = weapons[slotIndex];
        if (stack.weapon.itemType == EquipmentKind.Rune)
        {
            equippedRuneIndex = slotIndex;
            equippedRuneId = stack.weapon.weaponId;
            message = $"鑲嵌 {stack.weapon.displayName}。";
            return true;
        }

        equippedGauntletIndex = slotIndex;
        equippedWeaponId = stack.weapon.weaponId;
        message = $"裝備 {stack.weapon.displayName}。";
        return true;
    }

    public WeaponItem GetEquippedWeapon()
    {
        return GetEquippedGauntlet();
    }

    public WeaponItem GetEquippedGauntlet()
    {
        if (IsValidSlot(equippedGauntletIndex))
        {
            return weapons[equippedGauntletIndex].weapon;
        }

        int fallbackIndex = FindFirstSlotIndex(equippedWeaponId, true, EquipmentKind.Gauntlet);
        if (fallbackIndex >= 0)
        {
            equippedGauntletIndex = fallbackIndex;
            return weapons[fallbackIndex].weapon;
        }

        return null;
    }

    public WeaponItem GetEquippedRune()
    {
        if (IsValidSlot(equippedRuneIndex))
        {
            return weapons[equippedRuneIndex].weapon;
        }

        int fallbackIndex = FindFirstSlotIndex(equippedRuneId, true, EquipmentKind.Rune);
        if (fallbackIndex >= 0)
        {
            equippedRuneIndex = fallbackIndex;
            return weapons[fallbackIndex].weapon;
        }

        return null;
    }

    public int GetAttackDamage()
    {
        WeaponItem gauntlet = GetEquippedGauntlet();
        WeaponItem rune = GetEquippedRune();
        int damage = gauntlet != null ? gauntlet.damage : 0;
        damage += rune != null ? rune.damage : 0;
        damage += PlayerProgress.Ensure().DamageBonus;
        return damage;
    }

    public int GetBossDamageBonusPercent()
    {
        WeaponItem rune = GetEquippedRune();
        return rune != null && rune.weaponId == "legendary_thunder_rune" ? 300 : 0;
    }

    public bool IsSlotEquipped(int slotIndex)
    {
        return slotIndex == equippedGauntletIndex || slotIndex == equippedRuneIndex;
    }

    public IReadOnlyList<WeaponStack> GetWeapons()
    {
        return weapons;
    }

    public bool HasLegendaryWeapon()
    {
        return HasLegendaryRune();
    }

    public bool HasLegendaryRune()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].weapon.itemType == EquipmentKind.Rune && weapons[i].weapon.quality == WeaponQuality.Legendary)
            {
                return true;
            }
        }

        return false;
    }

    public bool HasEliteGauntletAndEpicRune()
    {
        WeaponItem gauntlet = GetEquippedGauntlet();
        WeaponItem rune = GetEquippedRune();
        return gauntlet != null
            && gauntlet.quality >= WeaponQuality.Elite
            && rune != null
            && rune.quality >= WeaponQuality.Epic;
    }

    public bool HasBossReadyEquipment()
    {
        WeaponItem gauntlet = GetEquippedGauntlet();
        WeaponItem rune = GetEquippedRune();
        return gauntlet != null
            && gauntlet.quality >= WeaponQuality.Elite
            && rune != null
            && rune.quality >= WeaponQuality.Legendary;
    }

    private void ResetRuntimeState()
    {
        gold = startingGold;
        herbs = Mathf.Max(0, startingHerbs);
        weapons.Clear();

        if (startingWeapons.Count == 0)
        {
            AddWeaponInternal(new WeaponItem("common_training_gauntlet", "練習拳套", WeaponQuality.Common, 10, 0, 5, null, EquipmentKind.Gauntlet));
            AddWeaponInternal(new WeaponItem("common_cracked_rune", "裂紋符文", WeaponQuality.Common, 2, 0, 3, null, EquipmentKind.Rune));
        }
        else
        {
            for (int i = 0; i < startingWeapons.Count; i++)
            {
                WeaponStack stack = startingWeapons[i];
                for (int j = 0; j < Mathf.Max(1, stack.count); j++)
                {
                    AddWeaponInternal(stack.weapon);
                }
            }
        }

        if (weapons.Count > 0)
        {
            equippedGauntletIndex = FindFirstSlotIndexByType(EquipmentKind.Gauntlet);
            equippedRuneIndex = FindFirstSlotIndexByType(EquipmentKind.Rune);
            equippedWeaponId = equippedGauntletIndex >= 0 ? weapons[equippedGauntletIndex].weapon.weaponId : null;
            equippedRuneId = equippedRuneIndex >= 0 ? weapons[equippedRuneIndex].weapon.weaponId : null;
        }
        else
        {
            equippedGauntletIndex = -1;
            equippedRuneIndex = -1;
            equippedWeaponId = null;
            equippedRuneId = null;
        }
    }

    private void EnsureBackpackUI()
    {
        if (GetComponent<BackpackUI>() == null)
        {
            gameObject.AddComponent<BackpackUI>();
        }
    }

    private void AddGoldInternal(int amount)
    {
        gold = Mathf.Max(0, gold + amount);
    }

    private void AddHerbsInternal(int amount)
    {
        herbs = Mathf.Max(0, herbs + amount);
    }

    private void AddWeaponInternal(WeaponItem weapon)
    {
        if (weapon == null || string.IsNullOrWhiteSpace(weapon.weaponId))
        {
            return;
        }

        weapon.NormalizeForCurrentStory();
        weapons.Add(new WeaponStack
        {
            weapon = weapon.Clone(),
            count = 1
        });

        if (weapon.itemType == EquipmentKind.Rune)
        {
            if (equippedRuneIndex < 0)
            {
                equippedRuneIndex = weapons.Count - 1;
                equippedRuneId = weapon.weaponId;
            }

            return;
        }

        if (equippedGauntletIndex < 0)
        {
            equippedGauntletIndex = weapons.Count - 1;
            equippedWeaponId = weapon.weaponId;
        }
    }

    private WeaponStack FindStack(string weaponId)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
        {
            return null;
        }

        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].weapon.weaponId == weaponId)
            {
                return weapons[i];
            }
        }

        return null;
    }

    private int FindFirstSlotIndex(string weaponId, bool allowEquipped)
    {
        return FindFirstSlotIndex(weaponId, allowEquipped, null);
    }

    private int FindFirstSlotIndex(string weaponId, bool allowEquipped, EquipmentKind? requiredType)
    {
        if (string.IsNullOrWhiteSpace(weaponId))
        {
            return -1;
        }

        for (int i = 0; i < weapons.Count; i++)
        {
            if (requiredType.HasValue && weapons[i].weapon.itemType != requiredType.Value)
            {
                continue;
            }

            if (weapons[i].weapon.weaponId == weaponId && (allowEquipped || !IsSlotEquipped(i)))
            {
                return i;
            }
        }

        return -1;
    }

    private int FindFirstSlotIndexByType(EquipmentKind type)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].weapon.itemType == type)
            {
                return i;
            }
        }

        return -1;
    }

    private bool IsValidSlot(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < weapons.Count;
    }
}
