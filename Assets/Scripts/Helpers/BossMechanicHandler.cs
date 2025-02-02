using System;
using System.Collections.Generic;
using UnityEngine;

public static class BossMechanicHandler
{
    public static void InitializeMechanic(EnemyMechanic mechanic, BattleManager battleManager, GameObject attacker)
    {
        Debug.Log(mechanic.mechanicStyle);
        if (mechanic.mechanicStyle == MechanicStyle.IMMEDIATE)
        {
            foreach (MechanicAttack attack in mechanic.mechanicAttacks)
            {
                switch (attack.attackType)
                {
                    case AttackType.SINGLE_TARGET:
                        InitializeSingleTargetAttack(mechanic, attack, battleManager, attacker);
                        break;
                }
            }

            CustomMechanicLogicHelper.ExecuteMechanic(mechanic.mechanicKey, battleManager, null, attacker);
        }
        else
        {
            float delay = -1f;
            foreach (MechanicAttack attack in mechanic.mechanicAttacks)
            {
                switch (attack.attackType)
                {
                    case AttackType.AOE:
                        if (attack.targetType == EntityTargetType.ALL)
                        {
                            List<GameObject> possibleChars = battleManager.GetAliveEntities();

                            foreach (var entity in possibleChars)
                            {
                                delay = Mathf.Max(InitializeAOEAttack(mechanic, attack, battleManager, attacker, entity), delay);
                            }
                        }
                        else
                            delay = Mathf.Max(InitializeAOEAttack(mechanic, attack, battleManager, attacker), delay);
                        break;
                    case AttackType.SINGLE_TARGET:
                        break;
                    case AttackType.TETHER:
                        break;

                }
            }
            if (!mechanic.active)
            {
                EntityStats stats = attacker.GetComponent<EntityStats>();
                stats.nextStaticDelay = delay + 1;
            }
        }
    }

    public static float InitializeAOEAttack(EnemyMechanic mechanic, MechanicAttack mechanicAttack, BattleManager battleManager, GameObject attacker, GameObject target = null)
    {
        if (mechanicAttack.attackType != AttackType.AOE)
            return -1f;

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
        else
        {
            aoe = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("line"), new Vector2(1000, 1000), Quaternion.identity);
        }
        
        BaseAoe aoeInfo = aoe.GetComponent<BaseAoe>();

        SkillInformation info = new SkillInformation();

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

        TurnEntity aoeEntity = new TurnEntity(aoe);
        float delay = turnManager.ReturnDelayNeededForTurn(mechanicAttack.turnOffset);
        aoeEntity.CalculateDirectDelay(delay);

        bool added = turnManager.InsertUnitIntoTurn(aoeEntity);

        if (!added)
        {
            turnManager.turnOrder.Add(aoeEntity);
        }

        turnManager.DisplayTurnOrder();

        return delay;
    }

    public static void ActivateAoeAttack(MechanicAttack mechanicAttack, BattleManager battleManager, GameObject attacker, BaseAoe aoe)
    {
        Dictionary<string, GameObject> targets = aoe.aoeData.TargetList;

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

                battleManager.DealDamage(entity.Value, entityDamage, attacker);
            }
        } catch (InvalidOperationException) { }

        
    }

    public static void InitializeSingleTargetAttack(EnemyMechanic mechanic, MechanicAttack mechanicAttack, BattleManager battleManager, GameObject attacker)
    {
        EnemyAI enemyAi = attacker.GetComponent<EnemyAI>();
        GameObject target = enemyAi.ChooseEntity(mechanicAttack.targetType);
        enemyAi.currTarget = target;
        enemyAi.currImmediateAttack = mechanic;

        Debug.Log("TARGET!: " + target);

        EntityController controller = attacker.GetComponent<EntityController>();
        controller.MoveTowards(target);

    }
    public static void ActivateSingleTargetAttack(MechanicAttack mechanicAttack, BattleManager battleManager, GameObject attacker, GameObject target)
    {
        PlayerSkill newSkill = (PlayerSkill)ScriptableObject.CreateInstance("PlayerSkill");
        newSkill.damageType = mechanicAttack.damageType;
        newSkill.scaler = mechanicAttack.scaler;
        newSkill.scaleMult = mechanicAttack.scaleMult;
        newSkill.baseValue = mechanicAttack.baseValue;
        newSkill.attackCount = 1;
        float entityDamage = GlobalDamageHelper.HandleActionCalculation(new ActionInformation(target, attacker, newSkill));
        battleManager.DealDamage(target, entityDamage, attacker);
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
}
