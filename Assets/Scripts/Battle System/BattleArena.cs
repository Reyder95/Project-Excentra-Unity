using UnityEngine;

public class BattleArena
{
    public float leftBound;
    public float rightBound;
    public float topBound;
    public float bottomBound;
    public float width;
    public float height;

    public BattleArena(Vector2 center, float width, float height)
    {
        float halfWidth = width / 2;
        float halfHeight = height / 2;
        this.width = width;
        this.height = height;
        this.leftBound = center.x - halfWidth;
        this.rightBound = center.x + halfWidth;
        this.topBound = center.y + halfHeight;
        this.bottomBound = center.y - halfHeight;
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
