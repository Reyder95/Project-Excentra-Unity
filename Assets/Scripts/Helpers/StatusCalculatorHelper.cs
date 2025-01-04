using System.Collections.Generic;
using UnityEngine;

public enum StatusDirection
{
    UP,
    DOWN
}

public static class StatusCalculatorHelper
{
    // Use Stat Types instead! (soontm)
    private static Dictionary<string, System.Func<float, StatusBattle, float>> statusStatFuncDict = new Dictionary<string, System.Func<float, StatusBattle, float>>()
    {
        { "Aegis Down", (float stat, StatusBattle effect) => AegisCalc(stat, effect)},
        { "Aegis Up", (float stat, StatusBattle effect) => AegisCalc(stat, effect)},
        { "Physical Damage Up", (float stat, StatusBattle effect) => PhysicalDamageCalc(stat, effect)},
        { "Physical Damage Down", (float stat, StatusBattle effect) => PhysicalDamageCalc(stat, effect)},
        { "Aggression Up", (float stat, StatusBattle effect) => AggressionGenCalc(stat, effect)},
        { "Aggression Down", (float stat, StatusBattle effect) => AggressionGenCalc(stat, effect)},
        { "Aggression Turn Up", (float stat, StatusBattle effect) => AggressionTurnCalc(stat, effect)},
        { "Armour Up", (float stat, StatusBattle effect) => ArmourCalc(stat, effect)},
        { "Armour Down", (float stat, StatusBattle effect) => ArmourCalc(stat, effect)},
        { "Evasion Up", (float stat, StatusBattle effect) => EvasionCalc(stat, effect)},
        { "Evasion Down", (float stat, StatusBattle effect) => EvasionCalc(stat, effect)},
        { "Magic Damage Up", (float stat, StatusBattle effect) => MagicalDamageCalc(stat, effect)},
        { "Magic Damage Down", (float stat, StatusBattle effect) => MagicalDamageCalc(stat, effect)},

    };

    private static Dictionary<string, System.Func<GameObject, StatusBattle, float>> statusDamageFuncDict = new Dictionary<string, System.Func<GameObject, StatusBattle, float>>()
    {
        { "Poison", (GameObject target, StatusBattle effect) => PoisonCalc(target, effect)}
    };

    // TODO: Improve calculations and add status improvements to entityStats so that can be used to further affect statuses.
    public static float CalculateNewStat(float stat, StatusBattle effect)
    {
        if (statusStatFuncDict.ContainsKey(effect.effect.effectName))
            return statusStatFuncDict[effect.effect.effectName](stat, effect);

        return stat;
    }

    public static float CalculateDamage(GameObject target, StatusBattle effect)
    {
        if (statusDamageFuncDict.ContainsKey(effect.effect.effectName))
            return statusDamageFuncDict[effect.effect.effectName](target, effect);

        return 0f;
    }

    private static float AegisCalc(float stat, StatusBattle effect)
    {
        StatusEffect statusEffect = effect.effect;

        float calc = (stat * 0.40f) * statusEffect.effectMagnitude;

        if (statusEffect.effectType == EffectType.STAT_UP)
            return stat + calc;
        else if (statusEffect.effectType == EffectType.STAT_DOWN)
            return stat - calc;

        return stat;
    }

    private static float PhysicalDamageCalc(float stat, StatusBattle effect)
    {
        StatusEffect statusEffect = effect.effect;

        float calc = (stat * 0.25f) * statusEffect.effectMagnitude;

        if (statusEffect.effectType == EffectType.STAT_UP)
            return stat + calc;
        else if (statusEffect.effectType == EffectType.STAT_DOWN)
            return stat - calc;

        return stat;
    }

    private static float AggressionGenCalc(float stat, StatusBattle effect)
    {
        StatusEffect statusEffect = effect.effect;

        float calc = (stat * 0.25f) * statusEffect.effectMagnitude;

        if (statusEffect.effectType == EffectType.STAT_UP)
            return stat + calc;
        else if (statusEffect.effectType == EffectType.STAT_DOWN)
            return stat - calc;

        return stat;
    }

    private static float AggressionTurnCalc(float stat, StatusBattle effect)
    {
        return stat + 1;
    }

    private static float ArmourCalc(float stat, StatusBattle effect)
    {
        StatusEffect statusEffect = effect.effect;

        float calc = (stat * 0.40f) * statusEffect.effectMagnitude;

        if (statusEffect.effectType == EffectType.STAT_UP)
            return stat + calc;
        else if (statusEffect.effectType == EffectType.STAT_DOWN)
            return stat - calc;

        return stat;
    }

    private static float EvasionCalc(float stat, StatusBattle effect)
    {
        StatusEffect statusEffect = effect.effect;

        float calc = (stat * 0.40f) * statusEffect.effectMagnitude;

        if (statusEffect.effectType == EffectType.STAT_UP)
            return stat + calc;
        else if (statusEffect.effectType == EffectType.STAT_DOWN)
            return stat - calc;

        return stat;
    }

    private static float MagicalDamageCalc(float stat, StatusBattle effect)
    {
        StatusEffect statusEffect = effect.effect;

        float calc = (stat * 0.25f) * statusEffect.effectMagnitude;

        if (statusEffect.effectType == EffectType.STAT_UP)
            return stat + calc;
        else if (statusEffect.effectType == EffectType.STAT_DOWN)
            return stat - calc;

        return stat;
    }

    private static float PoisonCalc(GameObject target, StatusBattle effect)
    {
        EntityStats targetStats = target.GetComponent<EntityStats>();
        StatusEffect statusEffect = effect.effect;
        float basePoisonDamage = 55;
        float calc = (targetStats.maximumHP * 0.10f) * statusEffect.effectMagnitude;

        return Mathf.Max(basePoisonDamage + calc, 1);
    }
}
