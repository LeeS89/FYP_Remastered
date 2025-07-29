using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class QuadtreeZoneManager : MonoBehaviour
{
    [Header("Map Settings")]
    public Vector2 mapSize = new Vector2(100f, 100f);
    public float minCellSize = 5f;

    [Header("NavMesh Sampling")]
    public float navMeshSampleDistance = 1.0f;
    public float visualY = 0.1f;

    private List<QuadtreeNode> leafNodes = new List<QuadtreeNode>();

    void Start()
    {
        Rect bounds = new Rect(
            transform.position.x - mapSize.x / 2f,
            transform.position.z - mapSize.y / 2f,
            mapSize.x,
            mapSize.y
        );

        var root = new QuadtreeNode(bounds, Vector2Int.zero);
        Subdivide(root, 0);
    }

    void Subdivide(QuadtreeNode node, int depth)
    {
        float walkability = GetWalkability(node.Bounds);

        if (node.Bounds.width <= minCellSize || walkability == 1f)
        {
            if (walkability > 0f)
            {
                node.Walkability = walkability;
                leafNodes.Add(node);
            }
            return;
        }

        Vector2 halfSize = node.Bounds.size / 2f;
        Vector2 origin = node.Bounds.position;
        Vector2Int gridOffset = node.GridPosition * 2;

        for (int i = 0; i < 4; i++)
        {
            Vector2 newPos = origin + new Vector2(i % 2 * halfSize.x, i / 2 * halfSize.y);
            Rect subRect = new Rect(newPos, halfSize);
            Vector2Int gridPos = gridOffset + new Vector2Int(i % 2, i / 2);

            float subWalkability = GetWalkability(subRect);
            if (subWalkability > 0f)
            {
                var child = new QuadtreeNode(subRect, gridPos);
                child.Walkability = subWalkability;
                node.Children.Add(child);
                node.IsLeaf = false;
                Subdivide(child, depth + 1);
            }
        }

        if (node.IsLeaf)
        {
            leafNodes.Add(node);
        }
    }

    float GetWalkability(Rect bounds)
    {
        float y = transform.position.y;
        float radius = navMeshSampleDistance;

        Vector3[] samplePoints = new Vector3[]
        {
            new Vector3(bounds.center.x, y, bounds.center.y),
            new Vector3(bounds.xMin, y, bounds.yMin),
            new Vector3(bounds.xMin, y, bounds.yMax),
            new Vector3(bounds.xMax, y, bounds.yMin),
            new Vector3(bounds.xMax, y, bounds.yMax)
        };

        int walkableCount = 0;
        foreach (var point in samplePoints)
        {
            if (NavMesh.SamplePosition(point, out _, radius, NavMesh.AllAreas))
                walkableCount++;
        }

        return walkableCount / (float)samplePoints.Length;
    }

    public QuadtreeNode GetZoneFromPosition(Vector3 position)
    {
        foreach (var node in leafNodes)
        {
            if (node.Contains(position))
                return node;
        }
        return null;
    }

    void OnDrawGizmosSelected()
    {
        if (leafNodes == null) return;

        foreach (var node in leafNodes)
        {
            float w = node.Walkability;
            Gizmos.color = Color.Lerp(Color.red, Color.green, w);

            Vector3 center = new Vector3(node.Bounds.center.x, transform.position.y + visualY, node.Bounds.center.y);
            Vector3 size = new Vector3(node.Bounds.width, 0.1f, node.Bounds.height);

            Gizmos.DrawCube(center, size);
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
