using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyPhase
{
    public List<EnemySkill> skills;
    public float hpPercentageThreshold;
}
