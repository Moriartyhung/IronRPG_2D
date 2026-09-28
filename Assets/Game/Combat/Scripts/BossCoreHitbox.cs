using UnityEngine;

[DisallowMultipleComponent]
public class BossCoreHitbox : MonoBehaviour, IBossDamageReceiver
{
    [SerializeField] private BossCoreNode node;

    public void Configure(BossCoreNode owner)
    {
        node = owner;
    }

    public bool ReceivePlayerAttack(int baseDamage, PlayerInventory inventory, out int damageDealt, out string message)
    {
        damageDealt = 0;
        message = string.Empty;
        return node != null && node.ReceiveCoreAttack(baseDamage, inventory, out damageDealt, out message);
    }
}
