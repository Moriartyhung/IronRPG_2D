using UnityEngine;
using UnityEngine.SceneManagement;

public class VillageDirectionGuideController : MonoBehaviour
{
    private const string VillageSceneName = "VillageScene";
    private const string ChiefMarkerName = "Direction_cheif";
    private const string WeaponMarkerName = "Direction_weapon";
    private const string HomeMarkerName = "Direction_home";
    private const string BarMarkerName = "Direction_bar";

    private Transform chiefMarker;
    private Transform weaponMarker;
    private Transform homeMarker;
    private Transform barMarker;
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
        if (scene.name != VillageSceneName)
        {
            return;
        }

        GameObject controllerObject = GameObject.Find(nameof(VillageDirectionGuideController));
        VillageDirectionGuideController controller = controllerObject != null
            ? controllerObject.GetComponent<VillageDirectionGuideController>()
            : null;

        if (controller == null)
        {
            controllerObject = controllerObject != null ? controllerObject : new GameObject(nameof(VillageDirectionGuideController));
            controller = controllerObject.AddComponent<VillageDirectionGuideController>();
        }

        controller.Refresh();
    }

    private void Refresh()
    {
        chiefMarker = FindMarker(ChiefMarkerName);
        weaponMarker = FindMarker(WeaponMarkerName);
        homeMarker = FindMarker(HomeMarkerName);
        barMarker = FindMarker(BarMarkerName);
    }

    private static Transform FindMarker(string markerName)
    {
        GameObject markerObject = GameObject.Find(markerName);
        return markerObject != null ? markerObject.transform : null;
    }

    private void OnGUI()
    {
        if (BackpackUI.IsOpen || Camera.main == null)
        {
            return;
        }

        EnsureStyle();
        DrawDirectionLabel(chiefMarker, "村長家", 88f);
        DrawDirectionLabel(weaponMarker, "武器鋪", 88f);
        DrawDirectionLabel(homeMarker, "主角家", 88f);
        DrawDirectionLabel(barMarker, "酒館", 76f);
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
