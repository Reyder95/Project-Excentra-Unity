using System.Collections.Generic;
using UnityEngine;

public class ActionInformation
{
    public GameObject target;
    public GameObject attacker;
    public Ability skill;
    public AoeData aoeData;

   public ActionInformation(GameObject target, GameObject attacker, Ability skill = null, AoeData aoeData = null)
    {
        this.target = target;
        this.attacker = attacker;
        this.skill = skill;
        this.aoeData = aoeData;
    }
}

public class GlobalDamageHelper
{
    public static float HandleActionCalculation(ActionInformation info)
    {
        if (info.skill == null)
            return BasicAttackDamageCalculation(info);
        else
        {
            return AreaSkillCalculation(info);
        }
    }

    private static float InitialDamageCalculation(float baseValue, float attackValue, float multiplier)
    {
        float damageValue = baseValue + (attackValue * multiplier);
        float damageOffset = damageValue * 0.20f;
        float randomDamage = Random.Range(damageValue - damageOffset, damageValue + damageOffset);
        return randomDamage;
    }

    private static float DamageCalculationPhysical(float damageValue, float armour, float attackCount)
    {
        return Mathf.Max(((int)(damageValue * 2) - (armour / 2)) / attackCount, 1);
    }

    private static float DamageCalculationMagical(float damageValue, float aegis, float attackCount)
    {
        return Mathf.Max(((int)(damageValue * 2) - (aegis / 2)) / attackCount, 1);
    }

    private static float BasicAttackDamageCalculation(ActionInformation info)
    {
        GameObject attackerObject = info.attacker;
        EntityStats attackerStats = attackerObject.GetComponent<EntityStats>();

        GameObject targetObject = info.target;
        EntityStats targetStats = targetObject.GetComponent<EntityStats>();

        float initialDamageCalculation = InitialDamageCalculation(attackerStats.CalculateAttack(), 0, 0);

        float damageCalculation = DamageCalculationPhysical(initialDamageCalculation, targetStats.CalculateArmour(), attackerStats.basicAttackCount);

        return damageCalculation;
    }

    private static float AreaSkillCalculation(ActionInformation info)
    {
        if (info.skill.damageType == DamageType.DAMAGE)
        {
            return IndividualSkillDamageCalculation(info.skill, info.target, info.attacker);
        }
        else if (info.skill.damageType == DamageType.HEAL)
        {
            return IndividualSkillHealCalculation(info.skill, info.target, info.attacker);
        }
        else if (info.skill.damageType == DamageType.STATUS)
        {
            return 1f;
        }
        else if (info.skill.damageType == DamageType.REVIVE)
        {
            return IndividualSkillHealCalculation(info.skill, info.target, info.attacker);
        }

        return 1f;

    }

    private static float IndividualSkillDamageCalculation(Ability info, GameObject target, GameObject attacker)
    {
        EntityStats targetStats = target.GetComponent<EntityStats>();
        EntityStats attackerStats = attacker.GetComponent<EntityStats>();
        
        if (info.scaler == Scaler.ATTACK)
        {
            float damageValue = InitialDamageCalculation(info.baseValue, attackerStats.CalculateAttack(), info.scaleMult);

            return DamageCalculationPhysical(damageValue, targetStats.CalculateArmour(), info.attackCount);
        }
        else if (info.scaler == Scaler.SPIRIT)
        {
            float damageValue = InitialDamageCalculation(info.baseValue, attackerStats.CalculateSpirit(), info.scaleMult);

            return DamageCalculationMagical(damageValue, targetStats.CalculateAegis(), info.attackCount);
        }

        return 1f;
    }

    private static float IndividualSkillHealCalculation(Ability info, GameObject target, GameObject healer)
    {
        EntityStats healerStats = healer.GetComponent<EntityStats>();
        float healValue = InitialDamageCalculation(info.baseValue, healerStats.CalculateSpirit(), info.scaleMult);
        return -healValue;
    }
}
