using UnityEngine;

public class MedicaAddTrigger : AbilityTrigger
{
    public override void ActivateTrigger(BattleManager battleManager, EnemyMechanic mechanic)
    {
        transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeSelf);
    }
}
