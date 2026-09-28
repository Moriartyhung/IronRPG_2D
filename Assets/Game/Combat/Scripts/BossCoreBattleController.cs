using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BossCoreBattleController : MonoBehaviour
{
    public static event Action BossDefeated;
    public static bool HasDefeated { get; private set; }

    [SerializeField] private int maxHealth = 15000;
    [SerializeField] private int armorHitsToBreak = 5;
    [SerializeField] private int coreHitsToRegrow = 2;
    [SerializeField] private BossCoreNode[] coreNodes;
    [SerializeField] private string bossName = "洞穴古樹";
    [SerializeField] private Color frameColor = new Color(0.36f, 0.20f, 0.10f, 0.96f);
    [SerializeField] private Color fillColor = new Color(0.72f, 0.34f, 0.15f, 1f);

    private int currentHealth;
    private bool battleActive;
    private BossCoreNode highlightedNode;
    private GUIStyle frameStyle;
    private GUIStyle labelStyle;
    private Texture2D frameTexture;

    public bool IsBattleActive => battleActive;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public void Configure(BossCoreNode[] nodes)
    {
        coreNodes = nodes;
    }

    public static void ResetForNewGame()
    {
        HasDefeated = false;
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        HasDefeated = false;
        ResetNodes();
    }

    private void OnEnable()
    {
        BossIntroDialogueTrigger.BossIntroCompleted += ActivateBattle;
        if (BossIntroDialogueTrigger.HasCompleted)
        {
            ActivateBattle();
        }
    }

    private void OnDisable()
    {
        BossIntroDialogueTrigger.BossIntroCompleted -= ActivateBattle;
    }

    public void ActivateBattle()
    {
        if (battleActive)
        {
            return;
        }

        currentHealth = maxHealth;
        HasDefeated = false;
        battleActive = true;
        ResetNodes();
        HighlightRandomArmor();
        CombatMessageDisplay.Show("Boss 核心被護甲包覆，先攻擊亮起的護甲。");
    }

    public bool ReceiveArmorHit(BossCoreNode node, out string message)
    {
        message = string.Empty;
        if (!battleActive || node == null)
        {
            return false;
        }

        if (node != highlightedNode)
        {
            message = "這塊護甲沒有反應，尋找亮起的護甲。";
            return true;
        }

        if (node.RegisterArmorHit(armorHitsToBreak))
        {
            highlightedNode = null;
            message = "護甲碎裂，核心暴露。";
            return true;
        }

        message = $"護甲出現裂痕，還需要 {node.GetArmorHitsRemaining(armorHitsToBreak)} 下。";
        return true;
    }

    public bool ReceiveCoreHit(BossCoreNode node, int baseDamage, PlayerInventory inventory, out int damageDealt, out string message)
    {
        damageDealt = 0;
        message = string.Empty;
        if (!battleActive || node == null || !node.IsArmorBroken)
        {
            return false;
        }

        damageDealt = CalculateBossDamage(baseDamage, inventory);
        currentHealth = Mathf.Max(0, currentHealth - damageDealt);
        if (currentHealth <= 0)
        {
            EndBattle();
            message = $"雷痕符文擊穿核心，造成 {damageDealt} 點傷害，Boss 被擊敗。";
            return true;
        }

        if (node.RegisterCoreHit(coreHitsToRegrow))
        {
            node.RegrowArmor();
            HighlightRandomArmor();
            message = $"核心受創，造成 {damageDealt} 點傷害。護甲重新生長。";
            return true;
        }

        message = $"核心受創，造成 {damageDealt} 點傷害。";
        return true;
    }

    private int CalculateBossDamage(int baseDamage, PlayerInventory inventory)
    {
        int bonusPercent = inventory != null ? inventory.GetBossDamageBonusPercent() : 0;
        float multiplier = 1f + bonusPercent / 100f;
        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier));
    }

    private void HighlightRandomArmor()
    {
        BossCoreNode[] candidates = GetAvailableArmorNodes();
        if (candidates.Length == 0)
        {
            highlightedNode = null;
            return;
        }

        highlightedNode = candidates[UnityEngine.Random.Range(0, candidates.Length)];
        for (int i = 0; i < coreNodes.Length; i++)
        {
            if (coreNodes[i] != null)
            {
                coreNodes[i].SetHighlighted(coreNodes[i] == highlightedNode);
            }
        }
    }

    private BossCoreNode[] GetAvailableArmorNodes()
    {
        int count = 0;
        for (int i = 0; i < coreNodes.Length; i++)
        {
            if (coreNodes[i] != null && !coreNodes[i].IsArmorBroken)
            {
                count++;
            }
        }

        BossCoreNode[] candidates = new BossCoreNode[count];
        int index = 0;
        for (int i = 0; i < coreNodes.Length; i++)
        {
            if (coreNodes[i] != null && !coreNodes[i].IsArmorBroken)
            {
                candidates[index++] = coreNodes[i];
            }
        }

        return candidates;
    }

    private void ResetNodes()
    {
        if (coreNodes == null)
        {
            return;
        }

        for (int i = 0; i < coreNodes.Length; i++)
        {
            if (coreNodes[i] != null)
            {
                coreNodes[i].ResetNode();
            }
        }
    }

    private void EndBattle()
    {
        battleActive = false;
        highlightedNode = null;
        HasDefeated = true;
        ResetNodes();
        GameAudioManager.PlayEnemyDefeated();
        CurrentObjectiveDisplay.SetObjective("離開洞穴，回村莊向村長回報");
        MainQuestState.SetStage(MainQuestStage.BossDefeated);
        BossDefeated?.Invoke();
        BossDefeatDialogueDisplay.ShowDialogue();
    }

    private void OnGUI()
    {
        if (!battleActive || BackpackUI.IsOpen)
        {
            return;
        }

        EnsureStyles();
        float scale = RuntimeHudLayout.PanelScale();
        float width = Mathf.Clamp(320f * scale, 220f, 320f);
        float height = Mathf.Clamp(58f * scale, 46f, 58f);
        float margin = Mathf.Max(18f, 24f * scale);
        Rect frameRect = new Rect(Screen.width - width - margin, margin, width, height);
        GUI.Box(frameRect, GUIContent.none, frameStyle);

        Rect labelRect = new Rect(frameRect.x + 14f * scale, frameRect.y + 7f * scale, frameRect.width - 28f * scale, 22f * scale);
        GUI.Label(labelRect, $"{bossName}  {currentHealth} / {maxHealth}", labelStyle);

        Rect barBack = new Rect(frameRect.x + 14f * scale, frameRect.y + 34f * scale, frameRect.width - 28f * scale, 12f * scale);
        GUI.color = Color.black;
        GUI.DrawTexture(barBack, Texture2D.whiteTexture);
        GUI.color = fillColor;
        GUI.DrawTexture(new Rect(barBack.x + 2f, barBack.y + 2f, (barBack.width - 4f) * currentHealth / maxHealth, barBack.height - 4f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void EnsureStyles()
    {
        if (frameTexture == null)
        {
            frameTexture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            frameTexture.SetPixel(0, 0, frameColor);
            frameTexture.Apply();
        }

        frameStyle ??= new GUIStyle(GUI.skin.box)
        {
            normal = { background = frameTexture },
            padding = new RectOffset(10, 10, 8, 8)
        };

        labelStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };
        labelStyle.fontSize = RuntimeHudLayout.FontSize(17, 12, 17);
    }

    private void OnDestroy()
    {
        if (frameTexture != null)
        {
            Destroy(frameTexture);
        }
    }
}
