using UnityEngine;

public class MedicaAddTrigger : AbilityTrigger
{
    public override void ActivateTrigger(BattleManager battleManager, EnemyMechanic mechanic, object data = null)
    {
        transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeSelf);
    }
}
