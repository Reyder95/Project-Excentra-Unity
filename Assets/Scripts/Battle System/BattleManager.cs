using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class BattleManager
{
    private readonly System.Func<GameObject, Vector2, GameObject> _instantiateFunction;

    private List<GameObject> playerCharacters = new List<GameObject>();
    private GameObject boss = new GameObject();

    private TurnManager turnManager = new TurnManager();
    private BattleVariables battleVariables = new BattleVariables();
    private AoeArenaData aoeArenadata = new AoeArenaData();
    private Dictionary<string, ProgressBar> hpDictionary = new Dictionary<string, ProgressBar>();
    private Dictionary<string, ProgressBar> mpDictionary = new Dictionary<string, ProgressBar>();

    VisualElement charPanel;
    VisualElement controlPanel;
    VisualElement specialPanel;
    ProgressBar bossHP;
    Label stateLabel;

    public BattleManager(System.Func<GameObject, Vector2, GameObject> instantiateFunction)
    {
        _instantiateFunction = instantiateFunction;
    }

    // Start initializing the battle with specific characters
    public void InitializeBattle(List<GameObject> playerCharacters, GameObject boss)
    {
        InstantiateCharacters(playerCharacters, boss);

        turnManager.InitializeTurnManager(this.playerCharacters, this.boss);

        ReferenceUI();

        StartTurn();
    }

    public void ReferenceUI()
    {
        VisualElement battleDoc = ExcentraDatabase.TryGetDocument("battle").rootVisualElement;
        charPanel = battleDoc.Q<VisualElement>("char-panel");
        controlPanel = battleDoc.Q<VisualElement>("control-panel");
        stateLabel = battleDoc.Q<Label>("state-label");
        bossHP = battleDoc.Q<ProgressBar>("boss-hp");
        specialPanel = battleDoc.Q<VisualElement>("special-panel");
        ExcentraGame.Instance.damageNumberHandlerScript.battleUIRoot = battleDoc;

        foreach (var character in playerCharacters)
        {
            EntityStats stats = character.GetComponent<EntityStats>();
            hpDictionary.Add(stats.entityName, charPanel.Q<VisualElement>(stats.entityName.ToLower()).Q<ProgressBar>("hp"));
            mpDictionary.Add(stats.entityName, charPanel.Q<VisualElement>(stats.entityName.ToLower()).Q<ProgressBar>("mp"));
        }
        EntityStats bossStats = boss.GetComponent<EntityStats>();
        hpDictionary.Add(bossStats.entityName, bossHP);

        stateLabel.style.visibility = Visibility.Hidden;

        ChangeState(BattleState.PLAYER_CHOICE);
        SetUIValues();
    }

    public void SetUIValues()
    {
        foreach (var character in playerCharacters)
        {
            EntityStats stats = character.GetComponent<EntityStats>();

            hpDictionary[stats.entityName].value = stats.CalculateHPPercentage();
            mpDictionary[stats.entityName].value = stats.CalculateMPPercentage();
        }

        EntityStats bossHPStats = boss.GetComponent<EntityStats>();
        hpDictionary[bossHPStats.entityName].value = bossHPStats.CalculateHPPercentage();

        controlPanel.Q<Button>("basic-control").clicked += OnBasicClicked;
        controlPanel.Q<Button>("special-control").clicked += OnSpecialClicked;
        controlPanel.Q<Button>("move-control").clicked += OnMoveClicked;
        controlPanel.Q<Button>("end-control").clicked += OnEndClicked;
    }

    // Place the characters down on the screen
    public void InstantiateCharacters(List<GameObject> playerCharacters, GameObject boss)
    {
        foreach (var character in playerCharacters) 
        {
            float x = Random.Range(-17, 0);
            float y = Random.Range(0, 8);
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

    public void StartTurn()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats stats = currTurn.GetComponent<EntityStats>();
        EntityController controller = currTurn.GetComponent<EntityController>();
        PlayerInput input = currTurn.GetComponent<PlayerInput>();

        

        if (stats.isPlayer)
        {
            input.enabled = true;
            controller.turnStartPos = currTurn.transform.position;
            controller.DrawMovementCircle();
            ChangeState(BattleState.PLAYER_CHOICE);
        }
        else
        {
            int randChar = Random.Range(0, playerCharacters.Count);
            currTurn.GetComponent<EntityController>().MoveTowards(playerCharacters[randChar]);
            ChangeState(BattleState.AWAIT_ENEMY);
        }
        
    }

    public void CleanupTurn()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        PlayerInput input = currTurn.GetComponent<PlayerInput>();
        
        battleVariables.attacker = null;
        ChangeState(BattleState.TURN_TRANSITION);
        input.enabled = false;
    }

    public void EndTurn()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats stats = currTurn.GetComponent<EntityStats>();
        EntityController controller = currTurn.GetComponent<EntityController>();

        if (stats.isPlayer)
        {
            controller.basicActive = false;
            battleVariables.isAttacking = false;
            battleVariables.currAbility = null;
            specialPanel.style.visibility = Visibility.Hidden;

            controller.lineRenderer.positionCount = 0;
            if (stats.arenaAoeIndex != -1)
            {
                GameObject aoe = aoeArenadata.PopAoe(stats.arenaAoeIndex);
                ExcentraGame.DestroyAoe(aoe);
                stats.arenaAoeIndex = -1;
            }
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
                        entityController.FaceRight();
                    else
                        entityController.FaceLeft();
                }
                else if (information != null && information.mousePosition != null)
                {
                    if (information.mousePosition.x > currTurn.transform.position.x)
                        entityController.FaceRight();
                    else
                        entityController.FaceLeft();
                }    

                if (battleVariables.battleState == BattleState.PLAYER_BASIC && information != null)
                {
                    battleVariables.target = information.target;
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
                        battleVariables.attacker = currTurn;
                        battleVariables.isAttacking = true;
                        currTurn.GetComponent<EntityController>().animator.SetTrigger("Special Attack");
                    }
                }
            }
            else
            {
                battleVariables.target = information.target;
                battleVariables.attacker = turnManager.GetCurrentTurn();
                currTurn.GetComponent<EntityController>().animator.SetTrigger("Basic Attack");
            }
        }

    }

    public List<GameObject> BuildTargetList()
    {
        List<GameObject> targetList = new List<GameObject>();

        if (battleVariables.currAoe == null)
        {
            targetList.Add(battleVariables.target);
        }
        else
        {
            AoeData aoeData = battleVariables.currAoe.GetComponent<ConeAoe>().aoeData;
            foreach (var entity in aoeData.TargetList)
            {
                targetList.Add(entity.Value);
            }
        }

        return targetList;
    }

    public void OnHit()
    {
        List<GameObject> targetList = BuildTargetList();

        foreach (var entity in targetList)
        {
            EntityController entityController = entity.GetComponent<EntityController>();
            EntityStats entityStats = entity.GetComponent<EntityStats>();

            int entityDamage = (int)GlobalDamageHelper.HandleActionCalculation(new ActionInformation(entity, battleVariables.attacker, battleVariables.currAbility));


            if (battleVariables.currAbility != null && battleVariables.currAbility.damageType == DamageType.DAMAGE || battleVariables.currAbility == null)
            {
                if (entityController.animator.GetCurrentAnimatorStateInfo(0).IsName("Damage"))
                {
                    entityController.animator.Play("Damage", -1, 0f);
                }
                else
                {
                    entityController.animator.SetTrigger("Damage");
                }
            }

            ExcentraGame.Instance.damageNumberHandlerScript.SpawnDamageNumber(entity, Mathf.Abs(entityDamage));
            Debug.Log(entityDamage);
            entityStats.currentHP = Mathf.Max(entityStats.currentHP - entityDamage, 0);
            if (entityStats.currentHP > entityStats.maximumHP)
                entityStats.currentHP = entityStats.maximumHP;
            hpDictionary[entityStats.entityName].value = entityStats.CalculateHPPercentage();
        }

        //EntityStats targetStats = null;
        //if (battleVariables.target)
        //{
        //    targetStats = battleVariables.target.GetComponent<EntityStats>();
        //}

        //EntityStats attackerStats = battleVariables.attacker.GetComponent<EntityStats>();

        //if (GetState() == BattleState.PLAYER_BASIC)
        //{
        //    if (targetStats != null)
        //    {
        //        EntityController controller = battleVariables.target.GetComponent<EntityController>();

        //        if (controller.animator.GetCurrentAnimatorStateInfo(0).IsName("Damage"))
        //        {
        //            controller.animator.Play("Damage", -1, 0f);
        //        }
        //        else
        //        {
        //            controller.animator.SetTrigger("Damage");
        //        }

        //        ExcentraGame.Instance.damageNumberHandlerScript.SpawnDamageNumber(battleVariables.target);
        //        targetStats.currentHP -= (int)GlobalDamageHelper.HandleActionCalculation(new ActionInformation(battleVariables.target, battleVariables.attacker));
        //        bossHP.value = targetStats.CalculateHPPercentage();
        //    }

        //} else if (GetState() == BattleState.PLAYER_SPECIAL)
        //{
        //    ConeAoe aoePrefabData = battleVariables.currAoe.GetComponent<ConeAoe>();
        //    AoeData internalAoeData = aoePrefabData.aoeData;

        //    foreach (var entity in internalAoeData.TargetList)
        //    {
        //        float entityDamage = GlobalDamageHelper.HandleActionCalculation(new ActionInformation(entity.Value, battleVariables.attacker, aoePrefabData.ability, internalAoeData));
        //        Debug.Log(entityDamage);
        //        EntityStats stats = entity.Value.GetComponent<EntityStats>();
        //        EntityController controller = entity.Value.GetComponent<EntityController>();
        //        controller.animator.SetTrigger("Damage");
        //        stats.currentHP -= (int)entityDamage;
        //        hpDictionary[stats.entityName].value = stats.CalculateHPPercentage();
        //    }
        //} else if (GetState() == BattleState.AWAIT_ENEMY)
        //{
        //    EntityController controller = battleVariables.target.GetComponent<EntityController>();
        //    controller.animator.SetTrigger("Damage");
        //    targetStats.currentHP -= Mathf.Max(((int)(attackerStats.attack * 6) - (targetStats.armour / 2)) / attackerStats.basicAttackCount, 1);
        //    charPanel.Q<VisualElement>(targetStats.entityName.ToLower()).Q<ProgressBar>("hp").value = targetStats.CalculateHPPercentage();
        //}

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
                    VisualElement element = (e.currentTarget as VisualElement);
                    if ((element.userData as Ability).baseAether > stats.currentAether)
                        return;

                    ChangeState(BattleState.PLAYER_SPECIAL);
                    GameObject aoe = ActivateAbilityTelegraph(element);
                    int aoeIndex = aoeArenadata.AddAoe(aoe);



                    stats.arenaAoeIndex = aoeIndex;
                });

                skillScroller.Add(newSkill);
            }
        }

    }

    public void OnMoveClicked()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats stats = currTurn.GetComponent<EntityStats>();
        EntityController entityController = currTurn.GetComponent<EntityController>();

        stats.moveDouble = true;

        entityController.DrawMovementCircle();
        
    }

    public void OnEndClicked()
    {
        if (battleVariables.battleState == BattleState.PLAYER_CHOICE)
        {
            EndTurn();
        }
        
    }

    public bool WithinBasicRange(GameObject entity)
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats stats = currTurn.GetComponent<EntityStats>();

        if (Vector2.Distance(currTurn.transform.position, entity.transform.position) < stats.CalculateBasicRangeRadius() / 2)
            return true;

        return false;
    }

    public bool BasicShouldBeHighlighted(EntityStats entity)
    {
        if (GetState() == BattleState.PLAYER_BASIC)
        {
            if (!entity.isPlayer)
                return true;
        }

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
            EntityStats stats = turnManager.GetCurrentTurn().GetComponent<EntityStats>();
            GameObject aoe = aoeArenadata.PopAoe(stats.arenaAoeIndex);
            ExcentraGame.DestroyAoe(aoe);
            stats.arenaAoeIndex = -1;
            ChangeState(BattleState.PLAYER_CHOICE);
            
            specialPanel.style.visibility = Visibility.Visible;
        }
    }
    
    public void OnAbilityShot()
    {
        GameObject currEntity = turnManager.GetCurrentTurn();
        EntityStats stats = currEntity.GetComponent<EntityStats>();
        GameObject currAoe = aoeArenadata.GetAoe(stats.arenaAoeIndex);
        ConeAoe aoeInit = currAoe.GetComponent<ConeAoe>();
        aoeInit.FreezeAoe();
        BattleClickInfo info = new BattleClickInfo();
        if (aoeInit.ability.shape == Shape.CONE)
        {
            info.mousePosition = aoeInit.frozenDestination;
        }
        else if (aoeInit.ability.shape == Shape.CIRCLE)
        {
            info.mousePosition = aoeInit.frozenPosition;
        }

        stats.currentAether = Mathf.Max(stats.currentAether - aoeInit.ability.baseAether, 0);
        mpDictionary[stats.entityName].value = stats.CalculateMPPercentage();
        
        HandleEntityAction(info);
        //Dictionary<string, GameObject> targetList = aoeInit.aoeData.TargetList;

        //foreach (var target in targetList)
        //{
        //    Debug.Log("Hitting " + target.Value.ToString());
        //}
    }

    public GameObject ActivateAbilityTelegraph(VisualElement element)
    {
        if (specialPanel.style.visibility == Visibility.Visible)
            specialPanel.style.visibility = Visibility.Hidden;

        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityController controller = currTurn.GetComponent<EntityController>();
        return controller.InitializeAoe(element.userData as Ability);
    }
}
