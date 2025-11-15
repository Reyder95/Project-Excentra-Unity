using UnityEngine;

public class CameraMovementHandler : MonoBehaviour
{
    Vector3 targetPos;
    GameObject entity;
    Vector3 velocity = Vector3.zero; // NEW: For SmoothDamp

    Camera mainCamera;

    public float cameraSmoothSpeed = 100f; // Recommended: 0.1f - 0.3f for smooth follow
    private float zoomTo = 0f;

    public float minZoom = 3f;
    public float maxZoom = 15f;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        zoomTo = mainCamera.orthographicSize;
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            zoomTo = Mathf.Clamp(mainCamera.orthographicSize - scroll * 20f, minZoom, maxZoom);
        }
    }

    void FixedUpdate()
    {
        try
        {
            var currPos = targetPos;
            if (entity != null)
            {
                currPos = entity.transform.position;
            }
            Vector3 desiredTargetPos = new Vector3(
                    currPos.x,
                    currPos.y,
                    -1);

                float clampedX = Mathf.Clamp(desiredTargetPos.x, ExcentraGame.battleManager.arena.leftBound, ExcentraGame.battleManager.arena.rightBound);
                float clampedY = Mathf.Clamp(desiredTargetPos.y, ExcentraGame.battleManager.arena.bottomBound, ExcentraGame.battleManager.arena.topBound);

                targetPos = new Vector3(clampedX, clampedY, desiredTargetPos.z);

                // Use SmoothDamp instead of Lerp
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, cameraSmoothSpeed / 20f);
            }
        catch (MissingReferenceException) { }


        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, zoomTo, 6f * Time.fixedDeltaTime);
    }

    public void SetCameraPosition(Vector3 position, GameObject entity, float zoom = -1f)
    {
        this.entity = null;

        this.targetPos = position;
        this.entity = entity;

        if (zoom != -1f)
            zoomTo = zoom;
    }
}
