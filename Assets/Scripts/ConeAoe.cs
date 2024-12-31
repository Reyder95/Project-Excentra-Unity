using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

public class ConeAoe : MonoBehaviour
{
    public Ability ability;
    public AoeData aoeData;
    public int arenaAoeIndex = -1;

    public GameObject originObject;
    public GameObject attackerObject;

    public bool freezeAoe = false;

    [Header("Cone")]
    public Vector2 destination;
    

    public float width = 4f;
    public GameObject triangle;
    public GameObject circle;
    public bool scaleX = true;      // Scale along the X-axis (default)
    Vector3 newScale;
    
    public Vector2 frozenScale;
    public Quaternion frozenRotation;
    public Vector2 frozenDestination;

    [Header("Circle")]
    public float radius = 2f;
    public GameObject circleAoe;
    public Vector2 circlePosition;

    public Vector2 frozenPosition;

    public void Start()
    {
        aoeData = new AoeData();
    }

    public void InitializeCone(GameObject originObject, GameObject attackerObject, Ability ability = null)
    {
        this.ability = ability;
        this.attackerObject = attackerObject;

        Color newColor = Color.red;
        SpriteRenderer triangleRenderer = triangle.GetComponent<SpriteRenderer>();
        SpriteRenderer circleRenderer = circle.GetComponent<SpriteRenderer>();

        triangleRenderer.color = newColor;
        Color colorWithAlpha = triangleRenderer.color;
        colorWithAlpha.a = 0.5f;
        triangleRenderer.color = colorWithAlpha;
        circleRenderer.color = newColor;
        colorWithAlpha = circleRenderer.color;
        colorWithAlpha.a = 0.5f;
        circleRenderer.color = colorWithAlpha;

        this.originObject = originObject;

        // Get the direction vector from the origin to the destination
        Vector2 direction = destination - (Vector2)originObject.transform.position;

        // Calculate the angle in radians and convert to degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Set the rotation of the sprite (Z-axis for 2D rotation)
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void InitializeCircle(GameObject originObject, GameObject attackerObject, Ability ability = null)
    {
        this.ability = ability;
        this.attackerObject = attackerObject;

        Color newColor = Color.red;
        SpriteRenderer circleRenderer = circleAoe.GetComponent<SpriteRenderer>();
        circleRenderer.color = newColor;
        Color colorWithAlpha = circleRenderer.color;
        colorWithAlpha.a = 0.5f;
        circleRenderer.color = colorWithAlpha;

        this.originObject = originObject;

        // Get the mouse position in screen coordinates (pixels)
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convert the screen position to world space (2D)
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // Set the z-value to 0 if you are working in 2D (to ignore depth)
        mouseWorldPosition.z = 0;

        circlePosition = mouseWorldPosition;

        if (!freezeAoe)
        {
            transform.position = circlePosition;
        }
        else
        {
            transform.position = frozenPosition;
        }
        

        circleAoe.SetActive(true);

    }

    public void Update()
    {
        // Get the mouse position in screen coordinates (pixels)
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convert the screen position to world space (2D)
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // Set the z-value to 0 if you are working in 2D (to ignore depth)
        mouseWorldPosition.z = 0;

        if (ability != null && ability.shape == Shape.CONE)
        {
            destination = mouseWorldPosition;

            transform.position = originObject.transform.position;
            // Calculate the distance between origin and destination
            float distance = Vector2.Distance(originObject.transform.position, destination);

            // Adjust the scale of the sprite
            newScale = transform.localScale;

            if (scaleX) newScale.x = distance;  // Scale along X-axis
            newScale.y = distance / 2;

            

            // Optional: Rotate the sprite to face the destination
            Vector2 direction = (destination - (Vector2)originObject.transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (!freezeAoe)
            {
                transform.rotation = Quaternion.Euler(0, 0, angle);
                transform.localScale = newScale;
            } else
            {
                transform.rotation = frozenRotation;
                transform.localScale = frozenScale;
            }

        }
        else if (ability != null && ability.shape == Shape.CIRCLE)
        {
            circlePosition = mouseWorldPosition;

            radius = ability.radius;

            Vector3 newScale = transform.localScale;

            newScale.y = radius;
            newScale.x = radius;

            transform.localScale = newScale;

            if (!freezeAoe)
            {
                transform.position = circlePosition;
            }
            else
            {
                transform.position = frozenPosition;
            }
            
        }

    }

    public void FreezeAoe()
    {
        if (ability != null)
        {
            freezeAoe = true;
            if (ability.shape == Shape.CIRCLE) 
            {
                frozenPosition = transform.position;
            }
            else if (ability.shape == Shape.CONE)
            {
                frozenScale = transform.localScale;
                frozenRotation = transform.rotation;
                frozenDestination = destination;
            }

        }
    }

    public void HandleAddTarget(GameObject target)
    {
        aoeData.AddTarget(target);
    }

    public void HandleRemoveTarget(GameObject target)
    {
        aoeData.RemoveTarget(target);
    }
}
