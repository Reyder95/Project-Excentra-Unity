using System.Collections.Generic;
using UnityEngine;

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

    [Header("Misc")]
    public int attackCount = 1;
    public List<StatusEffectChance> statusEffects;
}
