using System.Collections.Generic;
using UnityEngine;

public enum StatusDirection
{
    UP,
    DOWN
}

public static class StatusCalculatorHelper
{
    private static Dictionary<string, System.Func<float, StatusBattle, float>> statusStatFuncDict = new Dictionary<string, System.Func<float, StatusBattle, float>>()
    {
        { "Aegis Down", (float stat, StatusBattle effect) => AegisCalc(stat, effect, StatusDirection.DOWN)},
        { "Aegis Up", (float stat, StatusBattle effect) => AegisCalc(stat, effect, StatusDirection.UP)},
        { "Physical Damage Up", (float stat, StatusBattle effect) => PhysicalDamageCalc(stat, effect, StatusDirection.UP)},
        { "Physical Damage Down", (float stat, StatusBattle effect) => PhysicalDamageCalc(stat, effect, StatusDirection.DOWN)}
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

    private static float AegisCalc(float stat, StatusBattle effect, StatusDirection dir)
    {
        StatusEffect statusEffect = effect.effect;

        float calc = (stat * 0.40f) * statusEffect.effectMagnitude;

        if (dir == StatusDirection.UP)
            return stat + calc;
        else
            return stat - calc;
    }

    private static float PhysicalDamageCalc(float stat, StatusBattle effect, StatusDirection dir)
    {
        StatusEffect statusEffect = effect.effect;

        float calc = (stat * 0.25f) * statusEffect.effectMagnitude;

        if (dir == StatusDirection.UP)
            return stat + calc;
        else
            return stat - calc;
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
