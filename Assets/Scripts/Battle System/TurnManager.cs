// TurnManager.cs

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// Vitally important class that helps facilitate a turn order for the BattleManager.
// TODO: Instead of using GameObjects for turnOrder, I should create a helper TurnObject class, that may have entities but also AoEs and other
// possible turns. For example, some AoEs may operate independently on a turn-by-turn basis (helps for puzzle-like mechanics)
public class TurnManager
{
    List<GameObject> turnOrder = new List<GameObject>();    // A direct order of GameObjects (entities) in the battle, sorted by Delay. Lower Delay means they will be going before higher delays.
    Dictionary<string, GameObject> deadUnits = new Dictionary<string, GameObject>();    // A dictionary of all dead units. Makes it easy to bring back people into the turnOrder once they are revived
    
    // Visual Elements. One for the turn element (top left), and the turnOrderUI parent container element
    VisualTreeAsset turnElement;
    VisualElement turnOrderUI;


    public void InitializeTurnManager(List<GameObject> players, GameObject boss)
    {
        InitializeTurnOrder(players, boss);
        ReferenceUIElements();
        DisplayTurnOrder();
    }

    // Grabs all players and enemies (currently just a single boss), and sorts them by a delay value.
    public void InitializeTurnOrder(List<GameObject> players, GameObject boss)
    {
        turnOrder.Clear();

        // Adds all entities to a single turnOrder List
        turnOrder.AddRange(players);
        turnOrder.Add(boss);

        // Calculates the initial delay for all entities
        foreach (var character in turnOrder)
        {
            character.GetComponent<EntityStats>().CalculateDelay();
        }

        // Sorts them by the delay we've just calculated
        turnOrder.Sort((a, b) =>
        {
            return (int)a.GetComponent<EntityStats>().delay - (int)b.GetComponent<EntityStats>().delay;
        });
    }

    // For an entity who either gets revived, or just goes, we recalculate and insert them back
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

    // Clears and re-displays the appropriate turn order
    public void DisplayTurnOrder()
    {
        turnOrderUI.Clear();
        foreach (var character in turnOrder)
        {
            VisualElement charElement = turnElement.CloneTree();
            VisualElement charPortrait = charElement.Q<VisualElement>("portrait");
            Label delayLabel = charElement.Q<Label>("delay");
            charPortrait.style.backgroundImage = character.GetComponent<EntityStats>().portrait;
            delayLabel.text = ((int)character.GetComponent<EntityStats>().delay).ToString();
            turnOrderUI.Add(charElement);
        }
    }

    // TODO: Might need optimization
    public void KillEntity(GameObject entity)
    {
        turnOrder.Remove(entity);

        EntityStats entityStats = entity.GetComponent<EntityStats>();

        try
        {
            deadUnits.Add(entityStats.entityName, entity);
        }
        catch (ArgumentException)
        {
        }
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

    // Calculates the delay for all entities that already exist in the turn order.
    // When an entity goes, all other entities get their delay reduced by a percentage found in the CalculateDelay function
    public void CalculateAllDelay()
    {
        foreach (var character in turnOrder)
        {
            character.GetComponent<EntityStats>().CalculateDelay(true);
        }
    }

    // Ends the current turn, pops the unit out of the turn order, and replaces them back in with a newly fresh calculated delay
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
