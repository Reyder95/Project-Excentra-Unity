using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TurnManager
{
    List<GameObject> turnOrder = new List<GameObject>();
    Dictionary<string, GameObject> deadUnits = new Dictionary<string, GameObject>();
    VisualTreeAsset turnElement;
    VisualElement turnOrderUI;


    public void InitializeTurnManager(List<GameObject> players, GameObject boss)
    {
        InitializeTurnOrder(players, boss);
        ReferenceUIElements();
        DisplayTurnOrder();
    }

    public void InitializeTurnOrder(List<GameObject> players, GameObject boss)
    {
        turnOrder.Clear();

        turnOrder.AddRange(players);
        turnOrder.Add(boss);

        foreach (var character in turnOrder)
        {
            character.GetComponent<EntityStats>().CalculateDelay();
        }

        turnOrder.Sort((a, b) =>
        {
            return (int)a.GetComponent<EntityStats>().delay - (int)b.GetComponent<EntityStats>().delay;
        });
    }

    public bool InsertUnitIntoTurn(GameObject entity)
    {
        EntityStats entityStats = entity.GetComponent<EntityStats>();
        for (int i = 0; i < turnOrder.Count; i++)
        {
            EntityStats turnStats = turnOrder[i].GetComponent<EntityStats>();

            if (turnStats.delay > entityStats.delay)
            {
                turnOrder.Insert(i, entity);
                return true;
            }
        }

        return false;
    }

    public void ReferenceUIElements()
    {
        turnElement = ExcentraDatabase.TryGetSubDocument("turn-element");

        turnOrderUI = ExcentraDatabase.TryGetDocument("battle").rootVisualElement.Q<VisualElement>("turn-order");
    }

    public void DisplayTurnOrder()
    {
        turnOrderUI.Clear();
        foreach (var character in turnOrder)
        {
            VisualElement charElement = turnElement.CloneTree();
            VisualElement charPortrait = charElement.Q<VisualElement>("portrait");
            Label delayLabel = charElement.Q<Label>("delay");
            charPortrait.style.backgroundImage = character.GetComponent<EntityStats>().portrait;
            delayLabel.text = character.GetComponent<EntityStats>().delay.ToString();
            turnOrderUI.Add(charElement);
        }
    }

    // TODO: Might need optimization
    public void KillEntity(GameObject entity)
    {
        turnOrder.Remove(entity);

        EntityStats entityStats = entity.GetComponent<EntityStats>();
        deadUnits.Add(entityStats.entityName, entity);
    }

    public void ReviveEntity(GameObject entity)
    {
        deadUnits.Remove(entity.GetComponent<EntityStats>().entityName);

        EntityStats entityStats = entity.GetComponent<EntityStats>();
        entityStats.CalculateDelay();

        bool added = InsertUnitIntoTurn(entity);

        if (!added)
        {
            turnOrder.Add(entity);
        }
    }

    public void CalculateAllDelay()
    {
        foreach (var character in turnOrder)
        {
            character.GetComponent<EntityStats>().CalculateDelay(true);
        }
    }

    public void EndCurrentTurn()
    {
        GameObject currTurn = PopCurrentTurn();
        EntityStats currTurnStats = currTurn.GetComponent<EntityStats>();
        currTurnStats.CalculateDelay();
        
        CalculateAllDelay();

        bool added = InsertUnitIntoTurn(currTurn);

        if (currTurnStats.isPlayer)
        {
            PlayerInput input = currTurn.GetComponent<PlayerInput>();
            input.enabled = false;
        }

        if (!added)
            turnOrder.Add(currTurn);

        DisplayTurnOrder();

    }

    public GameObject PopCurrentTurn()
    {
        GameObject currTurn = turnOrder[0];
        turnOrder.RemoveAt(0);
        return currTurn;
    }

    public GameObject GetCurrentTurn()
    {
        return turnOrder[0];
    }
}
