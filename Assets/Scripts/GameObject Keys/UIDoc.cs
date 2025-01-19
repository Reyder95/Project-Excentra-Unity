using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class UIDoc 
{
    public string key;
    public UIDocument document;
}

[Serializable]
public class UIAsset
{
    public string key;
    public VisualTreeAsset document;
}
