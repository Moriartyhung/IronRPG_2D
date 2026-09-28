using UnityEngine;

[DisallowMultipleComponent]
public class BossCoreNode : MonoBehaviour, IBossDamageReceiver
{
    [SerializeField] private BossCoreBattleController controller;
    [SerializeField] private SpriteRenderer armorRenderer;
    [SerializeField] private Collider2D armorCollider;
    [SerializeField] private GameObject coreObject;
    [SerializeField] private Collider2D coreCollider;
    [SerializeField] private Color highlightColor = new Color(1f, 0.72f, 0.2f, 1f);

    private Color baseArmorColor = Color.white;
    private int armorHitCount;
    private int coreHitCount;
    private bool highlighted;
    private bool armorBroken;

    public bool IsArmorBroken => armorBroken;

    public void Configure(BossCoreBattleController battleController, SpriteRenderer renderer, Collider2D collider, GameObject core, Collider2D coreHitCollider)
    {
        controller = battleController;
        armorRenderer = renderer;
        armorCollider = collider;
        coreObject = core;
        coreCollider = coreHitCollider;
        CaptureBaseColor();
        ResetNode();
    }

    private void Awake()
    {
        if (armorRenderer == null)
        {
            armorRenderer = GetComponent<SpriteRenderer>();
        }

        if (armorCollider == null)
        {
            armorCollider = GetComponent<Collider2D>();
        }

        CaptureBaseColor();
        SetCoreActive(false);
    }

    private void Update()
    {
        if (!highlighted || armorBroken || armorRenderer == null)
        {
            return;
        }

        float pulse = 0.55f + Mathf.Sin(Time.time * 8f) * 0.25f;
        armorRenderer.color = Color.Lerp(baseArmorColor, highlightColor, pulse);
    }

    public void ResetNode()
    {
        armorHitCount = 0;
        coreHitCount = 0;
        armorBroken = false;
        SetHighlighted(false);

        if (armorRenderer != null)
        {
            armorRenderer.enabled = true;
            armorRenderer.color = baseArmorColor;
        }

        if (armorCollider != null)
        {
            armorCollider.enabled = true;
            armorCollider.isTrigger = true;
        }

        SetCoreActive(false);
    }

    public void SetHighlighted(bool value)
    {
        highlighted = value;
        if (armorRenderer == null || armorBroken)
        {
            return;
        }

        armorRenderer.color = value ? highlightColor : baseArmorColor;
    }

    public bool ReceivePlayerAttack(int baseDamage, PlayerInventory inventory, out int damageDealt, out string message)
    {
        damageDealt = 0;
        message = string.Empty;
        return controller != null && controller.ReceiveArmorHit(this, out message);
    }

    public bool ReceiveCoreAttack(int baseDamage, PlayerInventory inventory, out int damageDealt, out string message)
    {
        damageDealt = 0;
        message = string.Empty;
        return controller != null && controller.ReceiveCoreHit(this, baseDamage, inventory, out damageDealt, out message);
    }

    public bool RegisterArmorHit(int hitsToBreak)
    {
        if (!highlighted || armorBroken)
        {
            return false;
        }

        armorHitCount++;
        if (armorHitCount < hitsToBreak)
        {
            return false;
        }

        BreakArmor();
        return true;
    }

    public bool RegisterCoreHit(int hitsToRegrow)
    {
        if (!armorBroken)
        {
            return false;
        }

        coreHitCount++;
        return coreHitCount >= hitsToRegrow;
    }

    public int GetArmorHitsRemaining(int hitsToBreak)
    {
        return Mathf.Max(0, hitsToBreak - armorHitCount);
    }

    private void BreakArmor()
    {
        armorBroken = true;
        highlighted = false;

        if (armorRenderer != null)
        {
            armorRenderer.enabled = false;
        }

        if (armorCollider != null)
        {
            armorCollider.enabled = false;
        }

        SetCoreActive(true);
    }

    public void RegrowArmor()
    {
        armorHitCount = 0;
        coreHitCount = 0;
        armorBroken = false;

        if (armorRenderer != null)
        {
            armorRenderer.enabled = true;
            armorRenderer.color = baseArmorColor;
        }

        if (armorCollider != null)
        {
            armorCollider.enabled = true;
            armorCollider.isTrigger = true;
        }

        SetCoreActive(false);
    }

    private void SetCoreActive(bool active)
    {
        if (coreObject != null)
        {
            coreObject.SetActive(active);
        }

        if (coreCollider != null)
        {
            coreCollider.enabled = active;
            coreCollider.isTrigger = true;
        }
    }

    private void CaptureBaseColor()
    {
        if (armorRenderer != null)
        {
            baseArmorColor = armorRenderer.color;
        }
    }
}
