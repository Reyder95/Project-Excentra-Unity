using System.Collections;
using System.Collections.Specialized;
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

        OutputAggressionList();
    }

    // Adds an entity to the aggression list based on its aggressionValue. This aggressionList should always be sorted properly.
    public void AddEntityToAggressionList(AggressionElement element)
    {
        EntityStats stats = element.entity.GetComponent<EntityStats>();

        if (aggressionList.Contains(stats.entityName))
            return;

        int counter = 0;

        foreach (DictionaryEntry entry in aggressionList)
        {
            AggressionElement entryValue = entry.Value as AggressionElement;
            

            if (entryValue.aggressionValue < element.aggressionValue)
            {
                aggressionList.Insert(counter, stats.entityName, element);
                return;
            }

            counter++;
        }

        aggressionList.Add(stats.entityName, element);
    }

    public void RemoveEntityFromAggressionList(GameObject entity)
    {
        EntityStats stats = entity.GetComponent<EntityStats>();
        aggressionList.Remove(stats.entityName);
        Debug.Log("Displaying!");
        OutputAggressionList();
    }

    public void ModifyExistingEntityValue(AggressionElement element)
    {
        EntityStats stats = element.entity.GetComponent<EntityStats>();

        if (aggressionList.Contains(stats.entityName))
        {
            AggressionElement entryValue = aggressionList[stats.entityName] as AggressionElement;

            entryValue.aggressionValue = Mathf.Max(entryValue.aggressionValue, element.aggressionValue);
        }
    }

    public void ReduceAggressionEnmity()
    {
        foreach (DictionaryEntry entry in aggressionList)
        {
            AggressionElement entryValue = entry.Value as AggressionElement;

            entryValue.aggressionValue = entryValue.aggressionValue * 0.20f;
        }
    }

    public GameObject ReturnTargetEntity()
    {
        if (aggressionList.Count == 0)
            return null;

        AggressionElement entity = aggressionList[0] as AggressionElement;
        return entity.entity;
    }

    public void OutputAggressionList()
    {
        foreach (DictionaryEntry entry in aggressionList)
        {
            Debug.Log("Entity: " + entry.Key + " | " + "Aggression: " + (entry.Value as AggressionElement).aggressionValue);
        }
    }
}
