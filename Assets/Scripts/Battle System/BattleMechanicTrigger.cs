using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TriggerKey
{
    public string key;
    public GameObject trigger;
}

public class BattleMechanicTrigger : MonoBehaviour
{
    public List<TriggerKey> triggers;
    public Dictionary<string, GameObject> triggerDict = new Dictionary<string, GameObject>();

    public void LoadTriggers()
    {
        foreach (TriggerKey trigger in triggers)
        {
            triggerDict.Add(trigger.key, trigger.trigger);
        }
    }

    public void ActivateTrigger(BattleManager battleManager, EnemyMechanic mechanic, string key)
    {
        if (triggerDict.ContainsKey(key))
            triggerDict[key].GetComponent<AbilityTrigger>().ActivateTrigger(battleManager, mechanic);
    }
}
