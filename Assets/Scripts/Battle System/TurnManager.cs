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
    public List<TurnEntity> turnOrder = new List<TurnEntity>();    // A direct order of GameObjects (entities) in the battle, sorted by Delay. Lower Delay means they will be going before higher delays.
    Dictionary<string, GameObject> deadUnits = new Dictionary<string, GameObject>();    // A dictionary of all dead units. Makes it easy to bring back people into the turnOrder once they are revived
    
    // Visual Elements. One for the turn element (top left), and the turnOrderUI parent container element
    VisualTreeAsset turnElement;
    VisualElement turnOrderUI;

    float averageTurnDelay = 0;


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

        float totalDelay = 0;
        int count = 0;

        // Adds all entities to a single turnOrder List

        foreach (var player in players)
        {
            turnOrder.Add(new TurnEntity(player));
        }
        turnOrder.Add(new TurnEntity(boss));

        // Calculates the initial delay for all entities
        foreach (var character in turnOrder)
        {
            if (character.isEntity)
            {
                EnemyAI enemyAi;
                if (character.GetEntity().TryGetComponent<EnemyAI>(out enemyAi))
                {
                    if (enemyAi.enabled)
                    {
                        if (enemyAi.initialPhase != null)
                        {
                            character.CalculateDirectDelay(0);
                            continue;
                        }
                    }
                }
            }


            character.CalculateDelay();
            totalDelay += character.delay;
            count++;
        }

        averageTurnDelay = totalDelay / count;

        // Sorts them by the delay we've just calculated
        turnOrder.Sort((a, b) =>
        {
            return (int)a.delay - (int)b.delay;
        });
    }

    // For an entity who either gets revived, or just goes, we recalculate and insert them back
    public bool InsertUnitIntoTurn(TurnEntity entity)
    {
        for (int i = 0; i < turnOrder.Count; i++)
        {
            if (turnOrder[i].delay > entity.delay)
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
            if (character.isEntity)
                charPortrait.style.backgroundImage = character.GetEntity().GetComponent<EntityStats>().portrait;
            else
                charPortrait.style.backgroundImage = ExcentraDatabase.TryGetSkill("fireball").icon;
            delayLabel.text = ((int)character.delay).ToString();
            turnOrderUI.Add(charElement);
        }
    }

    // TODO: Might need optimization
    public void KillEntity(GameObject entity)
    {
        foreach (var loopedEntity in turnOrder)
        {
            if (loopedEntity.GetEntity() == entity)
            {
                turnOrder.Remove(loopedEntity);
                break;
            }
        }

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
        TurnEntity newEntity = new TurnEntity(entity);
        newEntity.CalculateDelay();

        bool added = InsertUnitIntoTurn(newEntity);

        if (!added)
        {
            turnOrder.Add(newEntity);
        }
    }

    public bool CheckIfMechanicOver(EnemyMechanic mechanic)
    {
        foreach (var entity in turnOrder)
        {
            if (!entity.isEntity)
            {
                GameObject aoe = entity.GetEntity();
                BaseAoe aoeInfo = aoe.GetComponent<BaseAoe>();

                if (aoeInfo.mechanic == mechanic)
                {
                    return false;
                }
            }
            else
            {
                if (entity.GetEntity().GetComponent<EntityStats>().addMechanic == mechanic)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // Calculates the delay for all entities that already exist in the turn order.
    // When an entity goes, all other entities get their delay reduced by a percentage found in the CalculateDelay function
    public void CalculateAllDelay()
    {
        foreach (var character in turnOrder)
        {
            if (character.isEntity)
            {
                EntityStats entityStats = character.GetEntity().GetComponent<EntityStats>();

                if (!entityStats.active)
                    continue;
            }    

            character.CalculateDelay(true);
        }
    }

    public void CalculateIndividualDelay(GameObject entity, float forcedDelay = -1f)
    {
        EntityStats stats = entity.GetComponent<EntityStats>();

        TurnEntity turnEntity = null;

        for (int i = 0; i < turnOrder.Count; i++)
        {
            if (turnOrder[i].GetEntity() == entity)
            {
                turnEntity = turnOrder[i];
                turnOrder.RemoveAt(i);
                break;
            }
        }

        if (turnEntity == null)
            return;

        if (forcedDelay == -1f)
            turnEntity.CalculateDelay();
        else
            turnEntity.CalculateDirectDelay(forcedDelay);

        bool added = InsertUnitIntoTurn(turnEntity);

        if (!added)
            turnOrder.Add(turnEntity);

        DisplayTurnOrder();
    }

    public float ReturnDelayNeededForTurn(int turnCount)
    {
        int counter = 0;
        int countdown = 0;
        foreach (var character in turnOrder)
        {
            if (character.isEntity)
            {
                if (character.GetEntity().GetComponent<EntityStats>().active == false && !character.GetEntity().GetComponent<EntityStats>().isPlayer)
                {
                    countdown++;
                    continue;
                }
            }
            if (counter == turnCount)
            {
                break;
            }
            
            counter++;
        }

        if (turnCount > turnOrder.Count)
        {
            return turnOrder[turnOrder.Count - 1 - countdown].delay + (averageTurnDelay * (turnCount - turnOrder.Count));
        }
            

        if (counter == turnOrder.Count)
            return turnOrder[counter - 1].delay + 1;

        return turnOrder[counter].delay;
    }

    public float ReturnDelayNeededForCharacter(GameObject entity)
    {
        foreach (var character in turnOrder)
        {
            if (character.GetEntity() == entity)
            {
                return character.delay + 1;

            }
        }

        return -1f;
    }

    // Ends the current turn, pops the unit out of the turn order, and replaces them back in with a newly fresh calculated delay
    public void EndCurrentTurn(float staticDelay = -1f)
    {
        TurnEntity currTurn = PopCurrentTurn();
        if (staticDelay == -1f)
            currTurn.CalculateDelay();
        else
            currTurn.CalculateDirectDelay(staticDelay);

        CalculateAllDelay();

        if (currTurn.isEntity)
        {
            bool added = InsertUnitIntoTurn(currTurn);

            EntityStats currTurnStats = currTurn.GetEntity().GetComponent<EntityStats>();
            if (currTurnStats.isPlayer)
            {
                PlayerInput input = currTurn.GetEntity().GetComponent<PlayerInput>();
                input.enabled = false;
            }

            if (!added)
                turnOrder.Add(currTurn);
        }

        DisplayTurnOrder();

    }

    public TurnEntity PopCurrentTurn()
    {
        TurnEntity currTurn = turnOrder[0];
        turnOrder.RemoveAt(0);
        return currTurn;
    }

    public GameObject GetCurrentTurn()
    {
        return turnOrder[0].GetEntity();
    }
}
