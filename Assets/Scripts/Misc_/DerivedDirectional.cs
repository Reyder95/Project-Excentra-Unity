using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DerivedDirectional : BaseAoe
{
    public Vector2 destination;
    public GameObject destinationObject;

    public GameObject triangle;
    public GameObject circle;
    public GameObject line;
    public bool scaleX = true;      // Scale along the X-axis (default)
    Vector3 newScale;

    public float width = 4f;
    public float distance = 0f;
    public Vector2 endPoint;

    public Vector2 frozenScale;
    public Quaternion frozenRotation;
    public Vector2 frozenDestination;
    public Vector2 frozenPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        // Get the mouse position in screen coordinates (pixels)
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convert the screen position to world space (2D)
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

        // Set the z-value to 0 if you are working in 2D (to ignore depth)
        mouseWorldPosition.z = 0;

        if (skill is PlayerSkill)
        {
            if (skill != null && ((skill as PlayerSkill).shape == Shape.CONE || (skill as PlayerSkill).shape == Shape.LINE))
            {
                destination = mouseWorldPosition;

                // Calculate the distance between origin and destination

                // Adjust the scale of the sprite
                newScale = transform.localScale;

                if (scaleX) newScale.x = distance;  // Scale along X-axis
                newScale.y = width;

                // Optional: Rotate the sprite to face the destination
                Vector2 direction = (destination - (Vector2)originObject.transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                endPoint = (Vector2)originObject.transform.position + direction * distance;

                if (!freezeAoe)
                {
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                    transform.localScale = newScale;
                    transform.position = originObject.transform.position;
                }
                else
                {
                    transform.rotation = frozenRotation;
                    transform.localScale = frozenScale;
                    transform.position = frozenPosition;
                }

            }
        }
        else
        {

            if (destinationObject)
                destination = destinationObject.transform.position;

            Vector2 direction = (destination - (Vector2)transform.position).normalized;
            Vector2 offsetVector = direction * mechanicAttack.distanceOffset;
            destination = destination + offsetVector;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (scaleX) newScale.x = Vector2.Distance(transform.position, destination);  // Scale along X-axis
            newScale.y = width;

            transform.rotation = Quaternion.Euler(0, 0, angle);
            transform.localScale = newScale;
        }

    }

    public override void InitializeEnemyAoe(GameObject attackerObject, EnemyMechanic mechanic, MechanicAttack attack, SkillInformation info)
    {
        base.aoeData = new AoeData();
        this.mechanicAttack = attack;
        this.mechanic = mechanic;
        this.attackerObject = attackerObject;
        this.destination = attack.endpoint;

        Debug.Log(info.objectOrigin);

        // See concern in DerivedCircle.cs

        if (info.objectOrigin != null)
        {
            transform.position = info.objectOrigin.transform.position;
        }
        else
        {
            transform.position = attack.customOrigin;
        }

        if (info.objectTarget != null)
        {
            this.destinationObject = info.objectTarget;
        }
        else
        {
            this.destination = attack.endpoint;
        }

        width = attack.size;

        if (attack.aoeShape == Shape.CONE)
        {
            ColorCone();
        } else
        {
            ColorLine();
        }
    }

    public override void InitializeAoe(GameObject originObject, GameObject attackerObject, BaseSkill skill = null)
    {
        base.aoeData = new AoeData();
        this.skill = skill;
        this.mechanicAttack = null;
        this.mechanic = null;
        this.attackerObject = attackerObject;

        width = skill.radius;
        distance = skill.range;

        if (skill is PlayerSkill)
        {
            if (skill != null && (skill as PlayerSkill).shape == Shape.LINE)
                ColorLine();
            else
                ColorCone();
        }


        this.originObject = originObject;

        // Get the direction vector from the origin to the destination
        Vector2 direction = destination - (Vector2)originObject.transform.position;

        // Calculate the angle in radians and convert to degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Set the rotation of the sprite (Z-axis for 2D rotation)
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void ColorLine()
    {
        Color newColor = Color.red;
        SpriteRenderer lineRenderer = line.GetComponent<SpriteRenderer>();

        lineRenderer.color = newColor;
        Color colorWithAlpha = lineRenderer.color;
        colorWithAlpha.a = 0.5f;
        lineRenderer.color = colorWithAlpha;
    }

    private void ColorCone()
    {
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
    }

    public override Vector2 FrozenInfo()
    {
        return this.frozenDestination;
    }

    public override void FreezeAoe()
    {
        if (skill is PlayerSkill)
        {
            if (skill != null && ((skill as PlayerSkill).shape == Shape.CONE || (skill as PlayerSkill).shape == Shape.LINE))
            {
                base.FreezeAoe();
                frozenScale = transform.localScale;
                frozenRotation = transform.rotation;
                frozenDestination = destination;
                frozenPosition = originObject.transform.position;
            }
        }

    }
}
