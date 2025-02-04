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
    public string enemyKey;
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

    // Aggression is like "aggro" in mmos. currently not implemented, but will be soon
    [Header("Aggression")]
    public float aggressionGen;
    public float aggressionDecay;
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
    [NonSerialized]
    public float nextStaticDelay = -1f;
    [NonSerialized]
    public GameObject addOwner = null;
    [NonSerialized]
    public bool targetable = true;
    [NonSerialized]
    public bool active = true;
    [NonSerialized]
    public string entityKey;
    [NonSerialized]
    public EnemyMechanic addMechanic;

    public event Action<EntityStats> OnStatusChanged;
    public event Action<EntityStats> OnHealthChanged;
    public event Action<EntityStats, BattleManager, EnemyMechanic> OnEntityKilled;
    public event Action<EntityStats> OnAetherChanged;

    public void ModifyStatus(StatusEffect effect = null, GameObject owner = null)
    {
        if (owner != null && effect != null)
            effectHandler.AddEffect(effect, owner);
        else if (effect != null)
            effectHandler.RemoveEffect(effect);
        else
            effectHandler.effects.Clear();

        OnStatusChanged?.Invoke(this);
    }

    public void ReduceStatusTurns(StatusEffect effect, int turns = 1)
    {
        effectHandler.ForceReduceTurnCount(effect, turns);

        OnStatusChanged?.Invoke(this);
    }

    public void ModifyHP(float amount)
    {
        currentHP = amount;

        if (currentHP > maximumHP)
            currentHP = maximumHP;
        else if (currentHP < 0)
            currentHP = 0;

        OnHealthChanged?.Invoke(this);

        if (currentHP == 0)
            OnEntityKilled?.Invoke(this, ExcentraGame.battleManager, addMechanic);   // Temp, battleManager should come from a direct send, not from a static variable
    }

    public void ModifyMP(float amount)
    {
        currentAether = amount;

        if (currentAether > maximumAether)
            currentAether = maximumAether;
        else if (currentAether < 0)
            currentAether = 0;

        OnAetherChanged?.Invoke(this);
    }

    // -- Calculations! Calculates various stats or important information based on stats, and status effects

    public void InitializeCurrentStats()
    {
        currentHP = maximumHP;
        currentAether = maximumAether;
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
            currMove *= 2.5f;
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

    public float CalculateAggressionGen()
    {
        return Calculation(StatType.AGGRESSION, aggressionGen);
    }

    public float CalculateAggressionDecay()
    {
        return Calculation(StatType.AGGRESSION_FALL, aggressionDecay);
    }
}
