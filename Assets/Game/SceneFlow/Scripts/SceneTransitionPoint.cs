using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SceneTransitionPoint : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetSpawnPointName;
    [SerializeField] private bool requireBossDefeatedBeforeUse;
    [SerializeField] private string sceneObjectToDisableAfterArrival;
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.8f, 1f, 0.8f);

    private static string pendingSpawnPointName;
    private static string pendingObjectToDisableAfterArrival;
    private bool isLoading;

    public string TargetSceneName => targetSceneName;
    public string TargetSpawnPointName => targetSpawnPointName;

    public void Configure(string sceneName, string spawnPointName = "", bool requireBossDefeated = false, string objectToDisableAfterArrival = "")
    {
        targetSceneName = sceneName;
        targetSpawnPointName = spawnPointName;
        requireBossDefeatedBeforeUse = requireBossDefeated;
        sceneObjectToDisableAfterArrival = objectToDisableAfterArrival;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading || !IsPlayer(other) || string.IsNullOrWhiteSpace(targetSceneName) || !CanUseTransition())
        {
            return;
        }

        isLoading = true;
        pendingSpawnPointName = targetSpawnPointName;
        pendingObjectToDisableAfterArrival = sceneObjectToDisableAfterArrival;
        SceneManager.LoadScene(targetSceneName);
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.GetComponent<PlayerMovement>() != null || other.gameObject.name == "Player";
    }

    private bool CanUseTransition()
    {
        if (!requireBossDefeatedBeforeUse)
        {
            return true;
        }

        BossCoreBattleController boss = FindAnyObjectByType<BossCoreBattleController>();
        return boss != null && boss.CurrentHealth <= 0;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneLoadedEvent()
    {
        SceneManager.sceneLoaded -= MovePlayerToPendingSpawnPoint;
        SceneManager.sceneLoaded += MovePlayerToPendingSpawnPoint;
    }

    private static void MovePlayerToPendingSpawnPoint(Scene scene, LoadSceneMode mode)
    {
        bool hasPendingSpawn = !string.IsNullOrWhiteSpace(pendingSpawnPointName);
        string objectToDisable = GetObjectToDisableAfterArrival(scene);
        bool hasPendingDisable = !string.IsNullOrWhiteSpace(objectToDisable);
        if (!hasPendingSpawn && !hasPendingDisable)
        {
            return;
        }

        if (hasPendingSpawn)
        {
            GameObject spawnPoint = GameObject.Find(pendingSpawnPointName);
            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            GameObject player = playerMovement != null ? playerMovement.gameObject : GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogWarning($"SceneTransitionPoint could not find Player in scene '{scene.name}'.");
            }
            else if (spawnPoint != null)
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
            else
            {
                Debug.LogWarning($"SceneTransitionPoint could not find spawn point '{pendingSpawnPointName}' in scene '{scene.name}'.");
            }
        }

        if (hasPendingDisable)
        {
            GameObject blocker = GameObject.Find(objectToDisable);
            if (blocker != null)
            {
                blocker.SetActive(false);
            }
        }

        pendingSpawnPointName = null;
        pendingObjectToDisableAfterArrival = null;
    }

    private static string GetObjectToDisableAfterArrival(Scene scene)
    {
        if (!string.IsNullOrWhiteSpace(pendingObjectToDisableAfterArrival))
        {
            return pendingObjectToDisableAfterArrival;
        }

        if (scene.name == "ToCaveScene" && pendingSpawnPointName == "Exitformcave")
        {
            return "special box";
        }

        return string.Empty;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
        Gizmos.DrawLine(transform.position + Vector3.left * 0.45f, transform.position + Vector3.right * 0.45f);
        Gizmos.DrawLine(transform.position + Vector3.down * 0.45f, transform.position + Vector3.up * 0.45f);
    }
}
