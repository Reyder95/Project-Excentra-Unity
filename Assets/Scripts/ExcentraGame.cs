using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcentraGame : MonoBehaviour
{
    [SerializeField]
    List<EntityPrefab> entityPrefabs = new List<EntityPrefab>();

    [SerializeField]
    List<UIDoc> uiDocs = new List<UIDoc>();

    [SerializeField]
    List<UIAsset> uiSubDocs = new List<UIAsset>();

    [SerializeField]
    List<AbilityKey> abilities = new List<AbilityKey>();

    [SerializeField]
    List<StatusEntry> statusEffects = new List<StatusEntry>();

    [SerializeField]
    List<NormalPrefab> miscPrefabs = new List<NormalPrefab>();

    public DamageNumberHandler damageNumberHandlerScript;

    public static ExcentraGame Instance { get; private set; }

    public static BattleManager battleManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        ExcentraDatabase.LoadEntities(entityPrefabs);
        ExcentraDatabase.LoadDocuments(uiDocs);
        ExcentraDatabase.LoadUIAssets(uiSubDocs);
        ExcentraDatabase.LoadAbilities(abilities);
        ExcentraDatabase.LoadStatuses(statusEffects);
        ExcentraDatabase.LoadMiscPrefabs(miscPrefabs);

        battleManager = new BattleManager((prefab, position) => Instantiate(prefab, position, Quaternion.identity));

        // Initialize the four player characters
        List<GameObject> playerCharacters = new List<GameObject>();
        playerCharacters.Add(ExcentraDatabase.TryGetEntity("Rioka"));
        playerCharacters.Add(ExcentraDatabase.TryGetEntity("Nancy"));
        playerCharacters.Add(ExcentraDatabase.TryGetEntity("Penny"));
        playerCharacters.Add(ExcentraDatabase.TryGetEntity("Nono"));

        GameObject boss = ExcentraDatabase.TryGetEntity("Orc");

        battleManager.InitializeBattle(playerCharacters, boss);


        DontDestroyOnLoad(this.gameObject);
    }

    public void WaitCoroutine(float waitTime, System.Action callback)
    {
        StartCoroutine(WaitAndExecute(waitTime, callback));
    }

    public static IEnumerator WaitAndExecute(float waitTime, System.Action callback)
    {
        yield return new WaitForSeconds(waitTime);
        callback?.Invoke();
    }

    public static void DestroyAoe(GameObject aoe)
    {
        Destroy(aoe);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
