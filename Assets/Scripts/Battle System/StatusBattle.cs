using System.Collections.Generic;
using UnityEngine;

public class StatusBattle
{
    public StatusEffect effect;
    public GameObject owner;
    public int turnsRemaining;

    public StatusBattle(StatusEffect effect, GameObject owner, int turnsRemaining)
    {
        this.effect = effect;
        this.owner = owner;
        this.turnsRemaining = turnsRemaining;
    }
}
