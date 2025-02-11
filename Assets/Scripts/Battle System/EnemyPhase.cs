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
    public List<EnemyMechanic> activeMechanics = new List<EnemyMechanic>();
    private List<EnemyMechanic> inactiveMechanics = new List<EnemyMechanic>();
    public List<EnemyMechanic> consistentMechanics = new List<EnemyMechanic>();
    public PhaseType phaseType;
    public float hpPercentageThreshold;

    public List<EnemyMechanic> GetMechanicsOfType(MechanicStyle style)
    {
        return activeMechanics.Where(mechanic => mechanic.mechanicStyle == style).ToList();
    }

    public EnemyMechanic ChooseMechanic(bool consistent = false)
    {
        if (phaseType == PhaseType.RANDOM)
        {
            List<EnemyMechanic> possibleMechanics;

            if (consistent)
                possibleMechanics = consistentMechanics;
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
        else if (phaseType == PhaseType.ORDER)
        {
            try
            {
                EnemyMechanic chosenMechanic;

                if (consistent)
                {
                    int randomIndex = UnityEngine.Random.Range(0, consistentMechanics.Count);
                    chosenMechanic = consistentMechanics[randomIndex];
                }
                else
                {
                    if (activeMechanics.Count == 0)
                        ReinsertMechanics();

                    Debug.Log("henlo" + activeMechanics[0]);

                    chosenMechanic = activeMechanics[0];

                    inactiveMechanics.Add(chosenMechanic);
                    activeMechanics.RemoveAt(0);
                }
                Debug.Log("henlo2" + chosenMechanic);
                return chosenMechanic;
            }
            catch
            {
                Debug.Log("Ensure that you have enough active mechanics, or CONSISTENT mechanics!!");
            }
        }


        return null;
    }

    public void ReinsertMechanics()
    {
        foreach (var inactiveMech in inactiveMechanics)
        {
            activeMechanics.Add(inactiveMech);
        }

        inactiveMechanics.Clear();
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
