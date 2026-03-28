using System.Collections.Generic;
using UnityEngine;

public class QuadtreeNode
{
    public Rect Bounds;
    public bool IsLeaf;
    public List<QuadtreeNode> Children;
    public Vector2Int GridPosition;

    public float Walkability; // 0 = not walkable, 1 = fully walkable

    public QuadtreeNode(Rect bounds, Vector2Int gridPosition)
    {
        Bounds = bounds;
        GridPosition = gridPosition;
        IsLeaf = true;
        Children = new List<QuadtreeNode>();
        Walkability = 0f;
    }

    public bool Contains(Vector3 point)
    {
        return Bounds.Contains(new Vector2(point.x, point.z));
    }
}
