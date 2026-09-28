using UnityEngine;

public class EnemyVisualAnimator : MonoBehaviour
{
    [SerializeField] private float moveBobHeight = 0.035f;
    [SerializeField] private float moveBobSpeed = 8f;
    [SerializeField] private float moveSquashAmount = 0.08f;
    [SerializeField] private float attackDuration = 0.38f;
    [SerializeField] private float attackLungeDistance = 0.22f;
    [SerializeField] private float attackScaleAmount = 0.34f;
    [SerializeField] private Color attackFlashColor = new Color(1f, 0.35f, 0.18f, 1f);

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer attackEffectRenderer;
    private SimpleEnemyChase chase;
    private Transform visualTransform;
    private Transform attackEffectTransform;
    private Vector3 baseLocalPosition;
    private Vector3 baseLocalScale;
    private Color baseColor = Color.white;
    private float attackUntil;
    private Vector2 attackDirection = Vector2.down;
    private static Sprite attackEffectSprite;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        chase = GetComponent<SimpleEnemyChase>();

        if (spriteRenderer != null)
        {
            visualTransform = spriteRenderer.transform;
            baseLocalPosition = visualTransform.localPosition;
            baseLocalScale = visualTransform.localScale;
            baseColor = spriteRenderer.color;
            CreateAttackEffect();
        }
    }

    private void Update()
    {
        if (visualTransform == null)
        {
            return;
        }

        bool isAttacking = Time.time < attackUntil;
        bool isMoving = chase != null && chase.IsMoving;
        Vector2 direction = GetFacingDirection();
        ApplyFacing(direction);

        if (isAttacking)
        {
            ApplyAttackAnimation(direction);
            return;
        }

        if (isMoving)
        {
            ApplyMoveAnimation();
            return;
        }

        ApplyIdleAnimation();
    }

    public void PlayAttack(Vector3 targetPosition)
    {
        Vector3 attackOrigin = spriteRenderer != null ? spriteRenderer.bounds.center : transform.position;
        Vector2 toTarget = targetPosition - attackOrigin;
        if (toTarget.sqrMagnitude > 0.01f)
        {
            attackDirection = toTarget.normalized;
        }

        attackUntil = Time.time + attackDuration;
    }

    private void CreateAttackEffect()
    {
        GameObject effect = new GameObject("AttackEffect");
        effect.transform.SetParent(transform, false);
        attackEffectTransform = effect.transform;
        attackEffectTransform.localPosition = baseLocalPosition;

        attackEffectRenderer = effect.AddComponent<SpriteRenderer>();
        attackEffectRenderer.sprite = GetAttackEffectSprite();
        attackEffectRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        attackEffectRenderer.sortingOrder = spriteRenderer.sortingOrder + 3;
        attackEffectRenderer.enabled = false;
    }

    private static Sprite GetAttackEffectSprite()
    {
        if (attackEffectSprite != null)
        {
            return attackEffectSprite;
        }

        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color core = new Color(1f, 0.95f, 0.35f, 1f);
        Color edge = new Color(1f, 0.18f, 0.08f, 0.9f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int diagonal = Mathf.Abs(y - (size - 1 - x));
                bool inMainSlash = diagonal <= 2 && x > 5 && x < 27 && y > 5 && y < 27;
                bool inEdgeSlash = diagonal <= 4 && x > 4 && x < 28 && y > 4 && y < 28;
                texture.SetPixel(x, y, inMainSlash ? core : inEdgeSlash ? edge : clear);
            }
        }

        texture.Apply();
        attackEffectSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        attackEffectSprite.hideFlags = HideFlags.HideAndDontSave;
        return attackEffectSprite;
    }

    private Vector2 GetFacingDirection()
    {
        if (Time.time < attackUntil)
        {
            return attackDirection;
        }

        if (chase != null && chase.LastMoveDirection.sqrMagnitude > 0.01f)
        {
            return chase.LastMoveDirection;
        }

        return attackDirection;
    }

    private void ApplyFacing(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > 0.05f)
        {
            spriteRenderer.flipX = direction.x < 0f;
        }
    }

    private void ApplyMoveAnimation()
    {
        float phase = Time.time * moveBobSpeed;
        float bob = Mathf.Abs(Mathf.Sin(phase)) * moveBobHeight;
        float squash = Mathf.Sin(phase) * moveSquashAmount;

        spriteRenderer.color = baseColor;
        SetAttackEffectVisible(false);
        visualTransform.localPosition = baseLocalPosition + Vector3.up * bob;
        visualTransform.localScale = new Vector3(
            baseLocalScale.x * (1f + squash * 0.35f),
            baseLocalScale.y * (1f - squash),
            baseLocalScale.z);
        visualTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(phase) * 2.2f);
    }

    private void ApplyAttackAnimation(Vector2 direction)
    {
        float normalized = 1f - Mathf.Clamp01((attackUntil - Time.time) / attackDuration);
        float windup = Mathf.Clamp01(normalized / 0.35f);
        float strike = Mathf.Clamp01((normalized - 0.24f) / 0.34f);
        float recover = Mathf.Clamp01((normalized - 0.58f) / 0.42f);
        float punch = Mathf.Sin(normalized * Mathf.PI);
        float lungeAmount = Mathf.Lerp(-attackLungeDistance * 0.45f, attackLungeDistance, strike) * (1f - recover * 0.65f);
        Vector3 lunge = (Vector3)(direction.normalized * lungeAmount);

        visualTransform.localPosition = baseLocalPosition + lunge;
        visualTransform.localScale = new Vector3(
            baseLocalScale.x * (1f + attackScaleAmount * punch),
            baseLocalScale.y * (1f - attackScaleAmount * 0.45f * punch),
            baseLocalScale.z);
        visualTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(normalized * Mathf.PI * 2f) * 11f);
        spriteRenderer.color = Color.Lerp(baseColor, attackFlashColor, Mathf.Sin(normalized * Mathf.PI));
        ApplyAttackEffect(direction, normalized, windup);
    }

    private void ApplyIdleAnimation()
    {
        float breathe = Mathf.Sin(Time.time * 2.4f) * 0.025f;
        spriteRenderer.color = baseColor;
        SetAttackEffectVisible(false);
        visualTransform.localPosition = baseLocalPosition;
        visualTransform.localScale = new Vector3(
            baseLocalScale.x * (1f + breathe),
            baseLocalScale.y * (1f - breathe),
            baseLocalScale.z);
        visualTransform.localRotation = Quaternion.identity;
    }

    private void ApplyAttackEffect(Vector2 direction, float normalized, float windup)
    {
        if (attackEffectRenderer == null || attackEffectTransform == null)
        {
            return;
        }

        bool showEffect = normalized > 0.22f && normalized < 0.72f;
        SetAttackEffectVisible(showEffect);
        if (!showEffect)
        {
            return;
        }

        Vector2 safeDirection = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector2.down;
        float angle = Mathf.Atan2(safeDirection.y, safeDirection.x) * Mathf.Rad2Deg - 45f;
        float pulse = Mathf.Sin(Mathf.InverseLerp(0.22f, 0.72f, normalized) * Mathf.PI);
        float spriteWidth = spriteRenderer != null && spriteRenderer.sprite != null
            ? spriteRenderer.sprite.bounds.size.x * Mathf.Abs(visualTransform.localScale.x)
            : 0.42f;

        attackEffectTransform.localPosition = baseLocalPosition
            + (Vector3)(safeDirection * (spriteWidth * (0.35f + 0.45f * windup) + attackLungeDistance * 1.1f));
        attackEffectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        attackEffectTransform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.35f, pulse);
        attackEffectRenderer.color = new Color(1f, 0.78f, 0.28f, 0.35f + 0.65f * pulse);
    }

    private void SetAttackEffectVisible(bool visible)
    {
        if (attackEffectRenderer != null)
        {
            attackEffectRenderer.enabled = visible;
        }
    }
}
