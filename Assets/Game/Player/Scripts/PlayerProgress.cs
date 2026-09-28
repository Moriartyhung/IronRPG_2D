using UnityEngine;

[DisallowMultipleComponent]
public class PlayerProgress : MonoBehaviour
{
    private const int MaxLevel = 30;
    private const int HealthPerLevel = 20;
    private const int DamagePerLevel = 3;

    [SerializeField] private int level = 1;
    [SerializeField] private int experience;

    public static PlayerProgress Instance { get; private set; }
    public int Level => level;
    public int Experience => experience;
    public int ExperienceToNextLevel => level >= MaxLevel ? 0 : 40 + level * 10;
    public int MaxHealthBonus => (level - 1) * HealthPerLevel;
    public int DamageBonus => (level - 1) * DamagePerLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static PlayerProgress Ensure()
    {
        if (Instance != null)
        {
            return Instance;
        }

        PlayerProgress existing = FindAnyObjectByType<PlayerProgress>();
        if (existing != null)
        {
            Instance = existing;
            return existing;
        }

        GameObject progressObject = new GameObject("PlayerProgress");
        return progressObject.AddComponent<PlayerProgress>();
    }

    public static void AddExperience(int amount)
    {
        Ensure().AddExperienceInternal(amount);
    }

    public static void ResetForNewGame()
    {
        PlayerProgress progress = Ensure();
        progress.level = 1;
        progress.experience = 0;
    }

    private void AddExperienceInternal(int amount)
    {
        if (amount <= 0 || level >= MaxLevel)
        {
            return;
        }

        experience += amount;
        CombatMessageDisplay.Show($"獲得 {amount} 點經驗");

        bool leveledUp = false;
        while (level < MaxLevel && experience >= ExperienceToNextLevel)
        {
            experience -= ExperienceToNextLevel;
            level++;
            leveledUp = true;
        }

        if (level >= MaxLevel)
        {
            experience = 0;
        }

        if (leveledUp)
        {
            CombatMessageDisplay.Show($"等級提升到 Lv.{level}，最大生命 +20，傷害 +3", 3f);
            PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.HealToFull();
            }
        }
    }
}
