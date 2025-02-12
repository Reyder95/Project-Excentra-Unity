using UnityEngine;

public enum ArenaPositionType
{
    CENTER,
    LEFT_HALF,
    RIGHT_HALF,
    TOP_HALF,
    BOTTOM_HALF,
    NONE
}

[System.Serializable]
public class MechanicAoePositionHelper
{
    public ArenaPositionType positionType;
    public Vector2 offset;
    public float leftPadding;
    public float rightPadding;
    public float topPadding;
    public float bottomPadding;
    public bool isHalfSize = false;
}
