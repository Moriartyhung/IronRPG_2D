using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerDeathRespawn
{
    private const string HomeSceneName = "PlayerHouseInteriorScene";
    private const string HomeSpawnPointName = "Entry_FromVillage";

    private static bool respawnPending;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneLoadedEvent()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    public static void RespawnAtHome()
    {
        if (respawnPending)
        {
            return;
        }

        respawnPending = true;
        PlayerInventory.ClearGold();
        CombatMessageDisplay.Show("主角倒下了，金幣全部遺失。", 3f);
        SceneManager.LoadScene(HomeSceneName);
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!respawnPending || scene.name != HomeSceneName)
        {
            return;
        }

        GameObject spawnPoint = GameObject.Find(HomeSpawnPointName);
        PlayerMovement playerMovement = Object.FindAnyObjectByType<PlayerMovement>();
        GameObject player = playerMovement != null ? playerMovement.gameObject : GameObject.Find("Player");
        if (player != null && spawnPoint != null)
        {
            Vector3 spawnPosition = spawnPoint.transform.position;
            player.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, player.transform.position.z);

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.position = spawnPosition;
                body.linearVelocity = Vector2.zero;
            }
        }

        PlayerHealth playerHealth = Object.FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.HealToFull();
            playerHealth.GrantInvincibility(2.5f);
        }

        respawnPending = false;
        CombatMessageDisplay.Show("在主角家醒來。", 2.5f);
    }
}
