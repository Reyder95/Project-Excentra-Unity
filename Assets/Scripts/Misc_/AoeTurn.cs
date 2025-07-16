using UnityEngine;
using System.Collections.Generic;

public class AoeTurn
{
    public List<GameObject> aoes = new List<GameObject>();

    public AoeTurn(List<GameObject> aoes)
    {
        this.aoes = aoes;
    }

    public void AddAoe(GameObject aoe)
    {
        if (aoe != null && !aoes.Contains(aoe))
        {
            aoes.Add(aoe);
        }
    }
}
