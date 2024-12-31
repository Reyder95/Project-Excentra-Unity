using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AoeArenaData
{
    private List<GameObject> aoes = new List<GameObject>();

    public GameObject GetAoe(int index)
    {
        return aoes[index];
    }

    public int AddAoe(GameObject aoe)
    {
        aoes.Add(aoe);

        return aoes.Count - 1;
    }

    public GameObject PopAoe(int index)
    {
        GameObject aoe = aoes[index];
        aoes.RemoveAt(index);
        return aoe;
    }
}
