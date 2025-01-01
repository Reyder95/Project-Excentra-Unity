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
    public GameObject target;
    public GameObject attacker;
    public Ability currAbility;

    public GameObject currAoe;

    public bool isAttacking = false;
}
