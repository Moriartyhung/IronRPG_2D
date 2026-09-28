using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossAttackDamageArea : MonoBehaviour
{
    [SerializeField] private int damage = 70;
    [SerializeField] private float damageInterval = 0.5f;
    [SerializeField] private bool repeatDamage = true;

    private readonly Dictionary<PlayerHealth, float> nextDamageTimes = new Dictionary<PlayerHealth, float>();
    private readonly HashSet<PlayerHealth> damagedPlayers = new HashSet<PlayerHealth>();

    public void Configure(int attackDamage, float interval, bool canRepeat)
    {
        damage = attackDamage;
        damageInterval = Mathf.Max(0.05f, interval);
        repeatDamage = canRepeat;
    }

    private void Awake()
    {
        Collider2D hitbox = GetComponent<Collider2D>();
        hitbox.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (GameplayPauseState.EnemiesPaused || BackpackUI.IsOpen)
        {
            return;
        }

        PlayerHealth player = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (player == null)
        {
            return;
        }

        if (!repeatDamage && damagedPlayers.Contains(player))
        {
            return;
        }

        if (nextDamageTimes.TryGetValue(player, out float nextAllowedTime) && Time.time < nextAllowedTime)
        {
            return;
        }

        bool damaged = repeatDamage
            ? player.TakeDamage(damage, damageInterval)
            : player.TakeDamage(damage);

        if (!damaged)
        {
            return;
        }

        damagedPlayers.Add(player);
        nextDamageTimes[player] = Time.time + damageInterval;
    }
}
