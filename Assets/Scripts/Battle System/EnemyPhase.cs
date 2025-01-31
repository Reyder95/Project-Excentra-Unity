using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyPhase
{
    public List<EnemyMechanic> mechanics;
    public float hpPercentageThreshold;
}
