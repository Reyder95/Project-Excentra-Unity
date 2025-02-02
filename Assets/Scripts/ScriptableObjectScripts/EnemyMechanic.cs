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
    TETHER
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
    public string mechanicName;
    public string mechanicKey;
    public MechanicStyle mechanicStyle;
    public bool active = true;
    public bool untargetable = false;
    public List<MechanicAttack> mechanicAttacks = new List<MechanicAttack>();
}

[System.Serializable]
public class MechanicAttack
{
    [Header("General")]
    public AttackType attackType;
    public string attackKey;
    public string targetKey;
    public EntityTargetType targetType;
    public int turnOffset;

    [Header("Positioning")]
    public bool originIsTarget;
    public bool originIsSelf;
    public bool endpointIsTarget;
    public Vector2 customOrigin;
    public float requiredRange = 0f;

    [Header("Movement")]
    public bool containsMovement = false;
    public MoveType moveType;

    [Header("AOE")]
    public Shape aoeShape;
    public bool isProximity;
    public bool isStack;
    public bool staticDistance = false;
    public float size;
    public float distance;
    public float distanceOffset;
    public Vector2 endpoint;
    public Color aoeColor;

    [Header("Tether")]
    public bool tetherStationary;
    public EntityTargetType secondTether;
    public Vector2 tetherLocation;
    public float tetherRange;

    [Header("Damage")]
    public DamageType damageType;
    public Scaler scaler;
    public float scaleMult;
    public int baseValue;
    public float attackCount = 1;

    [Header("Statuses")]
    public List<string> statusesToAdd = new List<string>();
    public List<string> statusesToRemove = new List<string>();

}
