using System.Collections.Generic;
using System.Linq;
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
}
