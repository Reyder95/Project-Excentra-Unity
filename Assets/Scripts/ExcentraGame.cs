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

    public static BattleManager battleManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ExcentraDatabase.LoadEntities(entityPrefabs);
        ExcentraDatabase.LoadDocuments(uiDocs);
        ExcentraDatabase.LoadUIAssets(uiSubDocs);
        ExcentraDatabase.LoadAbilities(abilities);

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

    public static void DestroyAoe(GameObject aoe)
    {
        Destroy(aoe);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
