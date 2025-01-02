using UnityEngine;
using System;

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
    ENEMY
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

[Serializable]
public class Ability
{
    public string abilityName;
    public Texture2D icon;
    public TargetMode targetMode;
    public Shape shape;
    public EntityType entityType;
    public int radius;
    public Scaler scaler;
    public int baseValue;
    public int baseAether;
    public float scaleMult;
    public float range;
    public int delayAdditive;
    public int attackCount = 1;
    public DamageType damageType;
}

[Serializable]
public class AbilityKey
{
    public string key;
    public Ability ability;
}
