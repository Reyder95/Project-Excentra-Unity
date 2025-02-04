// GlobalDamageHelper.cs

using UnityEngine;

public class ActionInformation
{
    public GameObject target;
    public GameObject attacker;
    public BaseSkill skill;
    public AoeData aoeData;

   public ActionInformation(GameObject target, GameObject attacker, BaseSkill skill = null, AoeData aoeData = null)
    {
        this.target = target;
        this.attacker = attacker;
        this.skill = skill;
        this.aoeData = aoeData;
    }
}

// One of the most important classes in the game. Helps standardize and centralize damage calculations for both magic and physical damage.
// Basically a "numbers only" class. The logic doesn't happen here, just a number with which to do logic with happens.
// Such as a revive wouldn't be handled here, or a status effect application, but the numbers on the initial application would be handled here, and the rest would be handled elsewhere
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

    // For skills, there are many forms of damage types (listed below). Based on the damage type, things must happen.
    private static float AreaSkillCalculation(ActionInformation info)
    {
        // If damage, do the damage
        if (info.skill.damageType == DamageType.DAMAGE)
        {
            return IndividualSkillDamageCalculation(info.skill, info.target, info.attacker);
        }
        // If heal, heal the entity
        else if (info.skill.damageType == DamageType.HEAL)
        {
            return IndividualSkillHealCalculation(info.skill, info.target, info.attacker);
        }
        // If status effect, do no damage at all
        else if (info.skill.damageType == DamageType.STATUS)
        {
            return 0f;
        }
        // If revive, do the heal calculation. A revive will occur on the battle manager side
        else if (info.skill.damageType == DamageType.REVIVE)
        {
            return IndividualSkillHealCalculation(info.skill, info.target, info.attacker);
        }

        return 1f;

    }

    private static float IndividualSkillDamageCalculation(BaseSkill skill, GameObject target, GameObject attacker)
    {
        EntityStats targetStats = target.GetComponent<EntityStats>();
        EntityStats attackerStats = attacker.GetComponent<EntityStats>();
        
        if (skill.scaler == Scaler.ATTACK)
        {
            float damageValue = InitialDamageCalculation(skill.baseValue, attackerStats.CalculateAttack(), skill.scaleMult);

            return DamageCalculationPhysical(damageValue, targetStats.CalculateArmour(), skill.attackCount);
        }
        else if (skill.scaler == Scaler.SPIRIT)
        {
            float damageValue = InitialDamageCalculation(skill.baseValue, attackerStats.CalculateSpirit(), skill.scaleMult);

            return DamageCalculationMagical(damageValue, targetStats.CalculateAegis(), skill.attackCount);
        }

        return 1f;
    }

    private static float IndividualSkillHealCalculation(BaseSkill skill, GameObject target, GameObject healer)
    {
        EntityStats healerStats = healer.GetComponent<EntityStats>();
        float healValue = InitialDamageCalculation(skill.baseValue, healerStats.CalculateSpirit(), skill.scaleMult);
        return -healValue;
    }
}
