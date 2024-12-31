using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class ExcentraDatabase
{
    private static Dictionary<string, GameObject> entityDictionary = new Dictionary<string, GameObject>();
    private static Dictionary<string, UIDocument> documentDictionary = new Dictionary<string, UIDocument>();
    private static Dictionary<string, VisualTreeAsset> uiAssetDictionary = new Dictionary<string, VisualTreeAsset>();
    private static Dictionary<string, Ability> abilityDictionary = new Dictionary<string, Ability>();

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

    public static void LoadAbilities(List<AbilityKey> abilities)
    {
        foreach (var ability in abilities)
        {
            abilityDictionary.Add(ability.key, ability.ability);
        }
    }

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

    public static Ability TryGetAbility(string key)
    {
        if (abilityDictionary.ContainsKey(key))
            return abilityDictionary[key];

        return null;
    }
}
