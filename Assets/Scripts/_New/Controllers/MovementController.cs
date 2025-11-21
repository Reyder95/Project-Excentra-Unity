using UnityEngine;

public class MovementController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Vector2 _inputVector;

    private GameState _state;
    [SerializeField] private float _baseSpeed;

    private void OnEnable()
    {
        if (GameContext.Current?.GameState != null)
            GameContext.Current.GameState.OnStateEntered += HandleStateChange;
    }

    private void OnDisable()
    {
        if (GameContext.Current?.GameState != null)
            GameContext.Current.GameState.OnStateEntered -= HandleStateChange;
    }

    public void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Start()
    {
        if (GameContext.Current != null)
            _state = GameContext.Current.GameState.CurrentState;
    }

    public void FixedUpdate()
    {
        Vector2 newPosition = _rb.position + _inputVector * _baseSpeed * Time.fixedDeltaTime;

        if (_state == GameState.Exploration)
        {

            _rb.MovePosition(newPosition);
        }
    }

    public void SetInputVector(Vector2 inputVector)
    {
        _inputVector = inputVector;
    }

    public void HandleStateChange(GameState prevState, GameState newState)
    {
        _state = newState;
    }
}
