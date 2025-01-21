using UnityEngine;

public class BattleArena
{
    public float leftBound;
    public float rightBound;
    public float topBound;
    public float bottomBound;

    public BattleArena(Vector2 center, float size)
    {
        float halfSize = size / 2;
        this.leftBound = center.x - halfSize;
        this.rightBound = center.x + halfSize;
        this.topBound = center.y + halfSize;
        this.bottomBound = center.y - halfSize;
    }

    public bool IsInsideArena(Vector2 position)
    {
        return position.x >= leftBound && position.x <= rightBound && position.y >= bottomBound && position.y <= topBound;
    }

    public Vector2 GetCenter()
    {
        float centerX = (leftBound + rightBound) / 2;
        float centerY = (topBound + bottomBound) / 2;
        return new Vector2(centerX, centerY);
    }
}
