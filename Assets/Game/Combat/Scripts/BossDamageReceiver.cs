public interface IBossDamageReceiver
{
    bool ReceivePlayerAttack(int baseDamage, PlayerInventory inventory, out int damageDealt, out string message);
}
