using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 6.5f;
    [SerializeField] private bool autoFitColliderToVisual = true;
    [SerializeField] private float collisionSkin = 0.02f;

    private Rigidbody2D body;
    private BoxCollider2D bodyCollider;
    private ContactFilter2D movementFilter;
    private readonly RaycastHit2D[] movementHits = new RaycastHit2D[8];
    private Vector2 moveInput;
    private Vector2 facingDirection = Vector2.down;
    private bool isRunning;
    private bool inputLocked;

    public Vector2 MoveInput => moveInput;
    public Vector2 FacingDirection => facingDirection;
    public bool IsMoving => moveInput.sqrMagnitude > 0.01f;
    public bool IsRunning => IsMoving && isRunning;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();

        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        movementFilter = new ContactFilter2D
        {
            useTriggers = false
        };
        movementFilter.SetLayerMask(Physics2D.AllLayers);
        PlayerInventory.Ensure();
        FitColliderToCurrentVisual();
    }

    private void Update()
    {
        ReadMovementInput();
    }

    private void FixedUpdate()
    {
        if (inputLocked)
        {
            return;
        }

        float speed = isRunning ? runSpeed : walkSpeed;
        Vector2 movement = moveInput * speed * Time.fixedDeltaTime;
        if (movement.sqrMagnitude > 0.000001f)
        {
            movement = ClampMovementToCollision(movement);
        }

        Vector2 nextPosition = body.position + movement;
        body.MovePosition(nextPosition);
    }

    public void FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.01f)
        {
            return;
        }

        facingDirection = direction.normalized;
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        if (locked)
        {
            moveInput = Vector2.zero;
            isRunning = false;
        }
    }

    public void SetScriptedMoveInput(Vector2 direction, bool running)
    {
        moveInput = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector2.zero;
        isRunning = running && moveInput.sqrMagnitude > 0.01f;
        if (moveInput.sqrMagnitude > 0.01f)
        {
            FaceDirection(moveInput);
        }
    }

    private void ReadMovementInput()
    {
        if (inputLocked)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            moveInput = Vector2.zero;
            isRunning = false;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            horizontal -= 1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            horizontal += 1f;
        }

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            vertical -= 1f;
        }

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            vertical += 1f;
        }

        moveInput = new Vector2(horizontal, vertical).normalized;
        isRunning = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            FaceDirection(moveInput);
        }
    }

    private void FitColliderToCurrentVisual()
    {
        if (!autoFitColliderToVisual)
        {
            return;
        }

        SpriteRenderer renderer = transform.Find("PlayerVisual")?.GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        if (bodyCollider == null || renderer == null || renderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize = renderer.sprite.bounds.size;
        Vector3 visualScale = renderer.transform.localScale;
        float visualWidth = Mathf.Abs(spriteSize.x * visualScale.x);
        float visualHeight = Mathf.Abs(spriteSize.y * visualScale.y);
        float width = Mathf.Max(0.42f, visualWidth * 0.48f);
        float height = Mathf.Max(0.42f, visualHeight * 0.44f);
        bodyCollider.size = new Vector2(width, height);
        bodyCollider.offset = new Vector2(0f, -visualHeight * 0.16f);
        bodyCollider.isTrigger = false;
    }

    private Vector2 ClampMovementToCollision(Vector2 movement)
    {
        if (bodyCollider == null)
        {
            return movement;
        }

        float distance = movement.magnitude;
        if (distance <= 0f)
        {
            return Vector2.zero;
        }

        Vector2 direction = movement / distance;
        int hitCount = bodyCollider.Cast(direction, movementFilter, movementHits, distance + collisionSkin);
        float allowedDistance = distance;
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitCollider = movementHits[i].collider;
            if (hitCollider == null || hitCollider == bodyCollider || hitCollider.transform.IsChildOf(transform))
            {
                continue;
            }

            allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0f, movementHits[i].distance - collisionSkin));
        }

        return direction * allowedDistance;
    }
}
