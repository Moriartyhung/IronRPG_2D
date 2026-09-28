using UnityEngine;
using UnityEngine.SceneManagement;

public static class ToCaveSceneTransitionBootstrap
{
    private const string ToCaveSceneName = "ToCaveScene";
    private const string CaveSceneName = "CaveScene";
    private const string ToCaveMarkerName = "tocave";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneLoadedEvent()
    {
        SceneManager.sceneLoaded -= ConfigureToCaveMarker;
        SceneManager.sceneLoaded += ConfigureToCaveMarker;
        ConfigureToCaveMarker(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void ConfigureToCaveMarker(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != ToCaveSceneName)
        {
            return;
        }

        GameObject marker = GameObject.Find(ToCaveMarkerName);
        if (marker == null)
        {
            return;
        }

        Collider2D trigger = marker.GetComponent<Collider2D>();
        if (trigger == null)
        {
            BoxCollider2D box = marker.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1.2f, 1.2f);
            trigger = box;
        }

        trigger.isTrigger = true;

        SceneTransitionPoint transition = marker.GetComponent<SceneTransitionPoint>();
        if (transition == null)
        {
            transition = marker.AddComponent<SceneTransitionPoint>();
        }

        transition.Configure(CaveSceneName);
    }
}
