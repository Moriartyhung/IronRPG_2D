using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private bool clampToSceneRenderers = true;

    private Vector3 velocity;
    private Camera followCamera;
    private Bounds sceneBounds;
    private bool hasSceneBounds;

    public void SetOffsetY(float y)
    {
        offset = new Vector3(offset.x, y, offset.z);
    }

    private void Awake()
    {
        followCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null)
            {
                target = playerMovement.transform;
            }

            if (target == null)
            {
                GameObject player = GameObject.Find("Player");
                if (player == null)
                {
                    return;
                }

                target = player.transform;
            }
        }

        Vector3 targetPosition = target.position + offset;
        targetPosition = ClampToSceneBounds(targetPosition);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    private Vector3 ClampToSceneBounds(Vector3 targetPosition)
    {
        if (!clampToSceneRenderers)
        {
            return targetPosition;
        }

        if (followCamera == null)
        {
            followCamera = GetComponent<Camera>();
        }

        if (followCamera == null || !followCamera.orthographic)
        {
            return targetPosition;
        }

        EnsureSceneBounds();
        if (!hasSceneBounds)
        {
            return targetPosition;
        }

        float halfHeight = followCamera.orthographicSize;
        float halfWidth = halfHeight * followCamera.aspect;
        Vector3 clampedPosition = targetPosition;

        float minX = sceneBounds.min.x + halfWidth;
        float maxX = sceneBounds.max.x - halfWidth;
        clampedPosition.x = minX > maxX
            ? sceneBounds.center.x
            : Mathf.Clamp(targetPosition.x, minX, maxX);

        float minY = sceneBounds.min.y + halfHeight;
        float maxY = sceneBounds.max.y - halfHeight;
        clampedPosition.y = minY > maxY
            ? sceneBounds.center.y
            : Mathf.Clamp(targetPosition.y, minY, maxY);

        return clampedPosition;
    }

    private void EnsureSceneBounds()
    {
        if (hasSceneBounds)
        {
            return;
        }

        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);
        foreach (Renderer sceneRenderer in renderers)
        {
            if (sceneRenderer == null || !sceneRenderer.enabled)
            {
                continue;
            }

            if (target != null && sceneRenderer.transform.root == target.root)
            {
                continue;
            }

            if (!hasSceneBounds)
            {
                sceneBounds = sceneRenderer.bounds;
                hasSceneBounds = true;
                continue;
            }

            sceneBounds.Encapsulate(sceneRenderer.bounds);
        }
    }
}
