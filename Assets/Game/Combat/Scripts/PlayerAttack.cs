using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private const int TestingOneShotDamage = 100000;

    [SerializeField] private float attackRange = 1.55f;
    [SerializeField] private float attackCooldown = 0.35f;
    [SerializeField] private int fallbackDamage = 1;
    [SerializeField] private bool oneShotEnemiesForTesting;
    [SerializeField] private Vector2 attackDirection = Vector2.left;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerHeldWeaponVisual heldWeaponVisual;
    [SerializeField] private bool useHeldWeaponVisual;
    [SerializeField] private PlayerAttackVisual attackVisual;
    [SerializeField] private PlayerSpriteAnimator spriteAnimator;

    private static int hintDrawFrame = -1;
    private static EventType hintDrawEventType;

    private float nextAttackTime;
    private GUIStyle hintStyle;

    private void Awake()
    {
        attackRange = Mathf.Max(attackRange, 1.55f);

        if (movement == null)
        {
            movement = GetComponent<PlayerMovement>();
        }

        if (heldWeaponVisual == null)
        {
            heldWeaponVisual = GetComponentInChildren<PlayerHeldWeaponVisual>(true);
        }

        if (attackVisual == null)
        {
            attackVisual = GetComponent<PlayerAttackVisual>();
        }

        if (spriteAnimator == null)
        {
            spriteAnimator = GetComponent<PlayerSpriteAnimator>();
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (BackpackUI.IsOpen)
        {
            return;
        }

        UpdateAttackDirection(keyboard);
        if (useHeldWeaponVisual && keyboard.rKey.wasPressedThisFrame && heldWeaponVisual != null)
        {
            heldWeaponVisual.ToggleWeaponDrawn();
            CombatMessageDisplay.Show(heldWeaponVisual.IsWeaponDrawn ? "進入戰鬥架勢。" : "放鬆戰鬥架勢。");
        }

        if (keyboard.jKey.wasPressedThisFrame)
        {
            Attack();
        }
    }

    private void Attack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        if (useHeldWeaponVisual && heldWeaponVisual != null && !heldWeaponVisual.IsWeaponDrawn)
        {
            CombatMessageDisplay.Show("先按 R 進入戰鬥架勢。");
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        if (movement != null)
        {
            movement.FaceDirection(attackDirection);
        }

        if (useHeldWeaponVisual && heldWeaponVisual != null)
        {
            heldWeaponVisual.PlayAttackSwing(attackDirection, attackCooldown * 0.7f);
        }

        if (spriteAnimator != null)
        {
            spriteAnimator.PlayAttack(attackDirection, attackCooldown * 0.85f);
        }

        float reach = GetCurrentAttackReach();
        if (attackVisual != null)
        {
            attackVisual.PlayAttack(attackDirection, reach);
        }

        PlayerInventory inventory = PlayerInventory.Ensure();
        int damage = oneShotEnemiesForTesting ? TestingOneShotDamage : Mathf.Max(fallbackDamage, inventory.GetAttackDamage());
        Vector2 center = (Vector2)transform.position + attackDirection.normalized * reach * 0.58f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, Mathf.Max(0.24f, reach * 0.38f));

        bool hitEnemy = false;
        bool hitBossPart = false;
        int shownDamage = 0;
        string bossMessage = string.Empty;
        HashSet<IBossDamageReceiver> bossReceivers = new HashSet<IBossDamageReceiver>();
        for (int i = 0; i < hits.Length; i++)
        {
            EnemyHealth enemy = hits[i].GetComponentInParent<EnemyHealth>();
            if (enemy == null)
            {
                MonoBehaviour[] directBehaviours = hits[i].GetComponents<MonoBehaviour>();
                bool handledDirectBossPart = false;
                for (int j = 0; j < directBehaviours.Length; j++)
                {
                    if (directBehaviours[j] is not IBossDamageReceiver bossReceiver || !bossReceivers.Add(bossReceiver))
                    {
                        continue;
                    }

                    handledDirectBossPart = true;
                    if (bossReceiver.ReceivePlayerAttack(damage, inventory, out int bossDamage, out string message))
                    {
                        hitBossPart = true;
                        shownDamage = Mathf.Max(shownDamage, bossDamage);
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            bossMessage = message;
                        }
                    }
                }

                if (handledDirectBossPart)
                {
                    continue;
                }

                MonoBehaviour[] parentBehaviours = hits[i].GetComponentsInParent<MonoBehaviour>();
                for (int j = 0; j < parentBehaviours.Length; j++)
                {
                    if (parentBehaviours[j] is not IBossDamageReceiver bossReceiver || !bossReceivers.Add(bossReceiver))
                    {
                        continue;
                    }

                    if (bossReceiver.ReceivePlayerAttack(damage, inventory, out int bossDamage, out string message))
                    {
                        hitBossPart = true;
                        shownDamage = Mathf.Max(shownDamage, bossDamage);
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            bossMessage = message;
                        }
                    }
                }

                continue;
            }

            int finalDamage = oneShotEnemiesForTesting ? enemy.CurrentHealth : damage;
            EliteGuardianCombat eliteGuardian = enemy.GetComponent<EliteGuardianCombat>();
            if (!oneShotEnemiesForTesting && eliteGuardian != null)
            {
                finalDamage = eliteGuardian.ModifyIncomingDamage(damage, inventory);
            }

            enemy.TakeDamage(finalDamage);
            hitEnemy = true;
            shownDamage = Mathf.Max(shownDamage, finalDamage);
        }

        if (hitBossPart)
        {
            GameAudioManager.PlayPlayerHitEnemy();
            CombatMessageDisplay.Show(!string.IsNullOrWhiteSpace(bossMessage) ? bossMessage : $"拳擊命中，造成 {shownDamage} 點傷害");
            return;
        }

        if (hitEnemy)
        {
            GameAudioManager.PlayPlayerHitEnemy();
        }
        else
        {
            GameAudioManager.PlayPlayerAttackMiss();
        }

        CombatMessageDisplay.Show(hitEnemy ? $"拳擊命中，造成 {shownDamage} 點傷害" : "攻擊揮空");
    }

    private void UpdateAttackDirection(Keyboard keyboard)
    {
        Vector2 direction = Vector2.zero;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            direction.x -= 1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            direction.x += 1f;
        }

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            direction.y -= 1f;
        }

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            direction.y += 1f;
        }

        if (direction.sqrMagnitude > 0.01f)
        {
            attackDirection = direction.normalized;
            return;
        }

        if (movement != null)
        {
            attackDirection = movement.FacingDirection;
        }
    }

    private void OnGUI()
    {
        if (BackpackUI.IsOpen)
        {
            return;
        }

        Event currentEvent = Event.current;
        EventType currentType = currentEvent != null ? currentEvent.type : EventType.Repaint;
        if (hintDrawFrame == Time.frameCount && hintDrawEventType == currentType)
        {
            return;
        }

        hintDrawFrame = Time.frameCount;
        hintDrawEventType = currentType;

        EnsureStyle();
        const string attackHint = "Shift 跑步 / J 攻擊 / B 背包 / E 調查或對話 / T 交易";
        GUI.Label(RuntimeHudLayout.ReserveTopLeftText(attackHint, hintStyle, 640f), attackHint, hintStyle);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.25f, 0.15f, 0.75f);
        float reach = GetCurrentAttackReach();
        Vector2 center = (Vector2)transform.position + attackDirection.normalized * reach * 0.58f;
        Gizmos.DrawWireSphere(center, Mathf.Max(0.24f, reach * 0.38f));
    }

    private float GetCurrentAttackReach()
    {
        return useHeldWeaponVisual && heldWeaponVisual != null ? heldWeaponVisual.GetAttackReach(attackRange) : attackRange;
    }

    private void EnsureStyle()
    {
        hintStyle ??= new GUIStyle(GUI.skin.label)
        {
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true
        };
        hintStyle.fontSize = RuntimeHudLayout.FontSize(16, 11, 16);
    }
}
