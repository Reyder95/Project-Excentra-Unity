using UnityEngine;

public enum ArrowDirection
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public class ArrowMover : MonoBehaviour
{
    public ArrowDirection direction;
    public bool inwards = true;
    private Vector2 startPoint;
    private float distance = 0.1f;
    private float speed = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPoint = gameObject.transform.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 moveDirection = Vector2.zero;
        if (direction == ArrowDirection.UP)
        {
            moveDirection = new Vector3(0, 1);
        }
        else if (direction == ArrowDirection.DOWN)
        {
            moveDirection = new Vector3(0, -1);
        }
        else if (direction == ArrowDirection.LEFT)
        {
            moveDirection = new Vector3(-1, 0);
        }
        else if (direction == ArrowDirection.RIGHT)
        {
            moveDirection = new Vector3(1, 0);
        }

        Vector2 prevPosition = gameObject.transform.localPosition;

        if (inwards)
        {
            gameObject.transform.localPosition += moveDirection * (speed / 5) * Time.fixedDeltaTime;
            if (Vector2.Distance(gameObject.transform.localPosition, startPoint) >= distance)
                inwards = false;
        }
        else
        {
            gameObject.transform.localPosition -= moveDirection * (speed / 5) * Time.fixedDeltaTime;
            if (Vector2.Distance(gameObject.transform.localPosition, startPoint) >= distance)
                inwards = true;
        }

        if (prevPosition == (Vector2)gameObject.transform.localPosition)
        {
            gameObject.transform.localPosition = startPoint;
        }
    }
}
