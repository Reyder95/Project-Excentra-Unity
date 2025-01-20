using UnityEngine;

public class AggressionElement
{
    public GameObject entity;
    public float aggressionValue;

    public AggressionElement(GameObject entity, float damageDealt)
    {
        EntityStats stats = entity.GetComponent<EntityStats>();
        this.entity = entity;
        this.aggressionValue = ((damageDealt * 0.3f) * (stats.CalculateAggressionGen()) * (1 + (stats.CalculateAggressionGen() / 100)) / 10);

        Debug.Log("Aggro! " + aggressionValue);
    }
}
