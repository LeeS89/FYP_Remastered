using System.Collections.Generic;
using UnityEngine;

public class ZoneCell
{
    public Rect Bounds { get; private set; }
    public Vector2Int GridPos { get; private set; }
    public float Walkability { get; private set; }
    public float SurfaceY { get; private set; }
    public Vector3 Center => new Vector3(Bounds.center.x, SurfaceY, Bounds.center.y);

    // Add these for precomputed neighbor and visibility data
    public Dictionary<ZoneCell, int> ReachableCellsWithinRange { get; private set; } = new Dictionary<ZoneCell, int>();
    public HashSet<ZoneCell> VisibleCells { get; private set; } = new HashSet<ZoneCell>();

    public ZoneCell(Rect bounds, Vector2Int gridPos, float walkability, float surfaceY)
    {
        Bounds = bounds;
        GridPos = gridPos;
        Walkability = walkability;
        SurfaceY = surfaceY;
    }

    public bool Contains(Vector3 position)
    {
        return Bounds.Contains(new Vector2(position.x, position.z));
    }
    /*public Rect Bounds;                  // The XZ area of this cell
    public Vector2Int GridPosition;      // Grid index
    public float Walkability;            // 0 to 1 value of how much of the cell is on the NavMesh
    public string Name;                  // Auto-generated name like "Cell_5_3"
    public float SurfaceY;               // Height on the NavMesh, used for positioning visuals

    public ZoneCell(Rect bounds, Vector2Int gridPos, float walkability, float surfaceY)
    {
        Bounds = bounds;
        GridPosition = gridPos;
        Walkability = walkability;
        SurfaceY = surfaceY;
        Name = $"Cell_{gridPos.x}_{gridPos.y}";
    }

    public bool Contains(Vector3 point)
    {
        // Only checks the XZ position
        return Bounds.Contains(new Vector2(point.x, point.z));
    }

    public Vector3 Center => new Vector3(Bounds.center.x, SurfaceY, Bounds.center.y);*/
}
