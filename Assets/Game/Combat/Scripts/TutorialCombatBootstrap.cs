using UnityEngine;
using UnityEngine.SceneManagement;

public static class TutorialCombatBootstrap
{
    private const string TutorialSceneName = "TutorialScene";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneLoadedEvent()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != TutorialSceneName)
        {
            return;
        }

        PlayerProgress.Ensure();
        PlayerInventory.Ensure();
        CombatMessageDisplay.Ensure();

        PlayerMovement movement = Object.FindAnyObjectByType<PlayerMovement>();
        GameObject player = movement != null ? movement.gameObject : GameObject.Find("Player");
        if (player == null)
        {
            return;
        }

        if (player.GetComponent<PlayerAttack>() == null)
        {
            player.AddComponent<PlayerAttack>();
        }

        if (player.GetComponent<PlayerHealth>() == null)
        {
            player.AddComponent<PlayerHealth>();
        }
    }
}
