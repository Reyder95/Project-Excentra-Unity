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
                FaceDirection(false);   // Normal scale for moving right
            }
            else if (inputVector.x < 0)
            {
                FaceDirection(true); // Flipped scale for moving left
            }

            Vector2 newPosition = rb.position + inputVector * moveSpeed * Time.fixedDeltaTime;

            if (Vector2.Distance(newPosition, turnStartPos) < (entityStats.CalculateMovementRadius() / 2))
            {
                rb.MovePosition(newPosition);
            }
        }

    }

    public void FaceDirection(bool left)
    {
        if (left)
            transform.localScale = new Vector2(localScale.x * -1, localScale.y); // Flipped scale for moving left
        else
            transform.localScale = localScale;
    }

    public void OnHit()
    {
        ExcentraGame.battleManager.OnHit();
    }

    public void OnActionEnd()
    {
        ExcentraGame.battleManager.EndTurn();
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
            spriteRenderer.material.SetFloat("_Thickness", 0.001f);
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
                    HandleTarget(true);
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

        if (currAbility == null || (currAbility != null && currAbility.areaStyle == AreaStyle.SINGLE))
        {
            if (ExcentraGame.battleManager.CheckWithinSkillRange(ExcentraGame.battleManager.GetCurrentAttacker(), this.gameObject, currAbility))
            {
                HandleTarget(false);

                BattleClickInfo info = new BattleClickInfo();
                info.target = this.gameObject;
                info.singleAbility = currAbility;
                ExcentraGame.battleManager.HandleEntityAction(info);
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
                HandleTarget(false);
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
