using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents an area-of-effect that can be either a cone or a circle,
/// linked to an Ability. This script updates the shape's visual transform
/// (scale/rotation) based on mouse position or an origin object.
/// </summary>
public class ConeAoe : MonoBehaviour
{
    [Header("References and Data")]
    public Ability ability;
    public AoeData aoeData;
    public int arenaAoeIndex = -1;

    [Tooltip("Origin object for the AoE shape, typically the caster/character origin.")]
    public GameObject originObject;

    [Tooltip("Attacker object for context or referencing the attacker specifically.")]
    public GameObject attackerObject;

    [Tooltip("When true, the AoE shape locks transform & scale to the moment freeze is called.")]
    public bool freezeAoe = false;

    [Header("Cone Settings")]
    public Vector2 destination;      // Where the cone is aiming
    public float width = 4f;
    public GameObject triangle;      // The sprite representing the cone shape
    public GameObject circle;        // Optional circle graphic for the tip, if needed
    public bool scaleX = true;       // Toggle for whether we scale along X-axis or not

    // We store the 'frozen' values for reapplying after freeze
    private Vector2 frozenScale;
    private Quaternion frozenRotation;
    private Vector2 frozenDestination;
    private Vector3 newScale;

    [Header("Circle Settings")]
    public float radius = 2f;
    [Tooltip("Separate GameObject for circle AoE, if we do not want to reuse 'triangle' for it.")]
    public GameObject circleAoe;
    public Vector2 circlePosition;
    private Vector2 frozenPosition;

    private void Start()
    {
        aoeData = new AoeData();
    }


    /// <summary>
    /// Initializes this AoE as a cone, coloring it, setting origin, etc.
    /// </summary>
    public void InitializeCone(GameObject origin, GameObject attacker, Ability sourceAbility = null)
    {
        ability = sourceAbility;
        attackerObject = attacker;
        originObject = origin;

        if (!triangle)
        {
            Debug.LogWarning("Triangle GameObject (cone sprite) not assigned.", this);
            return;
        }

        // Set default color & alpha for the cone
        Color tintedColor = new Color(1f, 0f, 0f, 0.5f); // red with half alpha
        SetSpriteColor(triangle, tintedColor);
        if (circle) SetSpriteColor(circle, tintedColor);

        // Compute direction & angle
        Vector2 originPos = originObject.transform.position;
        Vector2 direction = destination - originPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Now rotate the entire AoE object
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }


    /// <summary>
    /// Initializes this AoE as a circle, coloring it, setting origin, etc.
    /// </summary>
    public void InitializeCircle(GameObject origin, GameObject attacker, Ability sourceAbility = null)
    {
        ability = sourceAbility;
        attackerObject = attacker;
        originObject = origin;

        if (!circleAoe)
        {
            Debug.LogWarning("CircleAoe GameObject not assigned.", this);
            return;
        }

        // Set default color & alpha for the circle
        Color tintedColor = new Color(1f, 0f, 0f, 0.5f); // red with half alpha
        SetSpriteColor(circleAoe, tintedColor);

        // Convert the current mouse position to world space
        Vector3 mouseWorldPosition = GetMouseWorldPosition();

        circlePosition = mouseWorldPosition;

        if (!freezeAoe)
            transform.position = circlePosition;
        else
            transform.position = frozenPosition;

        circleAoe.SetActive(true);
    }


    private void Update()
    {
        // If no ability is associated, skip
        if (ability == null || originObject == null) return;

        // 1) Determine mouse world position
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        mouseWorldPosition.z = 0;

        // 2) Branch: circle vs cone
        if (ability.shape == Shape.CONE)
        {
            ConeUpdate(mouseWorldPosition);
        }
        else if (ability.shape == Shape.CIRCLE)
        {
            CircleUpdate(mouseWorldPosition);
        }
    }


    /// <summary>
    /// Updates the AoE if the shape is a cone. Called each Update().
    /// </summary>
    private void ConeUpdate(Vector3 mouseWorldPosition)
    {
        // Destination is the mouse
        destination = mouseWorldPosition;

        // Keep the cone's origin pinned to the originObject
        transform.position = originObject.transform.position;

        float distance = Vector2.Distance(originObject.transform.position, destination);

        // Adjust scale of the sprite
        newScale = transform.localScale;
        if (scaleX) newScale.x = distance;   // scale along X-axis
        newScale.y = distance / 2;

        Vector2 direction = (destination - (Vector2)originObject.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (!freezeAoe)
        {
            transform.rotation = Quaternion.Euler(0, 0, angle);
            transform.localScale = newScale;
        }
        else
        {
            transform.rotation = frozenRotation;
            transform.localScale = frozenScale;
        }
    }


    /// <summary>
    /// Updates the AoE if the shape is a circle. Called each Update().
    /// </summary>
    private void CircleUpdate(Vector3 mouseWorldPosition)
    {
        circlePosition = mouseWorldPosition;
        radius = ability.radius; // presumably from the ability itself

        Vector3 localScale = transform.localScale;
        localScale.x = radius;
        localScale.y = radius;
        transform.localScale = localScale;

        if (!freezeAoe)
            transform.position = circlePosition;
        else
            transform.position = frozenPosition;
    }


    /// <summary>
    /// On user action or after finalizing the AoE, freeze the shape's transform.
    /// </summary>
    public void FreezeAoe()
    {
        if (ability == null) return;
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


    /// <summary>
    /// Called externally, the AoE can track collisions or "entered" targets.
    /// </summary>
    public void HandleAddTarget(GameObject target)
    {
        aoeData.AddTarget(target);
    }

    public void HandleRemoveTarget(GameObject target)
    {
        aoeData.RemoveTarget(target);
    }


    /// <summary>
    /// Helper to unify alpha + color assignment for the sprite renderers.
    /// </summary>
    private void SetSpriteColor(GameObject go, Color newColor)
    {
        if (!go) return;
        var sr = go.GetComponent<SpriteRenderer>();
        if (!sr) return;
        sr.color = newColor;
    }


    /// <summary>
    /// Converts screen mouse position to world space (z=0).
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        return worldPos;
    }
}
