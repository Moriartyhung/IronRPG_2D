using System;
using UnityEngine;

public class EnemyLootDrop : MonoBehaviour
{
    [Serializable]
    public class WeaponDrop
    {
        public WeaponItem weapon = new WeaponItem();
        [Range(0f, 1f)] public float chance = 0.12f;
    }

    [SerializeField] private int minGold = 5;
    [SerializeField] private int maxGold = 8;
    [SerializeField] private int minHerbs;
    [SerializeField] private int maxHerbs;
    [SerializeField] private WeaponDrop[] weaponDrops = Array.Empty<WeaponDrop>();

    public void ConfigureBasicLoot(int minGoldAmount, int maxGoldAmount, int minHerbAmount = 0, int maxHerbAmount = 0)
    {
        minGold = Mathf.Max(0, minGoldAmount);
        maxGold = Mathf.Max(minGold, maxGoldAmount);
        minHerbs = Mathf.Max(0, minHerbAmount);
        maxHerbs = Mathf.Max(minHerbs, maxHerbAmount);
        weaponDrops = Array.Empty<WeaponDrop>();
    }

    public void DropLoot()
    {
        PlayerInventory.Ensure();
        int gold = UnityEngine.Random.Range(minGold, maxGold + 1);
        if (gold > 0)
        {
            PlayerInventory.AddGold(gold);
            CombatMessageDisplay.Show($"獲得 {gold} 枚金幣");
        }

        int herbs = UnityEngine.Random.Range(minHerbs, maxHerbs + 1);
        if (herbs > 0)
        {
            PlayerInventory.AddHerbs(herbs);
            CombatMessageDisplay.Show($"獲得 {herbs} 株藥草");
        }

        for (int i = 0; i < weaponDrops.Length; i++)
        {
            WeaponDrop drop = weaponDrops[i];
            if (drop == null || drop.weapon == null || UnityEngine.Random.value > drop.chance)
            {
                continue;
            }

            drop.weapon.NormalizeForCurrentStory();
            PlayerInventory.AddWeapon(drop.weapon);
            CombatMessageDisplay.Show($"掉落裝備：{drop.weapon.displayName}");
            break;
        }
    }
}
