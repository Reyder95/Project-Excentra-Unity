// StatusBattle.cs

using UnityEngine;

// Gets added to the list of statuses an Entity has. This gives each status a turn count, which gets depleted each StartTurn().
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
