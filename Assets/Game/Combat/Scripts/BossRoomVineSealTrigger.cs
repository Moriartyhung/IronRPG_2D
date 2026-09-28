using UnityEngine;

public class BossRoomVineSealTrigger : MonoBehaviour
{
    [SerializeField] private GameObject vineSeal;

    public void Configure(GameObject targetVineSeal)
    {
        vineSeal = targetVineSeal;
    }

    private void Awake()
    {
        if (vineSeal != null)
        {
            vineSeal.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        if (vineSeal != null)
        {
            vineSeal.SetActive(true);
        }

        enabled = false;
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.GetComponentInParent<PlayerMovement>() != null
            || other.GetComponentInParent<PlayerHealth>() != null;
    }
}
