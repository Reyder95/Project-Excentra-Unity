using UnityEngine;

public class CameraMovementHandler : MonoBehaviour
{
    Vector3 targetPos;
    Camera mainCamera;

    public float cameraSmoothSpeed = 0f;
    private float zoomTo = 0f;

    public float minZoom = 3f;
    public float maxZoom = 15f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    // Update is called once per frame
    void FixedUpdate()
    {
        if (ExcentraGame.battleManager != null)
        {
            targetPos = new Vector3(ExcentraGame.battleManager.turnManager.GetCurrentTurn().transform.position.x, ExcentraGame.battleManager.turnManager.GetCurrentTurn().transform.position.y, -1);

            transform.position = Vector3.Lerp(transform.position, targetPos, cameraSmoothSpeed * Time.fixedDeltaTime);
        }

        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, zoomTo, 6f * Time.fixedDeltaTime);
    }
}
