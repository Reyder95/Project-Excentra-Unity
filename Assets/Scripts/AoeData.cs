// AoeData.cs

using System.Collections.Generic;
using UnityEngine;

// Has the damage information for an AoE indicator. Handles hitting multiple entities at once.
public class AoeData
{
    private Dictionary<string, GameObject> targetList = new Dictionary<string, GameObject>();
    public GameObject sourceTarget;
    public GameObject attacker;

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
