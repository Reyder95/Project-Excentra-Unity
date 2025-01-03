using System.Collections.Generic;
using UnityEngine;

public class StatusEffectHandler
{
    public Dictionary<string, StatusBattle> effects = new Dictionary<string, StatusBattle>();

    public void AddEffect(StatusEffect effect, GameObject owner)
    {
        if (!effects.ContainsKey(effect.name))
            effects.Add(effect.effectName, new StatusBattle(effect, owner, effect.baseTurns));
        else
            effects[effect.name].turnsRemaining = effect.baseTurns;
    }

    public void RemoveEffect(StatusEffect effect)
    {
        effects.Remove(effect.effectName);
    }

    public StatusBattle GetEffect(StatusEffect effect)
    {
        if (effects.ContainsKey(effect.effectName))
        {
            return effects[effect.effectName];
        }

        return null;
    }

    public StatusBattle GetEffectByKey(string key)
    {
        if (effects.ContainsKey(key))
        {
            return effects[key];
        }

        return null;
    }
}
