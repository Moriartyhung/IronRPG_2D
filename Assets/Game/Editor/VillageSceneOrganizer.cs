using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class VillageSceneOrganizer
{
    [MenuItem("IronRPG/Day 12/Organize Village Objects And Colliders")]
    public static void OrganizeVillageObjectsAndColliders()
    {
        int renamedCount = RenameIncorrectObjects();

        Transform root = GetOrCreateGroup("VillageObjects", null);
        Transform buildings = GetOrCreateGroup("VillageBuildings", root);
        Transform boundary = GetOrCreateGroup("VillageBoundary", root);
        Transform decorations = GetOrCreateGroup("VillageObstacleDecorations", root);
        Transform paths = GetOrCreateGroup("VillagePathDetails", root);
        Transform devOnly = GetOrCreateGroup("VillageDevOnly", root);

        int movedCount = 0;
        int colliderCount = 0;

        movedCount += MoveObjects("PlayerHouse", buildings);
        movedCount += MoveObjects("EquipmentShop", buildings);
        movedCount += MoveObjects("ChiefHouse_Placeholder", buildings);
        movedCount += MoveObjects("Tavern_Placeholder", buildings);
        movedCount += MoveObjectsByPrefix("VillageHouse_", buildings);

        movedCount += MoveObjectsByPrefix("NorthFence_", boundary);
        movedCount += MoveObjectsByPrefix("FenceSegment_", boundary);

        movedCount += MoveObjectsByPrefix("BushCluster_", decorations);
        movedCount += MoveObjectsByPrefix("SmallTree_", decorations);
        movedCount += MoveObjectsByPrefix("RockDecoration_", decorations);
        movedCount += MoveObjectsByPrefix("Signpost_", decorations);
        movedCount += MoveObjectsByPrefix("VillageProp_", decorations);

        movedCount += MoveObjects("Dev_Unused_TestFloorSprite", devOnly);

        colliderCount += AddCollider("PlayerHouse", new Vector2(0f, 0f), new Vector2(0.7f, 0.6f));
        colliderCount += AddCollider("EquipmentShop", new Vector2(0f, 0f), new Vector2(0.63f, 0.78f));
        colliderCount += AddCollider("ChiefHouse_Placeholder", new Vector2(0f, 0f), new Vector2(0.7f, 0.6f));
        colliderCount += AddCollider("Tavern_Placeholder", new Vector2(0f, 0f), new Vector2(0.7f, 0.6f));
        colliderCount += AddColliderByPrefix("VillageHouse_", new Vector2(0f, 0f), new Vector2(0.7f, 0.6f));

        colliderCount += AddColliderByPrefix("NorthFence_", new Vector2(0f, 0f), new Vector2(0.32f, 0.25f));
        colliderCount += AddColliderByPrefix("FenceSegment_", new Vector2(0f, 0f), new Vector2(0.32f, 0.25f));

        colliderCount += AddColliderByPrefix("BushCluster_", new Vector2(0f, -0.05f), new Vector2(0.42f, 0.32f));
        colliderCount += AddColliderByPrefix("SmallTree_", new Vector2(0f, -0.03f), new Vector2(0.24f, 0.22f));
        colliderCount += AddColliderByPrefix("RockDecoration_", new Vector2(0f, 0f), new Vector2(0.25f, 0.22f));
        colliderCount += AddColliderByPrefix("Signpost_", new Vector2(0f, -0.05f), new Vector2(0.22f, 0.3f));
        colliderCount += AddColliderByPrefix("VillageProp_", new Vector2(0f, -0.05f), new Vector2(0.28f, 0.32f));

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "Village Organizer",
            $"Renamed: {renamedCount}\nMoved to groups: {movedCount}\nAdded colliders: {colliderCount}\n\nSmall trees are treated as obstacles. Dev objects were ignored.",
            "OK");
    }

    private static int RenameIncorrectObjects()
    {
        int count = 0;
        count += RenameNumberedObjects("PebblePath_", "SmallTree_", 15);
        count += RenameObject("VillageHouse_NorthWestMarker", "VillageProp_PlayerHouseYard_01");
        count += RenameObject("Unused_TestFloorSprite", "Dev_Unused_TestFloorSprite");
        return count;
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

    private static int MoveObjects(string objectName, Transform parent)
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

    private static int AddCollider(string objectName, Vector2 offset, Vector2 size)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
        {
            Debug.LogWarning($"VillageSceneOrganizer: Could not find {objectName}.");
            return 0;
        }

        return AddCollider(target, offset, size);
    }

    private static int AddColliderByPrefix(string prefix, Vector2 offset, Vector2 size)
    {
        int count = 0;
        foreach (GameObject target in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (!target.name.StartsWith(prefix))
            {
                continue;
            }

            count += AddCollider(target, offset, size);
        }

        return count;
    }

    private static int AddCollider(GameObject target, Vector2 offset, Vector2 size)
    {
        if (target.GetComponent<SpriteRenderer>() == null || target.GetComponent<BoxCollider2D>() != null)
        {
            return 0;
        }

        BoxCollider2D collider = Undo.AddComponent<BoxCollider2D>(target);
        collider.offset = offset;
        collider.size = size;
        EditorUtility.SetDirty(target);
        return 1;
    }
}
