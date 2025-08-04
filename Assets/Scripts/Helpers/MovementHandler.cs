using UnityEngine;
using System;

public class MovementHandler : MonoBehaviour
{
    private MovementKeyframe keyframe;
    private Vector2 calculatedDestination = Vector2.zero;

    private Vector2 localScale;

    // Boolean Checkers
    private bool teleportFadeout = false;
    private bool teleportFadein = false;
    private bool activeMovement = false;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private EntityController controller;
    private Animator animator;

    // Data for event
    public BattleManager battleManager;
    public EnemyMechanic mechanic;
    public GameObject attacker;

    public event Action<MovementHandler> OnMovementEnd;
    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<EntityController>();
        animator = GetComponent<Animator>();

        localScale = gameObject.transform.localScale;
    }

    private void Update()
    {
        if (keyframe == null || !activeMovement)
            return;

        CalculateDestination();

        if (keyframe.moveType == MechanicMoveType.RUN)
        {
            HandleRun();
        }
    }

    private void HandleTeleport()
    {

    }

    private void HandleRun()
    {
        animator.SetBool(keyframe.animationTrigger, true);
        Vector2 newPosition = Vector2.MoveTowards(transform.position, calculatedDestination, Time.deltaTime * controller.moveSpeed);
        Debug.Log(transform.position);
        Debug.Log(newPosition);
        if (newPosition.x > transform.position.x)
        {
            transform.localScale = localScale;   // Normal scale for moving right
        }
        else if (newPosition.x < transform.position.x)
        {
            transform.localScale = new Vector2(localScale.x * -1, localScale.y); // Flipped scale for moving left
        }
        rb.MovePosition(newPosition);
        if (Vector2.Distance(transform.position, calculatedDestination) < keyframe.distanceOffset)
        {
            animator.SetBool(keyframe.animationTrigger, false);
            activeMovement = false;
            OnMovementEnd?.Invoke(this);
            //BossMechanicHandler.InitializeMechanic(targetMechanic, ExcentraGame.battleManager, this.gameObject, true);
            //ExcentraGame.battleManager.HandleStartBossCasting(targetMechanic);
            //targetMechanic = null;

        }
    }

    private void CalculateDestination()
    {
        BattleArena arena = ExcentraGame.battleManager.arena;

        if (keyframe.arenaMovement)
        {
            switch (keyframe.arenaMoveType)
            {
                case ArenaPositionType.CENTER:
                    calculatedDestination = arena.GetCenter();
                    break;
                case ArenaPositionType.LEFT_HALF:
                    calculatedDestination = arena.GetLeftCenter();
                    break;
                case ArenaPositionType.RIGHT_HALF:
                    calculatedDestination = arena.GetRightCenter();
                    break;
                case ArenaPositionType.TOP_HALF:
                    calculatedDestination = arena.GetTopCenter();
                    break;
                case ArenaPositionType.BOTTOM_HALF:
                    calculatedDestination = arena.GetBottomCenter();
                    break;
                default:
                    calculatedDestination = Vector2.zero;
                    break;
            }
        }

        if (keyframe.specificMovement)
            calculatedDestination = keyframe.position;

        if (keyframe.target != null)
            calculatedDestination = keyframe.target.transform.position;

        Debug.Log("Target!!!");
        Debug.Log(keyframe.target);

        Debug.Log(calculatedDestination);

        calculatedDestination += keyframe.movementOffset;   
    }

    public void Play(MovementKeyframe keyframe)
    {
        this.keyframe = keyframe;
        activeMovement = true;

        
    }

}
