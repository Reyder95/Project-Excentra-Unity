// Ability.cs

using UnityEngine;
using System;
using System.Collections.Generic;

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

// Handles the data for skills. Entities can cast special skills.
//[Serializable]
//public class Ability
//{
//    public string abilityName;
//    public Texture2D icon;
//    public AreaStyle areaStyle;
//    public TargetMode targetMode;
//    public Shape shape;
//    public EntityType entityType;
//    public int radius;
//    public Scaler scaler;
//    public int baseValue;
//    public int baseAether;
//    public float scaleMult;
//    public float range;
//    public int delayAdditive;
//    public int attackCount = 1;
//    public DamageType damageType;
//    public List<StatusEffectChance> statusEffect;
//}

//[Serializable]
//public class AbilityKey
//{
//    public string key;
//    public Ability ability;
//}
