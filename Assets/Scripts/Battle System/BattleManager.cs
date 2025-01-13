// BattleManager.cs

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// Class that runs the battle, from turn processes, to damage being dealt, to handling statuses, killing off entities, and declaring a winning side.
// One of the most important classes at this point
public class BattleManager
{
    private readonly System.Func<GameObject, Vector2, GameObject> _instantiateFunction; // Allows instantiation outside of MonoBehaviour, but I'd recc to just use (UnityEngine.GameObject.Instantiate())

    private List<GameObject> playerCharacters = new List<GameObject>(); // All of the player character's Game Objects by reference
    private GameObject boss = new GameObject(); // The boss (will need modification for multiple enemies)

    public TurnManager turnManager = new TurnManager(); // The turn manager handles everything regarding the turn order, delays, etc. The battle manager uses the turnManager to facilitate the turns
    public BattleVariables battleVariables = new BattleVariables();     // The important battle variables, like current targets, current abilities being used, etc. Keeps track of everything important for the round.
    public AoeArenaData aoeArenadata = new AoeArenaData();      // Handles track of all the AoEs in the arena. Their targets, etc.
    
    // Dictionaries for the HP UI elements and the MP UI elements of the players and enemies (where applicable)
    private Dictionary<string, ProgressBar> hpDictionary = new Dictionary<string, ProgressBar>();
    private Dictionary<string, ProgressBar> mpDictionary = new Dictionary<string, ProgressBar>();

    // The debuff bar for allies or enemies. Similar vein to HP/MP
    private Dictionary<string, VisualElement> debuffScrollers = new Dictionary<string, VisualElement>();


    // Specific UI elements
    VisualElement charPanel;
    VisualElement controlPanel;
    VisualElement specialPanel;
    VisualElement endScreen;
    ProgressBar bossHP;
    Label stateLabel;

    // For Restarting
    private List<GameObject> tempCharacterPrefabs = new List<GameObject>();
    private GameObject tempBossPrefab = null;

    public BattleManager(System.Func<GameObject, Vector2, GameObject> instantiateFunction)
    {
        _instantiateFunction = instantiateFunction;
    }

    // Start initializing the battle with specific characters
    public void InitializeBattle(List<GameObject> playerCharacters, GameObject boss, bool restart = false)
    {
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

            debuffScrollers.Add(stats.entityName, charPanel.Q<VisualElement>(stats.entityName.ToLower()).Q<VisualElement>("debuff"));
        }
        EntityStats bossStats = boss.GetComponent<EntityStats>();
        hpDictionary.Add(bossStats.entityName, bossHP);
        debuffScrollers.Add(bossStats.entityName, bossHP.Q<VisualElement>("debuff"));

        stateLabel.style.visibility = Visibility.Hidden;

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
        }


        endScreen.style.display = DisplayStyle.None;
    }

    // Place the characters down on the screen
    public void InstantiateCharacters(List<GameObject> playerCharacters, GameObject boss, bool restart)
    {
        this.playerCharacters.Clear();
        foreach (var character in playerCharacters) 
        {
            float x = UnityEngine.Random.Range(-17, 0);
            float y = UnityEngine.Random.Range(0, 8);
            GameObject instantiatedCharacter = _instantiateFunction(character, new Vector2(x, y));
            instantiatedCharacter.GetComponent<EntityStats>().InitializeCurrentStats();
            this.playerCharacters.Add(instantiatedCharacter);
        }

        GameObject bossInstantiation = _instantiateFunction(boss, new Vector2(0, 0));
        bossInstantiation.GetComponent<EntityStats>().InitializeCurrentStats();
        SpriteRenderer bossSpriteRenderer = bossInstantiation.GetComponent<SpriteRenderer>();
        bossSpriteRenderer.material.SetFloat("_Thickness", 0f);

        this.boss = bossInstantiation;
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
                DealDamage(currTurn, damageToDeal);

                if (stats.currentHP <= 0)
                {
                    battleVariables.targets = new Dictionary<string, GameObject>() { { stats.entityName, currTurn } };
                    return true;
                }
                    

            }

            status.Value.turnsRemaining -= 1;

            if (status.Value.turnsRemaining == 0)
                effectsToRemove.Add(status.Value.effect);
        }

        foreach (var effect in effectsToRemove)
        {
            stats.effectHandler.RemoveEffect(effect);
        }

        DisplayStatuses(currTurn.GetComponent<EntityStats>());

        return false;
    }

    public void StartTurn()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats stats = currTurn.GetComponent<EntityStats>();
        EntityController controller = currTurn.GetComponent<EntityController>();
        PlayerInput input = currTurn.GetComponent<PlayerInput>();

        // Check an Entity's current set of statuses that have on turn start.
        // If they die, immediately end the turn
        if (HandleTurnStatuses())
        {
            EndTurn();
            return;
        }

        // Handle the turn if it's a player or enemy.
        if (stats.isPlayer)
        {
            // If player, enable their input and set some basic values. Change state to PLAYER_CHOICE.
            input.enabled = true;
            controller.turnStartPos = currTurn.transform.position;
            controller.DrawMovementCircle();
            ChangeState(BattleState.PLAYER_CHOICE);
        }
        else
        {
            // If enemy, find "alive" entity, and set them as the boss's target this turn. Change state to AWAIT_ENEMY
            var possibleChars = playerCharacters.Where(go => go.GetComponent<EntityStats>() != null && go.GetComponent<EntityStats>().currentHP > 0).ToList();
            int randChar = UnityEngine.Random.Range(0, possibleChars.Count);
            currTurn.GetComponent<EntityController>().MoveTowards(possibleChars[randChar]);
            ChangeState(BattleState.AWAIT_ENEMY);
        }
        
    }

    public void EndTurn()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats stats = currTurn.GetComponent<EntityStats>();
        EntityController controller = currTurn.GetComponent<EntityController>();
        bool isRevive = false;

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
                        turnManager.KillEntity(entity.Value);
                        entityStats.effectHandler.effects.Clear();
                        DisplayStatuses(entityStats);
                        entity.Value.GetComponent<EntityController>().animator.SetTrigger("Die");

                        bool isPlayer = entityStats.isPlayer;

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
                }
                else
                {
                    turnManager.ReviveEntity(entity.Value);
                    entity.Value.GetComponent<EntityController>().animator.SetTrigger("Revive");
                }

            }
        }

        // Clean up the turn, then wait a few seconds and then start the turn.
        CleanupTurn();

        ExcentraGame.Instance.WaitCoroutine(0.5f, () =>
        {
            turnManager.EndCurrentTurn();
            StartTurn();
        });
    }

    // Cleans up all the variables at the end of a turn
    public void CleanupTurn()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats currStats = currTurn.GetComponent<EntityStats>();
        EntityController controller = currTurn.GetComponent<EntityController>();
        PlayerInput input = currTurn.GetComponent<PlayerInput>();

        currStats.moveDouble = false;

        // Battle Variables cleanup
        battleVariables.targets = null;
        battleVariables.isAttacking = false;
        battleVariables.currSkill = null;

        // Controller cleanup
        controller.specialActive = false;
        controller.basicActive = false;
        controller.lineRenderer.positionCount = 0;

        // MIsc. cleanup
        specialPanel.style.visibility = Visibility.Hidden;
        DestroyAoe(currTurn);
        input.enabled = false;

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

        if (!battleVariables.isAttacking)
        {
            if (stats.isPlayer)
            {
                currTurn.GetComponent<PlayerInput>().enabled = false;

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
                        battleVariables.currSkill = aoeArenadata.GetAoe(stats.arenaAoeIndex).GetComponent<ConeAoe>().skill;
                        battleVariables.targets = battleVariables.currAoe.GetComponent<ConeAoe>().aoeData.TargetList;
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
                currTurn.GetComponent<EntityController>().animator.SetTrigger("Basic Attack");
            }
        }

    }

    // Spawns an AOE telegraph for the user
    public GameObject ActivateSkillTelegraph(VisualElement element)
    {
        return SpawnAoe((element.userData as Skill), turnManager.GetCurrentTurn(), turnManager.GetCurrentTurn());
    }

    // When the animation "hits" the target, this event is triggered. Does the specific attack towards this group of targets
    public void OnHit()
    {
        Dictionary<string, GameObject> targetList = battleVariables.targets;

        foreach (var entity in targetList)
        {
            float entityDamage = GlobalDamageHelper.HandleActionCalculation(new ActionInformation(entity.Value, turnManager.GetCurrentTurn(), battleVariables.currSkill));
            DealDamage(entity.Value, entityDamage);
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
                    entityStats.effectHandler.AddEffect(ExcentraDatabase.TryGetStatus(status.key), turnManager.GetCurrentTurn());
            }
        }

        DisplayStatuses(entityStats);
    }

    public void DealDamage(GameObject entity, float entityDamage)
    {
        EntityController entityController = entity.GetComponent<EntityController>();
        EntityStats entityStats = entity.GetComponent<EntityStats>();

        AddStatusToEnemy(entityStats);


        if (battleVariables.currSkill != null && battleVariables.currSkill.damageType == DamageType.DAMAGE || battleVariables.currSkill == null)
        {
            entityController.animator.Play("Damage", -1, 0f);
        }

        if (entityDamage > 0f)  
            ExcentraGame.Instance.damageNumberHandlerScript.SpawnDamageNumber(entity, Mathf.Abs((int)entityDamage));
        entityStats.currentHP = Mathf.Max(entityStats.currentHP - entityDamage, 0);
        if (entityStats.currentHP > entityStats.maximumHP)
            entityStats.currentHP = entityStats.maximumHP;
        hpDictionary[entityStats.entityName].value = entityStats.CalculateHPPercentage();
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
        }
    }

    // ----- BUTTON EVENTS
    public void OnBasicClicked()
    {
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
        else
        {
            ChangeState(BattleState.PLAYER_BASIC);
            controller.basicActive = true;
        }

    }

    public void OnSpecialClicked()
    {

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
            Skill currSkill = ExcentraDatabase.TryGetSkill(skill);

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
                    if ((element.userData as Skill).baseAether > stats.currentAether)
                        return;

                    battleVariables.currSkill = element.userData as Skill;
                    ChangeState(BattleState.PLAYER_SPECIAL);

                    if ((element.userData as Skill).areaStyle == AreaStyle.SINGLE || ((element.userData as Skill).shape == Shape.CIRCLE))
                        controller.specialActive = true;

                    if (specialPanel.style.visibility == Visibility.Visible)
                        specialPanel.style.visibility = Visibility.Hidden;

                    if (NeedsAoe(element.userData as Skill))
                        ActivateSkillTelegraph(element);

                });

                skillScroller.Add(newSkill);
            }
        }
    }

    public void OnMoveClicked()
    {
        if (battleVariables.GetState() == BattleState.PLAYER_CHOICE)
        {
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
        if (battleVariables.battleState == BattleState.PLAYER_CHOICE)
        {
            EndTurn();
        }

    }

    public void OnRestartClicked()
    {
        PostBattleCleanup();
        InitializeBattle(tempCharacterPrefabs, tempBossPrefab, true);
    }
    
    public void OnSkillShot()
    {
        try
        {
            GameObject currEntity = turnManager.GetCurrentTurn();
            EntityStats stats = currEntity.GetComponent<EntityStats>();
            EntityController controller = currEntity.GetComponent<EntityController>();
            GameObject currAoe = aoeArenadata.GetAoe(stats.arenaAoeIndex);
            ConeAoe aoeInit = currAoe.GetComponent<ConeAoe>();

            if (aoeInit.skill.shape == Shape.CIRCLE && aoeInit.skill.targetMode == TargetMode.SELECT)
            {
                if (!CheckWithinSkillRange(currEntity, currAoe, aoeInit.skill))
                    return;
            }

            aoeInit.FreezeAoe();
            BattleClickInfo info = new BattleClickInfo();
            if (aoeInit.skill.shape == Shape.CONE || aoeInit.skill.shape == Shape.LINE)
            {
                info.mousePosition = aoeInit.frozenDestination;
            }
            else if (aoeInit.skill.shape == Shape.CIRCLE)
            {
                info.mousePosition = aoeInit.frozenPosition;
            }

            stats.currentAether = Mathf.Max(stats.currentAether - aoeInit.skill.baseAether, 0);
            mpDictionary[stats.entityName].value = stats.CalculateMPPercentage();
            controller.specialActive = false;

            HandleEntityAction(info);
        } catch (NullReferenceException) { }
    }

    // ------ CLEANED UP PROPER FUNCTIONS ------------

    // Just spawns the AoE. Assumes all checks pass to allow the AoE to spawn
    public GameObject SpawnAoe(Skill skill, GameObject origin, GameObject attacker)
    {
        GameObject aoe = null;
        EntityStats stats = attacker.GetComponent<EntityStats>();

        if (skill.shape == Shape.CONE)
        {
            aoe = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("cone"), origin.transform.position, Quaternion.identity);
            aoe.GetComponent<ConeAoe>().InitializeCone(origin, attacker, skill);
        }
        else if (skill.shape == Shape.CIRCLE)
        {
            aoe = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("circle"), new Vector2(1000, 1000), Quaternion.identity);
            aoe.GetComponent<ConeAoe>().InitializeCircle(origin, attacker, skill);
        }
        else if (skill.shape == Shape.LINE)
        {
            aoe = UnityEngine.GameObject.Instantiate(ExcentraDatabase.TryGetMiscPrefab("line"), origin.transform.position, Quaternion.identity);
            aoe.GetComponent<ConeAoe>().InitializeLine(origin, attacker, skill);
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
        if (stats.arenaAoeIndex == -1)
            return;

        GameObject aoe = aoeArenadata.GetAoe(stats.arenaAoeIndex);
        UnityEngine.GameObject.Destroy(aoe);
        stats.arenaAoeIndex = -1;
    }

    public bool NeedsAoe(Skill skill)
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

        bool sameTeam = attackerStats.isPlayer == defenderStats.isPlayer;

        if (battleVariables.GetState() == BattleState.PLAYER_BASIC)
        {
            return !sameTeam;
        }
        else if (battleVariables.GetState() == BattleState.PLAYER_SPECIAL)
        {
            Skill currSkill = battleVariables.GetCurrentSkill();

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

        return false;
    }

    // Checks within a skill or basic range (combines both types of functions into one)
    // TODO: Might be better to have a range handler that we can pass in some parameters and it auto-calculates a range. Good to centralize things.
    public bool CheckWithinSkillRange(GameObject attacker, GameObject defender, Skill skill = null)
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
}
