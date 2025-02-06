using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum MechanicStyle
{
    IMMEDIATE,
    TURN_ORIENTED
}

public enum AttackType
{
    AOE,
    SINGLE_TARGET,
    TETHER,
    ADD
}

public enum MoveType
{
    ORIGIN,
    ENDPOINT,
    TARGET,
    CENTER,
    CUSTOM
}

[CreateAssetMenu(fileName = "EnemyMechanic", menuName = "Scriptable Objects/EnemyMechanic")]
public class EnemyMechanic : ScriptableObject
{
    [Tooltip("The name of the mechanic to be used visually in fights")]
    public string mechanicName;

    [Tooltip("The key of the mechanic. For CustomMechanicLogicHelper, the addendums are: _before, and _end. To handle logic before the mechanic and after it ends.")]
    public string mechanicKey;

    [Tooltip("TURN_ORIENTED means it will place its attacks in the turn order, IMMEDIATE means it will immediately activate attacks on its turn.")]
    public MechanicStyle mechanicStyle;

    [Tooltip("For specific cases like spawning Adds where you want it to fully act out its turn instead of casting then ending.")]
    public bool dontSkipTurn = false;

    [Tooltip("For turn_oriented attacks, do we want the caster to do other attacks during this mechanic?")]
    public bool active = true;

    [Tooltip("If not active, stay untargetable for this amount of time.")]
    public bool untargetable = false;

    [Tooltip("Allows us to make a custom logic script to enable targetability for attacker on specific triggers")]
    public bool targetScript = false;

    [Tooltip("If active, sets caster delay to 1000 and prevents reduction in delay. Allows us to create a script to enable activity under specific situations")]
    public bool activeScript = false;

    [Tooltip("How many turns until casting can we re-cast this ability?")]
    public int turnCooldown = 0;
    [System.NonSerialized] public int currTurns;
    public List<MechanicAttack> mechanicAttacks = new List<MechanicAttack>();
}

[System.Serializable]
public class MechanicAttack
{
    [Header("General")]
    [Tooltip("The type of attack (specifies large mechanical differences).")]
    public AttackType attackType;

    [Tooltip("The key used for logical changes via dictionary. This key is used in CustomMechanicLogicHelper.")]
    public string attackKey;

    [Tooltip("The key in the dictionary for changing targeting mechanics. Invalidates 'Target Type'.")]
    public string targetKey;

    [Tooltip("Simple target type that does not require unique logic. If you need more complex logic, use targetKey and build your logic in the CustomMechanicLogicHelper.")] 
    public EntityTargetType targetType;

    [Tooltip("Specifies the # of turns from the cast that this attack will activate (only useful in TURN_ORIENTED mechanics).")]
    public int turnOffset;

    [Tooltip("States if a mechanic can be pulled off of an ally to target you instead.")]
    public bool canBeShirked = false;

    [Header("Positioning")]
    [Tooltip("States if the mechanic AoEs origin is the target. For circles this is the center, for directionals it's the start of the aoe.")]
    public bool originIsTarget;

    [Tooltip("States if the mechanic AoEs origin is the caster. For circles this is the center, for directionals it's the start of the aoe.")]
    public bool originIsSelf;

    [Tooltip("States if the endpoint is your target. This is only applicable for directionals, as it will point from an origin to the endpoint.")]
    public bool endpointIsTarget;

    [Tooltip("If you want to place an aoe just in the arena, you would use this and set a static position.")]
    public Vector2 customOrigin;

    [Header("Movement")]
    [Tooltip("If enabled, there will be a move action before the attack")]
    public bool containsMovement = false;

    [Tooltip("Type of movement, for example move to target or move to the center")]
    public MoveType moveType;

    [Header("AOE")]
    [Tooltip("The shape of the AoE. Like Circle, or Cone, or Line.")]
    public Shape aoeShape;

    [Tooltip("States if this aoe attack is proximity. The further you are away from it, the less damage it will deal.")]
    public bool isProximity;

    [Tooltip("States if this aoe attack is a stack. If it's a stack, the damage dealt to anyone would be divided by the number of entities hit.")]
    public bool isStack;

    [Tooltip("The size of the aoe. For circles this is the diameter. For directionals this is the width.")]
    public float size;

    [Tooltip("Only applies to directionals. How much further or closer do you want the directional to end relative to the target")]
    public float distanceOffset;

    [Tooltip("Static endpoint for directionals if we don't want a target")]
    public Vector2 endpoint;

    [Tooltip("Enables custom AoE coloring")]
    public bool customColor = false;

    [Tooltip("Specifies the color of the aoe")]
    public Color aoeColor;

    [Header("Adds")]
    [Tooltip("Allows us to specify the different adds we want to spawn")]
    public List<AddSpawner> addKeys = new List<AddSpawner>();

    [Header("Tether")]
    public bool tetherStationary;
    public EntityTargetType secondTether;
    public Vector2 tetherLocation;
    public float tetherRange;

    [Header("Damage")]
    [Tooltip("The damage type. Do we want to deal damage, to heal, or something else")]
    public DamageType damageType;

    [Tooltip("The scaler. What stat do we want to scale the effect off of?")]
    public Scaler scaler;

    [Tooltip("By how much do we want to scale based off of the requested stat?")]
    public float scaleMult;

    [Tooltip("The base value, as in baseValue + (calculations)")]
    public int baseValue;

    [Tooltip("How many times does this attack hit? It divides the damage by this much")]
    public float attackCount = 1;

    [Header("Statuses")]
    public List<string> statusesToAdd = new List<string>();
    public List<string> statusesToRemove = new List<string>();

}
