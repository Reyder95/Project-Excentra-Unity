using System.Collections.Generic;
using UnityEngine;

public class AoeDictionary
{
    public Dictionary<int, GameObject> aoes = new Dictionary<int, GameObject>();
    int lastInsertedIndex = -1;

    public GameObject GetAoe(int index)
    {
        if (aoes.ContainsKey(index))
            return aoes[index];
        return null;
    }

    public int AddAoe(GameObject aoe)
    {
        Debug.Log("test!");
        lastInsertedIndex++;
        aoes.Add(lastInsertedIndex, aoe);
        return lastInsertedIndex;
    }

    public GameObject PopAoe(int index)
    {
        if (aoes.ContainsKey(index))
        {
            GameObject aoe = aoes[index];
            aoes.Remove(index);
            return aoe;
        }

        return null;
    }
}
