using UnityEngine;

public class TurnEntity
{
    GameObject entityTurn;
    GameObject aoeTurn;
    public bool isEntity = true;
    public float delay = 0f;

    public TurnEntity(GameObject entity)
    {
        if (entity.TryGetComponent(out EntityStats stats))
        {
            entityTurn = entity;
            isEntity = true;
        }
        else
        {
            aoeTurn = entity;
            isEntity = false;
        }
    }

    public GameObject GetEntity()
    {
        if (entityTurn != null)
            return entityTurn;

        return aoeTurn;
    }

    public void CalculateDelay(bool turn = false)
    {
        if (entityTurn != null)
        {
            if (!turn)
            {
                delay = (int)Mathf.Floor((500 + UnityEngine.Random.Range(10, 26) / (entityTurn.GetComponent<EntityStats>().CalculateSpeed() * 10.5f)) * UnityEngine.Random.Range(10, 26)) / entityTurn.GetComponent<EntityStats>().CalculateSpeed();
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
