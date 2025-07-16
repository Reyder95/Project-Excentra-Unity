using UnityEngine;

public class TurnEntityData
{
    public GameObject entityTurn;
    public AoeTurn aoeTurn;
}

public class TurnEntity
{
    //GameObject entityTurn;
    //AoeTurn aoeTurn;
    public TurnEntityData turnData = new TurnEntityData();
    public bool isEntity = true;
    public float delay = 0f;

    public TurnEntity(GameObject entity)
    {
        if (entity.TryGetComponent(out EntityStats stats))
        {
            turnData.entityTurn = entity;
            isEntity = true;
        }
    }

    public TurnEntity(AoeTurn aoe)
    {
        turnData.aoeTurn = aoe;
        isEntity = false;
    }

    public bool EqualsEntity(GameObject entity)
    {
        if (turnData.entityTurn != null)
            return turnData.entityTurn == entity;

        return turnData.aoeTurn.aoes.Contains(entity);
    }

    public TurnEntityData GetEntity()
    {
        return turnData;
    }

    public void CalculateDelay(bool turn = false)
    {
        if (turnData.entityTurn != null)
        {
            if (!turn)
            {
                delay = (int)Mathf.Floor((500 + UnityEngine.Random.Range(10, 26) / (turnData.entityTurn.GetComponent<EntityStats>().CalculateSpeed() * 10.5f)) * UnityEngine.Random.Range(10, 26)) / turnData.entityTurn.GetComponent<EntityStats>().CalculateSpeed();
            }
        }

        if (turn)
        {
            delay = (int)Mathf.Floor(delay * 0.80f);
        }
    }

    public void CalculateDirectDelay(float delay)
    {
        this.delay = delay;
    }
}
