using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BossVineAttackController : MonoBehaviour
{
    [SerializeField] private BossCoreBattleController battleController;
    [SerializeField] private Sprite stabSprite;
    [SerializeField] private Sprite slamSprite;
    [SerializeField] private Sprite sweepSprite;
    [SerializeField] private Vector2 bossOrigin = new Vector2(17.1f, 8.6f);
    [SerializeField] private Vector2 sweepCenter = new Vector2(17.1f, 2.4f);
    [SerializeField] private float startDelay = 1.1f;
    [SerializeField] private float minAttackCooldown = 0.85f;
    [SerializeField] private float maxAttackCooldown = 1.35f;

    [Header("Vine Stab")]
    [SerializeField] private int stabDamage = 70;
    [SerializeField] private float stabDamageInterval = 0.5f;
    [SerializeField] private float stabTelegraphTime = 0.45f;
    [SerializeField] private float stabExtendTime = 0.55f;
    [SerializeField] private float stabHoldTime = 0.25f;
    [SerializeField] private float stabRetractTime = 0.35f;
    [SerializeField] private float stabMaxDistance = 18f;
    [SerializeField] private float stabWidth = 0.75f;
    [SerializeField] private float stabMinCooldown = 0.35f;
    [SerializeField] private float stabMaxCooldown = 0.65f;

    [Header("Vine Slam")]
    [SerializeField] private int slamDamage = 150;
    [SerializeField] private int slamCount = 3;
    [SerializeField] private float slamWarningTime = 0.65f;
    [SerializeField] private float slamActiveTime = 0.42f;
    [SerializeField] private Vector2 slamSize = new Vector2(4.2f, 12f);
    [SerializeField] private float slamTargetJitter = 1.2f;

    [Header("Vine Sweep")]
    [SerializeField] private int sweepDamage = 300;
    [SerializeField] private float sweepWarningTime = 0.75f;
    [SerializeField] private float sweepActiveTime = 0.95f;
    [SerializeField] private float sweepWidth = 19f;
    [SerializeField] private float sweepHeight = 2.1f;

    [Header("Visual")]
    [SerializeField] private int attackSortingOrder = 80;
    [SerializeField] private Color warningColor = new Color(1f, 0.08f, 0.02f, 0.34f);

    private Coroutine stabRoutine;
    private Coroutine skillRoutine;
    private PlayerMovement player;
    private Sprite warningSprite;
    private Texture2D warningTexture;

    private void Awake()
    {
        if (battleController == null)
        {
            battleController = GetComponent<BossCoreBattleController>() ?? FindAnyObjectByType<BossCoreBattleController>();
        }
    }

    private void OnEnable()
    {
        BossIntroDialogueTrigger.BossIntroCompleted += ActivateAttacks;
        if (BossIntroDialogueTrigger.HasCompleted)
        {
            ActivateAttacks();
        }
    }

    private void OnDisable()
    {
        BossIntroDialogueTrigger.BossIntroCompleted -= ActivateAttacks;
        StopAttacks();
    }

    public void Configure(BossCoreBattleController controller, Sprite vineStab, Sprite vineSlam, Sprite vineSweep, Vector2 origin, Vector2 horizontalSweepCenter, float horizontalSweepWidth)
    {
        battleController = controller;
        stabSprite = vineStab;
        slamSprite = vineSlam;
        sweepSprite = vineSweep;
        bossOrigin = origin;
        sweepCenter = horizontalSweepCenter;
        sweepWidth = Mathf.Max(8f, horizontalSweepWidth);
    }

    private void ActivateAttacks()
    {
        if (stabRoutine == null)
        {
            stabRoutine = StartCoroutine(StabLoop());
        }

        if (skillRoutine == null)
        {
            skillRoutine = StartCoroutine(SkillLoop());
        }
    }

    private void StopAttacks()
    {
        if (stabRoutine != null)
        {
            StopCoroutine(stabRoutine);
            stabRoutine = null;
        }

        if (skillRoutine != null)
        {
            StopCoroutine(skillRoutine);
            skillRoutine = null;
        }
    }

    private IEnumerator StabLoop()
    {
        yield return WaitForUnpausedSeconds(startDelay);

        while (!IsBossDefeated() && (battleController == null || battleController.IsBattleActive))
        {
            if (!CanAttackNow())
            {
                yield return null;
                continue;
            }

            yield return PerformVineStab();
            yield return WaitForUnpausedSeconds(Random.Range(stabMinCooldown, stabMaxCooldown));
        }

        stabRoutine = null;
    }

    private IEnumerator SkillLoop()
    {
        yield return WaitForUnpausedSeconds(startDelay + 1.2f);

        while (!IsBossDefeated() && (battleController == null || battleController.IsBattleActive))
        {
            if (!CanAttackNow())
            {
                yield return null;
                continue;
            }

            if (Random.value < 0.55f)
            {
                yield return PerformVineSlam();
            }
            else
            {
                yield return PerformVineSweep();
            }

            yield return WaitForUnpausedSeconds(Random.Range(minAttackCooldown, maxAttackCooldown));
        }

        skillRoutine = null;
    }

    private IEnumerator PerformVineStab()
    {
        Transform playerTransform = GetPlayerTransform();
        if (playerTransform == null)
        {
            yield break;
        }

        Vector2 origin = bossOrigin;
        Vector2 lockedTarget = playerTransform.position;
        Vector2 direction = lockedTarget - origin;
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = Vector2.down;
        }

        direction.Normalize();
        float distance = Mathf.Min(stabMaxDistance, Vector2.Distance(origin, lockedTarget));
        GameObject warning = CreateWarning("BossVineStabWarning", origin + direction * (distance * 0.5f), new Vector2(distance, stabWidth), DirectionToAngle(direction));
        yield return WaitForUnpausedSeconds(stabTelegraphTime);
        Destroy(warning);

        GameObject attack = CreateAttack("BossVineStab", stabSprite, origin, DirectionToAngle(direction));
        BossAttackDamageArea damageArea = AddDamageArea(attack, stabDamage, stabDamageInterval, true);
        BoxCollider2D hitbox = attack.GetComponent<BoxCollider2D>();

        yield return AnimateLinearVine(attack.transform, hitbox, origin, direction, 0f, distance, stabWidth, stabExtendTime);
        yield return WaitForUnpausedSeconds(stabHoldTime);
        yield return AnimateLinearVine(attack.transform, hitbox, origin, direction, distance, 0f, stabWidth, stabRetractTime);

        Destroy(damageArea.gameObject);
    }

    private IEnumerator PerformVineSlam()
    {
        for (int i = 0; i < slamCount; i++)
        {
            Transform playerTransform = GetPlayerTransform();
            if (playerTransform == null)
            {
                yield break;
            }

            Vector2 target = (Vector2)playerTransform.position + Random.insideUnitCircle * slamTargetJitter;
            Vector2 areaSize = GetBossRoomSlamSize();
            Vector2 areaCenter = new Vector2(target.x, GetBossRoomSlamCenterY());
            GameObject warning = CreateWarning("BossVineSlamWarning", areaCenter, areaSize, 0f);
            yield return WaitForUnpausedSeconds(slamWarningTime);
            Destroy(warning);

            GameObject slam = CreateAttack("BossVineSlam", slamSprite, areaCenter + Vector2.up * 0.65f, 0f);
            AddDamageArea(slam, slamDamage, 1f, false);
            BoxCollider2D hitbox = slam.GetComponent<BoxCollider2D>();
            SetSpriteWorldSize(slam.transform, hitbox, slamSprite, areaSize);

            float elapsed = 0f;
            Vector3 start = areaCenter + Vector2.up * 0.65f;
            Vector3 end = areaCenter;
            while (elapsed < slamActiveTime)
            {
                if (IsBossDefeated())
                {
                    break;
                }

                if (CanAttackNow())
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / slamActiveTime);
                    slam.transform.position = Vector3.Lerp(start, end, 1f - Mathf.Pow(1f - t, 3f));
                }

                yield return null;
            }

            Destroy(slam);
            yield return WaitForUnpausedSeconds(0.22f);
        }
    }

    private IEnumerator PerformVineSweep()
    {
        yield return PerformOneSweep(true);
        yield return WaitForUnpausedSeconds(0.3f);
        yield return PerformOneSweep(false);
    }

    private IEnumerator PerformOneSweep(bool leftToRight)
    {
        Vector2 origin = bossOrigin;
        float halfWidth = Mathf.Max(5f, sweepWidth * 0.5f);
        float sweepDepth = Mathf.Max(6f, Mathf.Abs(origin.y - sweepCenter.y) + 7f);
        float sweepLength = Mathf.Sqrt(halfWidth * halfWidth + sweepDepth * sweepDepth);
        Vector2 warningSize = new Vector2(halfWidth, sweepDepth);
        float sideSign = leftToRight ? -1f : 1f;
        Vector2 warningCenter = origin + new Vector2(sideSign * halfWidth * 0.5f, -sweepDepth * 0.42f);
        GameObject warning = CreateWarning(leftToRight ? "BossVineSweepWarning_LR" : "BossVineSweepWarning_RL", warningCenter, warningSize, 0f);
        yield return WaitForUnpausedSeconds(sweepWarningTime);
        Destroy(warning);

        GameObject sweep = CreateAttack(leftToRight ? "BossVineSweep_LR" : "BossVineSweep_RL", sweepSprite, origin, 0f);
        AddDamageArea(sweep, sweepDamage, 1f, false);
        BoxCollider2D hitbox = sweep.GetComponent<BoxCollider2D>();

        float startAngle = leftToRight ? -168f : -84f;
        float endAngle = leftToRight ? -96f : -12f;
        float elapsed = 0f;
        while (elapsed < sweepActiveTime)
        {
            if (IsBossDefeated())
            {
                break;
            }

            if (CanAttackNow())
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / sweepActiveTime);
                float eased = Mathf.SmoothStep(0f, 1f, t);
                float angle = Mathf.Lerp(startAngle, endAngle, eased);
                Vector2 direction = DirectionFromAngle(angle);
                SetLinearObjectPose(sweep.transform, hitbox, sweepSprite, origin, direction, sweepLength, sweepHeight);
            }

            yield return null;
        }

        Destroy(sweep);
    }

    private IEnumerator AnimateLinearVine(Transform vine, BoxCollider2D hitbox, Vector2 origin, Vector2 direction, float fromLength, float toLength, float width, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (IsBossDefeated())
            {
                break;
            }

            if (CanAttackNow())
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                float length = Mathf.Lerp(fromLength, toLength, eased);
                SetLinearObjectPose(vine, hitbox, stabSprite, origin, direction, Mathf.Max(0.05f, length), width);
            }

            yield return null;
        }
    }

    private GameObject CreateAttack(string objectName, Sprite sprite, Vector2 position, float rotation)
    {
        GameObject attack = new GameObject(objectName);
        attack.transform.position = new Vector3(position.x, position.y, 0f);
        attack.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

        SpriteRenderer renderer = attack.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = attackSortingOrder;

        BoxCollider2D hitbox = attack.AddComponent<BoxCollider2D>();
        hitbox.isTrigger = true;
        return attack;
    }

    private GameObject CreateWarning(string objectName, Vector2 position, Vector2 size, float rotation)
    {
        GameObject warning = new GameObject(objectName);
        warning.transform.position = new Vector3(position.x, position.y, 0f);
        warning.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
        warning.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = warning.AddComponent<SpriteRenderer>();
        renderer.sprite = GetWarningSprite();
        renderer.color = warningColor;
        renderer.sortingOrder = attackSortingOrder - 1;
        return warning;
    }

    private BossAttackDamageArea AddDamageArea(GameObject attack, int attackDamage, float interval, bool repeat)
    {
        BossAttackDamageArea damageArea = attack.AddComponent<BossAttackDamageArea>();
        damageArea.Configure(attackDamage, interval, repeat);
        return damageArea;
    }

    private void SetLinearObjectPose(Transform target, BoxCollider2D hitbox, Sprite sprite, Vector2 origin, Vector2 direction, float length, float width)
    {
        target.position = origin + direction * (length * 0.5f);
        target.rotation = Quaternion.Euler(0f, 0f, DirectionToAngle(direction));
        SetSpriteWorldSize(target, hitbox, sprite, new Vector2(length, width));
    }

    private static void SetSpriteWorldSize(Transform target, BoxCollider2D hitbox, Sprite sprite, Vector2 worldSize)
    {
        if (sprite == null)
        {
            target.localScale = new Vector3(worldSize.x, worldSize.y, 1f);
            hitbox.size = Vector2.one;
            return;
        }

        Vector2 spriteSize = sprite.bounds.size;
        float scaleX = spriteSize.x <= 0f ? worldSize.x : worldSize.x / spriteSize.x;
        float scaleY = spriteSize.y <= 0f ? worldSize.y : worldSize.y / spriteSize.y;
        target.localScale = new Vector3(scaleX, scaleY, 1f);
        hitbox.size = spriteSize;
        hitbox.offset = Vector2.zero;
    }

    private bool CanAttackNow()
    {
        return (battleController == null || battleController.IsBattleActive)
            && !GameplayPauseState.EnemiesPaused
            && !BackpackUI.IsOpen;
    }

    private Vector2 GetBossRoomSlamSize()
    {
        float fullHeight = Mathf.Max(slamSize.y, Mathf.Abs(bossOrigin.y - sweepCenter.y) + 8f);
        return new Vector2(Mathf.Max(3.8f, slamSize.x), fullHeight);
    }

    private float GetBossRoomSlamCenterY()
    {
        float fullHeight = GetBossRoomSlamSize().y;
        return bossOrigin.y - fullHeight * 0.5f + 1.1f;
    }

    private float GetSweepReach()
    {
        return Mathf.Max(sweepWidth, Mathf.Abs(bossOrigin.y - sweepCenter.y) + 9f);
    }

    private IEnumerator WaitForUnpausedSeconds(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (IsBossDefeated())
            {
                yield break;
            }

            if (CanAttackNow())
            {
                elapsed += Time.deltaTime;
            }

            yield return null;
        }
    }

    private Transform GetPlayerTransform()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerMovement>();
        }

        return player != null ? player.transform : null;
    }

    private bool IsBossDefeated()
    {
        return battleController != null && battleController.CurrentHealth <= 0;
    }

    private Sprite GetWarningSprite()
    {
        if (warningSprite != null)
        {
            return warningSprite;
        }

        warningTexture = new Texture2D(1, 1)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        warningTexture.SetPixel(0, 0, Color.white);
        warningTexture.Apply();
        warningSprite = Sprite.Create(warningTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        warningSprite.hideFlags = HideFlags.HideAndDontSave;
        return warningSprite;
    }

    private static float DirectionToAngle(Vector2 direction)
    {
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private static Vector2 DirectionFromAngle(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;
    }

    private void OnDestroy()
    {
        if (warningSprite != null)
        {
            Destroy(warningSprite);
        }

        if (warningTexture != null)
        {
            Destroy(warningTexture);
        }
    }
}
