using UnityEngine;

public enum EffectType
{
    DAMAGE,
    STAT_UP,
    STAT_DOWN,
    HEAL,
    AETHER,
    STUN,
    SNARE,
    ANTI_PHYS,
    ANTI_MAGIC,
    CONFUSE,
    SPECIAL
}

public enum StatType
{
    NONE,
    ATTACK,
    SPIRIT,
    ALL_OFFENSE,
    AEGIS,
    ARMOUR,
    ALL_DEFENSE,
    AETHER,
    HEALTH,
    MOVE,
    SPEED,
    RANGE,
    EVASION,
    AGGRESSION,
    AGGRESSION_TURN,
    AGGRESSION_FALL
}

[CreateAssetMenu(fileName = "StatusEffect", menuName = "Scriptable Objects/StatusEffect")]
public class StatusEffect : ScriptableObject
{
    public string effectName;
    public Texture2D icon;

    [TextArea(3,10)]
    public string description;
    public EffectType effectType;
    public float effectMagnitude;
    public StatType statType;
    public int baseTurns;
}
