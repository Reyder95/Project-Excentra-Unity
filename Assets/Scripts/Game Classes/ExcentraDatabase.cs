// ExcentraDatabase.cs

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Massive class for storing important information in memory. Has *everything* needed to be accessed across the game, and is why it is a static class since it does not
// need multiple instances of the same thing
public static class ExcentraDatabase
{
    // Our dictionaries with which we query from
    private static Dictionary<string, GameObject> entityDictionary = new Dictionary<string, GameObject>();
    private static Dictionary<string, UIDocument> documentDictionary = new Dictionary<string, UIDocument>();
    private static Dictionary<string, VisualTreeAsset> uiAssetDictionary = new Dictionary<string, VisualTreeAsset>();
    private static Dictionary<string, PlayerSkill> skillDictionary = new Dictionary<string, PlayerSkill>();
    private static Dictionary<string, StatusEffect> statusDictionary = new Dictionary<string, StatusEffect>();
    private static Dictionary<string, GameObject> miscPrefabDictionary = new Dictionary<string, GameObject>();
    private static Dictionary<string, List<EnemyPhase>> bossPhaseDictionary = new Dictionary<string, List<EnemyPhase>>();

    // Potentially a poor way of doing this. Should this be in the status damage helper class? 
    // Potential future solution: In status helper, use status "effect type" in a dictionary, pointing it to various functions.
    private static Dictionary<StatType, List<string>> statStatusNames = new Dictionary<StatType, List<string>>();

    // Our load functions. We have various "key, object" classes, which we can turn the key into the dictionary key, for ease of use across the game
    public static void LoadEntities(List<EntityPrefab> entities)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            entityDictionary.Add(entities[i].key, entities[i].prefab);
        }
    }

    public static void LoadDocuments(List<UIDoc> documents)
    {
        foreach (var doc in documents)
        {
            documentDictionary.Add(doc.key, doc.document);
        }
    }

    public static void LoadUIAssets(List<UIAsset> uiAssets)
    {
        foreach(var asset in uiAssets)
        {
            uiAssetDictionary.Add(asset.key, asset.document);
        }
    }

    public static void LoadSkills(List<SkillKey> skills)
    {
        foreach (var skill in skills)
        {
            skillDictionary.Add(skill.key, skill.skill);
        }
    }

    public static void LoadStatuses(List<StatusEntry> statusEffects)
    {
        foreach (var status in statusEffects)
        {
            status.effect.key = status.key; // Binds outer key to inner key

            // Set up a separate dictionary to link a stat to a key
            if (!statStatusNames.ContainsKey(status.effect.statType))
                statStatusNames.Add(status.effect.statType, new List<string>() { status.effect.key });
            else
                statStatusNames[status.effect.statType].Add(status.effect.key);

            statusDictionary.Add(status.key, status.effect);
        }
    }

    public static void LoadMiscPrefabs(List<NormalPrefab> miscPrefabs)
    {
        foreach (var miscPrefab in miscPrefabs)
        {
            miscPrefabDictionary.Add(miscPrefab.key, miscPrefab.prefab);
        }
    }

    public static void LoadBossPhases(List<BossEnemyPhases> bossPhases)
    {
        foreach (var bossPhase in bossPhases)
        {
            bossPhaseDictionary.Add(bossPhase.key, bossPhase.phases);
        }
    }

    // Our tryget functions. These attempt to get a specific dictionary's contents through a key. If it fails, returns null.
    public static GameObject TryGetEntity(string key)
    {
        if (entityDictionary.ContainsKey(key))
            return entityDictionary[key];

        return null;
    }

    public static UIDocument TryGetDocument(string key)
    {
        if (documentDictionary.ContainsKey(key))
            return documentDictionary[key];

        return null;
    }

    public static VisualTreeAsset TryGetSubDocument(string key)
    {
        if (uiAssetDictionary.ContainsKey(key))
            return uiAssetDictionary[key];

        return null;
    }

    public static PlayerSkill TryGetSkill(string key)
    {
        if (skillDictionary.ContainsKey(key))
            return skillDictionary[key];

        return null;
    }

    public static StatusEffect TryGetStatus(string key)
    {
        if (statusDictionary.ContainsKey(key))
            return statusDictionary[key];

        return null;
    }

    public static List<string> TryGetStatusNames(StatType key)
    {
        if (statStatusNames.ContainsKey(key))
            return statStatusNames[key];

        return new List<string>();
    }

    public static GameObject TryGetMiscPrefab(string key)
    {
        if (miscPrefabDictionary.ContainsKey(key))
            return miscPrefabDictionary[key];

        return null;
    }

    public static List<EnemyPhase> TryGetBossPhases(string key)
    {
        if (bossPhaseDictionary.ContainsKey(key))
            return bossPhaseDictionary[key];
        return null;
    }
}
