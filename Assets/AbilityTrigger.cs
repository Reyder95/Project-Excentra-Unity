using UnityEngine;

public abstract class AbilityTrigger : MonoBehaviour
{
    public abstract void ActivateTrigger(BattleManager battleManager, EnemyMechanic mechanic, object data = null);
}
