// BattleClickInfo.cs

using UnityEngine;

// For when a player "clicks" on another entity for an attack, it provides information on the target entity, the skill if necessary, and the mouse position at the time if needed.
// Questionable if it's that important, or if I should centralize AoE and Single click abilities (probably should)
public class BattleClickInfo
{
    public GameObject target;
    public Vector2 mousePosition;
    public bool isSingleSkill = false;
    public BaseSkill singleSkill;
}
