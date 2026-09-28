using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SimpleEnemyChase : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.55f;
    [SerializeField] private float aggroRange = 5.5f;
    [SerializeField] private float stopDistance = 0.72f;

    private Rigidbody2D body;
    private Transform player;

    public bool IsMoving { get; private set; }
    public Vector2 LastMoveDirection { get; private set; } = Vector2.down;

    public void SetAggroRange(float range)
    {
        aggroRange = Mathf.Max(0f, range);
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }

    public void SetStopDistance(float distance)
    {
        stopDistance = Mathf.Max(0f, distance);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (GameplayPauseState.EnemiesPaused)
        {
            body.linearVelocity = Vector2.zero;
            IsMoving = false;
            return;
        }

        if (player == null)
        {
            PlayerMovement playerMovement = FindAnyObjectByType<PlayerMovement>();
            player = playerMovement != null ? playerMovement.transform : null;
        }

        if (player == null)
        {
            body.linearVelocity = Vector2.zero;
            IsMoving = false;
            return;
        }

        Vector2 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        if (distance > aggroRange || distance < stopDistance)
        {
            body.linearVelocity = Vector2.zero;
            IsMoving = false;
            return;
        }

        LastMoveDirection = toPlayer.normalized;
        body.linearVelocity = LastMoveDirection * moveSpeed;
        IsMoving = true;
    }
}
