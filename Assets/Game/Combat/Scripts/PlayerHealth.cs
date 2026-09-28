using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invincibleSeconds = 0.8f;
    [SerializeField] private float sceneStartInvincibleSeconds = 2.5f;

    private int currentHealth;
    private float nextDamageTime;
    private bool deathHandled;
    private GUIStyle healthStyle;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth + PlayerProgress.Ensure().MaxHealthBonus;

    private void Awake()
    {
        PlayerProgress.Ensure();
        currentHealth = MaxHealth;
        GrantInvincibility(sceneStartInvincibleSeconds);
    }

    public bool TakeDamage(int amount)
    {
        return TakeDamage(amount, invincibleSeconds);
    }

    public bool TakeDamage(int amount, float invincibleDuration)
    {
        if (amount <= 0 || Time.time < nextDamageTime)
        {
            return false;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        nextDamageTime = Time.time + Mathf.Max(0.05f, invincibleDuration);
        GameAudioManager.PlayPlayerHurt();
        CombatMessageDisplay.Show($"主角受到 {amount} 點傷害");

        if (currentHealth <= 0 && !deathHandled)
        {
            deathHandled = true;
            PlayerDeathRespawn.RespawnAtHome();
        }

        return true;
    }

    public void HealToFull()
    {
        currentHealth = MaxHealth;
        deathHandled = false;
    }

    public void GrantInvincibility(float seconds)
    {
        if (seconds <= 0f)
        {
            return;
        }

        nextDamageTime = Mathf.Max(nextDamageTime, Time.time + seconds);
    }

    private void OnGUI()
    {
        if (BackpackUI.IsOpen)
        {
            return;
        }

        EnsureStyle();
        PlayerProgress progress = PlayerProgress.Ensure();
        string expText = progress.Level >= 30
            ? "MAX"
            : $"{progress.Experience}/{progress.ExperienceToNextLevel}";
        string healthText = $"HP {currentHealth}/{MaxHealth}  Lv.{progress.Level} EXP {expText}";
        GUI.Label(RuntimeHudLayout.ReserveTopLeftText(healthText, healthStyle, 320f), healthText, healthStyle);
    }

    private void EnsureStyle()
    {
        healthStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,
            normal = { textColor = new Color(1f, 0.35f, 0.35f, 1f) }
        };
        healthStyle.fontSize = RuntimeHudLayout.FontSize(20, 12, 20);
    }
}
