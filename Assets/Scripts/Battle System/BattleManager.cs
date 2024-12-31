using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class BattleManager
{
    private readonly System.Func<GameObject, Vector2, GameObject> _instantiateFunction;

    private List<GameObject> playerCharacters = new List<GameObject>();
    private GameObject boss = new GameObject();

    private TurnManager turnManager = new TurnManager();
    private BattleVariables battleVariables = new BattleVariables();
    private AoeArenaData aoeArenadata = new AoeArenaData();

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

        stateLabel.style.visibility = Visibility.Hidden;

        ChangeState(BattleState.PLAYER_CHOICE);
        SetUIValues();
    }

    public void SetUIValues()
    {
        foreach (var character in playerCharacters)
        {
            EntityStats stats = character.GetComponent<EntityStats>();

            charPanel.Q<VisualElement>(stats.entityName.ToLower()).Q<ProgressBar>("hp").value = stats.CalculateHPPercentage();
            charPanel.Q<VisualElement>(stats.entityName.ToLower()).Q<ProgressBar>("mp").value = stats.CalculateMPPercentage();
        }

        EntityStats bossHPStats = boss.GetComponent<EntityStats>();
        bossHP.value = bossHPStats.CalculateHPPercentage();

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

    public void EndTurn()
    {
        GameObject currTurn = turnManager.GetCurrentTurn();
        EntityStats stats = currTurn.GetComponent<EntityStats>();
        EntityController controller = currTurn.GetComponent<EntityController>();

        if (stats.isPlayer)
        {
            controller.basicActive = false;
            battleVariables.isAttacking = false;
            specialPanel.style.visibility = Visibility.Hidden;

            controller.lineRenderer.positionCount = 0;
            if (stats.arenaAoeIndex != -1)
            {
                GameObject aoe = aoeArenadata.PopAoe(stats.arenaAoeIndex);
                ExcentraGame.DestroyAoe(aoe);
                stats.arenaAoeIndex = -1;
            }
        }
        

        turnManager.EndCurrentTurn();
        StartTurn();
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

    public void OnHit()
    {
        EntityStats targetStats = null;
        if (battleVariables.target)
        {
            targetStats = battleVariables.target.GetComponent<EntityStats>();
        }
        
        EntityStats attackerStats = battleVariables.attacker.GetComponent<EntityStats>();

        if (GetState() == BattleState.PLAYER_BASIC)
        {
            if (targetStats != null)
            {
                targetStats.currentHP -= Mathf.Max(((int)(attackerStats.attack * 6) - (targetStats.armour / 2)) / attackerStats.basicAttackCount, 1);
                bossHP.value = targetStats.CalculateHPPercentage();
            }

        } else if (GetState() == BattleState.PLAYER_SPECIAL)
        {
            ConeAoe aoePrefabData = battleVariables.currAoe.GetComponent<ConeAoe>();
            AoeData internalAoeData = aoePrefabData.aoeData;

            foreach (var entity in internalAoeData.TargetList)
            {
                EntityStats stats = entity.Value.GetComponent<EntityStats>();
                stats.currentHP = Mathf.Max(((int)(attackerStats.attack * 6) - (stats.armour / 2)) / attackerStats.basicAttackCount, 1);
                bossHP.value = stats.CalculateHPPercentage();
            }
        } else if (GetState() == BattleState.AWAIT_ENEMY)
        {
            targetStats.currentHP -= Mathf.Max(((int)(attackerStats.attack * 6) - (targetStats.armour / 2)) / attackerStats.basicAttackCount, 1);
            charPanel.Q<VisualElement>(targetStats.entityName.ToLower()).Q<ProgressBar>("hp").value = targetStats.CalculateHPPercentage();
        }

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
                    VisualElement element = (e.currentTarget as VisualElement);

                    ChangeState(BattleState.PLAYER_SPECIAL);
                    GameObject aoe = ActivateAbilityTelegraph(element);
                    int aoeIndex = aoeArenadata.AddAoe(aoe);

                    EntityStats stats = turnManager.GetCurrentTurn().GetComponent<EntityStats>();
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
        EndTurn();
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
        info.mousePosition = aoeInit.frozenDestination;
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
