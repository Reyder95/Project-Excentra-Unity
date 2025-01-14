//// ConeAoe.cs
//// Needs a name change to AoeComponent.cs

//using UnityEngine;

//// Handles all the physical game object effects on screen, colors, shapes, scaling, direction, etc
//public class ConeAoe : MonoBehaviour
//{
//    // General information
//    public Skill skill; // The skill associated with this AoE.
//    public AoeData aoeData; // The data for this aoe (targets, etc)
//    public int arenaAoeIndex = -1;  // The index in the arenaAoE section this is associated with.

//    // Who is the attacker, and where should this object be the origin to?
//    public GameObject originObject;
//    public GameObject attackerObject;

//    // Do we want to freeze the aoe in place and not interact based on other factors? (usually done when an aoe goes into attack phase)
//    public bool freezeAoe = false;

//    public Vector2 destination; // Destination, used for line and cone. Essentially allows us to get the direction of the cone/line based on destination
    
//    // Cone and line variables. Self explanatory for the most part
//    public float width = 4f;
//    public float distance = 0f;

//    // Game objects that are set in the inspector that we will manipulate via scale or color.
//    public GameObject triangle;
//    public GameObject circle;
//    public GameObject line;
//    public bool scaleX = true;      // Scale along the X-axis (default)
//    Vector3 newScale;
    
//    // Frozen information that we will save when freezeAoe is true, and used instead of the actual data
//    public Vector2 frozenScale;
//    public Quaternion frozenRotation;
//    public Vector2 frozenDestination;

//    // Circle information. 
//    public float radius = 2f;
//    public GameObject circleAoe;
//    public Vector2 circlePosition;

//    // Frozen position of the circle
//    public Vector2 frozenPosition;

//    public void Start()
//    {
//        aoeData = new AoeData();
//    }

//    // We initialize all the data of the cone and place that cone in a respective spot based on some information
//    public void InitializeCone(GameObject originObject, GameObject attackerObject, Skill skill = null)
//    {
//        this.skill = skill;
//        this.attackerObject = attackerObject;

//        Color newColor = Color.red;
//        SpriteRenderer triangleRenderer = triangle.GetComponent<SpriteRenderer>();
//        SpriteRenderer circleRenderer = circle.GetComponent<SpriteRenderer>();

//        width = skill.radius;
//        distance = skill.range;

//        triangleRenderer.color = newColor;
//        Color colorWithAlpha = triangleRenderer.color;
//        colorWithAlpha.a = 0.5f;
//        triangleRenderer.color = colorWithAlpha;
//        circleRenderer.color = newColor;
//        colorWithAlpha = circleRenderer.color;
//        colorWithAlpha.a = 0.5f;
//        circleRenderer.color = colorWithAlpha;

//        this.originObject = originObject;

//        // Get the direction vector from the origin to the destination
//        Vector2 direction = destination - (Vector2)originObject.transform.position;

//        // Calculate the angle in radians and convert to degrees
//        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

//        // Set the rotation of the sprite (Z-axis for 2D rotation)
//        transform.rotation = Quaternion.Euler(0, 0, angle);
//    }

//    public void InitializeLine(GameObject originObject, GameObject attackerObject, Skill skill = null)
//    {
//        this.skill = skill;
//        this.attackerObject = attackerObject;

//        Color newColor = Color.red;
//        SpriteRenderer lineRenderer = line.GetComponent<SpriteRenderer>();

//        width = skill.radius;
//        distance = skill.range;

//        lineRenderer.color = newColor;
//        Color colorWithAlpha = lineRenderer.color;
//        colorWithAlpha.a = 0.5f;
//        lineRenderer.color = colorWithAlpha;

//        this.originObject = originObject;

//        // Get the direction vector from the origin to the destination
//        Vector2 direction = destination - (Vector2)originObject.transform.position;

//        // Calculate the angle in radians and convert to degrees
//        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

//        // Set the rotation of the sprite (Z-axis for 2D rotation)
//        transform.rotation = Quaternion.Euler(0, 0, angle);
//    }

//    public void InitializeCircle(GameObject originObject, GameObject attackerObject, Skill skill = null)
//    {
//        this.skill = skill;
//        this.attackerObject = attackerObject;

//        Color newColor = Color.red;
//        SpriteRenderer circleRenderer = circleAoe.GetComponent<SpriteRenderer>();
//        circleRenderer.color = newColor;
//        Color colorWithAlpha = circleRenderer.color;
//        colorWithAlpha.a = 0.5f;
//        circleRenderer.color = colorWithAlpha;

//        this.originObject = originObject;

//        // Get the mouse position in screen coordinates (pixels)
//        Vector3 mouseScreenPosition = Input.mousePosition;

//        // Convert the screen position to world space (2D)
//        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

//        // Set the z-value to 0 if you are working in 2D (to ignore depth)
//        mouseWorldPosition.z = 0;

//        if (Vector2.Distance(this.attackerObject.transform.position, mouseWorldPosition) < skill.range)
//            circlePosition = mouseWorldPosition;

//        if (!freezeAoe)
//        {
//            transform.position = circlePosition;
//        }
//        else
//        {
//            transform.position = frozenPosition;
//        }
        

//        circleAoe.SetActive(true);

//    }

//    public void Update()
//    {
//        // Get the mouse position in screen coordinates (pixels)
//        Vector3 mouseScreenPosition = Input.mousePosition;

//        // Convert the screen position to world space (2D)
//        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

//        // Set the z-value to 0 if you are working in 2D (to ignore depth)
//        mouseWorldPosition.z = 0;

//        if (skill != null && (skill.shape == Shape.CONE || skill.shape == Shape.LINE) )
//        {
//            destination = mouseWorldPosition;

//            transform.position = originObject.transform.position;
//            // Calculate the distance between origin and destination

//            // Adjust the scale of the sprite
//            newScale = transform.localScale;

//            if (scaleX) newScale.x = distance;  // Scale along X-axis
//            newScale.y = width;

//            // Optional: Rotate the sprite to face the destination
//            Vector2 direction = (destination - (Vector2)originObject.transform.position).normalized;
//            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

//            if (!freezeAoe)
//            {
//                transform.rotation = Quaternion.Euler(0, 0, angle);
//                transform.localScale = newScale;
//            } else
//            {
//                transform.rotation = frozenRotation;
//                transform.localScale = frozenScale;
//            }

//        }
//        else if (skill != null && skill.shape == Shape.CIRCLE)
//        {
//            radius = skill.radius;
//            Vector3 newScale = transform.localScale;

//            newScale.y = radius;
//            newScale.x = radius;

//            transform.localScale = newScale;

//            if (skill.targetMode == TargetMode.FREE)
//            {
//                if (Vector2.Distance(this.attackerObject.transform.position, mouseWorldPosition) < skill.range / 2f)
//                    circlePosition = mouseWorldPosition;
//                else
//                {
//                    Vector2 direction = mouseWorldPosition - this.attackerObject.transform.position;
//                    direction = direction.normalized * (skill.range / 2);
//                    circlePosition = (Vector2)this.attackerObject.transform.position + direction;
//                }
//            }
//            else if (skill.targetMode == TargetMode.SELF)
//            {
//                circlePosition = originObject.transform.position;
//            }
//            else if (skill.targetMode == TargetMode.SELECT)
//            {
//                circlePosition = originObject.transform.position;
//            }

//            if (!freezeAoe)
//            {
//                transform.position = circlePosition;
//            }
//            else
//            {
//                transform.position = frozenPosition;
//            }


//        }

//    }

//    // Snapshots information for the freezing of the aoe position
//    public void FreezeAoe()
//    {
//        if (skill != null)
//        {
//            freezeAoe = true;
//            if (skill.shape == Shape.CIRCLE) 
//            {
//                frozenPosition = transform.position;
//            }
//            else if (skill.shape == Shape.CONE || skill.shape == Shape.LINE)
//            {
//                frozenScale = transform.localScale;
//                frozenRotation = transform.rotation;
//                frozenDestination = destination;
//            }

//        }
//    }

//    public void HandleAddTarget(GameObject target)
//    {
//        aoeData.AddTarget(target);
//    }

//    public void HandleRemoveTarget(GameObject target)
//    {
//        aoeData.RemoveTarget(target);
//    }
//}
