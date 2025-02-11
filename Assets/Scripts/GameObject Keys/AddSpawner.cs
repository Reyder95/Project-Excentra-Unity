using UnityEngine;

[System.Serializable]
public class AddSpawner
{
    public string entityKey;
    public string aiKey;
    public Vector2 bottomLeft;
    public Vector2 topRight;
    public bool next = false;
}
