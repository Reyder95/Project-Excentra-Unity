// BattleVariables.cs

using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    AWAIT_ENEMY,
    PLAYER_CHOICE,
    PLAYER_SPECIAL,
    PLAYER_BASIC,
    PLAYER_ACTION,
    TURN_TRANSITION
}

// Handles the internal variables for the current battle. Such things like current ability, etc.
// TODO: Re-add attacker back at some point, and only make everything go through battleVariables.GetCurrentAttacker(). This way we can lock down turnManager and everything goes through battleVariables for any needs
public class BattleVariables
{
    public BattleState battleState; // The current state of battle (as shown above). Determines player state, or if we're awaiting the enemy, or transitioning between turns.
    public Dictionary<string, GameObject> targets;  // The list of targets that will be attacked by the ability on the current turn. 
    public Ability currAbility; // The current ability used for calculations (whether healing, damage, or what kind of healing)

    public GameObject currAoe;  // The current AoE telegraph for additional calculations

    public bool isAttacking = false;    // Determines if an attacking animation is playing. Prevents some weird behaviours, and gets disabled once the turn ends.

    // -- Important battle gets and sets. Makes it a bit easier for other classes to obtain info for.
    public BattleState GetState()
    {
        return battleState;
    }

    public Ability GetCurrentAbility()
    {
        if (GetState() == BattleState.PLAYER_SPECIAL)
        {
            return currAbility;
        }

        return null;
    }

    public void SetCurrentAoe(GameObject aoe)
    {
        currAoe = aoe;
    }

    public GameObject GetCurrentAoe()
    {
        return currAoe;
    }

    public bool IsEntityAttacking()
    {
        return isAttacking;
    }

    public void DeleteCurrentAoe()
    {
        currAoe = null;
    }
}
