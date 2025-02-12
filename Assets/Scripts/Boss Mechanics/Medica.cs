using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class Medica
{
    public static MechanicLogic AcclimationEffectStart(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        List<GameObject> possibleTargets = battleManager.GetAliveEntities();

        foreach (var character in possibleTargets)
        {
            EntityStats stats = character.GetComponent<EntityStats>();


            stats.ReduceStatusTurns(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"));
            stats.ReduceStatusTurns(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"));

        }

        passthrough.attacker.GetComponent<EntityController>().ModifyOpacity(1f);

        return new MechanicLogic();
    }

    public static MechanicLogic AcclimationEffectEnd(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        List<GameObject> possibleTargets = battleManager.GetAliveEntities();

        Debug.Log("TEST!");
        foreach (var character in possibleTargets)
        {
            EntityStats stats = character.GetComponent<EntityStats>();
            StatusBattle status = stats.effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"));

            if (status != null)
            {

                if (status.turnsRemaining == 0)
                {
                    stats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"));

                    PlayerSkill newSkill = (PlayerSkill)ScriptableObject.CreateInstance("PlayerSkill");
                    newSkill.damageType = DamageType.DAMAGE;
                    newSkill.scaler = Scaler.ATTACK;
                    newSkill.scaleMult = 3.5f;
                    newSkill.baseValue = 150;
                    newSkill.attackCount = 1;

                    float entityDamage = GlobalDamageHelper.HandleActionCalculation(new ActionInformation(character, passthrough.attacker, newSkill, null));

                    battleManager.DealDamage(character, entityDamage, passthrough.attacker);

                    stats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"), passthrough.attacker);
                }
            }
            else
            {


                status = stats.effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"));

                if (status.turnsRemaining == 0)
                {
                    stats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"));

                    PlayerSkill newSkill = (PlayerSkill)ScriptableObject.CreateInstance("PlayerSkill");
                    newSkill.damageType = DamageType.DAMAGE;
                    newSkill.scaler = Scaler.ATTACK;
                    newSkill.scaleMult = 3.5f;
                    newSkill.baseValue = 150;
                    newSkill.attackCount = 1;

                    float entityDamage = GlobalDamageHelper.HandleActionCalculation(new ActionInformation(character, passthrough.attacker, newSkill));

                    battleManager.DealDamage(character, entityDamage, passthrough.attacker);

                    stats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"), passthrough.attacker);
                }
            }
        }

        return new MechanicLogic();
    }

    public static MechanicLogic LonelyGhostRedTarget(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        var possibleTargets = battleManager.playerCharacters;
        MechanicLogic logic = new MechanicLogic();

        foreach (var target in possibleTargets)
        {
            EntityStats stats = target.GetComponent<EntityStats>();

            if (!stats.mechanicVariables.targeted)
            {
                var effect = stats.effectHandler.GetEffectByKey("spirit_acclimation_blue");

                if (effect != null)
                {
                    stats.mechanicVariables.targeted = true;
                    logic.overriddenTarget = target;
                    return logic;
                }
            }
        }

        return null;
    }

    public static MechanicLogic LonelyGhostBlueTarget(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        var possibleTargets = battleManager.playerCharacters;
        MechanicLogic logic = new MechanicLogic();

        foreach (var target in possibleTargets)
        {
            EntityStats stats = target.GetComponent<EntityStats>();

            if (!stats.mechanicVariables.targeted)
            {
                var effect = stats.effectHandler.GetEffectByKey("spirit_acclimation_red");

                if (effect != null)
                {
                    stats.mechanicVariables.targeted = true;
                    logic.overriddenTarget = target;
                    return logic;
                }
            }
        }

        return null;
    }

    public static MechanicLogic RedAcclimationHit(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        List<GameObject> possibleChars = battleManager.GetAliveEntities();
        Debug.Log("Blue acclimation effect");

        MechanicLogic logic = new MechanicLogic();

        // Blue Acclimation should always have an AoE

        if (passthrough.aoe == null)
        {
            Debug.LogError("Blue Acclimation should always have an AoE");
            return logic;
        }

        EntityStats targetStats = passthrough.target.GetComponent<EntityStats>();

        StatusBattle statusEffect = targetStats.effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"));

        if (statusEffect != null)
        {
            targetStats.ModifyStatus(statusEffect.effect);
            targetStats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"), passthrough.attacker);
            logic.overrideDamage = true;
            logic.overriddenDamage = passthrough.entityDamage * 0.20f;
            return logic;
        }

        return logic;

    }

    public static MechanicLogic BlueAcclimationHit(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        List<GameObject> possibleChars = battleManager.GetAliveEntities();
        Debug.Log("Blue acclimation effect");

        MechanicLogic logic = new MechanicLogic();

        // Blue Acclimation should always have an AoE

        if (passthrough.aoe == null)
        {
            Debug.LogError("Blue Acclimation should always have an AoE");
            return logic;
        }

        EntityStats targetStats = passthrough.target.GetComponent<EntityStats>();

        StatusBattle statusEffect = targetStats.effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"));

        if (statusEffect != null)
        {
            targetStats.ModifyStatus(statusEffect.effect);
            targetStats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"), passthrough.attacker);
            logic.overrideDamage = true;
            logic.overriddenDamage = passthrough.entityDamage * 0.20f;
            return logic;
        }

        return logic;

    }

    public static MechanicLogic AddTarget(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        Debug.Log("TESTING!!");
        MechanicLogic logic = new MechanicLogic();
        EnemyContents enemyContents = passthrough.attacker.GetComponent<EnemyContents>();

        if (enemyContents.aggression.aggressionListCount() != 0)
            return logic;


        List<GameObject> possibleChars = battleManager.GetAliveEntities();
        List<GameObject> lineOfSight = new List<GameObject>();
        foreach (var character in possibleChars)
        {
            Vector2 startPosition = passthrough.attacker.transform.position;
            Vector2 endPosition = character.transform.position;

            Vector2 direction = (endPosition - startPosition).normalized;

            float distance = Vector2.Distance(startPosition, endPosition);

            RaycastHit2D hit = Physics2D.Raycast(startPosition, direction, distance, LayerMask.GetMask("Obstacles"));

            Debug.Log("Hit!! " + hit.collider);

            if (hit.collider == null)
            {
                Debug.Log("Add for position: " + startPosition + " may attack " + character);
                lineOfSight.Add(character);
            }
        }

        Debug.Log("LINE OF SIGHT: " + lineOfSight.Count);

        if (lineOfSight.Count == 0)
        {
            ExcentraGame.Instance.triggers.ActivateTrigger(battleManager, passthrough.mechanic, "adds");
            return logic;

        }

        logic.overriddenTarget = lineOfSight[Random.Range(0, lineOfSight.Count)];

        Debug.Log("OVERRIDDEN TARGET (inside): " + logic.overriddenTarget);



        return logic;
    }
    public static void SpawnAddsTrigger(EntityStats stats, BattleManager battleManager, EnemyMechanic mechanic)
    {
        foreach (var attack in mechanic.mechanicAttacks)
        {
            foreach (var addKey in attack.addKeys)
            {
                foreach (var enemy in battleManager.enemyList)
                {
                    EntityStats enemyStats = enemy.GetComponent<EntityStats>();
                    if (enemyStats.entityKey == addKey.entityKey)
                    {
                        if (enemyStats.currentHP > 0)
                        {
                            ExcentraGame.Instance.triggers.ActivateTrigger(battleManager, mechanic, "adds");
                            return;

                        }
                    }
                }
            }
        }

        GameObject owner = stats.addOwner;
        EntityStats ownerStats = owner.GetComponent<EntityStats>();
        ownerStats.active = true;
        ownerStats.targetable = true;

        //battleManager.turnManager.CalculateIndividualDelay(ownerStats.gameObject);

        EnemyAI enemyAi = owner.GetComponent<EnemyAI>();
        enemyAi.ChangePhase(true);
        battleManager.turnManager.CalculateIndividualDelay(owner.gameObject, battleManager.turnManager.ReturnDelayNeededForTurn(0));
    }

    public static MechanicLogic SweetBlissStart(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        MechanicLogic logic = AcclimationEffectStart(battleManager, passthrough);
        GameObject attacker = passthrough.attacker;

        Debug.Log("ATTACKER START IS " + attacker);

        // Get the Renderer component of the attacker
        attacker.GetComponent<EntityController>().ModifyOpacity(0f);

        return logic;

    }

    public static MechanicLogic SweetBlissEnd(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        return AcclimationEffectEnd(battleManager, passthrough);
    }
}
