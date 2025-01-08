using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class BattleManager
{
    private readonly System.Func<GameObject, Vector2, GameObject> _instantiateFunction;

    private List<GameObject> playerCharacters = new List<GameObject>();
    private GameObject boss = new GameObject();

    private TurnManager turnManager = new TurnManager();
    private BattleVariables battleVariables = new BattleVariables();
    public AoeArenaData aoeArenadata = new AoeArenaData();
    private Dictionary<string, ProgressBar> hpDictionary = new Dictionary<string, ProgressBar>();
    private Dictionary<string, ProgressBar> mpDictionary = new Dictionary<string, ProgressBar>();
    private Dictionary<string, VisualElement> debuffScrollers = new Dictionary<string, VisualElement>();

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

        if (HandleTurnStatuses())
        {
            EndTurn();
            return;
        }

        if (stats.isPlayer)
        {
            input.enabled = true;
            controller.turnStartPos = currTurn.transform.position;
            controller.DrawMovementCircle();
            ChangeState(BattleState.PLAYER_CHOICE);
        }
        else
        {
            var possibleChars = playerCharacters.Where(go => go.GetComponent<EntityStats>() != null && go.GetComponent<EntityStats>().currentHP > 0).ToList();
            int randChar = UnityEngine.Random.Range(0, possibleChars.Count);
            currTurn.GetComponent<EntityController>().MoveTowards(possibleChars[randChar]);
            ChangeState(BattleState.AWAIT_ENEMY);
        }
        
    }

    public void CleanupTurn()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats currStats = currTurn.GetComponent<EntityStats>();
        EntityController controller = currTurn.GetComponent<EntityController>();
        PlayerInput input = currTurn.GetComponent<PlayerInput>();

        currStats.moveDouble = false;

        battleVariables.attacker = null;
        battleVariables.targets = null;
        battleVariables.isAttacking = false;
        battleVariables.currAbility = null;

        controller.specialActive = false;
        

        ChangeState(BattleState.TURN_TRANSITION);
        input.enabled = false;
    }

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

    public void EndTurn()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats stats = currTurn.GetComponent<EntityStats>();
        EntityController controller = currTurn.GetComponent<EntityController>();
        bool isRevive = false;

        if (battleVariables.currAbility != null && battleVariables.currAbility.damageType == DamageType.REVIVE)
        {
            isRevive = true;
        }

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


        if (stats.isPlayer)
        {
            controller.basicActive = false;

            specialPanel.style.visibility = Visibility.Hidden;

            controller.lineRenderer.positionCount = 0;
            DestroyAoe(currTurn);
        }

        CleanupTurn();
        ExcentraGame.Instance.WaitCoroutine(0.5f, () =>
        {
            turnManager.EndCurrentTurn();
            StartTurn();
        });
    }

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
                    battleVariables.attacker = currTurn;
                    battleVariables.isAttacking = true;
                    currTurn.GetComponent<EntityController>().animator.SetTrigger("Basic Attack");
                }
                else if (battleVariables.battleState == BattleState.PLAYER_SPECIAL)
                {
                    if (stats.arenaAoeIndex != -1)
                    {
                        battleVariables.currAoe = aoeArenadata.GetAoe(stats.arenaAoeIndex);
                        battleVariables.currAbility = aoeArenadata.GetAoe(stats.arenaAoeIndex).GetComponent<ConeAoe>().ability;
                        battleVariables.targets = battleVariables.currAoe.GetComponent<ConeAoe>().aoeData.TargetList;
                        battleVariables.attacker = currTurn;
                        battleVariables.isAttacking = true;
                        currTurn.GetComponent<EntityController>().animator.SetTrigger("Special Attack");
                    }
                    else
                    {
                        battleVariables.currAbility = information.singleAbility;
                        battleVariables.targets = new Dictionary<string, GameObject>() { { information.target.GetComponent<EntityStats>().entityName, information.target } };
                        battleVariables.attacker = currTurn;
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
                battleVariables.attacker = turnManager.GetCurrentTurn();
                currTurn.GetComponent<EntityController>().animator.SetTrigger("Basic Attack");
            }
        }

    }

    public void OnHit()
    {
        Dictionary<string, GameObject> targetList = battleVariables.targets;

        foreach (var entity in targetList)
        {
            float entityDamage = GlobalDamageHelper.HandleActionCalculation(new ActionInformation(entity.Value, battleVariables.attacker, battleVariables.currAbility));
            DealDamage(entity.Value, entityDamage);
        }

    }

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
        if (battleVariables.currAbility != null)
        {
            foreach (var status in battleVariables.currAbility.statusEffect)
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


        if (battleVariables.currAbility != null && battleVariables.currAbility.damageType == DamageType.DAMAGE || battleVariables.currAbility == null)
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

    public BattleState GetState()
    {
        return battleVariables.battleState;
    }

    public Ability GetCurrentAbility()
    {
        if (GetState() == BattleState.PLAYER_SPECIAL)
        {
            return battleVariables.currAbility;
        }

        return null;
    }

    public GameObject GetCurrentAttacker()
    {
        return turnManager.GetCurrentTurn();
    }

    public void SetCurrentAoe(GameObject aoe)
    {
        battleVariables.currAoe = aoe;
    }

    public GameObject GetCurrentAoe()
    {
        return battleVariables.currAoe;
    }

    public bool IsEntityAttacking()
    {
        return battleVariables.isAttacking;
    }

    public void DeleteCurrentAoe()
    {
        battleVariables.currAoe = null;
    }

    public void OnEnableBasic()
    {
        ChangeState(BattleState.PLAYER_BASIC);
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityController controller = currTurn.GetComponent<EntityController>();

        controller.basicActive = true;
    }

    public void OnDisableBasic()
    {
        ChangeState(BattleState.PLAYER_CHOICE);
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityController controller = currTurn.GetComponent<EntityController>();

        controller.basicActive = false;
    }

    public void OnBasicClicked()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats currStats = currTurn.GetComponent<EntityStats>();

        if (currStats.moveDouble)
            return;

        if (battleVariables.battleState == BattleState.PLAYER_BASIC)
        {
            OnDisableBasic();
        } 
        else
        {
            OnEnableBasic();
        }
        
    }

    public void OnSpecialClicked()
    {

        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityController controller = currTurn.GetComponent<EntityController>();
        EntityStats currStats = currTurn.GetComponent<EntityStats>();

        if (currStats.moveDouble)
            return;

        //GameObject cone = _instantiateFunction(controller.coneAoe, currTurn.transform.position);

        VisualTreeAsset itemAsset = ExcentraDatabase.TryGetSubDocument("skill-item"); 
        VisualElement skillScroller = specialPanel.Q<VisualElement>("skill-scroller");

        controller.basicActive = false;

        if (specialPanel.style.visibility != Visibility.Visible)
            specialPanel.style.visibility = Visibility.Visible;
        else
            specialPanel.style.visibility = Visibility.Hidden;

        skillScroller.Clear();

        foreach (string ability in currStats.abilityKeys)
        {
            Ability currAbility = ExcentraDatabase.TryGetAbility(ability);

            if (currAbility != null)
            {
                VisualElement newSkill = itemAsset.CloneTree();
                newSkill.Q<Label>("skill-name").text = currAbility.abilityName;
                newSkill.Q<VisualElement>("image").style.backgroundImage = new StyleBackground(currAbility.icon);
                newSkill.Q<Label>("skill-cost").text = currAbility.baseAether + " Aether";
                newSkill.userData = currAbility;

                newSkill.RegisterCallback<ClickEvent>(e =>
                {
                    EntityStats stats = turnManager.GetCurrentTurn().GetComponent<EntityStats>();
                    EntityController controller = turnManager.GetCurrentTurn().GetComponent<EntityController>();
                    VisualElement element = (e.currentTarget as VisualElement);
                    if ((element.userData as Ability).baseAether > stats.currentAether)
                        return;

                    battleVariables.currAbility = element.userData as Ability;
                    ChangeState(BattleState.PLAYER_SPECIAL);

                    if ((element.userData as Ability).areaStyle == AreaStyle.SINGLE || ((element.userData as Ability).shape == Shape.CIRCLE))
                        controller.specialActive = true;

                    if (specialPanel.style.visibility == Visibility.Visible)
                        specialPanel.style.visibility = Visibility.Hidden;

                    if (NeedsAoe(element.userData as Ability))
                        ActivateAbilityTelegraph(element);

                });

                skillScroller.Add(newSkill);
            }
        }
    }

    public void OnMoveClicked()
    {
        if (GetState() == BattleState.PLAYER_CHOICE)
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
        if (GetState() == BattleState.PLAYER_SPECIAL)
        {
            EntityController controller = turnManager.GetCurrentTurn().GetComponent<EntityController>();
            DestroyAoe(turnManager.GetCurrentTurn());
            battleVariables.currAbility = null;
            controller.specialActive = false;
            ChangeState(BattleState.PLAYER_CHOICE);
            specialPanel.style.visibility = Visibility.Visible;
        }
    }

    public void OnRestartClicked()
    {
        PostBattleCleanup();
        InitializeBattle(tempCharacterPrefabs, tempBossPrefab, true);
    }
    
    public void OnAbilityShot()
    {
        try
        {
            GameObject currEntity = turnManager.GetCurrentTurn();
            EntityStats stats = currEntity.GetComponent<EntityStats>();
            EntityController controller = currEntity.GetComponent<EntityController>();
            GameObject currAoe = aoeArenadata.GetAoe(stats.arenaAoeIndex);
            ConeAoe aoeInit = currAoe.GetComponent<ConeAoe>();

            if (aoeInit.ability.shape == Shape.CIRCLE && aoeInit.ability.targetMode == TargetMode.SELECT)
            {
                if (!CheckWithinSkillRange(currEntity, currAoe, aoeInit.ability))
                    return;
            }

            aoeInit.FreezeAoe();
            BattleClickInfo info = new BattleClickInfo();
            if (aoeInit.ability.shape == Shape.CONE || aoeInit.ability.shape == Shape.LINE)
            {
                info.mousePosition = aoeInit.frozenDestination;
            }
            else if (aoeInit.ability.shape == Shape.CIRCLE)
            {
                info.mousePosition = aoeInit.frozenPosition;
            }

            stats.currentAether = Mathf.Max(stats.currentAether - aoeInit.ability.baseAether, 0);
            mpDictionary[stats.entityName].value = stats.CalculateMPPercentage();
            controller.specialActive = false;

            HandleEntityAction(info);
        } catch (NullReferenceException) { }
    }

    public GameObject ActivateAbilityTelegraph(VisualElement element)
    {
        return SpawnAoe((element.userData as Ability), GetCurrentAttacker(), GetCurrentAttacker());
    }

    // ------ CLEANED UP PROPER FUNCTIONS ------------

    // Just spawns the AoE. Assumes all checks pass to allow the AoE to spawn
    public GameObject SpawnAoe(Ability skill, GameObject origin, GameObject attacker)
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

    public bool NeedsAoe(Ability skill)
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
        debuffScrollers.Clear();
        battleVariables = new BattleVariables();
        turnManager = new TurnManager();
    }

    public bool TargetingEligible(GameObject attacker, GameObject defender)
    {
        EntityStats attackerStats = attacker.GetComponent<EntityStats>();
        EntityStats defenderStats = defender.GetComponent<EntityStats>();

        bool sameTeam = attackerStats.isPlayer == defenderStats.isPlayer;

        if (GetState() == BattleState.PLAYER_BASIC)
        {
            return !sameTeam;
        }
        else if (GetState() == BattleState.PLAYER_SPECIAL)
        {
            Ability currAbility = GetCurrentAbility();

            if (currAbility != null)
            {
                EntityType entityType = currAbility.entityType;

                if (entityType == EntityType.ALLY)
                {
                    bool revive = false;
                    

                    if (currAbility.damageType == DamageType.REVIVE)
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

    public bool CheckWithinSkillRange(GameObject attacker, GameObject defender, Ability skill = null)
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
