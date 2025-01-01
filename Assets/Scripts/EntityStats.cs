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
    public int maximumHP;
    [NonSerialized]
    public int currentHP;

    public int maximumAether;
    [NonSerialized]
    public int currentAether;

    [Header("Offensive Stats")]
    public int attack;
    public int spirit;

    [Header("Defensive Stats")]
    public int armour;
    public int evasion;
    public int aegis;

    [Header("Utility")]
    public int speed;
    public int move;
    public int basicRange;
    [NonSerialized]
    public int delay;

    [Header("Utility")]
    public int basicAttackCount = 1;

    // Additional useful information
    [NonSerialized]
    public int arenaAoeIndex = -1;
    public bool moveDouble = false;

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
