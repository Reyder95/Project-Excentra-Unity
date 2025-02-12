using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AggressionSystem
{
    OrderedDictionary aggressionList = new OrderedDictionary();

    public void AggressionEntryPoint(AggressionElement element)
    {
        EntityStats stats = element.entity.GetComponent<EntityStats>();
        if (aggressionList.Contains(stats.entityName))
        {
            ModifyExistingEntityValue(element);
        }
        else
        {
            AddEntityToAggressionList(element);
        }

        //OutputAggressionList();
    }

    // Adds an entity to the aggression list based on its aggressionValue. This aggressionList should always be sorted properly.
    public void AddEntityToAggressionList(AggressionElement element)
    {
        EntityStats stats = element.entity.GetComponent<EntityStats>();

        if (aggressionList.Contains(stats.entityName))
            return;

        aggressionList.Add(stats.entityName, element);

        SortAggressionList();
    }

    public void RemoveEntityFromAggressionList(GameObject entity)
    {
        EntityStats stats = entity.GetComponent<EntityStats>();
        aggressionList.Remove(stats.entityName);
        //OutputAggressionList();
    }

    public void ModifyExistingEntityValue(AggressionElement element)
    {
        EntityStats stats = element.entity.GetComponent<EntityStats>();

        if (aggressionList.Contains(stats.entityName))
        {
            AggressionElement entryValue = aggressionList[stats.entityName] as AggressionElement;

            entryValue.aggressionValue = Mathf.Max(entryValue.aggressionValue, element.aggressionValue);
        }

        SortAggressionList();
    }

    public void ReduceAggressionEnmity()
    {
        foreach (DictionaryEntry entry in aggressionList)
        {
            AggressionElement entryValue = entry.Value as AggressionElement;
            EntityStats stats = entryValue.entity.GetComponent<EntityStats>();

            entryValue.aggressionValue = entryValue.aggressionValue * (1 - 0.20f * (1 + (stats.aggressionDecay / 100)));
        }
        SortAggressionList();
        
        //OutputAggressionList();
    }

    public void SortAggressionList()
    {
        var sortedByValues = new OrderedDictionary();

        foreach (var entry in aggressionList.Cast<DictionaryEntry>().OrderByDescending(entry => (entry.Value as AggressionElement).aggressionValue))
        {
            sortedByValues.Add(entry.Key, entry.Value);
        }

        aggressionList = sortedByValues;
    }

    public void OutputAggressionList()
    {
        foreach (DictionaryEntry entry in aggressionList)
        {
            Debug.Log("Entity: " + entry.Key + " | " + "Aggression: " + (entry.Value as AggressionElement).aggressionValue);
        }
    }

    public GameObject GetFirstAggression()
    {
        if (aggressionList.Count == 0)
            return null;

        OutputAggressionList();
        AggressionElement entity = aggressionList[0] as AggressionElement;
        return entity.entity;
    }

    public GameObject GetSecondAggression()
    {
        if (aggressionList.Count < 2)
            return null;
        AggressionElement entity = aggressionList[1] as AggressionElement;
        return entity.entity;
    }

    public GameObject GetLastAggression()
    {
        if (aggressionList.Count == 0)
            return null;
        AggressionElement entity = aggressionList[aggressionList.Count - 1] as AggressionElement;
        return entity.entity;
    }

    public int aggressionListCount()
    {
        return aggressionList.Count;
    }
}
