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

[CreateAssetMenu(fileName = "Player Skill", menuName = "Scriptable Objects/Player Skill")]
public class PlayerSkill : BaseSkill
{
    [Header("Metadata")]
    public Texture2D icon;

    public TargetMode targetMode;

    [Header("Stats")]
    public int baseAether;

    public Shape shape;

}
