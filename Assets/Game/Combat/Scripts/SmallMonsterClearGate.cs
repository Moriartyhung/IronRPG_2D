using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class SmallMonsterClearGate : MonoBehaviour
{
    [SerializeField] private string monsterRootName = "ForestSmallMonsters";
    [SerializeField] private string lockedMessage = "森林裡的小怪還沒有清乾淨，不能直接前往精英怪。";
    [SerializeField] private string unlockedMessage = "小怪已經清乾淨，通往精英怪的小路打開了。";
    [SerializeField] private float messageInterval = 1.8f;

    private BoxCollider2D wallCollider;
    private bool opened;
    private float nextMessageTime;

    private void Awake()
    {
        wallCollider = GetComponent<BoxCollider2D>();
        wallCollider.isTrigger = false;
    }

    private void Update()
    {
        if (!opened && AreSmallMonstersCleared())
        {
            OpenGate();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryShowLockedMessage(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryShowLockedMessage(collision.collider);
    }

    private void TryShowLockedMessage(Collider2D other)
    {
        if (opened || !IsPlayer(other) || Time.time < nextMessageTime)
        {
            return;
        }

        CombatMessageDisplay.Show(lockedMessage, 2.2f);
        nextMessageTime = Time.time + messageInterval;
    }

    private bool AreSmallMonstersCleared()
    {
        GameObject monsterRoot = GameObject.Find(monsterRootName);
        if (monsterRoot == null)
        {
            return true;
        }

        EnemyHealth[] enemies = monsterRoot.GetComponentsInChildren<EnemyHealth>();
        return enemies == null || enemies.Length == 0;
    }

    private void OpenGate()
    {
        opened = true;
        if (wallCollider != null)
        {
            wallCollider.enabled = false;
        }

        CombatMessageDisplay.Show(unlockedMessage, 2.5f);
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.GetComponent<PlayerMovement>() != null
            || other.GetComponentInParent<PlayerMovement>() != null
            || other.gameObject.name == "Player";
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null)
        {
            return;
        }

        Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.45f);
        Vector3 center = transform.TransformPoint(box.offset);
        Vector3 size = new Vector3(box.size.x * transform.lossyScale.x, box.size.y * transform.lossyScale.y, 0.1f);
        Gizmos.DrawCube(center, size);
    }
}
