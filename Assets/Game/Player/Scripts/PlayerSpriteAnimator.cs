using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerSpriteAnimator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Animator animatorToDisable;
    [SerializeField] private float idleFramesPerSecond = 4f;
    [SerializeField] private float walkFramesPerSecond = 7f;
    [SerializeField] private float runFramesPerSecond = 10f;
    [SerializeField] private float attackFramesPerSecond = 14f;
    [SerializeField] private Sprite[] idleDown;
    [SerializeField] private Sprite[] idleUp;
    [SerializeField] private Sprite[] idleSide;
    [SerializeField] private Sprite[] walkDown;
    [SerializeField] private Sprite[] walkUp;
    [SerializeField] private Sprite[] walkSide;
    [SerializeField] private Sprite[] runDown;
    [SerializeField] private Sprite[] runUp;
    [SerializeField] private Sprite[] runSide;
    [SerializeField] private Sprite[] attackDown;
    [SerializeField] private Sprite[] attackUp;
    [SerializeField] private Sprite[] attackSide;

    private enum FacingGroup
    {
        Down,
        Up,
        Side
    }

    private Sprite[] activeFrames;
    private int frameIndex;
    private float frameTimer;
    private Sprite[] activeAttackFrames;
    private FacingGroup attackFacingGroup;
    private Vector2 attackFacingDirection = Vector2.down;
    private float attackEndsAt = -999f;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (movement == null)
        {
            movement = GetComponent<PlayerMovement>();
        }

        if (animatorToDisable == null)
        {
            animatorToDisable = GetComponent<Animator>();
        }

        if (animatorToDisable != null)
        {
            animatorToDisable.enabled = false;
        }
    }

    private void Update()
    {
        if (spriteRenderer == null || movement == null)
        {
            return;
        }

        if (Time.time < attackEndsAt && activeAttackFrames != null && activeAttackFrames.Length > 0)
        {
            spriteRenderer.flipX = attackFacingGroup == FacingGroup.Side && attackFacingDirection.x < 0f;
            AdvanceFrames(activeAttackFrames, attackFramesPerSecond, false);
            return;
        }

        FacingGroup group = GetFacingGroup(movement.FacingDirection);
        Sprite[] nextFrames = GetFrames(group);
        if (nextFrames == null || nextFrames.Length == 0)
        {
            return;
        }

        if (activeFrames != nextFrames)
        {
            activeFrames = nextFrames;
            frameIndex = 0;
            frameTimer = 0f;
        }

        spriteRenderer.flipX = group == FacingGroup.Side && movement.FacingDirection.x < 0f;
        AdvanceFrames(activeFrames, GetFramesPerSecond(), true);
    }

    public void PlayAttack(Vector2 direction, float duration)
    {
        if (direction.sqrMagnitude <= 0.01f)
        {
            direction = movement != null ? movement.FacingDirection : Vector2.down;
        }

        attackFacingDirection = direction.normalized;
        attackFacingGroup = GetFacingGroup(attackFacingDirection);
        activeAttackFrames = GetAttackFrames(attackFacingGroup);
        if (activeAttackFrames == null || activeAttackFrames.Length == 0)
        {
            return;
        }

        activeFrames = activeAttackFrames;
        frameIndex = 0;
        frameTimer = 0f;
        attackEndsAt = Time.time + Mathf.Max(0.08f, duration);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = attackFacingGroup == FacingGroup.Side && attackFacingDirection.x < 0f;
            spriteRenderer.sprite = activeAttackFrames[0];
        }
    }

    private Sprite[] GetFrames(FacingGroup group)
    {
        if (movement.IsRunning)
        {
            return group switch
            {
                FacingGroup.Up => runUp,
                FacingGroup.Side => runSide,
                _ => runDown
            };
        }

        if (movement.IsMoving)
        {
            return group switch
            {
                FacingGroup.Up => walkUp,
                FacingGroup.Side => walkSide,
                _ => walkDown
            };
        }

        return group switch
        {
            FacingGroup.Up => idleUp,
            FacingGroup.Side => idleSide,
            _ => idleDown
        };
    }

    private Sprite[] GetAttackFrames(FacingGroup group)
    {
        return group switch
        {
            FacingGroup.Up => attackUp,
            FacingGroup.Side => attackSide,
            _ => attackDown
        };
    }

    private float GetFramesPerSecond()
    {
        if (movement.IsRunning)
        {
            return runFramesPerSecond;
        }

        return movement.IsMoving ? walkFramesPerSecond : idleFramesPerSecond;
    }

    private void AdvanceFrames(Sprite[] frames, float framesPerSecond, bool loop)
    {
        if (frames == null || frames.Length == 0)
        {
            return;
        }

        frameTimer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = loop ? (frameIndex + 1) % frames.Length : Mathf.Min(frameIndex + 1, frames.Length - 1);
        }

        spriteRenderer.sprite = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
    }

    private static FacingGroup GetFacingGroup(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return FacingGroup.Side;
        }

        return direction.y > 0f ? FacingGroup.Up : FacingGroup.Down;
    }
}
