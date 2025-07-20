using NUnit.Framework.Internal.Commands;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MechanicAoeData
{
    public float delay;
    public GameObject aoeObject;

    public MechanicAoeData(float delay, GameObject aoeObject)
    {
        this.delay = delay;
        this.aoeObject = aoeObject;
    }
}

public static class BossMechanicHandler
{
    public static void InitializeMechanic(EnemyMechanic mechanic, BattleManager battleManager, GameObject attacker, bool skip = false)
    {
        if (mechanic.customScript && !skip)
        {
            CustomMechanicLogicHelper.ExecuteCustomMechanic(mechanic.customScriptKey, battleManager, mechanic);
        }

        if (mechanic.containsMovement && !skip)
        {
            EntityController controller = attacker.GetComponent<EntityController>();

            Vector2 targetPosition = Vector2.zero;

            targetPosition = battleManager.arena.GetCenter();

            controller.MoveTowards(targetPosition, mechanic);
            return;
        }

        CustomLogicPassthrough passthrough = new CustomLogicPassthrough(null, attacker, 0f, null, mechanic);
        CustomMechanicLogicHelper.ExecuteMechanic(mechanic.mechanicKey, battleManager, passthrough);

        if (mechanic.mechanicStyle == MechanicStyle.IMMEDIATE)
        {
            foreach (MechanicAttack attack in mechanic.mechanicAttacks)
            {
                switch (attack.attackType)
                {
                    case AttackType.SINGLE_TARGET:
                        InitializeSingleTargetAttack(mechanic, attack, battleManager, attacker);
                        break;
                    case AttackType.ADD:
                        InitializeAddAttack(mechanic, attack, battleManager, attacker);
                        break;
                }
            }
        }
        else
        {
            float delay = -1f;
            List<GameObject> aoes = new List<GameObject>();
            foreach (MechanicAttack attack in mechanic.mechanicAttacks)
            {
                MechanicLogic logic = CustomMechanicLogicHelper.ExecuteMechanic(attack.targetKey, battleManager, passthrough);
                switch (attack.attackType)
                {
                    // Custom targeting logic mechanic target key
                    case AttackType.AOE:
                        if (attack.targetType == EntityTargetType.ALL)
                        {
                            List<GameObject> possibleChars = battleManager.GetAliveEntities();

                            foreach (var entity in possibleChars)
                            {
                                MechanicAoeData mechanicAoeData = InitializeAOEAttack(mechanic, attack, battleManager, attacker, logic, entity);
                                delay = Mathf.Max(mechanicAoeData.delay, delay);

                                aoes.Add(mechanicAoeData.aoeObject);
                            }
                        }
                        else
                        {
                            MechanicAoeData mechanicAoeData = InitializeAOEAttack(mechanic, attack, battleManager, attacker, logic, logic.overriddenTarget);
                            delay = Mathf.Max(mechanicAoeData.delay, delay);
                            aoes.Add(mechanicAoeData.aoeObject);
                        }
                            
                        break;
                    case AttackType.SINGLE_TARGET:
                        break;
                    case AttackType.TETHER:
                        break;
                    case AttackType.ADD:
                        InitializeAddAttack(mechanic, attack, battleManager, attacker);
                        break;


                    //aoeEntity.CalculateDirectDelay(delay);
                    //bool added = turnManager.InsertUnitIntoTurn(aoeEntity);
                    //if (!added)
                        //{
                        //    turnManager.turnOrder.Add(aoeEntity);
                        //}

                        //turnManager.DisplayTurnOrder();
                }
            }

            if (mechanic.priorityIndex.Length > 0)
            {
                float maxDelay = -1f;
                foreach (MechanicPriorityIndex priorityElement in mechanic.priorityIndex)
                {
                    List<GameObject> prioAoes = new List<GameObject>();

                    foreach (int index in priorityElement.index)
                    {
                        if (index < aoes.Count)
                        {
                            prioAoes.Add(aoes[index]);
                        }
                    }

                    //if (delay == -1f)
                    //    delay = turnManager.ReturnDelayNeededForTurn(mechanicAttack.turnOffset);

                    //if (targetedLogic.overrideDelay)
                    //    delay = targetedLogic.overriddenDelay;

                    AoeTurn aoeTurn = new AoeTurn(prioAoes);
                    TurnEntity aoeEntity = new TurnEntity(aoeTurn);
                    delay = battleManager.turnManager.ReturnDelayNeededForTurn(priorityElement.turnOffset);

                    maxDelay = Mathf.Max(maxDelay, delay);
                    aoeEntity.CalculateDirectDelay(delay);
                    bool added = battleManager.turnManager.InsertUnitIntoTurn(aoeEntity);
                    if (!added)
                    {
                        battleManager.turnManager.turnOrder.Add(aoeEntity);
                    }
                }

                if (!mechanic.active)
                {
                    EntityStats stats = attacker.GetComponent<EntityStats>();
                    stats.nextStaticDelay = maxDelay + 1;
                }

            }
            else
            {
                AoeTurn aoeTurn = new AoeTurn(aoes);
                TurnEntity aoeEntity = new TurnEntity(aoeTurn);
                aoeEntity.CalculateDirectDelay(delay);
                if (!mechanic.active)
                {
                    EntityStats stats = attacker.GetComponent<EntityStats>();
                    stats.nextStaticDelay = delay + 1;
                }
                bool added = battleManager.turnManager.InsertUnitIntoTurn(aoeEntity);
                if (!added)
                {
                    battleManager.turnManager.turnOrder.Add(aoeEntity);
                }
            }

            battleManager.turnManager.DisplayTurnOrder();
        }
    }
    public static void EndMechanic(EnemyMechanic mechanic, BattleManager battleManager, GameObject attacker)
    {
        CustomLogicPassthrough passthrough = new CustomLogicPassthrough(null, attacker, 0f, null, mechanic);
        CustomMechanicLogicHelper.ExecuteMechanic(mechanic.mechanicKey + "_end", battleManager, passthrough);
    }
    public static MechanicAoeData InitializeAOEAttack(EnemyMechanic mechanic, MechanicAttack mechanicAttack, BattleManager battleManager, GameObject attacker, MechanicLogic targetedLogic, GameObject target = null)
    {
        if (mechanicAttack.attackType != AttackType.AOE)
            return null;

        GameObject actualTarget = null;

        EnemyAI enemyAi = attacker.GetComponent<EnemyAI>();

        if (mechanicAttack.targetType != EntityTargetType.NONE)
        {
            actualTarget = enemyAi.ChooseEntity(mechanicAttack.targetType);
        }

        if (target != null)
            actualTarget = target;

        TurnManager turnManager = battleManager.turnManager;

        GameObject aoe;

        if (mechanicAttack.aoeShape == Shape.CONE)
        {
            aoe = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("cone"), new Vector2(1000, 1000), Quaternion.identity);
        }
        else if (mechanicAttack.aoeShape == Shape.CIRCLE)
        {
            aoe = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("circle"), new Vector2(1000, 1000), Quaternion.identity);
        }
        else if (mechanicAttack.aoeShape == Shape.LINE)
        {
            aoe = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("line"), new Vector2(1000, 1000), Quaternion.identity);
        }
        else if (mechanicAttack.aoeShape == Shape.DONUT)
        {
            aoe = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("donut"), new Vector2(1000, 1000), Quaternion.identity);
        }
        else
        {
            aoe = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("box"), new Vector2(1000, 1000), Quaternion.identity);
        }
        
        BaseAoe aoeInfo = aoe.GetComponent<BaseAoe>();

        SkillInformation info = new SkillInformation();

        if (mechanicAttack.directTarget != null)
            actualTarget = mechanicAttack.directTarget;

        if (mechanicAttack.originIsSelf)
        {
            info.objectOrigin = attacker;
        }
        else if (actualTarget != null && mechanicAttack.originIsTarget)
        {
            info.objectOrigin = actualTarget;
        }
        
        if (actualTarget != null && mechanicAttack.endpointIsTarget)
        {
            info.objectTarget = actualTarget;
        }

        aoeInfo.InitializeEnemyAoe(attacker, mechanic, mechanicAttack, info);
        aoeInfo.arenaAoeIndex = battleManager.aoeArenadata.AddAoe(aoe);

        //TurnEntity aoeEntity = new TurnEntity(aoe);
        float delay = CustomMechanicLogicHelper.ExecuteMechanicDelay(mechanicAttack.attackKey, battleManager);
        if (delay == -1f)
        {
            delay = turnManager.ReturnDelayNeededForTurn(mechanicAttack.turnOffset);
        }
            

        //if (targetedLogic.overrideDelay)
        //    delay = targetedLogic.overriddenDelay;

        //aoeEntity.CalculateDirectDelay(delay);

        //bool added = turnManager.InsertUnitIntoTurn(aoeEntity);

        //if (!added)
        //{
        //    turnManager.turnOrder.Add(aoeEntity);
        //}

        //turnManager.DisplayTurnOrder();

        if (mechanicAttack.canBeShirked)
        {
            GameObject particleLineSpawned = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("particle-line"), new Vector2(1000, 1000), Quaternion.identity);
            particleLineSpawned.GetComponent<ParticleLine>().SetContents(attacker, actualTarget);
            aoeInfo.particleEmitter = particleLineSpawned;
        }


        return new MechanicAoeData(delay, aoe);
    }

    public static void ActivateAoeAttack(EnemyMechanic mechanic, MechanicAttack mechanicAttack, BattleManager battleManager, GameObject attacker, BaseAoe aoe)
    {
        Dictionary<string, GameObject> targets = aoe.aoeData.TargetList;

        Debug.Log("Target Count: " + targets.Count);

        CustomMechanicLogicHelper.ExecuteMechanic(mechanicAttack.attackKey + "_before", battleManager, new CustomLogicPassthrough(aoe, attacker, 0f, null, mechanic));

        try
        {
            foreach (var entity in targets)
            {
                PlayerSkill newSkill = (PlayerSkill)ScriptableObject.CreateInstance("PlayerSkill");
                newSkill.damageType = mechanicAttack.damageType;
                newSkill.scaler = mechanicAttack.scaler;
                newSkill.scaleMult = mechanicAttack.scaleMult;
                newSkill.baseValue = mechanicAttack.baseValue;
                newSkill.attackCount = 1;
                float entityDamage = GlobalDamageHelper.HandleActionCalculation(new ActionInformation(entity.Value, attacker, newSkill));

                if (mechanicAttack.isStack)
                    entityDamage = entityDamage / aoe.aoeData.TargetList.Count;

                CustomLogicPassthrough passthrough = new CustomLogicPassthrough(aoe, attacker, entityDamage, entity.Value, mechanic);

                MechanicLogic logic = CustomMechanicLogicHelper.ExecuteMechanic(mechanicAttack.attackKey, battleManager, passthrough);

                if (logic.overrideDamage)
                {
                    battleManager.DealDamage(entity.Value, logic.overriddenDamage, attacker);
                }
                else
                {
                    battleManager.DealDamage(entity.Value, entityDamage, attacker);
                }
            }

            if (mechanicAttack.isSoak)
            {
                if (aoe.aoeData.TargetList.Count == 0)
                {
                    List<GameObject> possibleCharacters = battleManager.GetAliveEntities();

                    foreach (GameObject character in possibleCharacters)
                    {
                        battleManager.DealDamage(character, mechanicAttack.soakDamage, attacker);
                    }
                }
            }

            if (aoe.particleEmitter != null)
            {
                UnityEngine.GameObject.Destroy(aoe.particleEmitter);
                aoe.particleEmitter = null;
            }

            Debug.Log(mechanic);

            if (mechanic.containsTrigger)
                ExcentraGame.Instance.triggers.ActivateTrigger(battleManager, mechanic, mechanicAttack.triggerKey);
        } catch (InvalidOperationException ex) {
            Debug.Log(ex);
        }

        
    }

    public static void InitializeSingleTargetAttack(EnemyMechanic mechanic, MechanicAttack mechanicAttack, BattleManager battleManager, GameObject attacker)
    {
        EnemyAI enemyAi = attacker.GetComponent<EnemyAI>();
        GameObject target = enemyAi.ChooseEntity(mechanicAttack.targetType);
        enemyAi.currTarget = target;
        enemyAi.currImmediateAttack = mechanic;
        MechanicLogic logic = CustomMechanicLogicHelper.ExecuteMechanic(mechanicAttack.targetKey, battleManager, new CustomLogicPassthrough(null, attacker, 0f, target, mechanic));
        if (logic.overriddenTarget != null)
            enemyAi.currTarget = logic.overriddenTarget;

        EntityController controller = attacker.GetComponent<EntityController>();
        controller.MoveTowards(enemyAi.currTarget, mechanic.animationTrigger);

    }
    public static void ActivateSingleTargetAttack(EnemyMechanic mechanic, MechanicAttack mechanicAttack, BattleManager battleManager, GameObject attacker, GameObject target)
    {
        PlayerSkill newSkill = (PlayerSkill)ScriptableObject.CreateInstance("PlayerSkill");
        newSkill.damageType = mechanicAttack.damageType;
        newSkill.scaler = mechanicAttack.scaler;
        newSkill.scaleMult = mechanicAttack.scaleMult;
        newSkill.baseValue = mechanicAttack.baseValue;
        newSkill.attackCount = 1;
        float entityDamage = GlobalDamageHelper.HandleActionCalculation(new ActionInformation(target, attacker, newSkill));
        battleManager.DealDamage(target, entityDamage, attacker);

        CustomLogicPassthrough passthrough = new CustomLogicPassthrough(null, attacker, entityDamage, target, mechanic);

        CustomMechanicLogicHelper.ExecuteMechanic(mechanicAttack.attackKey, battleManager, passthrough);
    }
    public static void ActivateTetherAttack(MechanicAttack mechanicAttack, BattleManager battleManager, GameObject tether1, GameObject tether2)
    {
        // Destroy tether

        if (Vector2.Distance(tether1.transform.position, tether2.transform.position) > mechanicAttack.tetherRange)
        {
            return;
        }

        // Apply damage to tethered entities
    }

    public static void InitializeAddAttack(EnemyMechanic mechanic, MechanicAttack attack, BattleManager battleManager, GameObject attacker)
    {
        foreach (AddSpawner add in attack.addKeys)
        {
            GameObject spawnedAdd = battleManager.SpawnNewEntity(ExcentraDatabase.TryGetEntity(add.entityKey), add.bottomLeft, add.entityKey, add.aiKey, add.next);

            EntityStats addStats = spawnedAdd.GetComponent<EntityStats>();

            System.Action<EntityStats, BattleManager, EnemyMechanic> addAction = CustomMechanicLogicHelper.ExecuteMechanicTrigger(attack.attackKey);

            if (addAction != null)
                addStats.OnEntityKilled += addAction;

            addStats.addOwner = attacker;
            addStats.addMechanic = mechanic;


        }
    }
}
