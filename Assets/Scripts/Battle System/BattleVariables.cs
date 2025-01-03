using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    AWAIT_ENEMY,
    PLAYER_CHOICE,
    PLAYER_SPECIAL,
    PLAYER_BASIC,
    PLAYER_ACTION,
    TURN_TRANSITION,

    // New states for death and revival
    UNIT_KO,          // Indicates at least one unit is KO/dead
    CHECK_REVIVE,     // State in which revival conditions are checked
    UNIT_REVIVED      // State after a successful revive
}

public class BattleVariables
{
    public BattleState battleState;
    public GameObject attacker;
    public Dictionary<string, GameObject> targets;
    public Ability currAbility;
    public GameObject currAoe;

    public bool isAttacking = false;

    // --- New fields to handle death and revival ---

    // Keep track of which units have been KO'ed or "dead"
    public HashSet<GameObject> fallenUnits = new HashSet<GameObject>();

    // Whether a revive action is currently allowed
    public bool canRevive = false;

    // Track the maximum number of revives available (or times you can revive each unit)
    public int maxRevives = 1;

    // Optional: track how many revives have been used so far
    public int revivesUsed = 0;
}
