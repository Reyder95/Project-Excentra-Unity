using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemyPhase
{
    public List<EnemyMechanic> mechanics;
    public float hpPercentageThreshold;

    public List<EnemyMechanic> GetMechanicsOfType(MechanicStyle style)
    {
        return mechanics.Where(mechanic => mechanic.mechanicStyle == style).ToList();
    }
}
