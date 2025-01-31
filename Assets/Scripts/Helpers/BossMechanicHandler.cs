using System;
using System.Collections.Generic;
using UnityEngine;

public static class BossMechanicHandler
{
    public static void InitializeMechanic(EnemyMechanic mechanic, BattleManager battleManager, GameObject attacker)
    {
        if (mechanic.mechanicStyle == MechanicStyle.IMMEDIATE)
        {
            return;
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
                                delay = Mathf.Max(InitializeAOEAttack(attack, battleManager, attacker, entity), delay);
                            }
                        }
                        else
                            delay = Mathf.Max(InitializeAOEAttack(attack, battleManager, attacker), delay);
                        break;
                    case AttackType.SINGLE_TARGET:
                        break;
                    case AttackType.TETHER:
                        break;

                }
            }
            EntityStats stats = attacker.GetComponent<EntityStats>();
            stats.nextStaticDelay = delay + 1;
        }
    }

    public static float InitializeAOEAttack(MechanicAttack mechanicAttack, BattleManager battleManager, GameObject attacker, GameObject target = null)
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
        GameObject aoe = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("cone"), new Vector2(1000, 1000), Quaternion.identity);
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

        aoeInfo.InitializeEnemyAoe(attacker, mechanicAttack, info);
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
                battleManager.DealDamage(entity.Value, entityDamage, attacker);
            }
        } catch (InvalidOperationException) { }

        
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
