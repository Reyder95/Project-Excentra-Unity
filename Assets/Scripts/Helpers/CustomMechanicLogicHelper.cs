using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public static class CustomMechanicLogicHelper
{
    private static Dictionary<string, System.Func<BattleManager, CustomLogicPassthrough, MechanicLogic>> mechDict = new Dictionary<string, System.Func<BattleManager, CustomLogicPassthrough, MechanicLogic>>()
    {
        { "reprisal", (BattleManager battleManager, CustomLogicPassthrough passthrough) => ReprisalEffect(battleManager, passthrough)},
        { "blue_acclimation", (BattleManager battleManager, CustomLogicPassthrough passthrough) => BlueAcclimationEffect(battleManager, passthrough) },
        { "red_acclimation", (BattleManager battleManager, CustomLogicPassthrough passthrough) => RedAcclimationEffect(battleManager, passthrough) },
        { "acclimation_end", (BattleManager battleManager, CustomLogicPassthrough passthrough) => Medica.AcclimationEffectEnd(battleManager, passthrough)   },
        { "acclimation", (BattleManager battleManager, CustomLogicPassthrough passthrough) => Medica.AcclimationEffectStart(battleManager, passthrough) },
        { "sweet_bliss", (BattleManager battleManager, CustomLogicPassthrough passthrough) => Medica.SweetBlissStart(battleManager, passthrough) },
        { "sweet_bliss_end", (BattleManager battleManager, CustomLogicPassthrough passthrough) => Medica.SweetBlissEnd(battleManager, passthrough) },
        { "red_acclimation_target", (BattleManager battleManager, CustomLogicPassthrough passthrough) => RedAcclimationTarget(battleManager, passthrough) },
        { "blue_acclimation_target", (BattleManager battleManager, CustomLogicPassthrough passthrough) => BlueAcclimationTarget(battleManager, passthrough) },
        { "soul-bomb-attack", (BattleManager battleManager, CustomLogicPassthrough passthrough) => SoulBomb(battleManager, passthrough) },
        { "soul-bomb_end", (BattleManager battleManager, CustomLogicPassthrough passthrough) => SoulBombEnd(battleManager, passthrough) },
        { "soul-bomb_target", (BattleManager battleManager, CustomLogicPassthrough passthrough) => SoulBombTarget(battleManager, passthrough) },
        { "lonely-ghost-red-target", (BattleManager battleManager, CustomLogicPassthrough passthrough) => Medica.LonelyGhostRedTarget(battleManager, passthrough) },
        { "lonely-ghost-blue-target", (BattleManager battleManager, CustomLogicPassthrough passthrough) => Medica.LonelyGhostBlueTarget(battleManager, passthrough) },
        { "red-acclimation-hit", (BattleManager battleManager, CustomLogicPassthrough passthrough) => Medica.RedAcclimationHit(battleManager, passthrough) },
        { "blue-acclimation-hit", (BattleManager battleManager, CustomLogicPassthrough passthrough) => Medica.BlueAcclimationHit(battleManager, passthrough) },
        { "adds-target", (BattleManager battleManager, CustomLogicPassthrough passthrough) => Medica.AddTarget(battleManager, passthrough) },
    
    };

    private static Dictionary<string, System.Action<EntityStats, BattleManager, EnemyMechanic>> mechTriggers = new Dictionary<string, System.Action<EntityStats, BattleManager, EnemyMechanic>>()
    {
        { "spawn_adds", (EntityStats stats, BattleManager battleManager, EnemyMechanic mechanic) => Medica.SpawnAddsTrigger(stats, battleManager, mechanic) },
        { "spawn-soul-attack", (EntityStats stats, BattleManager battleManager, EnemyMechanic mechanic) => SpawnSoulTrigger(stats, battleManager, mechanic) },
    };

    private static Dictionary<string, System.Func<BattleManager, float>> mechDelay = new Dictionary<string, System.Func<BattleManager, float>>()
    {
        { "soul-bomb-attack", (BattleManager battleManager) => SoulBombDelay(battleManager) },
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

    public static float ExecuteMechanicDelay(string mechanicKey, BattleManager battleManager)
    {
        if (mechDelay.ContainsKey(mechanicKey))
            return mechDelay[mechanicKey](battleManager);
        return -1f;
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

        if (targetableChars.Count == 0)
            return logic;

        Debug.Log("Red Target " + targetableChars.Count);

        logic.overriddenTarget = targetableChars[Random.Range(0, targetableChars.Count)];

        foreach (var character in battleManager.turnManager.turnOrder)
        {
            if (character.isEntity)
            {
                if (character.GetEntity().GetComponent<EntityStats>().effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue")) != null)
                {
                    logic.overrideDelay = true;
                    logic.overriddenDelay = battleManager.turnManager.ReturnDelayNeededForCharacter(character.GetEntity());
                    break;
                }
            }
        }

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

        foreach (var character in battleManager.turnManager.turnOrder)
        {
            if (character.isEntity)
            {
                if (character.GetEntity().GetComponent<EntityStats>().effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_red")) != null)
                {
                    logic.overrideDelay = true;
                    logic.overriddenDelay = battleManager.turnManager.ReturnDelayNeededForCharacter(character.GetEntity());
                    Debug.Log("HELLO!!");
                    break;
                }
            }
        }

        return logic;
    }

    public static MechanicLogic SoulBomb(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        Debug.Log("Soul Bomb Activation");

        return new MechanicLogic();
    }

    public static MechanicLogic SoulBombTarget(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        var possibleChars = battleManager.playerCharacters.Where(go => go.GetComponent<EntityStats>().entityName != "Rioka" && go.GetComponent<EntityStats>().currentHP > 0).ToList();
        int randomCharIndex = Random.Range(0, possibleChars.Count);

        MechanicLogic logic = new MechanicLogic();

        if (possibleChars.Count == 0)
            return logic;
        logic.overriddenTarget = possibleChars[randomCharIndex];

        return logic;
    }

    public static MechanicLogic SoulBombEnd(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {

        battleManager.KillEntity(passthrough.attacker);

        return new MechanicLogic();
    }

    public static float SoulBombDelay(BattleManager battleManager)
    {
        TurnManager turnManager = battleManager.turnManager;
        GameObject tank = battleManager.playerCharacters.Where(go => go.GetComponent<EntityStats>().entityName == "Rioka").FirstOrDefault();

        return turnManager.ReturnDelayNeededForCharacter(tank);
    }

    public static void SpawnSoulTrigger(EntityStats stats, BattleManager battleManager, EnemyMechanic mechanic)
    {
        Debug.Log("ENDING MECH");
        GameObject owner = stats.addOwner;
        battleManager.EndMechanic(mechanic, stats.addOwner);

        owner.GetComponent<EntityStats>().active = true;
        battleManager.turnManager.CalculateIndividualDelay(owner.gameObject);
    }
}
