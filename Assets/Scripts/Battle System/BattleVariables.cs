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

public class BattleVariables
{
    public BattleState battleState;
    public GameObject attacker;
    public Dictionary<string, GameObject> targets;
    public Ability currAbility;

    public GameObject currAoe;

    public bool isAttacking = false;

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
