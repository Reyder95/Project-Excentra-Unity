using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.TextCore.Text;

public class AetherialCalibrationData
{
    public EnemyMechanic mechanic;
    public int numRed = 0;
    public int numBlue = 0;
    public bool tank = false;
    public GameObject boss;
    public CustomLogicPassthrough passthrough;
    public BattleManager battleManager;

    public AetherialCalibrationData(EnemyMechanic mechanic, int numRed, int numBlue, GameObject boss, CustomLogicPassthrough passthrough, BattleManager battleManager, bool tank = false)
    {
        this.mechanic = mechanic;
        this.numRed = numRed;
        this.numBlue = numBlue;
        this.boss = boss;
        this.passthrough = passthrough;
        this.battleManager = battleManager;
        this.tank = tank;
    }
}

public static class Medica
{
    // Data for the entire mechanic
    // Stack for remaining Aetherial Calibration attacks
    // Temp data
    private static int numRed = 0;
    private static int numBlue = 0;

    private static string[] calibrationArrayNames = new string[4] {
        "Aetherial Calibration Alpha",
        "Aetherial Calibration Beta",
        "Aetherial Calibration Gamma",
        "Aetherial Calibration Delta"
    };

    private static int calibrationCounter = 0;

    public static List<AetherialCalibrationData> acData = new List<AetherialCalibrationData>();

    // MechanicAttack for Acclimation Resolve
    public static MechanicAttack AcclimationResolve()
    {
        MechanicAttack acclimationResolve = new MechanicAttack();
        acclimationResolve.attackKey = "acclimation-resolve";
        acclimationResolve.attackType = AttackType.AOE;
        acclimationResolve.aoeShape = Shape.DONUT;
        acclimationResolve.isInvisible = true;
        acclimationResolve.raidWide = true;

        return acclimationResolve;
    }

    public static MechanicAttack BittersweetsAoe(bool blue)
    {
        string attackKey = blue ? "blue-acclimation-hit" : "red-acclimation-hit";
        Color color = blue ? new Color(0f, 0f, 1f) : new Color(1f, 0f, 0f);

        MechanicAttack newAttack = new MechanicAttack();

        newAttack.attackType = AttackType.AOE;
        newAttack.attackKey = attackKey;
        newAttack.targetKey = "";
        newAttack.turnOffset = 4;
        newAttack.originIsTarget = true;
        newAttack.damageType = DamageType.DAMAGE;

        newAttack.aoeShape = Shape.CIRCLE;

        newAttack.size = 4;
        newAttack.distanceOffset = 1;
        newAttack.customColor = true;
        newAttack.aoeColor = color;

        newAttack.scaleMult = 3;
        newAttack.baseValue = 50;
        newAttack.attackCount = 1;

        return newAttack;
    }

    public static MechanicAttack AetherialCalibration22Aoe(float x, float y, bool blue)
    {
        Color color = blue ? new Color(0f, 0f, 1f) : new Color(1f, 0f, 0f);

        MechanicAttack newAttack = new MechanicAttack();
        newAttack.attackType = AttackType.AOE;
        newAttack.attackKey = blue ? "blue-acclimation-hit" : "red-acclimation-hit";
        newAttack.isSoak = true; 
        newAttack.soakDamage = 1000;
        newAttack.hasArenaPositioning = true;
        newAttack.aoePositionInformation = new MechanicAoePositionHelper();
        newAttack.aoePositionInformation.positionType = ArenaPositionType.CENTER;
        newAttack.aoePositionInformation.offset = new Vector2(x, y);
        newAttack.turnOffset = 4;
        newAttack.damageType = DamageType.DAMAGE;
        newAttack.aoeShape = Shape.CIRCLE;
        newAttack.size = 2;
        newAttack.customColor = true;
        newAttack.aoeColor = color;
        newAttack.scaleMult = 3;
        newAttack.baseValue = 50;
        newAttack.attackCount = 1;
        return newAttack;
    }

    public static MechanicAttack AetherialCalibration31Aoe(bool blue, bool isStack, GameObject target)
    {
        Color color = blue ? new Color(0f, 0f, 1f) : new Color(1f, 0f, 0f);

        MechanicAttack newAttack = new MechanicAttack();
        newAttack.attackType = AttackType.AOE;
        newAttack.attackKey = blue ? "blue-acclimation-hit" : "red-acclimation-hit";
        newAttack.directTarget = target;
        newAttack.turnOffset = 4;
        newAttack.damageType = DamageType.DAMAGE;
        newAttack.originIsTarget = true;
        newAttack.aoeShape = Shape.CIRCLE;
        newAttack.size = 15;
        newAttack.isStack = isStack;
        newAttack.customColor = true;
        newAttack.aoeColor = color;
        newAttack.scaleMult = isStack ? 18f : 5;
        newAttack.baseValue = isStack ? 150 : 50;
        newAttack.scaler = Scaler.ATTACK;
        newAttack.attackCount = 1;

        return newAttack;
    }

    public static MechanicAttack AetherialCalibration31TankAoe(bool blue)
    {
        Color color = blue ? new Color(0f, 0f, 1f) : new Color(1f, 0f, 0f);

        MechanicAttack newAttack = new MechanicAttack();
        newAttack.attackType = AttackType.AOE;
        newAttack.attackKey = blue ? "blue-acclimation-hit" : "red-acclimation-hit";
        newAttack.turnOffset = 4;
        newAttack.damageType = DamageType.DAMAGE;
        newAttack.targetType = EntityTargetType.FIRST_AGGRESSION;
        newAttack.originIsTarget = false;
        newAttack.endpointIsTarget = true;
        newAttack.originIsSelf = true;
        newAttack.aoeShape = Shape.CONE;
        newAttack.size = 30;
        newAttack.distanceOffset = 10;
        newAttack.customColor = true;
        newAttack.aoeColor = color;
        newAttack.scaleMult = 3.0f;
        newAttack.baseValue = 300;
        newAttack.scaler = Scaler.ATTACK;
        newAttack.attackCount = 1;

        return newAttack;
    }

    public static MechanicAttack AetherialCalibration31TankStackAoe(bool blue, GameObject target)
    {
        Color color = blue ? new Color(0f, 0f, 1f) : new Color(1f, 0f, 0f);

        MechanicAttack newAttack = new MechanicAttack();
        newAttack.attackType = AttackType.AOE;
        newAttack.attackKey = blue ? "blue-acclimation-hit" : "red-acclimation-hit";
        newAttack.directTarget = target;
        newAttack.turnOffset = 4;
        newAttack.damageType = DamageType.DAMAGE;
        newAttack.originIsTarget = true;
        newAttack.aoeShape = Shape.CIRCLE;
        newAttack.size = 7;
        newAttack.isStack = true;
        newAttack.customColor = true;
        newAttack.aoeColor = color;
        newAttack.scaleMult = 18f;
        newAttack.baseValue = 150;
        newAttack.scaler = Scaler.ATTACK;
        newAttack.attackCount = 1;

        return newAttack;
    }

    public static MechanicLogic ReprisalEffectPhaseOne(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {

        numRed = Random.Range(0, 2);

        if (numRed == 1)
        {
            numRed = 3;
            numBlue = 1;
        }
        else
        {
            numRed = 1;
            numBlue = 3;
        }

        acData.Add(new AetherialCalibrationData(ExcentraDatabase.TryGetEnemyMechanics("aetherial-calibration-22"), 2, 2, passthrough.attacker, passthrough, battleManager));
        acData.Add(new AetherialCalibrationData(ExcentraDatabase.TryGetEnemyMechanics("aetherial-calibration-31"), numRed, numBlue, passthrough.attacker, passthrough, battleManager));

        acData = acData.OrderBy(_ => Random.value).ToList();

        acData.Insert(Random.Range(1, acData.Count), new AetherialCalibrationData(ExcentraDatabase.TryGetEnemyMechanics("aetherial-calibration-31-tank"), 4 - numRed, 4 - numBlue, passthrough.attacker, passthrough, battleManager, true));

        ExcentraGame.Instance.triggers.ActivateTrigger(battleManager, passthrough.mechanic, "aetherial-calibration", acData[0]);

        return new MechanicLogic();
    }

    public static MechanicLogic ReprisalEffectPhaseTwo(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        List<GameObject> possibleChars = battleManager.GetAliveEntities();

        int counter = 0;
        while (possibleChars.Count > 0)
        {
            int randomCharIndex = Random.Range(0, possibleChars.Count);
            GameObject character = possibleChars[randomCharIndex];
            possibleChars.RemoveAt(randomCharIndex);

            EntityStats charStats = character.GetComponent<EntityStats>();
            EntityController charController = character.GetComponent<EntityController>();

            int randomAcclimation = Random.Range(0, 2);

            if (randomAcclimation == 0)
            {
                charStats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"), passthrough.attacker);
            }
            else
            {
                charStats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"), passthrough.attacker);
            }


            counter++;
        }

        GameObject particleLineSpawned = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("circle-burst"), battleManager.arena.GetCenter(), Quaternion.identity);
        particleLineSpawned.GetComponent<ParticleSystem>().Play();

        battleManager.EndTurn();

        return new MechanicLogic();
    }

    public static void Dissipation(BattleManager battleManager, EnemyMechanic mechanic)
    {
        AetherialCalibrationData currAcData = acData[0];

        if (currAcData.tank || currAcData.numRed == 4 || currAcData.numBlue == 4)
        {
            mechanic.mechanicAttacks[0].isSoak = false;
            mechanic.mechanicAttacks[1].isSoak = false;
        }
    }

    public static void SpiritBlast(BattleManager battleManager, EnemyMechanic mechanic)
    {
        MechanicAttack acclimationResolve = AcclimationResolve();

        mechanic.priorityIndex = new MechanicPriorityIndex[2];
        mechanic.priorityIndex[0] = new MechanicPriorityIndex();
        mechanic.priorityIndex[0].index = new int[1];
        mechanic.priorityIndex[0].index[0] = 0;
        mechanic.priorityIndex[0].turnOffset = 3;

        mechanic.mechanicAttacks.Add(acclimationResolve);

        mechanic.priorityIndex[1] = new MechanicPriorityIndex();
        mechanic.priorityIndex[1].index = new int[1];
        mechanic.priorityIndex[1].index[0] = 1;
        mechanic.priorityIndex[1].turnOffset = 4;
    }

    public static void AetherialCalibration31Tank(BattleManager battleManager, EnemyMechanic mechanic)
    {
        ExcentraGame.Instance.triggers.ActivateTrigger(battleManager, mechanic, "aetherial-calibration");
        List<GameObject> possibleChars = battleManager.GetAliveEntities();

        MechanicAttack tankAoe = AetherialCalibration31TankAoe(numRed == 1 ? true : false);
        mechanic.mechanicAttacks.Add(tankAoe);

        GameObject target = null;
        bool isBlue = false;

        foreach (GameObject character in possibleChars)
        {
            EntityStats stats = character.GetComponent<EntityStats>();

            if (numBlue == 1)
            {
                if (stats.effectHandler.GetEffectByKey("spirit_acclimation_red") != null)
                {
                    target = character;
                    isBlue = true;
                    break;
                }
            }
            else
            {
                if (stats.effectHandler.GetEffectByKey("spirit_acclimation_blue") != null)
                {
                    target = character;
                    isBlue = false;
                    break;
                }
            }
        }

        if (target == null)
        {
            target = possibleChars[0];
        }

        MechanicAttack stackAttack = AetherialCalibration31TankStackAoe(isBlue, target);
        mechanic.mechanicAttacks.Add(stackAttack);
    }

    public static void LonelyGhost(BattleManager battleManager, EnemyMechanic mechanic)
    {
        mechanic.priorityIndex = new MechanicPriorityIndex[2];
        mechanic.priorityIndex[0] = new MechanicPriorityIndex();
        mechanic.priorityIndex[0].index = new int[1];
        mechanic.priorityIndex[0].index[0] = 0;
        mechanic.priorityIndex[0].turnOffset = 5;

        mechanic.mechanicAttacks.Add(AcclimationResolve());

        // Get all player characters
        List<GameObject> playerCharacters = battleManager.GetAliveEntities();

        foreach (GameObject playerCharacter in playerCharacters)
        {
            EntityStats stats = playerCharacter.GetComponent<EntityStats>();
            // Check if the character has the "spirit_acclimation_blue" effect
            StatusBattle blueEffect = stats.effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"));
            if (blueEffect != null)
            {
                MechanicAttack newAttack = new MechanicAttack();

                newAttack.attackType = AttackType.AOE;
                newAttack.attackKey = "red-acclimation-hit";
                newAttack.targetKey = "";
                newAttack.directTarget = playerCharacter;
                newAttack.turnOffset = 3;
                newAttack.originIsSelf = true;
                newAttack.endpointIsTarget = true;
                newAttack.damageType = DamageType.DAMAGE;

                newAttack.aoeShape = Shape.CONE;

                newAttack.size = 3;
                newAttack.distanceOffset = 1;
                newAttack.customColor = true;
                newAttack.aoeColor = new Color(1f, 0f, 0f);

                newAttack.scaleMult = 3;
                newAttack.baseValue = 50;
                newAttack.attackCount = 1;

                mechanic.mechanicAttacks.Add(newAttack);
            }

            // Check if the character has the "spirit_acclimation_red" effect
            StatusBattle redEffect = stats.effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"));
            if (redEffect != null)
            {
                MechanicAttack newAttack = new MechanicAttack();

                newAttack.attackType = AttackType.AOE;
                newAttack.attackKey = "blue-acclimation-hit";
                newAttack.targetKey = "";
                newAttack.directTarget = playerCharacter;
                newAttack.turnOffset = 3;
                newAttack.originIsSelf = true;
                newAttack.endpointIsTarget = true;
                newAttack.damageType = DamageType.DAMAGE;

                newAttack.aoeShape = Shape.CONE;

                newAttack.size = 3;
                newAttack.distanceOffset = 1;
                newAttack.customColor = true;
                newAttack.aoeColor = new Color(0f, 0f, 1f);

                newAttack.scaleMult = 3;
                newAttack.baseValue = 50;
                newAttack.attackCount = 1;

                mechanic.mechanicAttacks.Add(newAttack);
            }
        }

        mechanic.priorityIndex[1] = new MechanicPriorityIndex();
        mechanic.priorityIndex[1].index = new int[mechanic.mechanicAttacks.Count - 1];
        mechanic.priorityIndex[1].turnOffset = 3;

        for (int i = 1; i < mechanic.mechanicAttacks.Count; i++)
        {
            mechanic.priorityIndex[1].index[i - 1] = i;
        }
    }

    public static MechanicLogic AcclimationResolve(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        Debug.Log("??");
        List<GameObject> possibleTargets = battleManager.GetAliveEntities();
        EntityStats stats = passthrough.target.GetComponent<EntityStats>();
        EntityController controller = passthrough.target.GetComponent<EntityController>();

        StatusBattle status = stats.effectHandler.GetEffectByKey("spirit_acclimation_blue");

        ExcentraGame.Instance.damageNumberHandlerScript.SpawnPopupText(controller.iconHeader, null, "Spirit Acclimation Down!", false);

        if (status != null)
        {
            Debug.Log(status.turnsRemaining);
        }

        stats.ReduceStatusTurns(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"));
        stats.ReduceStatusTurns(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"));

        passthrough.attacker.GetComponent<EntityController>().ModifyOpacity(1f);

        return new MechanicLogic();
    }

    public static void BittersweetSpirits(BattleManager battleManager, EnemyMechanic mechanic)
    {
        mechanic.priorityIndex = new MechanicPriorityIndex[2];
        mechanic.priorityIndex[0] = new MechanicPriorityIndex();
        mechanic.priorityIndex[0].index = new int[1];
        mechanic.priorityIndex[0].index[0] = 0;
        mechanic.priorityIndex[0].turnOffset = 5;

        mechanic.mechanicAttacks.Add(AcclimationResolve());

        List<GameObject> possibleTargets = battleManager.GetAliveEntities();

        GameObject target1 = null;
        GameObject target2 = null;

        MechanicAttack aoe1 = null;
        MechanicAttack aoe2 = null;

        possibleTargets = possibleTargets.OrderBy(_ => Random.value).ToList();

        // Find the first blue target and set them as target 1.
        foreach (GameObject target in possibleTargets) 
        {
            EntityStats stats = target.GetComponent<EntityStats>();

            if (stats.effectHandler.GetEffectByKey("spirit_acclimation_blue") != null)
            {
                target1 = target;
                aoe1 = BittersweetsAoe(false);
                break;
            }
        }

        // Find the first red target and set them as target 2.
        foreach (GameObject target in possibleTargets)
        {
            EntityStats stats = target.GetComponent<EntityStats>();
            if (stats.effectHandler.GetEffectByKey("spirit_acclimation_red") != null)
            {
                target2 = target;
                aoe2 = BittersweetsAoe(true);
                break;
            }
        }

        if (target1 == null)
        {
            target1 = possibleTargets[possibleTargets.Count - 1];
            aoe1 = BittersweetsAoe(true);
        }
        else if (target2 == null)
        {
            target2 = possibleTargets[possibleTargets.Count - 1];
            aoe2 = BittersweetsAoe(false);
        }

        if (aoe1 == null || aoe2 == null)
            return;

        aoe1.directTarget = target1;
        aoe2.directTarget = target2;

        mechanic.mechanicAttacks.Add(aoe1);
        mechanic.mechanicAttacks.Add(aoe2);

        mechanic.priorityIndex[1] = new MechanicPriorityIndex();
        mechanic.priorityIndex[1].index = new int[mechanic.mechanicAttacks.Count - 1];
        mechanic.priorityIndex[1].turnOffset = 4;

        for (int i = 1; i < mechanic.mechanicAttacks.Count; i++)
        {
            mechanic.priorityIndex[1].index[i - 1] = i;
        }

    }

    public static void AetherialCalibration22(BattleManager battleManager, EnemyMechanic mechanic)
    {
        ExcentraGame.Instance.triggers.ActivateTrigger(battleManager, mechanic, "aetherial-calibration");
        EnemyAI medica = battleManager.boss.GetComponent<EnemyAI>();
        medica.currPhase.InsertMechanicAt(ExcentraDatabase.TryGetEnemyMechanics("aetherial-calibration-22-p2"), 0);

        float radius = 2.5f;
        int blueCount = 0;
        int redCount = 0;
        bool[] blueArray = new bool[4];

        // Randomizing the blue and red values for the 4 aoes
        for (int i = 0; i < 4; i++)
        {
            bool blueValue = Random.Range(0, 2) == 0 ? true : false;

            if (blueCount < 2 && redCount < 2)
            {
                blueArray[i] = blueValue;
                
                if (blueValue)
                {
                    blueCount++;
                }
                else
                {
                    redCount++;
                }
            }
            else if (blueCount >= 2)
            {
                blueArray[i] = false;

                redCount++;
            }
            else if (redCount >= 2)
            {
                blueArray[i] = true;

                blueCount++;
            }
        }

        MechanicAttack topAoe = AetherialCalibration22Aoe(0f, radius, blueArray[0]);
        MechanicAttack bottomAoe = AetherialCalibration22Aoe(0f, -radius, blueArray[1]);
        MechanicAttack leftAoe = AetherialCalibration22Aoe(-radius, 0f, blueArray[2]);
        MechanicAttack rightAoe = AetherialCalibration22Aoe(radius, 0f, blueArray[3]);

        mechanic.mechanicAttacks.Add(topAoe);
        mechanic.mechanicAttacks.Add(bottomAoe);
        mechanic.mechanicAttacks.Add(leftAoe);
        mechanic.mechanicAttacks.Add(rightAoe);
    }

    public static void AetherialCalibration22p2(BattleManager battleManager, EnemyMechanic mechanic)
    {
        float radius = 2.5f;
        float diagonal = radius / Mathf.Sqrt(2);
        int blueCount = 0;
        int redCount = 0;
        bool[] blueArray = new bool[4];

        for (int i = 0; i < 4; i++)
        {
            bool blueValue = Random.Range(0, 2) == 0 ? true : false;

            if (blueCount < 2 && redCount < 2)
            {
                blueArray[i] = blueValue;

                if (blueValue)
                {
                    blueCount++;
                }
                else
                {
                    redCount++;
                }
            }
            else if (blueCount >= 2)
            {
                blueArray[i] = false;

                redCount++;
            }
            else if (redCount >= 2)
            {
                blueArray[i] = true;

                blueCount++;
            }
        }

        MechanicAttack topAoe = AetherialCalibration22Aoe(diagonal, diagonal, blueArray[0]);
        MechanicAttack bottomAoe = AetherialCalibration22Aoe(-diagonal, diagonal, blueArray[1]);
        MechanicAttack leftAoe = AetherialCalibration22Aoe(-diagonal, -diagonal, blueArray[2]);
        MechanicAttack rightAoe = AetherialCalibration22Aoe(diagonal, -diagonal, blueArray[3]);

        mechanic.mechanicAttacks.Add(topAoe);
        mechanic.mechanicAttacks.Add(bottomAoe);
        mechanic.mechanicAttacks.Add(leftAoe);
        mechanic.mechanicAttacks.Add(rightAoe);
    }

    public static void AetherialCalibration31(BattleManager battleManager, EnemyMechanic mechanic)
    {
        ExcentraGame.Instance.triggers.ActivateTrigger(battleManager, mechanic, "aetherial-calibration");
        List<GameObject> possibleTargets = battleManager.GetAliveEntities();

        possibleTargets = possibleTargets.OrderBy(_ => Random.value).ToList();

        GameObject redTarget = possibleTargets[possibleTargets.Count - 1];
        GameObject blueTarget = possibleTargets[possibleTargets.Count - 1];

        MechanicAttack redAttack = null;
        MechanicAttack blueAttack = null;



        // Find red target
        foreach (GameObject target in possibleTargets)
        {
            EntityStats stats = target.GetComponent<EntityStats>();
            if (stats.effectHandler.GetEffectByKey("spirit_acclimation_blue") != null)
            {
                redTarget = target;
                break;
            }
        }

        // Find blue target
        foreach (GameObject target in possibleTargets)
        {
            EntityStats stats = target.GetComponent<EntityStats>();
            if (stats.effectHandler.GetEffectByKey("spirit_acclimation_red") != null)
            {
                blueTarget = target;
                break;
            }
        }

        bool isStack;
        bool isBlue;
        if (numRed == 1)
        {
            isStack = true;
            isBlue = false;
            
        }
        else
        {
            isStack = false;
            isBlue = false;
        }

        redAttack = AetherialCalibration31Aoe(isBlue, isStack, redTarget);

        if (numBlue == 1)
        {
            isStack = true;
            isBlue = true;
        }
        else
        {
            isStack = false;
            isBlue = true;
        }

        blueAttack = AetherialCalibration31Aoe(isBlue, isStack, blueTarget);

        mechanic.mechanicAttacks.Add(redAttack);
        mechanic.mechanicAttacks.Add(blueAttack);
    }

    public static MechanicLogic AetherialCalibration22End(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        battleManager.turnManager.CalculateIndividualDelay(battleManager.turnManager.GetTurnEntityData(passthrough.attacker), 0f);


        return new MechanicLogic();
    }

    public static MechanicLogic AetherialCalibration22p2End(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        EntityStats stats = passthrough.attacker.GetComponent<EntityStats>();
        battleManager.turnManager.CalculateIndividualDelay(battleManager.turnManager.GetTurnEntityData(passthrough.attacker));
        stats.active = true;


        ExcentraGame.Instance.triggers.ActivateTrigger(battleManager, passthrough.mechanic, "aetherial-calibration", acData[0]);

        return new MechanicLogic();
    }

    public static MechanicLogic AetherialCalibration31End(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        ExcentraGame.Instance.triggers.ActivateTrigger(battleManager, passthrough.mechanic, "aetherial-calibration", acData[0]);

        return new MechanicLogic();
    }

    public static MechanicLogic AetherialCalibration31TankEnd(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        ExcentraGame.Instance.triggers.ActivateTrigger(battleManager, passthrough.mechanic, "aetherial-calibration", acData[0]);

        return new MechanicLogic();
    }

    public static void AetherialCalibrationBase(BattleManager battleManager, EnemyMechanic mechanic)
    {
        EnemyMechanic currentMechanic = acData[0].mechanic;
        mechanic.mechanicName = calibrationArrayNames[calibrationCounter];
        mechanic.containsTrigger = currentMechanic.containsTrigger;
        mechanic.mechanicKey = currentMechanic.mechanicKey;
        mechanic.mechanicStyle = currentMechanic.mechanicStyle;
        mechanic.dontSkipTurn = currentMechanic.dontSkipTurn;
        mechanic.active = currentMechanic.active;
        mechanic.untargetable = currentMechanic.untargetable;
        mechanic.targetScript = currentMechanic.targetScript;
        mechanic.activeScript = currentMechanic.activeScript;
        mechanic.goNext = currentMechanic.goNext;
        mechanic.containsMovement = currentMechanic.containsMovement;
        mechanic.movementType = currentMechanic.movementType;
        mechanic.animationTrigger = currentMechanic.animationTrigger;
        mechanic.priorityIndex = currentMechanic.priorityIndex;
        mechanic.customScript = currentMechanic.customScript;
        mechanic.customScriptKey = currentMechanic.customScriptKey;
        mechanic.turnCooldown = currentMechanic.turnCooldown;

        calibrationCounter++;

        numRed = acData[0].numRed;
        numBlue = acData[0].numBlue;

        acData.RemoveAt(0);

        //ExcentraGame.Instance.triggers.ActivateTrigger(battleManager, mechanic, "aetherial-calibration");

        //if (mechanic.mechanicKey == "aetherial-calibration-22")
        //{
        //    AetherialCalibration22(battleManager, mechanic);
        //}
        //else if (mechanic.mechanicKey == "aetherial-calibration-31")
        //{
        //    AetherialCalibration31(battleManager, mechanic);
        //}
        //else if (mechanic.mechanicKey == "aetherial-calibration-31-tank")
        //{
        //    AetherialCalibration31Tank(battleManager, mechanic);
        //}
    }

        // -- OLD UNUSED MECHANICS HERE

    public static MechanicLogic RedAcclimationTarget(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        List<GameObject> possibleChars = battleManager.GetAliveEntities();
        MechanicLogic logic = new MechanicLogic();

        

        List<GameObject> targetableChars = possibleChars.Where(go => go.GetComponent<EntityStats>().effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue")) != null).ToList();

        if (targetableChars.Count == 0)
            return logic;

        logic.overriddenTarget = targetableChars[Random.Range(0, targetableChars.Count)];

        foreach (var character in battleManager.turnManager.turnOrder)
        {
            if (character.isEntity)
            {
                if (character.GetEntity().entityTurn.GetComponent<EntityStats>().effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue")) != null)
                {
                    logic.overrideDelay = true;
                    logic.overriddenDelay = battleManager.turnManager.ReturnDelayNeededForCharacter(character.GetEntity().entityTurn);
                    break;
                }
            }
        }

        return logic;
    }

    public static MechanicLogic BlueAcclimationTarget(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        List<GameObject> possibleChars = battleManager.GetAliveEntities();
        MechanicLogic logic = new MechanicLogic();

        List<GameObject> targetableChars = possibleChars.Where(go => go.GetComponent<EntityStats>().effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_red")) != null).ToList();

        if (targetableChars.Count == 0)
            return logic;

        logic.overriddenTarget = targetableChars[Random.Range(0, targetableChars.Count)];

        foreach (var character in battleManager.turnManager.turnOrder)
        {
            if (character.isEntity)
            {
                if (character.GetEntity().entityTurn.GetComponent<EntityStats>().effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_red")) != null)
                {
                    logic.overrideDelay = true;
                    logic.overriddenDelay = battleManager.turnManager.ReturnDelayNeededForCharacter(character.GetEntity().entityTurn);
                    break;
                }
            }
        }

        return logic;
    }

    public static MechanicLogic AcclimationEffectEnd(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        //List<GameObject> possibleTargets = battleManager.GetAliveEntities();

        //Debug.Log("TEST!");
        //foreach (var character in possibleTargets)
        //{
        //    EntityStats stats = character.GetComponent<EntityStats>();
        //    StatusBattle status = stats.effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"));

        //    if (status != null)
        //    {

        //        if (status.turnsRemaining == 0)
        //        {
        //            stats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"));

        //            PlayerSkill newSkill = (PlayerSkill)ScriptableObject.CreateInstance("PlayerSkill");
        //            newSkill.damageType = DamageType.DAMAGE;
        //            newSkill.scaler = Scaler.ATTACK;
        //            newSkill.scaleMult = 3.5f;
        //            newSkill.baseValue = 150;
        //            newSkill.attackCount = 1;

        //            float entityDamage = GlobalDamageHelper.HandleActionCalculation(new ActionInformation(character, passthrough.attacker, newSkill, null));

        //            battleManager.DealDamage(character, entityDamage, passthrough.attacker);

        //            stats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"), passthrough.attacker);
        //        }
        //    }
        //    else
        //    {


        //        status = stats.effectHandler.GetEffect(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"));

        //        if (status.turnsRemaining == 0)
        //        {
        //            stats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_red"));

        //            PlayerSkill newSkill = (PlayerSkill)ScriptableObject.CreateInstance("PlayerSkill");
        //            newSkill.damageType = DamageType.DAMAGE;
        //            newSkill.scaler = Scaler.ATTACK;
        //            newSkill.scaleMult = 3.5f;
        //            newSkill.baseValue = 150;
        //            newSkill.attackCount = 1;

        //            float entityDamage = GlobalDamageHelper.HandleActionCalculation(new ActionInformation(character, passthrough.attacker, newSkill));

        //            battleManager.DealDamage(character, entityDamage, passthrough.attacker);

        //            stats.ModifyStatus(ExcentraDatabase.TryGetStatus("spirit_acclimation_blue"), passthrough.attacker);
        //        }
        //    }
        //}

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
        Debug.Log("????");
        List<GameObject> possibleChars = battleManager.GetAliveEntities();

        MechanicLogic logic = new MechanicLogic();

        EntityController controller = passthrough.target.GetComponent<EntityController>();

        ExcentraGame.Instance.damageNumberHandlerScript.SpawnPopupText(controller.iconHeader, null, "Acclimation Swapped!", false);

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
            Debug.Log("Red");
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

        MechanicLogic logic = new MechanicLogic();

        EntityController controller = passthrough.target.GetComponent<EntityController>();
        ExcentraGame.Instance.damageNumberHandlerScript.SpawnPopupText(controller.iconHeader, null, "Acclimation Swapped!", false);

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
            Debug.Log("Blue");
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

            if (hit.collider == null)
            {
                Debug.Log("Add for position: " + startPosition + " may attack " + character);
                lineOfSight.Add(character);
            }
        }

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
        GameObject owner = stats.addOwner;
        EntityController controller = stats.gameObject.GetComponent<EntityController>();
        controller.markForDespawn = true;
        controller.ModifyOpacity(0f);

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
        EntityStats ownerStats = owner.GetComponent<EntityStats>();
        ownerStats.active = true;
        ownerStats.targetable = true;

        //battleManager.turnManager.CalculateIndividualDelay(ownerStats.gameObject);

        EnemyAI enemyAi = owner.GetComponent<EnemyAI>();
        enemyAi.ChangePhase(true);
        battleManager.turnManager.CalculateIndividualDelay(battleManager.turnManager.GetTurnEntityData(owner), battleManager.turnManager.ReturnDelayNeededForTurn(0));
    }

    //public static MechanicLogic SweetBlissStart(BattleManager battleManager, CustomLogicPassthrough passthrough)
    //{
    //    MechanicLogic logic = AcclimationEffectStart(battleManager, passthrough);
    //    GameObject attacker = passthrough.attacker;

    //    Debug.Log("ATTACKER START IS " + attacker);

    //    // Get the Renderer component of the attacker
    //    attacker.GetComponent<EntityController>().ModifyOpacity(0f);

    //    return logic;

    //}

    public static MechanicLogic SweetBlissEnd(BattleManager battleManager, CustomLogicPassthrough passthrough)
    {
        return AcclimationEffectEnd(battleManager, passthrough);
    }
}
