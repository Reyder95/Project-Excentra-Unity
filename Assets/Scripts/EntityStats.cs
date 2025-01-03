using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class EntityStats : MonoBehaviour
{
    [Header("Abilities")]
    public List<string> abilityKeys = new List<string>();

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

    [Header("Aggression")]
    public float aggressionGen;
    public float aggressionDropoff;
    public float aggressionTurns;

    [Header("Misc.")]
    public int basicAttackCount = 1;

    // Additional useful information
    [NonSerialized]
    public int arenaAoeIndex = -1;
    [NonSerialized]
    public bool moveDouble = false;
    [NonSerialized]
    public StatusEffectHandler effectHandler = new StatusEffectHandler();

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
}
