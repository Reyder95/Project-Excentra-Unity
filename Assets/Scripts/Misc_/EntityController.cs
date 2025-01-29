/* EntityController.cs
 * 
 * Handles all the basic "functionality" of the character. Has simple WASD movement, and specific OnMouseEnter triggers that affect the entity as well.
 * Maybe some of that logic needs to split, but for now it's fine.
 * 
 * Movement only functions if playerInput is enabled, which is handled each round (only current user gets playerInput enabled).
 */

using UnityEngine;
using UnityEngine.InputSystem;

public class EntityController : MonoBehaviour
{
    // Movement
    public float moveSpeed = 0f;
    private Vector2 inputVector;    // Using Unity's InputSystem, this generates an x,y vector of where the unit is movement. From -1 to 1.

    private Vector2 localScale;     // Snapshots the base localScale. Used for switching direction.
    public Vector3 turnStartPos;    // Snapshots the start position of an entity during their turn. Useful for putting them back at the start based on movement enable/disable

    // Boss
    private GameObject target;      // Target entity to move towards
    private bool autoMove = false;  // Enables auto movement for boss. If this is triggered, the boss will move towards the target directly (Navigation not implemented yet)

    // Range - Shows range that entity can attack within. Basic chooses their "basic range", special chooses the specific skill's range
    public bool basicActive = false;
    public bool specialActive = false;

    // Range Objects - The actual sprite for the range is here. 
    public GameObject circleRange;  // Prefab
    public GameObject circleBasicRangeInstance; // Current instance of the range on the entity
    public SpriteRenderer circleBasicRangeRenderer; // Sprite Renderer for changing colors and such

    // Components - Self Explanatory
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public LineRenderer lineRenderer;   // Shows the range of movement around the entity on their turn.
    public EntityStats entityStats;
    public PlayerInput playerInput;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;

    // Misc.
    public GameObject charGround;   // A simple object denoting an entity's ground. Used for handling depth (displaying entities in front and behind each other)

    [Header("Collider Hitboxes")]
    public Vector2 aliveOffset;
    public Vector2 aliveSize;
    public Vector2 deadOffset;
    public Vector2 deadSize;

    // Info for moving skills
    private Vector2 targetLocation;
    private float skillMoveSpeed = 0f;
    private bool isSkillMoving = false;

    private EnemyAI enemyAi;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        entityStats = GetComponent<EntityStats>();
        playerInput = GetComponent<PlayerInput>();
        lineRenderer = GetComponent<LineRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        enemyAi = GetComponent<EnemyAI>();

        spriteRenderer.material.SetFloat("_Thickness", 0f); // Outline, by default should be hidden.
        localScale = gameObject.transform.localScale;

        // Instance of range is instantiated on everyone, just hidden until needed
        circleBasicRangeInstance = Instantiate(circleRange, transform.position, Quaternion.identity);
        circleBasicRangeRenderer = circleBasicRangeInstance.GetComponent<SpriteRenderer>();

    }

    private void Update()
    {

        if (entityStats.currentHP <= 0)
        {
            boxCollider.offset = deadOffset;
            boxCollider.size = deadSize;
        }
        else
        {
            boxCollider.offset = aliveOffset;
            boxCollider.size = aliveSize;
        }


        // Sorts entity based on x/y position. Multiplies by 100 as x/y may be a decimal.
        // Divides by -1 such that higher y is lower, lower y is higher. Gives illusion of being behind/in front of other entities
        int sortingLayer = (int)Mathf.Floor(charGround.transform.position.y * 100 / -1);
        spriteRenderer.sortingOrder = sortingLayer;

        // Display range renderer if either our basic or special are active.
        circleBasicRangeRenderer.enabled = basicActive || specialActive;
        DrawBasicRangeCircle();

        // Moves entity towards target at a set speed. When within range, attack target.
        if (autoMove)
        {
            Vector2 newPosition = Vector2.MoveTowards(transform.position, target.transform.position, Time.deltaTime * moveSpeed);

            if (newPosition.x > transform.position.x)
            {
                transform.localScale = localScale;   // Normal scale for moving right
            }
            else if (newPosition.x < transform.position.x)
            {
                transform.localScale = new Vector2(localScale.x * -1, localScale.y); // Flipped scale for moving left
            }
            rb.MovePosition(newPosition);

            if (Vector2.Distance(transform.position, target.transform.position) < 2f)
            {
                autoMove = false;
                animator.SetBool("IsWalk", false);
                BattleClickInfo info = new BattleClickInfo();
                info.target = target;
                info.singleSkill = enemyAi.currAttack;
                ExcentraGame.battleManager.HandleEntityAction(info);
            }
        }
        // If not autoMove, allows for entity to move using WASD (if playerInput is enabled)
        else
        {
        }

    }

    public void FixedUpdate()
    {
        if (isSkillMoving)
        {
            Vector2 newPosition = Vector2.MoveTowards(transform.position, targetLocation, Time.fixedDeltaTime * skillMoveSpeed);
            rb.MovePosition(newPosition);

            if (newPosition.x > transform.position.x)
            {
                transform.localScale = localScale;   // Normal scale for moving right
            }
            else if (newPosition.x < transform.position.x)
            {
                transform.localScale = new Vector2(localScale.x * -1, localScale.y); // Flipped scale for moving left
            }

            if ((Vector2)transform.position == targetLocation)
                ResetMovementSkill();
        }
        else if (!isSkillMoving && !autoMove)
        {
            if (inputVector != Vector2.zero)
            {
                animator.SetBool("IsWalk", true);
            }
            else
            {
                animator.SetBool("IsWalk", false);
            }

            // Flip the sprite based on movement direction
            if (inputVector.x > 0)
            {
                FaceDirection(false);
            }
            else if (inputVector.x < 0)
            {
                FaceDirection(true);
            }

            // Calculates future movement. If entity will go beyond their "move" radius, prevent them from doing so.
            Vector2 newPosition = rb.position + inputVector * (moveSpeed) * Time.deltaTime;

            newPosition.x = Mathf.Clamp(newPosition.x, ExcentraGame.battleManager.arena.leftBound, ExcentraGame.battleManager.arena.rightBound);
            newPosition.y = Mathf.Clamp(newPosition.y, ExcentraGame.battleManager.arena.bottomBound, ExcentraGame.battleManager.arena.topBound);



            if (Vector2.Distance(newPosition, turnStartPos) < (entityStats.CalculateMovementRadius() / 2))
            {
                rb.MovePosition(newPosition);
            }
        }

    }

    public void ActivateMovementSkill(float movementSpeed, Vector2 destination, float offset)
    {
        Vector2 direction = (destination - (Vector2)transform.position).normalized;
        Vector2 offsetVector = direction * offset;
        Vector2 targetPoint = destination + offsetVector;

        targetLocation = targetPoint;

        skillMoveSpeed = movementSpeed;
        isSkillMoving = true;
    }

    public void ResetMovementSkill()
    {
        isSkillMoving = false;
    }

    /// <summary>
    /// Draws a radius around the <b>Entity</b> denoting an attack range.
    /// </summary>
    public void DrawBasicRangeCircle()
    {
        circleBasicRangeInstance.transform.position = transform.position;

        if (basicActive)
        {
            // TODO: Need to work on centralizing range. If drawing range, should be normal. If calculating distance, should be divided by 2.
            circleBasicRangeInstance.transform.localScale = new Vector2(entityStats.CalculateBasicRangeRadius(), entityStats.CalculateBasicRangeRadius());
        }
        else if (specialActive)
        {
            float range = ExcentraGame.battleManager.battleVariables.GetCurrentSkill().range;
            circleBasicRangeInstance.transform.localScale = new Vector2(range, range);
        }
            
    }

    /// <summary>
    /// Draws movement circle around character upon starting a character's turn.
    /// </summary>
    public void DrawMovementCircle()
    {
        float movement = entityStats.CalculateMovementRadius() / 2;
        int segments = 100;

        lineRenderer.positionCount = segments + 1;
        lineRenderer.loop = true;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        Vector3[] points = new Vector3[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 2 * Mathf.PI / segments;
            points[i] = new Vector3(Mathf.Cos(angle) * movement, Mathf.Sin(angle) * movement, 0f) + turnStartPos;
        }

        lineRenderer.SetPositions(points);
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
    }

    // Placed in Update(). Simple AI code to force move the entity to a target.
    public void MoveTowards(GameObject target)
    {
        animator.SetBool("IsWalk", true);
        this.target = target;
        autoMove = true;
    }

    /// <summary>
    /// Faces an <b>Entity</b> towards a direction.
    /// 
    /// <b>left</b> - states whether you want to face the <b>Entity</b> left or right.
    /// </summary>
    /// <param name="left"></param>
    public void FaceDirection(bool left)
    {
        if (left)
            transform.localScale = new Vector2(localScale.x * -1, localScale.y); // Flipped scale for moving left
        else
            transform.localScale = localScale;
    }

    /// <summary>
    /// Handles what to do when the respective <b>Entity</b>
    /// becomes targeted through such means (such as an <b>Area of Effect</b>
    /// or <b>Mouse Over</b> during a <b>Single</b> attack).
    /// <br/><br/>
    /// <b>active</b> - Provides information on whether or not the <b>Entity</b> is active or not.
    /// </summary>
    /// <param name="active">Determines whether or not...</param>
    public void HandleTarget(bool active)
    {
        if (active)
        {
            if (!entityStats.isPlayer)
                spriteRenderer.material.SetColor("_Color", new Color(1f, 0f, 0f));
            else
                spriteRenderer.material.SetColor("_Color", new Color(0f, 1f, 0f));
            spriteRenderer.material.SetFloat("_Thickness", 0.001f);
        }
            
        else
            spriteRenderer.material.SetFloat("_Thickness", 0f);
    }

    public bool CheckIfDistanceOutsideBase()
    {
        if (entityStats.moveDouble)
        {
            if (Vector2.Distance(transform.position, turnStartPos) > (((entityStats.CalculateMovementRadius() / 1.4f) / 2f)))
            {
                return true;
            }
        }
        return false;
    }

    public void ResetPosition()
    {
        transform.position = turnStartPos;
    }

    public void Cleanup()
    {
        Destroy(circleBasicRangeInstance);
        circleBasicRangeRenderer = null;
    }

    // -- EVENTS --

    public void OnHit() // Upon this Entity attacking another Entity
    {
        ExcentraGame.battleManager.OnHit();
    }

    public void OnActionEnd()   // Event upon when a specific action animation ends (attacks, special attacks, etc)
    {
        ExcentraGame.battleManager.EndTurn();
    }

    public void OnSpecialAttack(InputAction.CallbackContext context)    // When left click is pressed during the Special Attack state of battle
    {
        if (ExcentraGame.battleManager.battleVariables.GetState() == BattleState.PLAYER_SPECIAL)
        {
            if (context.started)
            {
                ExcentraGame.battleManager.OnSkillShot();
            }
        }
    }   

    public void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            
            ExcentraGame.battleManager.EscapePressed();
        }
    }   // Cancels the User Interface for Special attacks.

    public void OnRightClickPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ExcentraGame.battleManager.RightClickPressed();
        }
    }   // Cancels a special attack telegraph and goes back to the User Interface

    public void OnMouseEnter()
    {
        if (ExcentraGame.battleManager.battleVariables.GetState() == BattleState.AWAIT_ENEMY)
            return;

        if (!ExcentraGame.battleManager.battleVariables.IsEntityAttacking())
        {
            if (ExcentraGame.battleManager.TargetingEligible(ExcentraGame.battleManager.turnManager.GetCurrentTurn(), this.gameObject))
            {
                PlayerSkill currAbility = ExcentraGame.battleManager.battleVariables.GetCurrentSkill() as PlayerSkill;

                if (currAbility != null && currAbility.targetMode == TargetMode.SELF && currAbility.areaStyle == AreaStyle.SINGLE)
                    return;

                if (currAbility == null || (currAbility != null && currAbility.areaStyle == AreaStyle.SINGLE))
                {
                    HandleTarget(true);
                    return;
                }

                if (currAbility.targetMode == TargetMode.SELECT && currAbility.areaStyle != AreaStyle.SINGLE)
                {

                    ExcentraGame.battleManager.SpawnAoe(currAbility, this.gameObject, ExcentraGame.battleManager.turnManager.GetCurrentTurn());
                }
            }
        }
    }

    public void OnMouseDown()
    {

        PlayerSkill currAbility = ExcentraGame.battleManager.battleVariables.GetCurrentSkill() as PlayerSkill;
        GameObject currAttacker = ExcentraGame.battleManager.GetCurrentAttacker();
        EntityStats currStats = currAttacker.GetComponent<EntityStats>();

        // If the enemy is dead, check if we are able to revive them.
        if (entityStats.currentHP <= 0 && (currAbility == null || currAbility.damageType != DamageType.REVIVE))
            return;

        if (ExcentraGame.battleManager.battleVariables.GetState() == BattleState.AWAIT_ENEMY)
            return;


        if (ExcentraGame.battleManager.TargetingEligible(ExcentraGame.battleManager.turnManager.GetCurrentTurn(), this.gameObject))
        {
            if (currAbility == null || (currAbility != null && currAbility.areaStyle == AreaStyle.SINGLE))
            {
                if (ExcentraGame.battleManager.CheckWithinSkillRange(ExcentraGame.battleManager.turnManager.GetCurrentTurn(), this.gameObject, currAbility))
                {

                    HandleTarget(false);

                    BattleClickInfo info = new BattleClickInfo();
                    info.target = this.gameObject;
                    info.singleSkill = currAbility;
                    info.mousePosition = transform.position;
                    if (currAbility != null)
                    {
                        currStats.ModifyMP(Mathf.Max(currStats.currentAether - currAbility.baseAether, 0));
                    }

                    ExcentraGame.battleManager.battleVariables.targets = new() { { entityStats.entityName, this.gameObject } };
                    ExcentraGame.battleManager.HandleEntityAction(info);
                }

            }
        }
    }

    public void OnMouseExit()
    {
        if (ExcentraGame.battleManager.battleVariables.GetState() == BattleState.AWAIT_ENEMY)
            return;

        if (ExcentraGame.battleManager.TargetingEligible(ExcentraGame.battleManager.turnManager.GetCurrentTurn(), this.gameObject))
        {
            PlayerSkill currAbility = ExcentraGame.battleManager.battleVariables.GetCurrentSkill() as PlayerSkill;

            if (currAbility != null && currAbility.targetMode == TargetMode.SELF && currAbility.areaStyle == AreaStyle.SINGLE)
                return;

            if (currAbility == null || (currAbility != null && currAbility.areaStyle == AreaStyle.SINGLE))
            {
                HandleTarget(false);
                return;
            }

            if (currAbility.targetMode == TargetMode.SELECT && currAbility.areaStyle != AreaStyle.SINGLE)
            {
                if (!ExcentraGame.battleManager.battleVariables.IsEntityAttacking())
                    ExcentraGame.battleManager.DestroyAoe(ExcentraGame.battleManager.turnManager.GetCurrentTurn());
            }
        }
    }
}
