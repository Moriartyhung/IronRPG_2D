public static class GameplayPauseState
{
    private static int enemyPauseRequests;
    private static float enemiesPausedUntil;

    public static bool EnemiesPaused => enemyPauseRequests > 0 || UnityEngine.Time.time < enemiesPausedUntil;

    public static void RequestEnemyPause()
    {
        enemyPauseRequests++;
    }

    public static void ReleaseEnemyPause()
    {
        if (enemyPauseRequests > 0)
        {
            enemyPauseRequests--;
        }
    }

    public static void PauseEnemiesFor(float seconds)
    {
        if (seconds <= 0f)
        {
            return;
        }

        enemiesPausedUntil = UnityEngine.Mathf.Max(enemiesPausedUntil, UnityEngine.Time.time + seconds);
    }

    public static void ClearEnemyPauses()
    {
        enemyPauseRequests = 0;
        enemiesPausedUntil = 0f;
    }
}
