using System.Collections.Generic;
using UnityEngine;

public enum EntityTargetType
{
    FIRST_AGGRESSION,
    SECOND_AGGRESSION,
    THIRD_AGGRESSION,
    LAST_AGGRESSION,
    TANK,
    HEALER,
    DAMAGE,
    NONE,
    ALL,
    RANDOM
}

[CreateAssetMenu(fileName = "Enemy Skill", menuName = "Scriptable Objects/Enemy Skill")]
public class EnemySkill : BaseSkill
{
    public string key;
    public EntityTargetType targetType = EntityTargetType.FIRST_AGGRESSION;
    public List<EnemyAoeData> aoeData;
    public int turnWindup;
}
