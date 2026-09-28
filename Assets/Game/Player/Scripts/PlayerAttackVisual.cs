using UnityEngine;

public class PlayerAttackVisual : MonoBehaviour
{
    [SerializeField] private Transform visualTransform;
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private SpriteRenderer slashRenderer;
    [SerializeField] private Sprite slashSprite;
    [SerializeField] private float attackDuration = 0.24f;
    [SerializeField] private float lungeDistance = 0.14f;
    [SerializeField] private float squashAmount = 0.1f;
    [SerializeField] private float slashDistanceRatio = 0.54f;
    [SerializeField] private float slashScale = 1f;

    private Vector3 baseVisualPosition;
    private Vector3 baseVisualScale = Vector3.one;
    private Vector2 attackDirection = Vector2.down;
    private float attackReach = 1f;
    private float attackStartedAt = -999f;
    private bool hasBasePose;
    private static Sprite runtimeSlashSprite;

    private void Awake()
    {
        ResolveReferences();
        CaptureBasePose();
        HideSlash();
    }

    private void LateUpdate()
    {
        ResolveReferences();
        if (Time.time >= attackStartedAt + attackDuration)
        {
            RestoreBasePose();
            HideSlash();
            return;
        }

        float normalized = Mathf.Clamp01((Time.time - attackStartedAt) / Mathf.Max(0.01f, attackDuration));
        ApplyBodyMotion(normalized);
        ApplySlashEffect(normalized);
    }

    public void PlayAttack(Vector2 direction, float reach)
    {
        ResolveReferences();
        CaptureBasePose();

        if (direction.sqrMagnitude > 0.01f)
        {
            attackDirection = SnapToCardinal(direction.normalized);
        }

        attackReach = Mathf.Max(0.45f, reach);
        attackStartedAt = Time.time;
    }

    private void ResolveReferences()
    {
        if (playerRenderer == null)
        {
            Transform childVisual = transform.Find("PlayerVisual");
            playerRenderer = childVisual != null
                ? childVisual.GetComponent<SpriteRenderer>()
                : GetComponentInChildren<SpriteRenderer>();
        }

        if (visualTransform == null && playerRenderer != null)
        {
            visualTransform = playerRenderer.transform;
        }

        if (slashRenderer == null)
        {
            Transform slash = transform.Find("PlayerImpactEffect");
            if (slash == null)
            {
                slash = transform.Find("PlayerSlashEffect");
                if (slash != null)
                {
                    slash.name = "PlayerImpactEffect";
                }
            }

            if (slash == null)
            {
                GameObject slashObject = new GameObject("PlayerImpactEffect");
                slashObject.transform.SetParent(transform, false);
                slash = slashObject.transform;
            }

            slashRenderer = slash.GetComponent<SpriteRenderer>();
            if (slashRenderer == null)
            {
                slashRenderer = slash.gameObject.AddComponent<SpriteRenderer>();
            }
        }

        if (slashRenderer != null)
        {
            slashRenderer.sprite = slashSprite != null && slashSprite.name.Contains("punch")
                ? slashSprite
                : GetRuntimeSlashSprite();
            int baseOrder = playerRenderer != null ? playerRenderer.sortingOrder : 20;
            slashRenderer.sortingOrder = baseOrder + 6;
        }
    }

    private void CaptureBasePose()
    {
        if (visualTransform == null)
        {
            return;
        }

        baseVisualPosition = visualTransform.localPosition;
        baseVisualScale = visualTransform.localScale;
        hasBasePose = true;
    }

    private void RestoreBasePose()
    {
        if (!hasBasePose || visualTransform == null)
        {
            return;
        }

        visualTransform.localPosition = baseVisualPosition;
        visualTransform.localScale = baseVisualScale;
        visualTransform.localRotation = Quaternion.identity;
    }

    private void ApplyBodyMotion(float normalized)
    {
        if (!hasBasePose || visualTransform == null)
        {
            return;
        }

        float lunge = Mathf.Sin(normalized * Mathf.PI) * lungeDistance;
        Vector3 direction = new Vector3(attackDirection.x, attackDirection.y, 0f);
        visualTransform.localPosition = baseVisualPosition + direction * lunge;

        float squash = Mathf.Sin(normalized * Mathf.PI) * squashAmount;
        bool horizontal = Mathf.Abs(attackDirection.x) > Mathf.Abs(attackDirection.y);
        float xScale = horizontal ? 1f + squash : 1f - squash * 0.45f;
        float yScale = horizontal ? 1f - squash * 0.35f : 1f + squash;
        visualTransform.localScale = new Vector3(
            baseVisualScale.x * xScale,
            baseVisualScale.y * yScale,
            baseVisualScale.z);
    }

    private void ApplySlashEffect(float normalized)
    {
        if (slashRenderer == null)
        {
            return;
        }

        bool show = normalized >= 0.18f && normalized <= 0.68f;
        slashRenderer.enabled = show;
        if (!show)
        {
            return;
        }

        float pulse = Mathf.Sin(Mathf.InverseLerp(0.18f, 0.68f, normalized) * Mathf.PI);
        Transform slashTransform = slashRenderer.transform;
        slashTransform.localPosition = new Vector3(
            attackDirection.x * attackReach * slashDistanceRatio,
            attackDirection.y * attackReach * slashDistanceRatio,
            0f);
        slashTransform.localRotation = Quaternion.Euler(0f, 0f, GetSlashAngle(attackDirection));
        slashTransform.localScale = Vector3.one * Mathf.Lerp(slashScale * 0.82f, slashScale * 1.18f, pulse);
        slashRenderer.color = new Color(1f, 0.9f, 0.38f, Mathf.Lerp(0.35f, 1f, pulse));
    }

    private void HideSlash()
    {
        if (slashRenderer != null)
        {
            slashRenderer.enabled = false;
        }
    }

    private static Vector2 SnapToCardinal(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x < 0f ? Vector2.left : Vector2.right;
        }

        return direction.y > 0f ? Vector2.up : Vector2.down;
    }

    private static float GetSlashAngle(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x < 0f ? 180f : 0f;
        }

        return direction.y > 0f ? 90f : -90f;
    }

    private static Sprite GetRuntimeSlashSprite()
    {
        if (runtimeSlashSprite != null)
        {
            return runtimeSlashSprite;
        }

        Texture2D texture = CreateSlashTexture();
        runtimeSlashSprite = Sprite.Create(texture, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f), 64f);
        runtimeSlashSprite.hideFlags = HideFlags.HideAndDontSave;
        return runtimeSlashSprite;
    }

    public static Texture2D CreateSlashTexture()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color edge = new Color(0.55f, 0.25f, 1f, 0.8f);
        Color body = new Color(1f, 0.85f, 0.2f, 0.95f);
        Color core = new Color(1f, 1f, 0.86f, 1f);
        Vector2 center = new Vector2(32f, 32f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 offset = new Vector2(x + 0.5f, y + 0.5f) - center;
                float radius = offset.magnitude;
                float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
                bool innerBurst = radius <= 7f;
                bool midBurst = radius > 7f && radius <= 13f && Mathf.Abs(Mathf.Sin(angle * Mathf.Deg2Rad * 4f)) > 0.32f;
                bool outerSpark = radius > 13f && radius <= 24f && Mathf.Abs(Mathf.Sin(angle * Mathf.Deg2Rad * 6f)) > 0.72f;
                bool lightning = Mathf.Abs(offset.x) <= 2f && offset.y > -22f && offset.y < 22f;
                bool lightningBend = offset.x > -9f && offset.x < 10f && Mathf.Abs(offset.y - offset.x * 1.2f) < 2.1f && offset.y > -13f && offset.y < 13f;

                texture.SetPixel(x, y, innerBurst ? core : midBurst ? body : outerSpark || lightning || lightningBend ? edge : clear);
            }
        }

        texture.Apply();
        return texture;
    }
}
