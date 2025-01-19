using System.Collections.Generic;
using UnityEngine;

public enum AreaStyle
{
    SINGLE,
    AREA
}

public enum TargetMode
{
    SELECT,
    FREE,
    SELF
}

public enum Shape
{
    CIRCLE,
    CONE,
    LINE
}

public enum EntityType
{
    ALLY,
    ENEMY,
    SELF
}

public enum Scaler
{
    ATTACK,
    SPIRIT
}

public enum DamageType
{
    HEAL,
    STATUS,
    DAMAGE,
    REVIVE
}

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
public class Skill : ScriptableObject
{
    [Header("Metadata")]
    public string skillName;
    public Texture2D icon;

    [Header("Area Type (single target or AoE)")]
    public AreaStyle areaStyle;
    public TargetMode targetMode;
    public Shape shape;
    public EntityType entityType;
    public Scaler scaler;

    [Header("Deeper Stats")]
    public int radius;
    public float range;

    [Header("Stats")]
    public int baseValue;
    public int baseAether;
    public float scaleMult;
    public int delayAdditive;
    public DamageType damageType;

    [Header("Movement")]
    public bool containsMovement = false;
    public bool selfMove = false;   // If self move is true, move the caster towards the location. Otherwise, move the targets towards the caster.
    public float offsetDistance = 0f;
    public bool selfTarget = false;
    public float moveSpeed = 3f;

    [Header("Misc")]
    public int attackCount = 1;
    public List<StatusEffectChance> statusEffects;
}
