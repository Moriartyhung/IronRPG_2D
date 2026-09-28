using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Day15VillageBuildingOrganizer
{
    [MenuItem("IronRPG/Day 15/Organize Village Building Roles")]
    public static void OrganizeVillageBuildingRoles()
    {
        if (EditorSceneManager.GetActiveScene().name != "VillageScene")
        {
            bool continueAnyway = EditorUtility.DisplayDialog(
                "Day 15 Village Organizer",
                "The active scene is not VillageScene. Continue anyway?",
                "Continue",
                "Cancel");

            if (!continueAnyway)
            {
                return;
            }
        }

        int renamedCount = 0;
        renamedCount += RenameObject("ChiefHouse_Placeholder", "ChiefHouse");
        renamedCount += RenameObject("Tavern_Placeholder", "TavernBuildSite");
        renamedCount += RenameNumberedObjects("BushCluster_", "VillagerHouse_", 5);

        Transform root = GetOrCreateGroup("VillageObjects", null);
        Transform buildings = GetOrCreateGroup("VillageBuildings", root);
        Transform enterableBuildings = GetOrCreateGroup("EnterableBuildings", buildings);
        Transform villagerHouses = GetOrCreateGroup("VillagerHouses", buildings);
        Transform serviceBuildings = GetOrCreateGroup("ServiceBuildings", buildings);
        Transform plannedBuildings = GetOrCreateGroup("PlannedBuildings", buildings);

        int movedCount = 0;
        movedCount += MoveObject("PlayerHouse", enterableBuildings);
        movedCount += MoveObject("ChiefHouse", enterableBuildings);
        movedCount += MoveObject("EquipmentShop", serviceBuildings);
        movedCount += MoveObject("TavernBuildSite", plannedBuildings);
        movedCount += MoveObjectsByPrefix("VillagerHouse_", villagerHouses);

        Transform markers = GetOrCreateGroup("VillageSceneMarkers", null);
        int markerCount = 0;
        markerCount += CreateEntryMarkerNearBuilding("Entry_PlayerHouse", "PlayerHouse", markers);
        markerCount += CreateEntryMarkerNearBuilding("Entry_ChiefHouse", "ChiefHouse", markers);
        markerCount += CreateEntryMarkerNearBuilding("Entry_Tavern", "TavernBuildSite", markers);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Day 15 Village Organizer",
            $"Renamed objects: {renamedCount}\nMoved objects: {movedCount}\nCreated entry markers: {markerCount}\n\nSave VillageScene after checking the result.",
            "OK");
    }

    [MenuItem("IronRPG/Day 15/Fix Tavern Planning Marker")]
    public static void FixTavernPlanningMarker()
    {
        Transform root = GetOrCreateGroup("VillageObjects", null);
        Transform buildings = GetOrCreateGroup("VillageBuildings", root);
        Transform plannedBuildings = GetOrCreateGroup("PlannedBuildings", buildings);
        Transform decorations = GetOrCreateGroup("VillageObstacleDecorations", root);
        Transform markers = GetOrCreateGroup("VillageSceneMarkers", null);

        int fixedTreeCount = RestoreMistakenTavernTree(decorations);

        GameObject tavernBuildSite = GameObject.Find("TavernBuildSite");
        int markerCount = 0;
        if (tavernBuildSite == null)
        {
            tavernBuildSite = new GameObject("TavernBuildSite");
            Undo.RegisterCreatedObjectUndo(tavernBuildSite, "Create TavernBuildSite");
            tavernBuildSite.transform.position = new Vector3(-33f, -12f, 0f);
            markerCount++;
        }

        Undo.SetTransformParent(tavernBuildSite.transform, plannedBuildings, "Move TavernBuildSite");

        GameObject entryMarker = GameObject.Find("Entry_Tavern");
        if (entryMarker == null)
        {
            entryMarker = new GameObject("Entry_Tavern");
            Undo.RegisterCreatedObjectUndo(entryMarker, "Create Entry_Tavern");
            markerCount++;
        }

        Undo.SetTransformParent(entryMarker.transform, markers, "Move Entry_Tavern");
        entryMarker.transform.position = tavernBuildSite.transform.position + new Vector3(0f, -1.2f, 0f);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Day 15 Tavern Fix",
            $"Restored mistaken tree: {fixedTreeCount}\nCreated or updated tavern markers: {markerCount}\n\nTavernBuildSite is now only an empty planning marker in the lower-left area.",
            "OK");
    }

    private static int RenameNumberedObjects(string oldPrefix, string newPrefix, int count)
    {
        int renamedCount = 0;
        for (int i = 1; i <= count; i++)
        {
            renamedCount += RenameObject($"{oldPrefix}{i:00}", $"{newPrefix}{i:00}");
        }

        return renamedCount;
    }

    private static int RenameObject(string oldName, string newName)
    {
        if (GameObject.Find(newName) != null)
        {
            return 0;
        }

        GameObject target = GameObject.Find(oldName);
        if (target == null)
        {
            return 0;
        }

        Undo.RecordObject(target, $"Rename {oldName}");
        target.name = newName;
        EditorUtility.SetDirty(target);
        return 1;
    }

    private static Transform GetOrCreateGroup(string groupName, Transform parent)
    {
        GameObject existing = GameObject.Find(groupName);
        if (existing != null)
        {
            if (parent != null && existing.transform.parent != parent)
            {
                Undo.SetTransformParent(existing.transform, parent, $"Move {groupName}");
            }

            return existing.transform;
        }

        GameObject group = new GameObject(groupName);
        Undo.RegisterCreatedObjectUndo(group, $"Create {groupName}");

        if (parent != null)
        {
            Undo.SetTransformParent(group.transform, parent, $"Move {groupName}");
        }

        return group.transform;
    }

    private static int MoveObject(string objectName, Transform parent)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null || target.transform.parent == parent)
        {
            return 0;
        }

        Undo.SetTransformParent(target.transform, parent, $"Move {objectName}");
        return 1;
    }

    private static int MoveObjectsByPrefix(string prefix, Transform parent)
    {
        int count = 0;
        foreach (GameObject target in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (!target.name.StartsWith(prefix) || target.transform.parent == parent)
            {
                continue;
            }

            Undo.SetTransformParent(target.transform, parent, $"Move {target.name}");
            count++;
        }

        return count;
    }

    private static int RestoreMistakenTavernTree(Transform decorations)
    {
        GameObject target = GameObject.Find("TavernBuildSite");
        if (target == null || target.GetComponent<SpriteRenderer>() == null)
        {
            return 0;
        }

        Undo.RecordObject(target, "Restore mistaken tavern tree");
        target.name = GetAvailableNumberedName("SmallTree_", 16);
        Undo.SetTransformParent(target.transform, decorations, $"Move {target.name}");
        EditorUtility.SetDirty(target);
        return 1;
    }

    private static string GetAvailableNumberedName(string prefix, int startNumber)
    {
        for (int i = startNumber; i < 100; i++)
        {
            string candidate = $"{prefix}{i:00}";
            if (GameObject.Find(candidate) == null)
            {
                return candidate;
            }
        }

        return $"{prefix}{System.DateTime.Now:HHmmss}";
    }

    private static int CreateEntryMarkerNearBuilding(string markerName, string buildingName, Transform parent)
    {
        if (GameObject.Find(markerName) != null)
        {
            return 0;
        }

        GameObject building = GameObject.Find(buildingName);
        if (building == null)
        {
            return 0;
        }

        GameObject marker = new GameObject(markerName);
        Undo.RegisterCreatedObjectUndo(marker, $"Create {markerName}");
        Undo.SetTransformParent(marker.transform, parent, $"Move {markerName}");

        Vector3 position = building.transform.position;
        marker.transform.position = new Vector3(position.x, position.y - 0.9f, position.z);
        return 1;
    }
}
