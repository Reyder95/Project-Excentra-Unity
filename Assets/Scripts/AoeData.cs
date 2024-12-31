using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class AoeData
{
    private Dictionary<string, GameObject> targetList = new Dictionary<string, GameObject>();

    public void AddTarget(GameObject target)
    {
        EntityStats stats = target.GetComponent<EntityStats>();
        targetList.Add(stats.entityName, target);
    }

    public void RemoveTarget(GameObject target)
    {
        EntityStats stats = target.GetComponent<EntityStats>();
        targetList.Remove(stats.entityName);
    }

    public Dictionary<string, GameObject> TargetList
    {
        get { return targetList; }
    }
}
