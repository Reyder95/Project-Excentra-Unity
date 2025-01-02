using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AoeArenaData
{
    private List<GameObject> aoes = new List<GameObject>();

    public GameObject GetAoe(int index)
    {
        if (index < aoes.Count && index != -1)
            return aoes[index];

        return null;
    }

    public int AddAoe(GameObject aoe)
    {
        Debug.Log("Test!");
        aoes.Add(aoe);

        return aoes.Count - 1;
    }

    public GameObject PopAoe(int index)
    {
        Debug.Log("Index: " + index);
        GameObject aoe = aoes[index];
        aoes.RemoveAt(index);
        return aoe;
    }
}
