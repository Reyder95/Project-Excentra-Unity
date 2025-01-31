using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossEnemyPhases
{
    public string key;
    public List<EnemyPhase> phases;
    public InitialPhase initialPhase;
}
