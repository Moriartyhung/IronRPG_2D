using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private string enemyName = "森林小怪";
    [SerializeField] private int maxHealth = 200;
    [SerializeField] private Vector2 healthBarOffset = new Vector2(0f, 0.18f);
    [SerializeField] private int experienceReward = 10;

    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private float flashUntil;
    private GUIStyle nameStyle;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public event Action<EnemyHealth> Defeated;

    public void ConfigureStats(string displayName, int health, int reward, Vector2 barOffset)
    {
        enemyName = displayName;
        maxHealth = Mathf.Max(1, health);
        experienceReward = Mathf.Max(0, reward);
        healthBarOffset = barOffset;
        currentHealth = maxHealth;
    }

    private void Awake()
    {
        EliteGuardianCombat eliteGuardian = GetComponent<EliteGuardianCombat>();
        if (eliteGuardian != null)
        {
            maxHealth = Mathf.Max(maxHealth, 6000);
            experienceReward = 0;
        }
        else
        {
            maxHealth = Mathf.Max(maxHealth, 300);
            experienceReward = Mathf.Max(experienceReward, 10);
        }

        currentHealth = maxHealth;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Update()
    {
        if (spriteRenderer != null && Time.time > flashUntil)
        {
            spriteRenderer.color = originalColor;
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            flashUntil = Time.time + 0.08f;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        EnemyLootDrop lootDrop = GetComponent<EnemyLootDrop>();
        if (lootDrop != null)
        {
            lootDrop.DropLoot();
        }

        if (experienceReward > 0)
        {
            PlayerProgress.AddExperience(experienceReward);
        }

        GameAudioManager.PlayEnemyDefeated();
        CombatMessageDisplay.Show($"{enemyName} 被擊敗");
        Defeated?.Invoke(this);
        Destroy(gameObject);
    }

    private void OnGUI()
    {
        if (BackpackUI.IsOpen || GameplayPauseState.EnemiesPaused)
        {
            return;
        }

        if (currentHealth <= 0 || Camera.main == null)
        {
            return;
        }

        EnsureStyle();
        Vector3 screen = Camera.main.WorldToScreenPoint(GetHealthBarWorldPosition());
        if (screen.z < 0f)
        {
            return;
        }

        Rect barRect = new Rect(screen.x - 32f, Screen.height - screen.y, 64f, 7f);
        GUI.color = Color.black;
        GUI.DrawTexture(barRect, Texture2D.whiteTexture);
        GUI.color = Color.red;
        GUI.DrawTexture(new Rect(barRect.x + 1f, barRect.y + 1f, (barRect.width - 2f) * currentHealth / maxHealth, barRect.height - 2f), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(screen.x - 44f, Screen.height - screen.y - 18f, 88f, 18f), enemyName, nameStyle);
    }

    private Vector3 GetHealthBarWorldPosition()
    {
        if (spriteRenderer == null)
        {
            return transform.position + (Vector3)healthBarOffset;
        }

        Bounds bounds = spriteRenderer.bounds;
        float verticalPadding = Mathf.Clamp(healthBarOffset.y, 0.08f, 0.22f);
        return new Vector3(
            bounds.center.x + healthBarOffset.x,
            bounds.max.y + verticalPadding,
            transform.position.z);
    }

    private void EnsureStyle()
    {
        if (nameStyle != null)
        {
            return;
        }

        nameStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
    }
}
