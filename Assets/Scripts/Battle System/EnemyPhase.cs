using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PhaseType
{
    RANDOM,
    ORDER
}

[System.Serializable]
public class EnemyPhase
{
    public List<EnemyMechanic> activeMechanics;
    public List<EnemyMechanic> inactiveMechanics;
    public PhaseType phaseType;
    public float hpPercentageThreshold;

    public List<EnemyMechanic> GetMechanicsOfType(MechanicStyle style)
    {
        return activeMechanics.Where(mechanic => mechanic.mechanicStyle == style).ToList();
    }

    public EnemyMechanic ChooseMechanic(bool immediate = false)
    {
        List<EnemyMechanic> possibleMechanics;

        if (immediate)
            possibleMechanics = GetMechanicsOfType(MechanicStyle.IMMEDIATE);
        else
            possibleMechanics = activeMechanics;

        int randomIndex = UnityEngine.Random.Range(0, possibleMechanics.Count);
        EnemyMechanic chosenMechanic = possibleMechanics[randomIndex];
        chosenMechanic.currTurns = chosenMechanic.turnCooldown;

        ReduceCooldowns();

        if (chosenMechanic.currTurns > 0)
        {
            activeMechanics.RemoveAt(randomIndex);
            inactiveMechanics.Add(chosenMechanic);
        }



        return chosenMechanic;

    }

    public void ReduceCooldowns()
    {
        EnemyMechanic thisMechanic = null;
        int counter = 0;
        foreach (EnemyMechanic mechanic in inactiveMechanics)
        {
            if (mechanic.currTurns > 0)
                mechanic.currTurns--;
            else
            {
                thisMechanic = mechanic;
                break;

            }

            counter++;
        }

        if (thisMechanic != null)
        {
            inactiveMechanics.RemoveAt(counter);
            activeMechanics.Add(thisMechanic);
        }

    }
}
