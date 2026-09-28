using UnityEngine;

public class PlayerHeldWeaponVisual : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private SpriteRenderer weaponRenderer;
    [SerializeField] private Transform weaponSpriteTransform;
    [SerializeField] private SpriteRenderer handRenderer;
    [SerializeField] private Transform handSpriteTransform;
    [SerializeField] private float targetWeaponLength = 0.7f;
    [SerializeField] private float attackSwingDegrees = 10f;
    [SerializeField] private float gripToCenterRatio = 0.28f;
    [SerializeField] private float minGripToCenterDistance = 0.08f;
    [SerializeField] private float maxGripToCenterDistance = 0.24f;
    [SerializeField] private float minAttackReach = 0.65f;
    [SerializeField] private float maxAttackReach = 1.05f;

    private WeaponItem currentWeapon;
    private float swingTimer;
    private float swingDuration = 0.18f;
    private Vector2 swingDirection = Vector2.down;
    private bool weaponDrawn;

    public bool IsWeaponDrawn => weaponDrawn;

    private void Awake()
    {
        if (movement == null)
        {
            movement = GetComponentInParent<PlayerMovement>();
        }

        EnsureWeaponSprite();
        EnsureHandSprite();
        SetWeaponDrawn(false);
    }

    private void Update()
    {
        RefreshEquippedWeapon();
        UpdateWeaponPose();
    }

    public void PlayAttackSwing(Vector2 direction, float duration)
    {
        if (!weaponDrawn)
        {
            return;
        }

        if (direction.sqrMagnitude > 0.01f)
        {
            swingDirection = direction.normalized;
        }

        swingDuration = Mathf.Max(0.08f, duration);
        swingTimer = swingDuration;
    }

    public void ToggleWeaponDrawn()
    {
        SetWeaponDrawn(!weaponDrawn);
    }

    public void SetWeaponDrawn(bool drawn)
    {
        weaponDrawn = drawn;
        UpdateVisibility();
    }

    public float GetAttackReach(float fallbackReach)
    {
        if (!weaponDrawn || weaponRenderer == null || weaponRenderer.sprite == null)
        {
            return fallbackReach;
        }

        return Mathf.Clamp(GetWeaponVisualLength() * 0.9f, minAttackReach, maxAttackReach);
    }

    private void RefreshEquippedWeapon()
    {
        if (weaponRenderer == null)
        {
            return;
        }

        PlayerInventory inventory = PlayerInventory.Ensure();
        WeaponItem equippedWeapon = inventory.GetEquippedWeapon();
        if (currentWeapon == equippedWeapon)
        {
            return;
        }

        currentWeapon = equippedWeapon;
        weaponRenderer.sprite = equippedWeapon != null ? equippedWeapon.icon : null;
        UpdateVisibility();
    }

    private void UpdateWeaponPose()
    {
        if (!weaponDrawn || weaponRenderer == null || weaponRenderer.sprite == null)
        {
            UpdateVisibility();
            return;
        }

        Vector2 direction = swingTimer > 0f ? swingDirection : GetFacingDirection();
        bool horizontal = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);
        bool facingLeft = horizontal && direction.x < 0f;
        bool facingUp = !horizontal && direction.y > 0f;
        float attackProgress = swingTimer > 0f ? 1f - swingTimer / swingDuration : 0f;

        weaponRenderer.sortingOrder = facingUp ? 19 : 24;
        if (handRenderer != null)
        {
            handRenderer.sortingOrder = facingUp ? 20 : 25;
            handRenderer.enabled = false;
        }

        weaponRenderer.flipX = facingLeft;

        Vector3 handPosition;
        float baseRotation;
        if (horizontal)
        {
            handPosition = facingLeft ? new Vector3(-0.2f, -0.1f, 0f) : new Vector3(0.2f, -0.1f, 0f);
            baseRotation = facingLeft ? 105f : -105f;
        }
        else if (facingUp)
        {
            handPosition = new Vector3(0.1f, 0f, 0f);
            baseRotation = 15f;
        }
        else
        {
            handPosition = new Vector3(0.14f, -0.18f, 0f);
            baseRotation = 165f;
        }

        handPosition += GetAttackHandOffset(direction, attackProgress);

        float swingOffset = 0f;
        if (swingTimer > 0f)
        {
            swingTimer = Mathf.Max(0f, swingTimer - Time.deltaTime);
            float windUp = Mathf.Lerp(-attackSwingDegrees * 0.38f, attackSwingDegrees, Mathf.SmoothStep(0f, 1f, attackProgress));
            swingOffset = windUp;
            if (facingLeft)
            {
                swingOffset *= -1f;
            }
        }

        transform.localScale = Vector3.one;
        transform.localPosition = handPosition;
        transform.localRotation = Quaternion.Euler(0f, 0f, baseRotation + swingOffset);

        if (weaponSpriteTransform != null)
        {
            weaponSpriteTransform.localScale = Vector3.one * GetWeaponAutoScale();
            weaponSpriteTransform.localPosition = Vector3.up * GetGripToCenterDistance();
            weaponSpriteTransform.localRotation = Quaternion.identity;
        }

        if (handSpriteTransform != null)
        {
            handSpriteTransform.localPosition = Vector3.zero;
            handSpriteTransform.localRotation = Quaternion.identity;
            handSpriteTransform.localScale = new Vector3(0.09f, 0.09f, 1f);
        }
    }

    private Vector2 GetFacingDirection()
    {
        if (movement == null)
        {
            return Vector2.down;
        }

        return movement.FacingDirection;
    }

    private float GetGripToCenterDistance()
    {
        return Mathf.Clamp(GetWeaponVisualLength() * gripToCenterRatio, minGripToCenterDistance, maxGripToCenterDistance);
    }

    private float GetWeaponVisualLength()
    {
        if (weaponRenderer == null || weaponRenderer.sprite == null)
        {
            return 1f;
        }

        Vector2 size = weaponRenderer.sprite.bounds.size;
        return Mathf.Max(size.x, size.y) * GetWeaponAutoScale();
    }

    private void EnsureWeaponSprite()
    {
        SpriteRenderer oldRenderer = weaponRenderer;
        if (weaponSpriteTransform == null)
        {
            Transform existingChild = transform.Find("WeaponSprite");
            if (existingChild == null)
            {
                GameObject spriteObject = new GameObject("WeaponSprite");
                spriteObject.transform.SetParent(transform, false);
                weaponSpriteTransform = spriteObject.transform;
            }
            else
            {
                weaponSpriteTransform = existingChild;
            }
        }

        SpriteRenderer childRenderer = weaponSpriteTransform.GetComponent<SpriteRenderer>();
        if (childRenderer == null)
        {
            childRenderer = weaponSpriteTransform.gameObject.AddComponent<SpriteRenderer>();
        }

        if (oldRenderer != null && oldRenderer != childRenderer && oldRenderer.transform == transform)
        {
            oldRenderer.enabled = false;
        }

        weaponRenderer = childRenderer;
        UpdateVisibility();
        weaponSpriteTransform.localScale = Vector3.one * GetWeaponAutoScale();
    }

    private void EnsureHandSprite()
    {
        if (handSpriteTransform == null)
        {
            Transform existingChild = transform.Find("HandSprite");
            if (existingChild == null)
            {
                GameObject spriteObject = new GameObject("HandSprite");
                spriteObject.transform.SetParent(transform, false);
                handSpriteTransform = spriteObject.transform;
            }
            else
            {
                handSpriteTransform = existingChild;
            }
        }

        if (handRenderer == null)
        {
            handRenderer = handSpriteTransform.GetComponent<SpriteRenderer>();
        }

        if (handRenderer == null)
        {
            handRenderer = handSpriteTransform.gameObject.AddComponent<SpriteRenderer>();
        }

        if (handRenderer.sprite == null)
        {
            handRenderer.sprite = CreateHandSprite();
        }

        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (weaponRenderer != null)
        {
            weaponRenderer.enabled = weaponDrawn && weaponRenderer.sprite != null;
        }

        if (handRenderer != null)
        {
            handRenderer.enabled = false;
        }
    }

    private float GetWeaponAutoScale()
    {
        if (weaponRenderer == null || weaponRenderer.sprite == null)
        {
            return 1f;
        }

        Vector2 size = weaponRenderer.sprite.bounds.size;
        float length = Mathf.Max(size.x, size.y);
        if (length <= 0.01f)
        {
            return 1f;
        }

        return targetWeaponLength / length;
    }

    private static Vector3 GetAttackHandOffset(Vector2 direction, float progress)
    {
        if (progress <= 0f)
        {
            return Vector3.zero;
        }

        Vector3 forward;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            forward = direction.x < 0f ? Vector3.left : Vector3.right;
        }
        else
        {
            forward = direction.y > 0f ? Vector3.up : Vector3.down;
        }

        float forwardAmount = Mathf.Sin(progress * Mathf.PI) * 0.16f;
        float recoilAmount = Mathf.Sin(progress * Mathf.PI * 2f) * 0.04f;
        return forward * forwardAmount - forward * recoilAmount;
    }

    private static Sprite CreateHandSprite()
    {
        Texture2D texture = new Texture2D(8, 8)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color outline = new Color(0.25f, 0.12f, 0.05f, 1f);
        Color skin = new Color(0.92f, 0.62f, 0.36f, 1f);
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                bool body = x >= 2 && x <= 5 && y >= 1 && y <= 6;
                bool edge = body && (x == 2 || x == 5 || y == 1 || y == 6);
                texture.SetPixel(x, y, !body ? clear : edge ? outline : skin);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 16f);
    }
}
