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

    private void Awake()
    {
        Debug.Log("TEST");
    }

    public void InitializeAI(List<GameObject> possibleTargets)
    {
        enemyContents = GetComponent<EnemyContents>();
        stats = GetComponent<EntityStats>();
        this.possibleTargets = possibleTargets;
        BossEnemyPhases bossEnemyPhases = ExcentraDatabase.TryGetBossPhases(stats.enemyKey);
        initialPhase = bossEnemyPhases.initialPhase;
        enemyPhases = bossEnemyPhases.phases;
        currPhase = enemyPhases[0];

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
                List<EnemyMechanic> possibleMechanics = currPhase.mechanics;
                Debug.Log("Possible Mechanics (top): " + possibleMechanics.Count);
                int randomIndex = UnityEngine.Random.Range(0, possibleMechanics.Count);

                if (possibleMechanics[randomIndex].mechanicStyle != MechanicStyle.IMMEDIATE)
                {
                    currAttack = possibleMechanics[randomIndex];
                    return null;
                }

                return possibleMechanics[randomIndex];
            }
            else
            {
                List<EnemyMechanic> possibleMechanics = currPhase.GetMechanicsOfType(MechanicStyle.IMMEDIATE);
                Debug.Log("Possible Mechanics (bottom): " + possibleMechanics.Count);
                int randomIndex = UnityEngine.Random.Range(0, possibleMechanics.Count);

                return possibleMechanics[randomIndex];
            }

        }
        catch (ArgumentOutOfRangeException)
        {
            InitializeAI(possibleTargets);
            Debug.Log("TEST!");
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

        Debug.Log("COUNT!" + possibleChars.Count);

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
        
        
        
        return possibleChars[randChar];

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
