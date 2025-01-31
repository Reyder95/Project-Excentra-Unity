using System.Collections.Generic;
using UnityEngine;

public static class CustomMechanicLogicHelper
{
    private static Dictionary<string, System.Action<BattleManager, BaseAoe, GameObject>> mechDict = new Dictionary<string, System.Action<BattleManager, BaseAoe, GameObject>>()
    {
        { "reprisal", (BattleManager battleManager, BaseAoe aoe, GameObject boss) => ReprisalEffect(battleManager, aoe, boss)}
    };

    public static void ExecuteMechanic(string mechanicKey, BattleManager battleManager, BaseAoe aoe, GameObject boss)
    {
        Debug.Log("Executing mechanic: " + mechanicKey);
        if (mechDict.ContainsKey(mechanicKey))
            mechDict[mechanicKey](battleManager, aoe, boss);
    }

    public static void ReprisalEffect(BattleManager battleManager, BaseAoe aoe, GameObject boss)
    {
        List<GameObject> possibleChars = battleManager.GetAliveEntities();

        Debug.Log("Reprisal effect");

        int counter = 0;

        foreach (var character in possibleChars)
        {
            EntityStats stats = character.GetComponent<EntityStats>();
            if (counter % 2 == 0)
                stats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"), boss);
            else
                stats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"), boss);
            counter++;
        }
    }
}
