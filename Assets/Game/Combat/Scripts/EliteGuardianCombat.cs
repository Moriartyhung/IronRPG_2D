using UnityEngine;

public class EliteGuardianCombat : MonoBehaviour
{
    [SerializeField] private WeaponQuality requiredGauntletQuality = WeaponQuality.Elite;
    [SerializeField] private WeaponQuality requiredRuneQuality = WeaponQuality.Epic;
    [SerializeField] private int weakEquipmentDamage = 10;
    [SerializeField] private float warningInterval = 2.5f;
    [SerializeField] private string lockedBeforeDialogueMessage = "精英怪正盯著你。先靠近完成對話，戰鬥才會開始。";
    [SerializeField] private string weakEquipmentMessage = "普通拳套和低階符文只能刮傷精英怪，需要精英拳套搭配史詩符文才打得動。";
    [SerializeField] private string defeatedMessage = "精英怪倒下了，通往寶箱的路打開了。";
    [SerializeField] private float eliteAggroRange = 6f;
    [SerializeField] private float eliteAttackRange = 6f;
    [SerializeField] private float eliteDamageInterval = 0.55f;

    private EnemyHealth health;
    private float nextWarningTime;
    private bool combatStarted;

    public static bool IsForestEliteDefeated { get; private set; }
    public bool CombatStarted => combatStarted;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.Defeated += HandleDefeated;
        }

        ConfigureCombatRanges();
        SetCombatComponentsEnabled(false);
    }

    private void Start()
    {
        ConfigureCombatRanges();
        SetCombatComponentsEnabled(combatStarted);
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.Defeated -= HandleDefeated;
        }
    }

    public int ModifyIncomingDamage(int baseDamage, PlayerInventory inventory)
    {
        if (!combatStarted)
        {
            ShowWarning(lockedBeforeDialogueMessage, 2.4f);
            return 0;
        }

        WeaponItem gauntlet = inventory != null ? inventory.GetEquippedGauntlet() : null;
        WeaponItem rune = inventory != null ? inventory.GetEquippedRune() : null;
        bool gauntletReady = gauntlet != null && gauntlet.quality >= requiredGauntletQuality;
        bool runeReady = rune != null && rune.quality >= requiredRuneQuality;
        if (gauntletReady && runeReady)
        {
            return baseDamage;
        }

        ShowWarning(weakEquipmentMessage, 2.4f);
        return Mathf.Min(weakEquipmentDamage, baseDamage);
    }

    public void BeginCombat()
    {
        if (combatStarted)
        {
            return;
        }

        combatStarted = true;
        ConfigureCombatRanges();
        SetCombatComponentsEnabled(true);
        CombatMessageDisplay.Show("精英怪發出低吼，戰鬥開始。", 2.4f);
    }

    private void HandleDefeated(EnemyHealth defeatedHealth)
    {
        IsForestEliteDefeated = true;
        CombatMessageDisplay.Show(defeatedMessage, 3f);
    }

    private void ShowWarning(string message, float duration)
    {
        if (Time.time < nextWarningTime)
        {
            return;
        }

        CombatMessageDisplay.Show(message, duration);
        nextWarningTime = Time.time + warningInterval;
    }

    private void SetCombatComponentsEnabled(bool enabled)
    {
        SimpleEnemyChase chase = GetComponent<SimpleEnemyChase>();
        if (chase != null)
        {
            chase.enabled = enabled;
        }

        EnemyContactDamage contactDamage = GetComponent<EnemyContactDamage>();
        if (contactDamage != null)
        {
            contactDamage.enabled = enabled;
        }
    }

    private void ConfigureCombatRanges()
    {
        SimpleEnemyChase chase = GetComponent<SimpleEnemyChase>();
        if (chase != null)
        {
            chase.SetAggroRange(eliteAggroRange);
        }

        EnemyContactDamage contactDamage = GetComponent<EnemyContactDamage>();
        if (contactDamage != null)
        {
            contactDamage.SetMaxAttackDistance(eliteAttackRange);
            contactDamage.SetDamageInterval(eliteDamageInterval);
        }
    }

    public static void ResetForestEliteState()
    {
        IsForestEliteDefeated = false;
    }
}
