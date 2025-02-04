using System.Collections.Generic;
using UnityEngine;

public class BaseSkill : ScriptableObject
{

    [Header("Base Skill Info")]
    public string skillName;

    public AreaStyle areaStyle;

    public float scaleMult;


    public EntityType entityType;
    public Scaler scaler;

    public int radius;
    public float range;

    public int baseValue;

    public int delayAdditive;
    public DamageType damageType;

    public bool containsMovement = false;
    public bool selfMove = false;   // If self move is true, move the caster towards the location. Otherwise, move the targets towards the caster.
    public float offsetDistance = 0f;
    public bool selfTarget = false;
    public float moveSpeed = 3f;

    public int attackCount = 1;
    public List<StatusEffectChance> statusEffects;

    public bool removeAll = true;
    public List<StatusEffect> removeStatuses;
}
