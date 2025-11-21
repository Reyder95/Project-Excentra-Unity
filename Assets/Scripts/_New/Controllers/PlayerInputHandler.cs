using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput _playerInput;
    private MovementController _movementController;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _movementController = GetComponent<MovementController>();
    }

    public void Move(InputAction.CallbackContext context)
    {
        _movementController.SetInputVector(context.ReadValue<Vector2>());
    }
}
