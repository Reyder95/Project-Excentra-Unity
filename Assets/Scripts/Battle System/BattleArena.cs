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

    public Vector2 GetLeftCenter()
    {
        return new Vector2((leftBound + GetCenter().x) / 2, GetCenter().y);
    }

    public Vector2 GetRightCenter()
    {
        return new Vector2((rightBound + GetCenter().x) / 2, GetCenter().y);
    }

    public Vector2 GetTopCenter()
    {
        return new Vector2(GetCenter().x, (topBound + GetCenter().y) / 2);
    }

    public Vector2 GetBottomCenter()
    {
        return new Vector2(GetCenter().x, (bottomBound + GetCenter().y) / 2);
    }

    public float GetHalfSize(ArenaPositionType positionType)
    {
        switch (positionType)
        {
            case ArenaPositionType.LEFT_HALF:
                return Mathf.Abs(leftBound - GetCenter().x);
            case ArenaPositionType.RIGHT_HALF:
                return Mathf.Abs(GetCenter().x - rightBound);
            case ArenaPositionType.TOP_HALF:
                return Mathf.Abs(topBound - GetCenter().y);
            case ArenaPositionType.BOTTOM_HALF:
                return Mathf.Abs(GetCenter().y - bottomBound);
        }

        return 0f;
    }
}
