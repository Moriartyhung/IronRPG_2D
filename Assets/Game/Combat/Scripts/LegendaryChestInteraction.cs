using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider2D))]
public class LegendaryChestInteraction : MonoBehaviour
{
    [SerializeField] private WeaponItem legendaryRune = new WeaponItem(
        "legendary_thunder_rune",
        "雷痕符文",
        WeaponQuality.Legendary,
        300,
        0,
        0,
        null,
        EquipmentKind.Rune);
    [SerializeField] private string lockedMessage = "精英怪還守在前方，現在還不能開啟寶箱。";
    [SerializeField] private string openedMessage = "獲得傳說符文：雷痕符文。雷痕符文已承認你。";
    [SerializeField] private float interactionDistance = 1.8f;

    private bool playerInRange;
    private bool opened;
    private BoxCollider2D trigger;
    private GUIStyle promptStyle;

    private void Awake()
    {
        trigger = GetComponent<BoxCollider2D>();
        trigger.isTrigger = true;
    }

    private void Update()
    {
        RefreshPlayerInRange();
        if (!playerInRange || opened)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            TryOpenChest();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerInRange = false;
        }
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.GetComponent<PlayerMovement>() != null
            || other.GetComponentInParent<PlayerMovement>() != null
            || other.gameObject.name == "Player";
    }

    private void RefreshPlayerInRange()
    {
        Transform player = GetClosestPlayerTransform();
        if (player == null)
        {
            playerInRange = false;
            return;
        }

        Vector2 playerPosition = player.position;
        if (trigger != null)
        {
            Vector2 closestPoint = trigger.ClosestPoint(playerPosition);
            float distanceToTrigger = Vector2.Distance(playerPosition, closestPoint);
            float fallbackDistance = Vector2.Distance(transform.position, playerPosition);
            playerInRange = distanceToTrigger <= 0.45f || fallbackDistance <= interactionDistance;
            return;
        }

        playerInRange = Vector2.Distance(transform.position, playerPosition) <= interactionDistance;
    }

    private Transform GetClosestPlayerTransform()
    {
        PlayerMovement[] players = FindObjectsByType<PlayerMovement>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Transform closest = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < players.Length; i++)
        {
            CheckClosest(players[i].transform, ref closest, ref closestDistance);
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (taggedPlayer != null)
        {
            CheckClosest(taggedPlayer.transform, ref closest, ref closestDistance);
        }

        GameObject namedPlayer = GameObject.Find("Player");
        if (namedPlayer != null)
        {
            CheckClosest(namedPlayer.transform, ref closest, ref closestDistance);
        }

        return closest;
    }

    private void CheckClosest(Transform candidate, ref Transform closest, ref float closestDistance)
    {
        if (candidate == null || !candidate.gameObject.activeInHierarchy)
        {
            return;
        }

        float distance = Vector2.Distance(candidate.position, transform.position);
        if (distance < closestDistance)
        {
            closestDistance = distance;
            closest = candidate;
        }
    }

    private void TryOpenChest()
    {
        if (!EliteGuardianCombat.IsForestEliteDefeated)
        {
            CombatMessageDisplay.Show(lockedMessage, 2.6f);
            return;
        }

        PlayerInventory.AddWeapon(legendaryRune);
        opened = true;
        CombatMessageDisplay.Show(openedMessage, 3f);
        CurrentObjectiveDisplay.SetObjective("回到武器鋪，請武器匠確認雷痕符文");
    }

    private void OnGUI()
    {
        if (BackpackUI.IsOpen)
        {
            return;
        }

        if (!playerInRange || opened)
        {
            return;
        }

        EnsureStyle();
        string prompt = EliteGuardianCombat.IsForestEliteDefeated ? "按 E 開啟寶箱" : "擊敗精英怪後才能開啟寶箱";
        float width = Mathf.Min(360f, Screen.width - 40f);
        GUI.Box(new Rect((Screen.width - width) * 0.5f, Screen.height - 128f, width, 42f), prompt, promptStyle);
    }

    private void EnsureStyle()
    {
        if (promptStyle != null)
        {
            return;
        }

        promptStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
    }
}
