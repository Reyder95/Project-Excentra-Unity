using System;
using UnityEngine;

public enum MechanicMoveType
{
    RUN,
    DASH,
    TELEPORT
}

[Serializable]
public class MovementKeyframe
{
    public bool arenaMovement = false;
    [Tooltip("Useful if we want to move to a preset position in the arena")]
    public ArenaPositionType arenaMoveType;

    public bool specificMovement = false;
    [Tooltip("Useful if we want to move to a specific position in the arena")]
    public Vector2 position;

    [Tooltip("Makes medica move to the current target. Only useful for single target attacks really.")]
    public bool moveToCurrTarget = false;

    // This is internally for if we are custom writing mechanics and have a very specific target we'd like to move to.
    [NonSerialized]
    public GameObject target;
    
    public MechanicMoveType moveType;

    [Tooltip("The animation trigger we'd like to play when this keyframe is activated.")]
    public string animationTrigger;

    public Vector2 movementOffset;

    public float distanceOffset;

    public MovementKeyframe Clone()
    {
        return new MovementKeyframe
        {
            arenaMovement = this.arenaMovement,
            arenaMoveType = this.arenaMoveType,
            specificMovement = this.specificMovement,
            position = this.position,
            moveToCurrTarget = this.moveToCurrTarget,
            target = this.target,
            moveType = this.moveType,
            animationTrigger = this.animationTrigger,
            movementOffset = this.movementOffset,
            distanceOffset = this.distanceOffset
        };
    }
}
