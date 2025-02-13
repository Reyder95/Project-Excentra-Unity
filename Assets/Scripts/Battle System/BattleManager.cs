// BattleManager.cs

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.UI.Image;

// Class that runs the battle, from turn processes, to damage being dealt, to handling statuses, killing off entities, and declaring a winning side.
// One of the most important classes at this point
public class BattleManager
{
    private readonly System.Func<GameObject, Vector2, GameObject> _instantiateFunction; // Allows instantiation outside of MonoBehaviour, but I'd recc to just use (UnityEngine.GameObject.Instantiate())

    public List<GameObject> playerCharacters = new List<GameObject>(); // All of the player character's Game Objects by reference
    public List<GameObject> enemyList = new List<GameObject>();
    public Dictionary<string, int> nameCounts = new Dictionary<string, int>();
    private GameObject boss = new GameObject(); // The boss (will need modification for multiple enemies)

    public TurnManager turnManager = new TurnManager(); // The turn manager handles everything regarding the turn order, delays, etc. The battle manager uses the turnManager to facilitate the turns
    public BattleVariables battleVariables = new BattleVariables();     // The important battle variables, like current targets, current abilities being used, etc. Keeps track of everything important for the round.
    public AoeDictionary aoeArenadata = new AoeDictionary();      // Handles track of all the AoEs in the arena. Their targets, etc.
    
    // Dictionaries for the HP UI elements and the MP UI elements of the players and enemies (where applicable)
    private Dictionary<string, ProgressBar> hpDictionary = new Dictionary<string, ProgressBar>();
    private Dictionary<string, ProgressBar> mpDictionary = new Dictionary<string, ProgressBar>();

    public List<GameObject> despawnBuffer = new List<GameObject>();

    // The debuff bar for allies or enemies. Similar vein to HP/MP
    private Dictionary<string, VisualElement> debuffScrollers = new Dictionary<string, VisualElement>();

    public BattleArena arena;


    // Specific UI elements
    VisualElement charPanel;
    VisualElement controlPanel;
    VisualElement specialPanel;
    VisualElement endScreen;
    ProgressBar bossHP;
    Label stateLabel;

    VisualElement currTooltip = null;
    float tooltipHeight = 250;
    float tooltipWidth = 500;

    // For Restarting
    private List<GameObject> tempCharacterPrefabs = new List<GameObject>();
    private GameObject tempBossPrefab = null;

    public bool overButton = false;
    public bool initialPhaseChecker = false;

    public BattleManager(System.Func<GameObject, Vector2, GameObject> instantiateFunction)
    {
        _instantiateFunction = instantiateFunction;
    }

    // Start initializing the battle with specific characters
    public void InitializeBattle(List<GameObject> playerCharacters, GameObject boss, BattleArena arena, bool restart = false)
    {
        this.arena = arena;

        InstantiateCharacters(playerCharacters, boss, restart);

        turnManager.InitializeTurnManager(this.playerCharacters, this.boss);

        ReferenceUI(restart);

        StartTurn();
    }

    // Grabs UI elements and saves them to variables
    public void ReferenceUI(bool restart = false)
    {
        VisualElement battleDoc = ExcentraDatabase.TryGetDocument("battle").rootVisualElement;
        charPanel = battleDoc.Q<VisualElement>("char-panel");
        controlPanel = battleDoc.Q<VisualElement>("control-panel");
        stateLabel = battleDoc.Q<Label>("state-label");
        bossHP = battleDoc.Q<ProgressBar>("boss-hp");
        specialPanel = battleDoc.Q<VisualElement>("special-panel");
        endScreen = battleDoc.Q<VisualElement>("end-screen");
        
        ExcentraGame.Instance.damageNumberHandlerScript.battleUIRoot = battleDoc;

        foreach (var character in playerCharacters)
        {
            EntityStats stats = character.GetComponent<EntityStats>();
            hpDictionary.Add(stats.entityName, charPanel.Q<VisualElement>(stats.entityName.ToLower()).Q<ProgressBar>("hp"));
            mpDictionary.Add(stats.entityName, charPanel.Q<VisualElement>(stats.entityName.ToLower()).Q<ProgressBar>("mp"));

            stats.OnStatusChanged += DisplayStatuses;
            stats.OnHealthChanged += HPChangeEvent;
            stats.OnAetherChanged += SetMPProgress;

            debuffScrollers.Add(stats.entityName, charPanel.Q<VisualElement>(stats.entityName.ToLower()).Q<VisualElement>("debuff"));
        }
        EntityStats bossStats = boss.GetComponent<EntityStats>();
        bossStats.OnStatusChanged += DisplayStatuses;
        bossStats.OnHealthChanged += HPChangeEvent;
        bossStats.OnAetherChanged += SetMPProgress;
        hpDictionary.Add(bossStats.entityName, bossHP);
        debuffScrollers.Add(bossStats.entityName, bossHP.Q<VisualElement>("debuff"));

        stateLabel.style.visibility = Visibility.Hidden;

        currTooltip = ExcentraDatabase.TryGetSubDocument("debuff-tooltip").CloneTree();
        battleDoc.Add(currTooltip);
        currTooltip = currTooltip.Q<VisualElement>("root");
        currTooltip.style.opacity = 0;

        battleDoc.RegisterCallback<MouseMoveEvent>(evt => UpdateTooltipPosition(evt));

        ChangeState(BattleState.PLAYER_CHOICE);
        SetUIValues(restart);
    }

    // Sets various values of UI elements, such as HP bars and things. Also does some slight additional referencing
    public void SetUIValues(bool restart = false)
    {
        foreach (var character in playerCharacters)
        {
            EntityStats stats = character.GetComponent<EntityStats>();

            hpDictionary[stats.entityName].value = stats.CalculateHPPercentage();
            mpDictionary[stats.entityName].value = stats.CalculateMPPercentage();
        }

        EntityStats bossHPStats = boss.GetComponent<EntityStats>();
        hpDictionary[bossHPStats.entityName].value = bossHPStats.CalculateHPPercentage();

        if (!restart)
        {
            controlPanel.Q<Button>("basic-control").clicked += OnBasicClicked;
            controlPanel.Q<Button>("special-control").clicked += OnSpecialClicked;
            controlPanel.Q<Button>("move-control").clicked += OnMoveClicked;
            controlPanel.Q<Button>("end-control").clicked += OnEndClicked;
            endScreen.Q<Button>("restart-button").clicked += OnRestartClicked;

            controlPanel.RegisterCallback<MouseEnterEvent>(ev => MouseEnterButton(ev));
            controlPanel.RegisterCallback<MouseLeaveEvent>(ev => MouseLeaveButton(ev));
        }


        endScreen.style.display = DisplayStyle.None;
    }

    public void SetMPProgress(EntityStats stats)
    {
        mpDictionary[stats.entityName].value = stats.CalculateMPPercentage();
    }

    public void SetHPProgress(EntityStats stats)
    {
        hpDictionary[stats.entityName].value = stats.CalculateHPPercentage();
    }

    public void HPChangeEvent(EntityStats stats)
    {
        SetHPProgress(stats);

        EnemyAI enemyAi = stats.GetComponent<EnemyAI>();

        if (enemyAi.enabled)
        {
            bool phaseChanged = enemyAi.ChangePhase();


            if (phaseChanged)
            {

                Debug.Log("LOLLLL!");
                turnManager.CalculateIndividualDelay(stats.gameObject, turnManager.ReturnDelayNeededForTurn(0));
            }
            
        }
    }

    // Place the characters down on the screen
    public void InstantiateCharacters(List<GameObject> playerCharacters, GameObject boss, bool restart)
    {
        this.playerCharacters.Clear();
        foreach (var character in playerCharacters) 
        {
            float x = UnityEngine.Random.Range(arena.GetCenter().x - 2, arena.GetCenter().x + 2);
            float y = UnityEngine.Random.Range(arena.GetCenter().y - 3, arena.GetCenter().y - 2);
            GameObject instantiatedCharacter = _instantiateFunction(character, new Vector2(x, y));
            instantiatedCharacter.GetComponent<EntityStats>().InitializeCurrentStats();
            this.playerCharacters.Add(instantiatedCharacter);
        }

        GameObject bossInstantiation = _instantiateFunction(boss, new Vector2(0, 1));
        bossInstantiation.GetComponent<EntityStats>().InitializeCurrentStats();
        SpriteRenderer bossSpriteRenderer = bossInstantiation.GetComponent<SpriteRenderer>();
        bossSpriteRenderer.material.SetFloat("_Thickness", 0f);

        this.boss = bossInstantiation;
        enemyList.Add(bossInstantiation);

        EnemyAI enemyAi = this.boss.GetComponent<EnemyAI>();
        enemyAi.InitializeAI(this.playerCharacters); // Initialize the boss AI
    }

    // Handles each status an entity has upon the start of the turn. Returns the alive status of the entity by the end. For example: Poisons that may drop an Entity's HP to 0.
    // Removes a turn (or the entire status) each time it "triggers"
    public bool HandleTurnStatuses()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats stats = currTurn.GetComponent<EntityStats>();

        List<StatusEffect> effectsToRemove = new List<StatusEffect>();

        foreach (var status in stats.effectHandler.effects)
        {
            float damageToDeal = StatusCalculatorHelper.CalculateDamage(currTurn, status.Value);
            if (damageToDeal != 0f)
            {
                DealDamage(currTurn, damageToDeal, status.Value.owner);

                if (stats.currentHP <= 0)
                {
                    battleVariables.targets = new Dictionary<string, GameObject>() { { stats.entityName, currTurn } };
                    return true;
                }
                    

            }

            if (status.Value.effect.customTurnLogic)
                continue;

            status.Value.turnsRemaining -= 1;

            if (status.Value.turnsRemaining == 0)
                effectsToRemove.Add(status.Value.effect);
        }

        foreach (var effect in effectsToRemove)
        {
            stats.ModifyStatus(effect);
        }

        DisplayStatuses(stats);

        return false;
    }

    public void StartTurn()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();

        if (currTurn.GetComponent<EntityStats>() != null)
        {
            EntityStats stats = currTurn.GetComponent<EntityStats>();
            EntityController controller = currTurn.GetComponent<EntityController>();
            PlayerInput input = currTurn.GetComponent<PlayerInput>();

            if (controller.animator.GetCurrentAnimatorStateInfo(0).IsName("Dead"))
            {
                controller.animator.Play("Idle");
            }

            // Check an Entity's current set of statuses that have on turn start.
            // If they die, immediately end the turn
            if (HandleTurnStatuses())
            {
                EndTurn();
                return;
            }

            bool skipEndTurn = false;

            // Handle the turn if it's a player or enemy.
            if (stats.isPlayer)
            {
                // If player, enable their input and set some basic values. Change state to PLAYER_CHOICE.
                input.enabled = true;
                controller.turnStartPos = currTurn.transform.position;
                float percentDecimalHP = stats.healthRegenRate / 100f;
                float percentDecimalAether = stats.aetherRegenRate / 100f;
                stats.ModifyHP(stats.currentHP + (stats.maximumHP * percentDecimalHP));
                stats.ModifyMP(stats.currentAether + (stats.maximumAether * percentDecimalAether));
                controller.DrawMovementCircle();
                ChangeState(BattleState.PLAYER_CHOICE);
            }
            else
            {
                EnemyContents contents = currTurn.GetComponent<EnemyContents>();
                EnemyAI enemyAi = currTurn.GetComponent<EnemyAI>();
                contents.aggression.ReduceAggressionEnmity();

                try
                {
                    if (!initialPhaseChecker && enemyAi.initialPhase != null)
                    {
                        BossMechanicHandler.InitializeMechanic(enemyAi.initialPhase.mechanic, this, boss);
                        stats.nextStaticDelay = enemyAi.initialPhase.delayBonus;
                    }
                    else
                    {
                        EnemyMechanic mechanic = enemyAi.ChooseAttack(); // Choose an attack for the enemy ai

                        if (mechanic == null)
                        {
                            Debug.Log("Current Attack " + enemyAi.currAttack);
                            BossMechanicHandler.InitializeMechanic(enemyAi.currAttack, this, currTurn);

                        }
                        else
                        {
                            // Do immediate attack
                            BossMechanicHandler.InitializeMechanic(mechanic, this, currTurn);
                        }

                        if (mechanic != null && mechanic.mechanicStyle == MechanicStyle.IMMEDIATE && !mechanic.dontSkipTurn)
                            skipEndTurn = true;

                        if (mechanic != null)
                            stats.targetable = !mechanic.untargetable;
                        else
                            stats.targetable = !enemyAi.currAttack.untargetable;

                        if (stats.targetable == false)
                            stats.effectHandler.effects.Clear();

                        if (mechanic != null)
                        {
                            if (mechanic.activeScript)
                            {

                                stats.active = mechanic.active;
                            }
                        }

                        if (enemyAi.currAttack != null)
                        {
                            if (enemyAi.currAttack.activeScript)
                            {
                                stats.active = enemyAi.currAttack.active;
                            }
                        }
                    }
                } catch (NullReferenceException ex)
                {
                    Debug.Log(ex);
                    Debug.Log("Mechanic is null when it should not have been. Maybe Initialization error again? Or check if current phase is set to proper HP threshold");
                    skipEndTurn = false;
                }


                initialPhaseChecker = true;
                ChangeState(BattleState.AWAIT_ENEMY);

                if (!skipEndTurn)
                    EndTurn();
            }
        }
        else
        {
            BaseAoe aoe = currTurn.GetComponent<BaseAoe>();
            EnemyAI enemyAi = aoe.attackerObject.GetComponent<EnemyAI>();
            if (aoe)
            Debug.Log("GO IN!");
            // Find some way to store attackers of skills
            BossMechanicHandler.ActivateAoeAttack(enemyAi.currAttack, aoe.mechanicAttack, this, aoe.attackerObject, aoe);
            EndTurn();
        }
    }

    public void EndTurn()
    {
        ChangeState(BattleState.TURN_TRANSITION);
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats stats = currTurn.GetComponent<EntityStats>();
        EntityController controller = currTurn.GetComponent<EntityController>();
        bool isRevive = false;
        EnemyAI enemyAi = boss.GetComponent<EnemyAI>();

        // Checks if we need to revive dead Entities during this EndTurn() phase
        if (battleVariables.currSkill != null && battleVariables.currSkill.damageType == DamageType.REVIVE)
        {
            isRevive = true;
        }

        // If there are targets during this EndTurn() phase
        // Loop through them all to check health statuses. If they are <= to 0, either revive (if isRevive is true), or kill if otherwise.
        if (battleVariables.targets != null)
        {
            foreach (var entity in battleVariables.targets)
            {
                EntityStats entityStats = entity.Value.GetComponent<EntityStats>();
                if (!isRevive)
                {
                    if (entityStats.currentHP <= 0)
                    {
                        Debug.Log("KILLING " + entity);
                        KillEntity(entity.Value);

                    }
                }
                else
                {
                    Debug.Log("REVIVING ENTITY!");
                    turnManager.ReviveEntity(entity.Value);
                    entity.Value.GetComponent<EntityController>().animator.SetTrigger("Revive");
                }

            }
        }

        foreach (var entity in despawnBuffer)
        { 
            foreach (var enemy in enemyList)
            {
                if (enemy.GetComponent<EntityStats>().entityName == entity.GetComponent<EntityStats>().entityName)
                {
                    enemyList.Remove(enemy);
                    break;
                }
            }
            GameObject.Destroy(entity);
        }

        despawnBuffer.Clear();

        ExcentraGame.Instance.WaitCoroutine(0.5f, () =>
        {
            try
            {
                if (stats != null)
                    if (!stats.active)
                        stats.nextStaticDelay = 1000f;


                // Clean up the turn, then wait a few seconds and then start the turn.
                CleanupTurn();
                if (stats == null || (stats != null && stats.nextStaticDelay == -1))
                    turnManager.EndCurrentTurn();
                else
                {
                    turnManager.EndCurrentTurn(stats.nextStaticDelay);
                    stats.nextStaticDelay = -1f;
                }

                if (currTurn.GetComponent<BaseAoe>() != null)
                {
                    if (enemyAi.enabled)
                    {
                        if (enemyAi.currAttack != null)
                        {
                            EndMechanic(currTurn.GetComponent<BaseAoe>().mechanic, currTurn.GetComponent<BaseAoe>().attackerObject);
                        }
                    }
                }
            }
            catch (MissingReferenceException) { }

            
            StartTurn();
        });
    }

    public void KillEntity(GameObject entity)
    {
        EntityStats stats = entity.GetComponent<EntityStats>();
        EntityController controller = entity.GetComponent<EntityController>();

        turnManager.KillEntity(entity);

        // Just boss right now, modify for use with other enemies when time comes
        EnemyContents contents = boss.GetComponent<EnemyContents>();
        contents.aggression.RemoveEntityFromAggressionList(entity);

        if (stats.currentHP > 0)
            stats.ModifyHP(0);

        stats.ModifyStatus();
        Debug.Log("Hello!");
        controller.animator.SetTrigger("Die");

        turnManager.DisplayTurnOrder();

        bool isPlayer = stats.isPlayer;

        if (IsLose(isPlayer))
        {
            CleanupTurn();
            if (isPlayer)
                endScreen.Q<Label>("defeat").style.display = DisplayStyle.Flex;
            else
                endScreen.Q<Label>("victory").style.display = DisplayStyle.Flex;
            endScreen.style.display = DisplayStyle.Flex;

            return;
        }
    }

    public void EndMechanic(EnemyMechanic mechanic, GameObject attacker)
    {
        if (turnManager.CheckIfMechanicOver(mechanic))
        {
            EnemyAI enemyAi = attacker.GetComponent<EnemyAI>();
            BossMechanicHandler.EndMechanic(mechanic, this, attacker);
            enemyAi.currAttack = null;
            enemyAi.stats.targetable = true;

            if (mechanic.goNext)
            {
                Debug.Log("HELLO!ASDASDAD");
                turnManager.CalculateIndividualDelay(attacker, 0);
            }

            foreach (var character in playerCharacters)
            {
                EntityStats stats = character.GetComponent<EntityStats>();
                stats.mechanicVariables.targeted = false;
            }
        }
    }

    public GameObject SpawnNewEntity(GameObject entity, Vector2 pos, string entityKey, string aiKey, bool next)
    {
        Debug.Log("SPAWN ENTITY");
        GameObject spawnedEntity = GameObject.Instantiate(entity, pos, Quaternion.identity);
        EntityStats spawnedEntityStats = spawnedEntity.GetComponent<EntityStats>();
        spawnedEntity.GetComponent<EntityStats>().InitializeCurrentStats();

        if (nameCounts.ContainsKey(spawnedEntityStats.entityName))
            nameCounts[spawnedEntityStats.entityName]++;
        else
            nameCounts.Add(spawnedEntityStats.entityName, 1);

        spawnedEntityStats.entityName = GetVariantCharacter(spawnedEntityStats.entityName);
        if (aiKey != "")
            spawnedEntityStats.enemyKey = aiKey;
        spawnedEntity.GetComponent<EnemyAI>().InitializeAI(playerCharacters);
        TurnEntity newSpawnedEntity = new TurnEntity(spawnedEntity);

        if (!next)
            newSpawnedEntity.CalculateDelay();
        else
            newSpawnedEntity.CalculateDirectDelay(turnManager.ReturnDelayNeededForTurn(0));

        bool added = turnManager.InsertUnitIntoTurn(newSpawnedEntity);

        if (!added)
            turnManager.turnOrder.Add(newSpawnedEntity);

        spawnedEntityStats.entityKey = entityKey;

        turnManager.DisplayTurnOrder();
        enemyList.Add(spawnedEntity);

        return spawnedEntity;
    }

    public string GetVariantCharacter(string name)
    {
        int count = nameCounts[name];

        if (count == 1)
            return name;
        else
            return name + " " + count;
    }

    public List<GameObject> GetAliveEntities()
    {
        var possibleChars = playerCharacters.Where(go => go.GetComponent<EntityStats>() != null && go.GetComponent<EntityStats>().currentHP > 0).ToList();

        return possibleChars;
    }

    // Cleans up all the variables at the end of a turn
    public void CleanupTurn()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        try
        {
            if (currTurn.GetComponent<EntityStats>() != null)
            {
                EntityStats currStats = currTurn.GetComponent<EntityStats>();
                EntityController controller = currTurn.GetComponent<EntityController>();
                PlayerInput input = currTurn.GetComponent<PlayerInput>();
                currStats.moveDouble = false;

                if (!controller.inEnemyAoe)
                    controller.HandleTarget(false);

                // Controller cleanup
                controller.specialActive = false;
                controller.basicActive = false;
                controller.lineRenderer.positionCount = 0;
                controller.HandleTarget(false);

                input.enabled = false;

                DestroyAoe(currTurn);
            }
            else
            {
                BaseAoe aoe = currTurn.GetComponent<BaseAoe>();
                aoeArenadata.PopAoe(aoe.arenaAoeIndex);
                GameObject.Destroy(aoe.gameObject);
            }
        } catch (MissingReferenceException) { }


        // Battle Variables cleanup
        battleVariables.CleanVariables();

        // MIsc. cleanup
        specialPanel.style.visibility = Visibility.Hidden;

        // Transition to next turn (requires this state)
        ChangeState(BattleState.TURN_TRANSITION);
    }

    // Checks whether a side (enemy or player) loses. Typically checked upon one death from either side (to avoid excess calls)
    public bool IsLose(bool isPlayer)
    {
        if (isPlayer)
        {
            foreach (var player in playerCharacters)
            {
                if (player.GetComponent<EntityStats>().currentHP > 0)
                    return false;
            }
        }
        else
        {
            if (boss.GetComponent<EntityStats>().currentHP > 0)
                return false;
        }

        return true;
    }

    // -- DURING TURN
    // Handles an entity's action providing an information key. Information tends to be sent in if doing a SINGLE click, but 
    // aoes don't usually have one.
    public void HandleEntityAction(BattleClickInfo information = null)
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats stats = currTurn.GetComponent<EntityStats>();
        EntityController entityController = currTurn.GetComponent<EntityController>();
        PlayerSkill currSkill = battleVariables.GetCurrentSkill() as PlayerSkill;

        if (!battleVariables.isAttacking)
        {
            if (stats.isPlayer)
            {
                currTurn.GetComponent<PlayerInput>().enabled = false;
                
                if (currSkill != null && currSkill.containsMovement)
                {

                    if (currSkill.selfMove == false)
                    {
                        Vector2 targetLocation = currTurn.transform.position;
                        foreach (var entity in battleVariables.targets)
                        {
                            EntityController controller = entity.Value.GetComponent<EntityController>();
                            controller.ActivateMovementSkill(currSkill.moveSpeed, targetLocation, currSkill.offsetDistance);
                        }
                    }
                    else
                    {
                        Vector2 targetLocation = information.mousePosition;
                        if (stats.arenaAoeIndex != -1)
                        {
                            if (currSkill.shape != Shape.CIRCLE)
                            {
                                GameObject aoe = aoeArenadata.GetAoe(stats.arenaAoeIndex);
                                targetLocation = (aoe.GetComponent<BaseAoe>() as DerivedDirectional).endPoint;
                            }
                        }
                        
                        entityController.ActivateMovementSkill(currSkill.moveSpeed, targetLocation, currSkill.offsetDistance);
                    }


                }

                if (information != null && information.target != null)
                {
                    if (information.target.transform.position.x > currTurn.transform.position.x)
                        entityController.FaceDirection(false);
                    else
                        entityController.FaceDirection(true);
                }
                else if (information != null && information.mousePosition != null)
                {
                    if (information.mousePosition.x > currTurn.transform.position.x)
                        entityController.FaceDirection(false);
                    else
                        entityController.FaceDirection(true);
                }    

                if (battleVariables.battleState == BattleState.PLAYER_BASIC && information != null)
                {
                    Dictionary<string, GameObject> targetList = new Dictionary<string, GameObject>();
                    targetList.Add(information.target.GetComponent<EntityStats>().entityName, information.target);
                    battleVariables.targets = targetList;
                    battleVariables.isAttacking = true;
                    currTurn.GetComponent<EntityController>().animator.SetTrigger("Basic Attack");
                }
                else if (battleVariables.battleState == BattleState.PLAYER_SPECIAL)
                {
                    if (stats.arenaAoeIndex != -1)
                    {
                        battleVariables.currAoe = aoeArenadata.GetAoe(stats.arenaAoeIndex);
                        battleVariables.currSkill = aoeArenadata.GetAoe(stats.arenaAoeIndex).GetComponent<BaseAoe>().skill;
                        battleVariables.targets = battleVariables.currAoe.GetComponent<BaseAoe>().aoeData.TargetList;
                        battleVariables.isAttacking = true;
                        currTurn.GetComponent<EntityController>().animator.SetTrigger("Special Attack");
                    }
                    else
                    {
                        battleVariables.currSkill = information.singleSkill;
                        battleVariables.targets = new Dictionary<string, GameObject>() { { information.target.GetComponent<EntityStats>().entityName, information.target } };
                        battleVariables.isAttacking = true;
                        currTurn.GetComponent<EntityController>().animator.SetTrigger("Special Attack");
                    }
                }
            }
            else
            {
                
                Dictionary<string, GameObject> targetList = new Dictionary<string, GameObject>();
                targetList.Add(information.target.GetComponent<EntityStats>().entityName, information.target);
                battleVariables.targets = targetList;
                battleVariables.currSkill = information.singleSkill;
                EnemyAI enemyAi = currTurn.GetComponent<EnemyAI>();
                EnemyContents contents = currTurn.GetComponent<EnemyContents>();
                EnemySkill enemySkill = (battleVariables.currSkill as EnemySkill);
                if (enemySkill.aoeData.Count != 0)
                {
                    foreach(var aoe in enemySkill.aoeData)
                    {
                        if (aoe.subTargetType == EntityTargetType.NONE)
                        {
                            aoe.objectTarget = enemyAi.ChooseEntity(enemySkill.targetType);
                        }
                        else
                        {
                            aoe.objectTarget = enemyAi.ChooseEntity(aoe.subTargetType);
                        }

                        Debug.Log(aoe.objectTarget);

                        if (aoe.onSelf)
                            aoe.objectOrigin = currTurn;
                        else if (aoe.onTarget)
                            aoe.objectOrigin = aoe.objectTarget;

                        //GameObject enemyAoe = SpawnEnemyAoe(enemySkill, aoe, currTurn);
                    }
                }
                currTurn.GetComponent<EntityController>().animator.SetTrigger("Basic Attack");
            }
        }

    }

    // Spawns an AOE telegraph for the user
    public GameObject ActivateSkillTelegraph(VisualElement element)
    {
        return SpawnAoe((element.userData as PlayerSkill), turnManager.GetCurrentTurn(), turnManager.GetCurrentTurn());
    }

    // When the animation "hits" the target, this event is triggered. Does the specific attack towards this group of targets
    public void OnHit()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats stats = currTurn.GetComponent<EntityStats>();

        if (!stats.isPlayer)
        {
            EnemyAI enemyAi = currTurn.GetComponent<EnemyAI>();

            if (enemyAi.currImmediateAttack != null)
            {
                // Need to handle a loop for movement. For each attack, move to target, then activate attack. This is temporary
                foreach (var attack in enemyAi.currImmediateAttack.mechanicAttacks) {
                    BossMechanicHandler.ActivateSingleTargetAttack(enemyAi.currImmediateAttack, attack, this, currTurn, enemyAi.currTarget);
                }   
            }
        }


        Dictionary<string, GameObject> targetList = battleVariables.targets;

        foreach (var entity in targetList)
        {
            float entityDamage = GlobalDamageHelper.HandleActionCalculation(new ActionInformation(entity.Value, turnManager.GetCurrentTurn(), battleVariables.currSkill));
            DealDamage(entity.Value, entityDamage);

            if (battleVariables.currSkill == null)
                return;

            if ((battleVariables.currSkill as PlayerSkill).grabAggro)
            {
                foreach (var aoe in aoeArenadata.aoes)
                {
                    if (aoe.Value == null)
                        continue;

                    BaseAoe baseAoe = aoe.Value.GetComponent<BaseAoe>();
                    
                    if (baseAoe.attackerObject == entity.Value && baseAoe.mechanicAttack.canBeShirked)
                    {
                        baseAoe.ChangeTarget(currTurn);
                    }
                }
            }
        }

    }

    // Displays statuses to the screen for each entity in play
    public void DisplayStatuses(EntityStats entityStats)
    {
        try
        {
            VisualElement scroller = debuffScrollers[entityStats.entityName];
            VisualTreeAsset statusTemplate = ExcentraDatabase.TryGetSubDocument("status-effect");


            scroller.Clear();
            foreach (var status in entityStats.effectHandler.effects)
            {
                VisualElement statusInstance = statusTemplate.CloneTree();

                statusInstance.Q<Label>("turn-count").text = status.Value.turnsRemaining.ToString();

                if (entityStats.isPlayer)
                {
                    statusInstance.style.flexShrink = 0;
                    statusInstance.style.width = new StyleLength(Length.Percent(25f));
                }
                else
                {
                    statusInstance.style.marginRight = 5f;
                }

                statusInstance.Q<VisualElement>("image").style.backgroundImage = status.Value.effect.icon;

                statusInstance.RegisterCallback<MouseEnterEvent>(evt => ShowTooltip(evt, status.Value.effect));
                statusInstance.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());

                scroller.Add(statusInstance);
            }
        }
        catch (KeyNotFoundException) { }


    }

    public void AddStatusToEnemy(EntityStats entityStats)
    {
        if (battleVariables.currSkill != null)
        {
            foreach (var status in battleVariables.currSkill.statusEffects)
            {
                float statusChance = status.chance;
                float randomNum = UnityEngine.Random.Range(0, 100);

                if (randomNum <= statusChance)
                    entityStats.ModifyStatus(ExcentraDatabase.TryGetStatus(status.key), turnManager.GetCurrentTurn());
            }
        }
    }

    public void DealDamage(GameObject entity, float entityDamage, GameObject attacker = null)
    {
        EntityController entityController = entity.GetComponent<EntityController>();
        EntityStats entityStats = entity.GetComponent<EntityStats>();
        EnemyContents contents = entity.GetComponent<EnemyContents>();
        GameObject currAttacker = null;

        if (attacker != null)
            currAttacker = attacker;
        else
            currAttacker = turnManager.GetCurrentTurn();

        Vector2 centerPoint = currAttacker.transform.position;

        if ((battleVariables.currSkill != null && !battleVariables.currSkill.ignoresLineOfSight && !HasLineOfSight(centerPoint, entity)))
            return;

        AddStatusToEnemy(entityStats);

        if ((battleVariables.currSkill != null && battleVariables.currSkill.damageType == DamageType.DAMAGE) || battleVariables.currSkill == null)
        {
            if (entityStats.currentHP > 0f)
                entityController.animator.Play("Damage", -1, 0f);
        }

        if (entityDamage > 0f)
        {
            ExcentraGame.Instance.damageNumberHandlerScript.SpawnDamageNumber(entity, Mathf.Abs((int)entityDamage));
            if (contents.enabled)
            {
                Debug.Log("Adding Aggression: " + currAttacker);
                contents.aggression.AggressionEntryPoint(new AggressionElement(currAttacker, entityDamage));
            }
        }

        entityStats.ModifyHP(Mathf.Max(entityStats.currentHP - entityDamage, 0));

        if (entityStats.currentHP <= 0)
        {
            KillEntity(entity);
        }
    }

    public bool HasLineOfSight(Vector2 centerPoint, GameObject defender)
    {

        Vector2 startPosition = centerPoint;
        Vector2 endPosition = defender.transform.position;

        Vector2 direction = (endPosition - startPosition).normalized;

        float distance = Vector2.Distance(startPosition, endPosition);

        RaycastHit2D hit = Physics2D.Raycast(startPosition, direction, distance, LayerMask.GetMask("Obstacles"));

        Debug.Log(hit);

        if (hit.collider != null)
            return false;

        return true;
    }

    public void ChangeState(BattleState newState)
    {
        battleVariables.battleState = newState;

        if (battleVariables.battleState == BattleState.PLAYER_BASIC)
        {
            stateLabel.style.visibility = Visibility.Visible;
            stateLabel.text = "Basic Attack";
        }
        else if (battleVariables.battleState == BattleState.PLAYER_CHOICE)
        {
            stateLabel.style.visibility = Visibility.Visible;
            stateLabel.text = "Awaiting Player Choice...";
        }
        else if (battleVariables.battleState == BattleState.PLAYER_SPECIAL)
        {
            stateLabel.style.visibility= Visibility.Visible;
            stateLabel.text = "Special Attack";
        }
        else if (battleVariables.battleState == BattleState.AWAIT_ENEMY)
        {
            stateLabel.style.visibility = Visibility.Visible;
            stateLabel.text = "Awaiting enemy";
        }
        else if (battleVariables.battleState == BattleState.TURN_TRANSITION)
        {
            stateLabel.style.visibility = Visibility.Visible;
            stateLabel.text = "Transitioning Turn...";
        }
    }

    // TODO: Could be on the entity itself honestly
    public bool IsAlive(GameObject entity)
    {
        EntityStats stats = entity.GetComponent<EntityStats>();
        if (stats.currentHP > 0)
            return true;
        return false;
    }

    public void EscapePressed()
    {
        if (specialPanel.style.visibility == Visibility.Visible)
            specialPanel.style.visibility = Visibility.Hidden;

        if (battleVariables.GetState() == BattleState.PLAYER_BASIC)
        {
            OnBasicClicked();
        }
        else if (battleVariables.GetState() == BattleState.PLAYER_SPECIAL)
        {
            RightClickPressed();
        }
    }

    public void RightClickPressed()
    {
        if (battleVariables.GetState() == BattleState.PLAYER_SPECIAL)
        {
            EntityController controller = turnManager.GetCurrentTurn().GetComponent<EntityController>();
            DestroyAoe(turnManager.GetCurrentTurn());
            battleVariables.currSkill = null;
            controller.specialActive = false;
            ChangeState(BattleState.PLAYER_CHOICE);
            specialPanel.style.visibility = Visibility.Visible;

            foreach (var character in playerCharacters)
            {
                EntityController charController = character.GetComponent<EntityController>();
                charController.HandleTarget(false);
            }

            EntityController bossController = boss.GetComponent<EntityController>();
            bossController.HandleTarget(false);
        }
        else if (battleVariables.GetState() == BattleState.PLAYER_BASIC)
        {
            foreach (var character in playerCharacters)
            {
                EntityController charController = character.GetComponent<EntityController>();
                charController.HandleTarget(false);
            }

            EntityController bossController = boss.GetComponent<EntityController>();
            bossController.HandleTarget(false);
            OnBasicClicked();
        }
    }

    // ----- BUTTON EVENTS
    public void OnBasicClicked()
    {
        if (battleVariables.GetState() == BattleState.TURN_TRANSITION || battleVariables.GetState() == BattleState.AWAIT_ENEMY)
            return;

        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats currStats = currTurn.GetComponent<EntityStats>();
        EntityController controller = currTurn.GetComponent<EntityController>();

        if (currStats.moveDouble)
            return;

        if (battleVariables.battleState == BattleState.PLAYER_BASIC)
        {
            ChangeState(BattleState.PLAYER_CHOICE);
            controller.basicActive = false;
        }
        else if (battleVariables.battleState == BattleState.PLAYER_CHOICE)
        {
            EscapePressed();
            ChangeState(BattleState.PLAYER_BASIC);
            controller.basicActive = true;
        }

    }

    public void OnSpecialClicked()
    {
        if (battleVariables.GetState() == BattleState.TURN_TRANSITION || battleVariables.GetState() == BattleState.AWAIT_ENEMY)
            return;

        if (battleVariables.GetState() == BattleState.PLAYER_BASIC)
            ChangeState(BattleState.PLAYER_CHOICE);

        RightClickPressed();

        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityController controller = currTurn.GetComponent<EntityController>();
        EntityStats currStats = currTurn.GetComponent<EntityStats>();

        if (currStats.moveDouble)
            return;

        VisualTreeAsset itemAsset = ExcentraDatabase.TryGetSubDocument("skill-item");
        VisualElement skillScroller = specialPanel.Q<VisualElement>("skill-scroller");

        controller.basicActive = false;

        if (specialPanel.style.visibility != Visibility.Visible)
            specialPanel.style.visibility = Visibility.Visible;
        else
            specialPanel.style.visibility = Visibility.Hidden;

        skillScroller.Clear();

        foreach (string skill in currStats.skillKeys)
        {
            PlayerSkill currSkill = ExcentraDatabase.TryGetSkill(skill);

            if (currSkill != null)
            {
                VisualElement newSkill = itemAsset.CloneTree();
                newSkill.Q<Label>("skill-name").text = currSkill.skillName;
                newSkill.Q<VisualElement>("image").style.backgroundImage = new StyleBackground(currSkill.icon);
                newSkill.Q<Label>("skill-cost").text = currSkill.baseAether + " Aether";
                newSkill.userData = currSkill;

                newSkill.RegisterCallback<ClickEvent>(e =>
                {
                    EntityStats stats = turnManager.GetCurrentTurn().GetComponent<EntityStats>();
                    EntityController controller = turnManager.GetCurrentTurn().GetComponent<EntityController>();
                    VisualElement element = (e.currentTarget as VisualElement);
                    if ((element.userData as PlayerSkill).baseAether > stats.currentAether)
                        return;

                    battleVariables.currSkill = element.userData as PlayerSkill;
                    ChangeState(BattleState.PLAYER_SPECIAL);

                    if ((element.userData as PlayerSkill).areaStyle == AreaStyle.SINGLE || ((element.userData as PlayerSkill).shape == Shape.CIRCLE))
                    {
                        controller.specialActive = true;

                        if ((element.userData as PlayerSkill).targetMode == TargetMode.SELF)
                            controller.HandleTarget(true);
                    }
                        

                    if (specialPanel.style.visibility == Visibility.Visible)
                        specialPanel.style.visibility = Visibility.Hidden;

                    if (NeedsAoe(element.userData as PlayerSkill))
                        ActivateSkillTelegraph(element);

                });

                newSkill.RegisterCallback<MouseEnterEvent>(evt => ShowSkillTooltip(evt, newSkill.userData as PlayerSkill));
                newSkill.RegisterCallback<MouseLeaveEvent>(evt => HideTooltip());

                skillScroller.Add(newSkill);
            }
        }
    }

    public void OnMoveClicked()
    {
        if (battleVariables.GetState() == BattleState.TURN_TRANSITION || battleVariables.GetState() == BattleState.AWAIT_ENEMY)
            return;

        if (battleVariables.GetState() == BattleState.PLAYER_CHOICE)
        {
            if (specialPanel.style.visibility == Visibility.Visible)
                specialPanel.style.visibility = Visibility.Hidden;

            GameObject currTurn = turnManager.GetCurrentTurn();
            EntityStats stats = currTurn.GetComponent<EntityStats>();
            EntityController entityController = currTurn.GetComponent<EntityController>();

            if (entityController.CheckIfDistanceOutsideBase())
                entityController.ResetPosition();

            stats.moveDouble = !stats.moveDouble;

            entityController.DrawMovementCircle();
        }


    }

    public void OnEndClicked()
    {
        if (battleVariables.GetState() == BattleState.TURN_TRANSITION || battleVariables.GetState() == BattleState.AWAIT_ENEMY)
            return;

        if (battleVariables.battleState == BattleState.PLAYER_CHOICE)
        {
            EndTurn();
        }

    }

    public void OnRestartClicked()
    {
        PostBattleCleanup();
        InitializeBattle(tempCharacterPrefabs, tempBossPrefab, arena, true);
    }
    
    public void OnSkillShot()
    {
        if (overButton)
            return;
        GameObject currEntity = turnManager.GetCurrentTurn();
        EntityStats stats = currEntity.GetComponent<EntityStats>();
        EntityController controller = currEntity.GetComponent<EntityController>();
        PlayerSkill currSkill = battleVariables.GetCurrentSkill() as PlayerSkill;
        if (currSkill.targetMode == TargetMode.SELF && currSkill.areaStyle == AreaStyle.SINGLE)
        {
            BattleClickInfo info = new BattleClickInfo();
            info.target = currEntity;
            info.singleSkill = currSkill;
            HandleEntityAction(info);
            stats.ModifyMP(Mathf.Max(stats.currentAether - currSkill.baseAether, 0));
            return;
        }

        try
        {
            GameObject currAoe = aoeArenadata.GetAoe(stats.arenaAoeIndex);
            BaseAoe aoeInit = currAoe.GetComponent<BaseAoe>();

            if ((aoeInit.skill as PlayerSkill).shape == Shape.CIRCLE && (aoeInit.skill as PlayerSkill).targetMode == TargetMode.SELECT)
            {
                if (!CheckWithinSkillRange(currEntity, currAoe, (aoeInit.skill as PlayerSkill)))
                    return;
            }

            aoeInit.FreezeAoe();
            BattleClickInfo info = new BattleClickInfo();
            info.mousePosition = aoeInit.FrozenInfo();

            stats.ModifyMP(Mathf.Max(stats.currentAether - (aoeInit.skill as PlayerSkill).baseAether, 0));
            controller.specialActive = false;

            HandleEntityAction(info);
        }
        catch (NullReferenceException) {}
    }

    // ------ CLEANED UP PROPER FUNCTIONS ------------

    // Just spawns the AoE. Assumes all checks pass to allow the AoE to spawn
    public GameObject SpawnAoe(PlayerSkill skill, GameObject origin, GameObject attacker)
    {
        GameObject aoe = null;
        EntityStats stats = attacker.GetComponent<EntityStats>();

        if (skill.shape == Shape.CONE)
        {
            aoe = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("cone"), origin.transform.position, Quaternion.identity);
            aoe.GetComponent<BaseAoe>().InitializeAoe(origin, attacker, skill);

        }
        else if (skill.shape == Shape.CIRCLE)
        {
            aoe = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("circle"), new Vector2(1000, 1000), Quaternion.identity);
            aoe.GetComponent<BaseAoe>().InitializeAoe(origin, attacker, skill);
        }
        else if (skill.shape == Shape.LINE)
        {
            aoe = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("line"), origin.transform.position, Quaternion.identity);
            aoe.GetComponent<BaseAoe>().InitializeAoe(origin, attacker, skill);
        }

        if (aoe != null)
        {
            int index = aoeArenadata.AddAoe(aoe);
            stats.arenaAoeIndex = index;
        }

        return aoe;
    }


    // Each entity should have their aoe as an index on their stats. This is done via the above function.
    public void DestroyAoe(GameObject owner)
    {
        EntityStats stats = owner.GetComponent<EntityStats>();

        if (!stats.isPlayer)
        {
            EnemyContents contents = owner.GetComponent<EnemyContents>();

            foreach (var aoeIndex in contents.aoeIndexList)
            {
                GameObject enemyAoe = aoeArenadata.GetAoe(aoeIndex);
                UnityEngine.GameObject.Destroy(enemyAoe);
            }

            foreach (var aoeIndex in contents.aoeIndexList)
            {
                aoeArenadata.PopAoe(aoeIndex);
            }

            contents.aoeIndexList.Clear();
        }
        
        if (stats.arenaAoeIndex == -1)
            return;

        GameObject aoe = aoeArenadata.GetAoe(stats.arenaAoeIndex);
        UnityEngine.GameObject.Destroy(aoe);
        stats.arenaAoeIndex = -1;
    }

    public bool NeedsAoe(PlayerSkill skill)
    {
        if (skill.areaStyle == AreaStyle.SINGLE || skill.targetMode == TargetMode.SELECT)
            return false;

        return true;
    }
    
    // Change how this works when new key system in place.
    public void PostBattleCleanup()
    {
        CleanupTurn();
        List<string> newPlayerCharacters = new List<string>();
        string bossName = boss.GetComponent<EntityStats>().entityName;
        UnityEngine.Object.Destroy(boss);

        foreach (var player in playerCharacters)
        {
            EntityStats stats = player.GetComponent<EntityStats>();
            EntityController controller = player.GetComponent<EntityController>();
            

            newPlayerCharacters.Add(stats.entityName);
            controller.Cleanup();
            UnityEngine.Object.Destroy(player);
        }

        playerCharacters.Clear();
        boss = null;

        tempCharacterPrefabs.Clear();
        tempBossPrefab = null;

        foreach (var playerName in newPlayerCharacters)
        {
            tempCharacterPrefabs.Add(ExcentraDatabase.TryGetEntity(playerName));
        }
        tempBossPrefab = ExcentraDatabase.TryGetEntity(bossName);

        for (int i = 0; i < aoeArenadata.aoes.Count; i++)
        {
            GameObject aoe = aoeArenadata.PopAoe(i);

            UnityEngine.Object.Destroy(aoe);
        }

        hpDictionary.Clear();
        mpDictionary.Clear();

        foreach (var debuffScroller in debuffScrollers)
        {
            debuffScroller.Value.Clear();
        }
        debuffScrollers.Clear();
        battleVariables = new BattleVariables();
        turnManager = new TurnManager();
    }

    // Checks if an attacker can even target the defender
    public bool TargetingEligible(GameObject attacker, GameObject defender)
    {
        EntityStats attackerStats = attacker.GetComponent<EntityStats>();
        EntityStats defenderStats = defender.GetComponent<EntityStats>();
        
        if (attackerStats == null || defenderStats == null)
            return false;

        if (!defenderStats.targetable)
            return false;

        bool sameTeam = attackerStats.isPlayer == defenderStats.isPlayer;

        if (battleVariables.GetState() == BattleState.PLAYER_BASIC)
        {
            if (defenderStats.currentHP <= 0)
                return false;

            return !sameTeam && HasLineOfSight(attacker.transform.position, defender);
        }
        else if (battleVariables.GetState() == BattleState.PLAYER_SPECIAL)
        {
            PlayerSkill currSkill = battleVariables.GetCurrentSkill() as PlayerSkill;

            if (currSkill != null)
            {
                EntityType entityType = currSkill.entityType;
                
                if (entityType == EntityType.ALLY)
                {
                    bool revive = false;
                    

                    if (currSkill.damageType == DamageType.REVIVE)
                        revive = true;

                    if (sameTeam)
                    {
                        if (revive)
                        {
                            if (defenderStats.currentHP <= 0)
                                return true;
                        }
                        else
                        {
                            if (defenderStats.currentHP > 0)
                                return true;
                        }
                    }

                }
                else if (entityType == EntityType.ENEMY)
                {
                    if (!sameTeam && defenderStats.currentHP > 0)
                    {
                        return true;
                    }
                }
            }
        }
        else if (battleVariables.GetState() == BattleState.AWAIT_ENEMY)
        {
            EnemySkill enemySkill = battleVariables.currSkill as EnemySkill;
            if (!sameTeam && defenderStats.currentHP > 0)
                return true;
        }

        return false;
    }
    
    // Checks within a skill or basic range (combines both types of functions into one)
    // TODO: Might be better to have a range handler that we can pass in some parameters and it auto-calculates a range. Good to centralize things.
    public bool CheckWithinSkillRange(GameObject attacker, GameObject defender, PlayerSkill skill = null)
    {
        EntityStats attackerStats = attacker.GetComponent<EntityStats>();

        if (skill == null)
        {
            if (Vector2.Distance(attacker.transform.position, defender.transform.position) < attackerStats.CalculateBasicRangeRadius() / 2)
                return true;
        }
        else
        {
            if (Vector2.Distance(attacker.transform.position, defender.transform.position) < skill.range / 2)
                return true;
        }

        return false;
    }

    public GameObject GetCurrentAttacker()
    {
        return turnManager.GetCurrentTurn();
    }

    public void MouseEnterButton(MouseEnterEvent ev)
    {
        overButton = true;
    }

    public void MouseLeaveButton(MouseLeaveEvent ev)
    {
        overButton = false;
    }

    public void ShowTooltip(MouseEnterEvent evt, StatusEffect effect)
    {
        currTooltip.style.left = evt.mousePosition.x + 10; // Offset to avoid overlap
        currTooltip.style.top = Mathf.Clamp((evt.mousePosition.y - currTooltip.resolvedStyle.height) + 10, 0, 4000);

        currTooltip.Q<VisualElement>("debuff-icon").style.backgroundImage = effect.icon;
        currTooltip.Q<Label>("debuff-name").text = effect.effectName;
        currTooltip.Q<Label>("debuff-description").text = effect.description;

        currTooltip.style.opacity = 2;
    }

    public void ShowSkillTooltip(MouseEnterEvent evt, PlayerSkill skill)
    {
        currTooltip.style.left = evt.mousePosition.x + 10; // Offset to avoid overlap
        currTooltip.style.top = Mathf.Clamp((evt.mousePosition.y - currTooltip.resolvedStyle.height) + 10, 0, 4000);
        currTooltip.Q<VisualElement>("debuff-icon").style.backgroundImage = skill.icon;
        currTooltip.Q<Label>("debuff-name").text = skill.skillName;
        currTooltip.Q<Label>("debuff-description").text = skill.description;
        currTooltip.style.opacity = 2;
    }

    public void UpdateTooltipPosition(MouseMoveEvent evt)
    {
        currTooltip.style.left = Mathf.Clamp(evt.mousePosition.x + 10, 0, 4000); // Offset to avoid overlap
        currTooltip.style.top = Mathf.Clamp((evt.mousePosition.y - currTooltip.resolvedStyle.height) + 10, 0, 4000);    
    }

    public void HideTooltip()
    {
        currTooltip.style.opacity = 0;
    }
}
