// EntityStats.cs

using System.Collections.Generic;
using System;
using UnityEngine;

// Handles all the base stats of an entity. Also has calculations for calculating the base stat into the actual stat used for damage/healing/defense.
[System.Serializable]
public class EntityStats : MonoBehaviour
{
    [Header("Abilities")]
    public List<string> skillKeys = new List<string>();

    [Header("General Information")]
    public string entityName;
    public Texture2D portrait;
    public bool isPlayer;

    [Header("Character Resources")]
    public float maximumHP;
    [NonSerialized]
    public float currentHP;

    public float maximumAether;
    [NonSerialized]
    public float currentAether;

    [Header("Offensive Stats")]
    public float attack;
    public float spirit;

    [Header("Defensive Stats")]
    public float armour;
    public float evasion;
    public float aegis;

    [Header("Utility")]
    public float speed;
    public float move;
    public float basicRange;
    [NonSerialized]
    public float delay;

    // Aggression is like "aggro" in mmos. currently not implemented, but will be soon
    [Header("Aggression")]
    public float aggressionGen;
    public float aggressionDropoff;
    public float aggressionTurns;

    // Has to be accurate with the animation. For example, if an animation hits 3 times. This should be equal to 3.
    // This divides the total damage by the # of hits. Meaning you can set a "total" damage, rather than a "damage per hit".
    [Header("Misc.")]
    public int basicAttackCount = 1;

    // Additional useful information
    [NonSerialized]
    public int arenaAoeIndex = -1;
    [NonSerialized]
    public bool moveDouble = false;
    [NonSerialized]
    public StatusEffectHandler effectHandler = new StatusEffectHandler();

    // -- Calculations! Calculates various stats or important information based on stats, and status effects
    public void CalculateDelay(bool turn = false)
    {
        if (!turn)
        {
            delay = (int)Mathf.Floor((500 + UnityEngine.Random.Range(10, 26) / (speed * 10.5f)) * UnityEngine.Random.Range(10, 26)) / speed;
        }
        else
        {
            delay = (int)Mathf.Floor(delay * 0.80f);
        }
    }

    public void InitializeCurrentStats()
    {
        currentHP = maximumHP;
        currentAether = maximumAether;
        delay = 0;
    }

    public float CalculateHPPercentage()
    {
        return ((float)currentHP / (float)maximumHP) * 100;
    }

    public float CalculateMPPercentage()
    {
        return ((float)currentAether / (float)maximumAether) * 100;
    }

    public float CalculateMovementRadius()
    {
        float currMove = move;
        if (moveDouble)
            currMove *= 1.4f;
        return currMove / 2f;
    }

    public float CalculateBasicRangeRadius()
    {
        return basicRange / 5f;
    }

    // Standard calculation function. Takes in a stat
    public float Calculation(StatType stat, float statValue)
    {
        float finalStat = statValue;

        // Gets the statuses that would affect the "stat key" that we send in.
        // For example, "attack" would get back phys damage related statuses, since they effect the end stat of Attack
        List<string> statusNames = ExcentraDatabase.TryGetStatusNames(stat);

        foreach (var statusName in statusNames)
        {
            StatusBattle statusBattle = effectHandler.GetEffectByKey(statusName);

            if (statusBattle != null)
                finalStat = StatusCalculatorHelper.CalculateNewStat(finalStat, statusBattle);
        }

        return finalStat;
    }

    // Stat Calculations (use these for all damage calcs)
    public float CalculateAttack()
    {
        return Calculation(StatType.ATTACK, attack);
    }

    public float CalculateSpirit()
    {
        return Calculation(StatType.SPIRIT, spirit);
    }

    public float CalculateArmour()
    {
        return Calculation(StatType.ARMOUR, armour);
    }

    public float CalculateEvasion()
    {
        return Calculation(StatType.EVASION, evasion);
    }

    public float CalculateAegis()
    {
        return Calculation(StatType.EVASION, aegis);
    }

    public float CalculateSpeed()
    {
        return Calculation(StatType.SPEED, speed);
    }

    public float CalculateMove()
    {
        return Calculation(StatType.MOVE, move);
    }
}
