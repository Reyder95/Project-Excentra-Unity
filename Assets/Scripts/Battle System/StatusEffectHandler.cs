// StatusEffectHandler.cs

using System.Collections.Generic;
using UnityEngine;

// Handler that goes on each EntityStats component. Essentially it is its own handler that you can get any effect that an entity has on them
public class StatusEffectHandler
{
    public Dictionary<string, StatusBattle> effects = new Dictionary<string, StatusBattle>();

    public void AddEffect(StatusEffect effect, GameObject owner)
    {
        if (!effects.ContainsKey(effect.key))
            effects.Add(effect.key, new StatusBattle(effect, owner, effect.baseTurns));
        else
            effects[effect.key].turnsRemaining = effect.baseTurns;
    }

    public void ForceReduceTurnCount(StatusEffect effect, int turns = 1)
    {
        if (effects.ContainsKey(effect.key))
            effects[effect.key].turnsRemaining -= turns;
    }

    public void RemoveEffect(StatusEffect effect)
    {
        effects.Remove(effect.key);
    }

    public StatusBattle GetEffect(StatusEffect effect)
    {
        if (effects.ContainsKey(effect.key))
        {
            return effects[effect.key];
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
