// ExcentraGame.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Our base game class that runs everything necessary.
public class ExcentraGame : MonoBehaviour
{
    // key/value Lists that hold information we will send into our ExcentraDatabase. Easy for inspector use
    [SerializeField]
    List<EntityPrefab> entityPrefabs = new List<EntityPrefab>();

    [SerializeField]
    List<UIDoc> uiDocs = new List<UIDoc>();

    [SerializeField]
    List<UIAsset> uiSubDocs = new List<UIAsset>();

    //[SerializeField]
    //List<AbilityKey> abilities = new List<AbilityKey>();

    [SerializeField]
    List<SkillKey> skills = new List<SkillKey>();

    [SerializeField]
    List<StatusEntry> statusEffects = new List<StatusEntry>();

    [SerializeField]
    List<NormalPrefab> miscPrefabs = new List<NormalPrefab>();

    [SerializeField]
    List<BossEnemyPhases> bossPhases = new List<BossEnemyPhases>();

    public DamageNumberHandler damageNumberHandlerScript;   // Needs to be on monobehaviour so we can place and delete the damage numbers

    public static ExcentraGame Instance { get; private set; }   // Singleton used in very specific circumstances

    public static BattleManager battleManager;  // The battle manager that handles all battles

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Load everything into our database
        ExcentraDatabase.LoadEntities(entityPrefabs);
        ExcentraDatabase.LoadDocuments(uiDocs);
        ExcentraDatabase.LoadUIAssets(uiSubDocs);
        ExcentraDatabase.LoadSkills(skills);
        ExcentraDatabase.LoadStatuses(statusEffects);
        ExcentraDatabase.LoadMiscPrefabs(miscPrefabs);
        ExcentraDatabase.LoadBossPhases(bossPhases);

        // Initialize our battle manager (for the initial battle)
        battleManager = new BattleManager((prefab, position) => Instantiate(prefab, position, Quaternion.identity));

        // Initialize the four player characters
        List<GameObject> playerCharacters = new List<GameObject>();
        playerCharacters.Add(ExcentraDatabase.TryGetEntity("Rioka"));
        playerCharacters.Add(ExcentraDatabase.TryGetEntity("Nancy"));
        playerCharacters.Add(ExcentraDatabase.TryGetEntity("Penny"));
        playerCharacters.Add(ExcentraDatabase.TryGetEntity("Nono"));

        GameObject boss = ExcentraDatabase.TryGetEntity("Orc");

        battleManager.InitializeBattle(playerCharacters, boss, new BattleArena(new Vector2(0, 0), 20f));


        DontDestroyOnLoad(this.gameObject);
    }

    // Helper coroutine function for awaiting (used right now in battle manager to await in between turns)
    public void WaitCoroutine(float waitTime, System.Action callback)
    {
        StartCoroutine(WaitAndExecute(waitTime, callback));
    }

    public static IEnumerator WaitAndExecute(float waitTime, System.Action callback)
    {
        yield return new WaitForSeconds(waitTime);
        callback?.Invoke();
    }

    // TODO: Use UnityEngine.GameObject.Destroy() potentially? Though I forget if that's the exact function
    public static void DestroyAoe(GameObject aoe)
    {
        Destroy(aoe);
    }
}
