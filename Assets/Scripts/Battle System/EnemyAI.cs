using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [System.NonSerialized] public List<GameObject> possibleTargets;

    [System.NonSerialized] public EnemyPhase currPhase;   // List of attacks that the enemy can use for this phase
    [System.NonSerialized] public List<EnemyPhase> enemyPhases;   // List of phases that the enemy can go through
    [System.NonSerialized] public InitialPhase initialPhase;
    [System.NonSerialized] public GameObject currTarget;
    [System.NonSerialized] public Vector2 moveLocation;
    [System.NonSerialized] public EnemyMechanic currAttack;
    [System.NonSerialized] public EnemyMechanic currImmediateAttack;
    [System.NonSerialized] public EnemyContents enemyContents;
    [System.NonSerialized] public EntityStats stats;
    [System.NonSerialized] public int phaseCount = 0;
    [System.NonSerialized] public bool isPhaseTrigger = false;

    private void Awake()
    {
    }

    public void InitializeAI(List<GameObject> possibleTargets)
    {
        enemyContents = GetComponent<EnemyContents>();
        stats = GetComponent<EntityStats>();
        this.possibleTargets = possibleTargets;
        BossEnemyPhases bossEnemyPhases = ExcentraDatabase.TryGetBossPhases(stats.enemyKey);
        initialPhase = bossEnemyPhases.initialPhase;
        enemyPhases = bossEnemyPhases.phases;
        currPhase = GetNextValidPhase(false);
    }

    public EnemyMechanic ChooseAttack()
    {
        if (currPhase == null)
        {
            currAttack = null;
            return null;
        }

        try
        {
            if (currAttack == null)
            {
                EnemyMechanic chosenMechanic = currPhase.ChooseMechanic();

                Debug.Log(chosenMechanic);
                if (chosenMechanic.mechanicStyle != MechanicStyle.IMMEDIATE)
                {
                    currAttack = chosenMechanic;
                    return null;
                }

                return chosenMechanic;
            }
            else
            {
                EnemyMechanic chosenMechanic = currPhase.ChooseMechanic(true);

                return chosenMechanic;
            }

        }
        catch (ArgumentOutOfRangeException ex)
        {
            InitializeAI(possibleTargets);
            return null;
        }

    }

    public void HandleMoveLocation(Vector2 moveLocation)
    {
        this.moveLocation = moveLocation;
    }

    public void TargetEntity(EntityTargetType targetType)
    {
        currTarget = null;
        if (currAttack == null)
            return;

        var possibleChars = possibleTargets.Where(go => go.GetComponent<EntityStats>() != null && go.GetComponent<EntityStats>().currentHP > 0).ToList();

        int randChar = UnityEngine.Random.Range(0, possibleChars.Count);

        AggressionSystem aggressionList = enemyContents.aggression;

        if (targetType == EntityTargetType.FIRST_AGGRESSION)
            currTarget = aggressionList.GetFirstAggression();
        else if (targetType == EntityTargetType.SECOND_AGGRESSION)
            currTarget = aggressionList.GetSecondAggression();
        else if (targetType == EntityTargetType.LAST_AGGRESSION)
            currTarget = aggressionList.GetLastAggression();
        else
        {
            GameObject target = FindRole(targetType, possibleChars);

            if (target != null)
                currTarget = target;

            if (currTarget != null)
                    return;
        }
        
        currTarget = possibleChars[randChar];
    }

    public GameObject ChooseEntity(EntityTargetType targetType)
    {
        currTarget = null;

        var possibleChars = possibleTargets.Where(go => go.GetComponent<EntityStats>() != null && go.GetComponent<EntityStats>().currentHP > 0).ToList();

        if (possibleChars.Count == 0)
            return null;

        int randChar = UnityEngine.Random.Range(0, possibleChars.Count);

        AggressionSystem aggressionList = enemyContents.aggression;

        GameObject target = null;

        if (targetType == EntityTargetType.FIRST_AGGRESSION)
            target = aggressionList.GetFirstAggression();
        else if (targetType == EntityTargetType.SECOND_AGGRESSION)
            target = aggressionList.GetSecondAggression();
        else if (targetType == EntityTargetType.LAST_AGGRESSION)
            target = aggressionList.GetLastAggression();
        else
        {
            target = FindRole(targetType, possibleChars);

            if (target != null)
                return target;
        }


        if (target != null)
            return target;
        
        return possibleChars[randChar];

    }

    public EnemyPhase GetNextValidPhase(bool force, bool changeHP = true)
    {
        EnemyPhase selectedPhase = null;

        if (force)
        {
            selectedPhase = enemyPhases[phaseCount];
            phaseCount++;
            isPhaseTrigger = false;

            return selectedPhase;
        }

        //while (enemyPhases.Count > 0)
        //{
        //    if (stats.CalculateHPPercentage() <= enemyPhases[0].hpPercentageThreshold)
        //    {
        //        selectedPhase = enemyPhases[0];
        //        enemyPhases.RemoveAt(0);
        //    }
        //    else
        //    {
        //        break;
        //    }
        //}

        if (phaseCount >= enemyPhases.Count)
            return null;

        if (stats.CalculateHPPercentage() <= enemyPhases[phaseCount].hpPercentageThreshold && !isPhaseTrigger)
        {
            Debug.Log(phaseCount);
            Debug.Log(enemyPhases.Count);
            selectedPhase = enemyPhases[phaseCount];
            phaseCount++;

            if (selectedPhase.isTriggerPhase)
                isPhaseTrigger = true;
        }


        if (changeHP && selectedPhase != null)
        {
            Debug.Log(stats.maximumHP * (selectedPhase.hpPercentageThreshold / 100));
            stats.ModifyHP(stats.maximumHP * (selectedPhase.hpPercentageThreshold / 100));
        }

        //for (int i = phaseCount; i < enemyPhases.Count; i++)
        //{
        //    if (stats.CalculateHPPercentage() <= enemyPhases[i].hpPercentageThreshold)
        //    {
        //        selectedPhase = enemyPhases[i];
        //    }
        //    else
        //    {
        //        phaseCount = i;
        //        break;
        //    }
        //}

        return selectedPhase;
    }

    public bool ChangePhase(bool force = false)
    {
        if (enemyPhases != null)
        {
            EnemyPhase nextPhase = GetNextValidPhase(force);
            if (nextPhase != null && nextPhase != currPhase)
            {
                currPhase = nextPhase;
                return true;
            }
        }

        return false;
    }

    public GameObject FindRole(EntityTargetType targetType, List<GameObject> possibleCharacters)
    {
        if (targetType == EntityTargetType.TANK)
        {
            foreach (var character in possibleCharacters)
            {
                if (character.GetComponent<EntityStats>().entityName == "Rioka")
                    return character;
            }
        }
        else if (targetType == EntityTargetType.HEALER)
        {
            foreach (var character in possibleCharacters)
            {
                if (character.GetComponent<EntityStats>().entityName == "Nono")
                    return character;
            }
        }
        else if (targetType == EntityTargetType.DAMAGE)
        {
            List<GameObject> possibleChars = new List<GameObject>();
            foreach (var character in possibleCharacters)
            {

                if (character.GetComponent<EntityStats>().entityName == "Nancy" || character.GetComponent<EntityStats>().entityName == "Penny")
                    possibleChars.Add(character);
            }

            return possibleChars[UnityEngine.Random.Range(0, possibleChars.Count)];
        }

        return null;
    }
}
