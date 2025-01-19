// AoeArenaData.cs

using System;
using System.Collections.Generic;
using UnityEngine;

// Class that handles all the information for AoEs within the arena itself. Will be used very importantly for boss AoEs as well.
// Might have turn support for AoEs as such that can throw them into the turn order (this might be primarily handled by the turnManager class)
public class AoeArenaData
{
    public List<GameObject> aoes = new List<GameObject>();

    public GameObject GetAoe(int index)
    {
        if (index < aoes.Count && index != -1)
            return aoes[index];

        return null;
    }

    public int AddAoe(GameObject aoe)
    {
        aoes.Add(aoe);

        return aoes.Count - 1;
    }

    public GameObject PopAoe(int index)
    {
        try
        {
            GameObject aoe = aoes[index];
            aoes.RemoveAt(index);
            return aoe;
        } catch (ArgumentOutOfRangeException ex)
        {
            Debug.Log(ex.Message);
            return null;
        }

    }
}
