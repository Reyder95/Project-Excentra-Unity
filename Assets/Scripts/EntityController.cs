using JetBrains.Annotations;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Rendering;

public class EntityController : MonoBehaviour
{
    public float moveSpeed = 0f;
    private Vector2 inputVector;
    private Rigidbody2D rb;
    public Animator animator;
    private Vector2 localScale;
    public LineRenderer lineRenderer;
    public EntityStats entityStats;
    public PlayerInput playerInput;

    public Vector3 turnStartPos;

    private GameObject target;
    private bool autoMove = false;
    public bool basicActive = false;
    public bool specialActive = false;

    public GameObject circleRange;

    public GameObject circleBasicRangeInstance;
    public SpriteRenderer circleBasicRangeRenderer;

    SpriteRenderer spriteRenderer;
    public GameObject charGround;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        entityStats = GetComponent<EntityStats>();
        playerInput = GetComponent<PlayerInput>();

        spriteRenderer.material.SetFloat("_Thickness", 0f);
        localScale = gameObject.transform.localScale;
        circleBasicRangeInstance = Instantiate(circleRange, transform.position, Quaternion.identity);
        circleBasicRangeRenderer = circleBasicRangeInstance.GetComponent<SpriteRenderer>();

        lineRenderer = GetComponent<LineRenderer>();

    }

    private void Update()
    {
        int sortingLayer = (int)Mathf.Floor(charGround.transform.position.y * 100 / -1);
        spriteRenderer.sortingOrder = sortingLayer;

        circleBasicRangeRenderer.enabled = basicActive || specialActive;

        DrawBasicRangeCircle();

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
                ExcentraGame.battleManager.HandleEntityAction(info);
            }
        }

    }

    public void DrawBasicRangeCircle()
    {
        circleBasicRangeInstance.transform.position = transform.position;

        if (basicActive)
            circleBasicRangeInstance.transform.localScale = new Vector2(entityStats.CalculateBasicRangeRadius(), entityStats.CalculateBasicRangeRadius());
        else if (specialActive)
        {
            float range = ExcentraGame.battleManager.GetCurrentAbility().range;
            circleBasicRangeInstance.transform.localScale = new Vector2(range, range);
        }
            
    }

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

    public void MoveTowards(GameObject target)
    {
        animator.SetBool("IsWalk", true);
        this.target = target;
        autoMove = true;
    }

    private void FixedUpdate()
    {
        if (!autoMove)
        {        
            // Calculate the distance between the two points


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
                FaceRight();   // Normal scale for moving right
            }
            else if (inputVector.x < 0)
            {
                FaceLeft(); // Flipped scale for moving left
            }

            Vector2 newPosition = rb.position + inputVector * moveSpeed * Time.fixedDeltaTime;

            if (Vector2.Distance(newPosition, turnStartPos) < (entityStats.CalculateMovementRadius() / 2))
            {
                rb.MovePosition(newPosition);
            }
        }

    }

    public void FaceRight()
    {
        transform.localScale = localScale;
    }

    public void FaceLeft()
    {
        transform.localScale = new Vector2(localScale.x * -1, localScale.y); // Flipped scale for moving left
    }

    public void OnHit()
    {
        ExcentraGame.battleManager.OnHit();
    }

    public void OnActionEnd()
    {
        ExcentraGame.battleManager.EndTurn();
    }

    public void EnableOutline()
    {
        spriteRenderer.material.SetFloat("_Thickness", 0.001f);
    }

    public void DisableOutline()
    {
        spriteRenderer.material.SetFloat("_Thickness", 0f);
    }

    public void HandleAoEOver()
    {
        EnableOutline();
    }

    public void HandleAoELeave()
    {
        DisableOutline();
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

    public void OnSpecialAttack(InputAction.CallbackContext context)
    {
        if (ExcentraGame.battleManager.GetState() == BattleState.PLAYER_SPECIAL)
        {
            if (context.started)
            {
                ExcentraGame.battleManager.OnAbilityShot();
            }
        }
    }

    public void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ExcentraGame.battleManager.EscapePressed();
        }
    }

    public void OnRightClickPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ExcentraGame.battleManager.RightClickPressed();
        }
    }

    public void OnMouseEnter()
    {
        if (!ExcentraGame.battleManager.IsEntityAttacking())
        {
            if (ExcentraGame.battleManager.TargetingEligible(ExcentraGame.battleManager.GetCurrentAttacker(), this.gameObject))
            {
                Ability currAbility = ExcentraGame.battleManager.GetCurrentAbility();

                if (currAbility == null || (currAbility != null && currAbility.areaStyle == AreaStyle.SINGLE))
                {
                    EnableOutline();
                    return;
                }

                if (currAbility.targetMode == TargetMode.SELECT && currAbility.areaStyle != AreaStyle.SINGLE)
                {

                    ExcentraGame.battleManager.SpawnAoe(currAbility, this.gameObject, ExcentraGame.battleManager.GetCurrentAttacker());
                }
            }
        }
    }

    public void OnMouseDown()
    {

        Ability currAbility = ExcentraGame.battleManager.GetCurrentAbility();
        EntityStats stats = GetComponent<EntityStats>();

        // Needs a global range checker. ATM only works with basic range!
        // Some place where everything that needs a range can check it!
        if (ExcentraGame.battleManager.TargetingEligible(ExcentraGame.battleManager.GetCurrentAttacker(), this.gameObject) && ExcentraGame.battleManager.CheckWithinSkillRange(ExcentraGame.battleManager.GetCurrentAttacker(), this.gameObject) && ExcentraGame.battleManager.IsAlive(this.gameObject))
        {
            DisableOutline();
            BattleClickInfo info = new BattleClickInfo();
            info.target = this.gameObject;
            info.singleAbility = currAbility;
            ExcentraGame.battleManager.HandleEntityAction(info);
        }


        GameObject currAttacker = ExcentraGame.battleManager.GetCurrentAttacker();
        EntityStats currAttackerStats = currAttacker.GetComponent<EntityStats>();

        // Need to tidy this up (please)
        if (currAbility != null && currAbility.areaStyle == AreaStyle.SINGLE)
        {
            if (Vector2.Distance(currAttacker.transform.position, this.gameObject.transform.position) < currAbility.range / 2f)
            {
                bool canTarget = false;

                if (currAbility.entityType == EntityType.ALLY)
                {
                    canTarget = currAttackerStats.isPlayer == entityStats.isPlayer;

                    if (currAbility.damageType == DamageType.REVIVE)
                    {
                        if (stats.currentHP > 0)
                        {
                            canTarget = false;
                        }
                    }

                }
                else if (currAbility.entityType == EntityType.ENEMY)
                    canTarget = currAttackerStats.isPlayer != entityStats.isPlayer;

                if (canTarget)
                {
                    DisableOutline();
                    BattleClickInfo info = new BattleClickInfo();
                    info.target = this.gameObject;
                    info.isSingleSkill = true;
                    info.singleAbility = currAbility;
                    ExcentraGame.battleManager.HandleEntityAction(info);
                }

            }
        }

    }

    public void OnMouseExit()
    {
        if (ExcentraGame.battleManager.TargetingEligible(ExcentraGame.battleManager.GetCurrentAttacker(), this.gameObject))
        {
            Ability currAbility = ExcentraGame.battleManager.GetCurrentAbility();

            if (currAbility == null || (currAbility != null && currAbility.areaStyle == AreaStyle.SINGLE))
            {
                DisableOutline();
                return;
            }

            if (currAbility.targetMode == TargetMode.SELECT && currAbility.areaStyle != AreaStyle.SINGLE)
            {
                if (!ExcentraGame.battleManager.IsEntityAttacking())
                    ExcentraGame.battleManager.DestroyAoe(ExcentraGame.battleManager.GetCurrentAttacker());
            }
        }
    }
}
