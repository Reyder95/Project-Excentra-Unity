using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public static class CustomMechanicLogicHelper
{
    private static Dictionary<string, System.Func<BattleManager, CustomLogicPassthrough, MechanicLogic>> mechDict = new Dictionary<string, System.Func<BattleManager, CustomLogicPassthrough, MechanicLogic>>()
    {
        { "reprisal", (BattleManager battleManager, CustomLogicPassthrough passthrough) => ReprisalEffect(battleManager, passthrough)},
        { "blue_acclimation", (BattleManager battleManager, CustomLogicPassthrough passthrough) => BlueAcclimationEffect(battleManager, passthrough) },
        { "red_acclimation", (BattleManager battleManager, CustomLogicPassthrough passthrough) => RedAcclimationEffect(battleManager, passthrough) },
        { "acclimation_end", (BattleManager battleManager, CustomLogicPassthrough passthrough) => AcclimationEffectEnd(battleManager, passthrough)   },
        { "acclimation", (BattleManager battleManager, CustomLogicPassthrough passthrough) => AcclimationEffectStart(battleManager, passthrough) },
        { "red_acclimation_target", (BattleManager battleManager, CustomLogicPassthrough passthrough) => RedAcclimationTarget(battleManager, passthrough) },
        { "blue_acclimation_target", (BattleManager battleManager, CustomLogicPassthrough passthrough) => BlueAcclimationTarget(battleManager, passthrough) },
        { "soul-bomb-attack", (BattleManager battleManager, CustomLogicPassthrough passthrough) => SoulBomb(battleManager, passthrough) },
        { "soul-bomb_end", (BattleManager battleManager, CustomLogicPassthrough passthrough) => SoulBombEnd(battleManager, passthrough) },
    };

    private static Dictionary<string, System.Action<EntityStats, BattleManager, EnemyMechanic>> mechTriggers = new Dictionary<string, System.Action<EntityStats, BattleManager, EnemyMechanic>>()
    {
        { "spawn_adds", (EntityStats stats, BattleManager battleManager, EnemyMechanic mechanic) => SpawnAddsTrigger(stats, battleManager, mechanic) }
    };

    public static MechanicLogic ExecuteMechanic(string mechanicKey, BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        if (mechDict.ContainsKey(mechanicKey))
            return mechDict[mechanicKey](battleManager, passthrough);

        return new MechanicLogic();
    }

    public static System.Action<EntityStats, BattleManager, EnemyMechanic> ExecuteMechanicTrigger(string mechanicKey)
    {
        if (mechTriggers.ContainsKey(mechanicKey))
            return mechTriggers[mechanicKey];

        return null;
    }

    public static MechanicLogic ReprisalEffect(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        List<GameObject> possibleChars = battleManager.GetAliveEntities();

        int counter = 0;
        while (possibleChars.Count > 0)
        {
            int randomCharIndex = Random.Range(0, possibleChars.Count);
            GameObject character = possibleChars[randomCharIndex];
            possibleChars.RemoveAt(randomCharIndex);

            EntityStats charStats = character.GetComponent<EntityStats>();

            if (counter % 2 == 0)
                charStats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"), passthrough.attacker);
            else
                charStats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"), passthrough.attacker);

            counter++;
        }

        //foreach (var character in possibleChars)
        //{
        //    EntityStats stats = character.GetComponent<EntityStats>();
        //    if (counter % 2 == 0)
        //        stats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"), passthrough.attacker);
        //    else
        //        stats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"), passthrough.attacker);
        //    counter++;
        //}

        return new MechanicLogic();
    }

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

    public static MechanicLogic BlueAcclimationEffect(BattleManager battleManager, CustomLogicPassthrough passthrough)
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

    public static MechanicLogic RedAcclimationEffect(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        List<GameObject> possibleChars = battleManager.GetAliveEntities();
        Debug.Log("Red acclimation effect");

        MechanicLogic logic = new MechanicLogic();

        // Red Acclimation should always have an AoE

        if (passthrough.aoe == null)
        {
            Debug.LogError("Red Acclimation should always have an AoE");
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

    public static MechanicLogic RedAcclimationTarget(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        List<GameObject> possibleChars = battleManager.GetAliveEntities();
        Debug.Log("Red acclimation target effect");
        MechanicLogic logic = new MechanicLogic();

        List<GameObject> targetableChars = possibleChars.Where(go => go.GetComponent<EntityStats>().effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue")) != null).ToList();

        logic.overriddenTarget = targetableChars[Random.Range(0, targetableChars.Count)];

        return logic;
    }

    public static MechanicLogic BlueAcclimationTarget(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        List<GameObject> possibleChars = battleManager.GetAliveEntities();
        Debug.Log("Blue acclimation target effect");
        MechanicLogic logic = new MechanicLogic();
        

        List<GameObject> targetableChars = possibleChars.Where(go => go.GetComponent<EntityStats>().effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_red")) != null).ToList();

        if (targetableChars.Count == 0)
            return logic;

        logic.overriddenTarget = targetableChars[Random.Range(0, targetableChars.Count)];

        return logic;
    }

    public static MechanicLogic SoulBomb(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        Debug.Log("Soul Bomb Activation");

        return new MechanicLogic();
    }

    public static MechanicLogic SoulBombEnd(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        battleManager.KillEntity(passthrough.attacker);

        return new MechanicLogic();
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
                            Debug.Log("ENEMY ALIVE!!!");
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
}
