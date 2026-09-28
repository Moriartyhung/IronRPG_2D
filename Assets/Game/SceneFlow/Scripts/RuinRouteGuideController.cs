using UnityEngine;
using UnityEngine.SceneManagement;

public class RuinRouteGuideController : MonoBehaviour
{
    private const string TutorialSceneName = "TutorialScene";
    private const string VillageDirectionMarker = "Direction_ToVillage";
    private const string ForestDirectionMarker = "Direction_forest";
    private const string CaveRoadDirectionMarker = "Direction_caveroad";
    private const string ForestBlockerName = "special box1";
    private const string CaveBlockerName = "special box2";

    private Transform villageMarker;
    private Transform forestMarker;
    private Transform caveRoadMarker;
    private GUIStyle labelStyle;
    private Texture2D labelBackground;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneLoadedEvent()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != TutorialSceneName)
        {
            return;
        }

        GameObject controllerObject = GameObject.Find(nameof(RuinRouteGuideController));
        RuinRouteGuideController controller = controllerObject != null
            ? controllerObject.GetComponent<RuinRouteGuideController>()
            : null;

        if (controller == null)
        {
            controllerObject = controllerObject != null ? controllerObject : new GameObject(nameof(RuinRouteGuideController));
            controller = controllerObject.AddComponent<RuinRouteGuideController>();
        }

        controller.Refresh();
    }

    private void Update()
    {
        RefreshRouteBlockers();
    }

    private void Refresh()
    {
        villageMarker = FindMarker(VillageDirectionMarker);
        forestMarker = FindMarker(ForestDirectionMarker);
        caveRoadMarker = FindMarker(CaveRoadDirectionMarker);
        RefreshRouteBlockers();
    }

    private static Transform FindMarker(string markerName)
    {
        GameObject markerObject = GameObject.Find(markerName);
        return markerObject != null ? markerObject.transform : null;
    }

    private static void RefreshRouteBlockers()
    {
        MainQuestStage stage = MainQuestState.CurrentStage;
        SetBlockerActive(ForestBlockerName, stage < MainQuestStage.ForestInvestigation);
        SetBlockerActive(CaveBlockerName, stage < MainQuestStage.PrepareForCave);
    }

    private static void SetBlockerActive(string blockerName, bool active)
    {
        GameObject blocker = FindSceneObject(blockerName);
        if (blocker != null && blocker.activeSelf != active)
        {
            blocker.SetActive(active);
        }
    }

    private static GameObject FindSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject candidate = objects[i];
            if (candidate != null && candidate.name == objectName && candidate.scene.IsValid())
            {
                return candidate;
            }
        }

        return null;
    }

    private void OnGUI()
    {
        if (BackpackUI.IsOpen || Camera.main == null)
        {
            return;
        }

        EnsureStyle();
        DrawDirectionLabel(villageMarker, "↓ 村莊", 84f);
        DrawDirectionLabel(forestMarker, "← 森林", 84f);
        DrawDirectionLabel(caveRoadMarker, "→ 洞穴道路", 112f);
    }

    private void DrawDirectionLabel(Transform marker, string text, float width)
    {
        if (marker == null)
        {
            return;
        }

        Vector3 screen = Camera.main.WorldToScreenPoint(marker.position + Vector3.up * 0.75f);
        if (screen.z < 0f)
        {
            return;
        }

        Rect rect = new Rect(screen.x - width * 0.5f, Screen.height - screen.y - 16f, width, 30f);
        GUI.Label(rect, text, labelStyle);
    }

    private void EnsureStyle()
    {
        if (labelStyle != null)
        {
            return;
        }

        labelBackground = new Texture2D(1, 1)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        labelBackground.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.72f));
        labelBackground.Apply();

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = new Color(1f, 0.88f, 0.42f, 1f),
                background = labelBackground
            }
        };
    }

    private void OnDestroy()
    {
        if (labelBackground != null)
        {
            Destroy(labelBackground);
        }
    }
}
