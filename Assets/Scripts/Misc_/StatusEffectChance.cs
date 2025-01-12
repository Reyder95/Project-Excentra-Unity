using UnityEngine;

[System.Serializable]
public class StatusEffectChance
{
    public string key;

    [Range(0, 100)]
    public float chance;
}
