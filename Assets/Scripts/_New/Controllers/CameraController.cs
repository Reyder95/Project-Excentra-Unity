// CameraController.cs
// Handles the movement of the camera depending on the current GameState
// Utilizes SmoothDamp for a smooth and seamless camera.

using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _mainCamera; // The current main camera in the scene
    private GameObject _target; // The current leader (obtained from OverworldController)
    private Vector3 _velocity = Vector3.zero;   // Reference velocity for SmoothDamp
    private float _cameraSmoothSpeed = 3f;  // Helps fine tune the camera smooth speed

    private void Start()
    {
        _mainCamera = GetComponent<Camera>();

        // When leader changes, we can directly grab the new leader
        GameContext.Current.OverworldController.OnLeaderChanged += SetTarget;
    }

    private void FixedUpdate()
    {
        if (_target == null)
            return;

        // Sets the current position to the target of the camera's position
        var currPos = _target.transform.position;

        // Sets the potential new target position
        Vector3 desiredTargetPos = new Vector3(
                currPos.x,
                currPos.y,
                -1);

        // SmoothDamps it towards the intended final target position
        transform.position = Vector3.SmoothDamp(transform.position, desiredTargetPos, ref _velocity, _cameraSmoothSpeed / 20f);
    }

    public void SetTarget(GameObject target)
    {
        _target = target;
    }

}
