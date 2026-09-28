using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField] private int damage = 8;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float maxAttackDistance = 0f;

    private float nextDamageTime;

    public void SetDamage(int amount)
    {
        damage = Mathf.Max(0, amount);
    }

    private void Awake()
    {
        if (GetComponent<EliteGuardianCombat>() == null)
        {
            damageInterval = Mathf.Min(damageInterval, 0.65f);
        }
    }

    public void SetMaxAttackDistance(float distance)
    {
        maxAttackDistance = Mathf.Max(0f, distance);
    }

    public void SetDamageInterval(float interval)
    {
        damageInterval = Mathf.Max(0.05f, interval);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamage(collision.collider);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (GameplayPauseState.EnemiesPaused || Time.time < nextDamageTime)
        {
            return;
        }

        PlayerHealth player = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (player == null)
        {
            return;
        }

        if (maxAttackDistance > 0f && Vector2.Distance(transform.position, player.transform.position) > maxAttackDistance)
        {
            return;
        }

        if (!player.TakeDamage(damage))
        {
            return;
        }

        EnemyVisualAnimator visualAnimator = GetComponent<EnemyVisualAnimator>();
        if (visualAnimator != null)
        {
            visualAnimator.PlayAttack(player.transform.position);
        }

        nextDamageTime = Time.time + damageInterval;
    }
}
