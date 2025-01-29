using System;
using UnityEngine;

[System.Serializable]
public class EnemyAoeData
{
    // If onSelf, self is origin, and for directional AoEs, endpoint is based on the
    // EntityTargetType. If not onSelf, objectOrigin is based on the EntityTargetType.

    // If stationary AoE, instead of an objectOrigin, we have an origin and an endpoint.

    public bool stationary = true;

    public Shape shape;

    // if stationary
    public Vector2 origin;
    public Vector2 endPoint;
    public float size;

    // if not stationary
    public bool onSelf;
    public bool onTarget;
    public EntityTargetType subTargetType = EntityTargetType.NONE; //None means it inherits the original target type
    [System.NonSerialized]
    public GameObject objectOrigin; // Object origin is always used
    [System.NonSerialized]
    public GameObject objectTarget; // If the item is a directional and directly targeting an entity, this gets filled

    public string prefabKey;    // Can be used to lookup the prefab in the ExcentraDatabase prefab dictionary
}
