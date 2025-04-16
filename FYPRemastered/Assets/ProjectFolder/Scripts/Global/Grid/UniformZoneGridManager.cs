// UniformZoneGridManager.cs - NavMesh Aware Grid with Sample Cube Centers as Cells
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif
using TMPro;
/*#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ZoneSource
{
    public GameObject sourceObject;
    public float cellSize = 5f;
}*/

public class UniformZoneGridManager : MonoBehaviour
{
    [Header("Cube Placement")]
    public float cellSize = 1f;
    public float sampleRadius = 2f;
    public GameObject markerPrefab;

    [Header("Step Logic")]
    public float stepSize = 1f; // Step bands: step 2 = 2–2.99, etc.
    public int maxSteps = 30;


    [HideInInspector] public List<Transform> manualSamplePoints = new List<Transform>();
    public List<SamplePointData> savedPoints = new List<SamplePointData>();

    // ----------------------------------------
    // Auto Place Cubes (NavMesh-aware)
    // ----------------------------------------
    [ContextMenu("Auto Place Sample Cubes on NavMesh")]
    public void AutoPlaceCubesOnNavMesh()
    {
        ClearExistingSampleCubes();

        Renderer rend = GetComponent<Renderer>();
        if (rend == null || markerPrefab == null)
        {
            Debug.LogError("Missing Renderer or Marker Prefab");
            return;
        }

        Bounds bounds = rend.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        int numX = Mathf.CeilToInt((max.x - min.x) / cellSize);
        int numZ = Mathf.CeilToInt((max.z - min.z) / cellSize);

        for (int x = 0; x < numX; x++)
        {
            for (int z = 0; z < numZ; z++)
            {
                float posX = min.x + x * cellSize + cellSize * 0.5f;
                float posZ = min.z + z * cellSize + cellSize * 0.5f;
                Vector3 sample = new Vector3(posX, bounds.max.y, posZ);

                if (NavMesh.SamplePosition(sample, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
                {
                    GameObject cube = Instantiate(markerPrefab, hit.position, Quaternion.identity);
                    cube.name = $"SampleNav_{x}_{z}";
                    cube.transform.SetParent(transform);
                    manualSamplePoints.Add(cube.transform);
                }
            }
        }

        Debug.LogError($"Placed {manualSamplePoints.Count} sample cubes on NavMesh.");
    }

    // ----------------------------------------
    // Collect cubes manually edited in scene
    // ----------------------------------------
    [ContextMenu("Collect Sample Cubes")]
    public void CollectSampleCubes()
    {
        manualSamplePoints.Clear();

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                manualSamplePoints.Add(child);
            }
        }

        Debug.LogError($"Collected {manualSamplePoints.Count} sample cubes.");
    }

    [ContextMenu("Clear Sample Cubes")]
    private void ClearExistingSampleCubes()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        manualSamplePoints.Clear();
        Debug.LogError("Cleared existing sample cubes.");
    }

    // ----------------------------------------
    // Generate data structure with step distances
    // ----------------------------------------
    void Start()
    {
        GenerateSamplePointData();
    }

    [Header("Runtime Info (Debug / Optional)")]
    public Transform player; // Assign in inspector

    public int nearestPointToPlayer = -1;

    public Transform nearestPointTransform;

    public void GenerateSamplePointData()
    {
        savedPoints.Clear();

        // Store each cube's position
        foreach (Transform point in manualSamplePoints)
        {
            savedPoints.Add(new SamplePointData
            {
                position = point.position
            });
        }

        int count = savedPoints.Count;

        for (int i = 0; i < count; i++)
        {
            var from = savedPoints[i];

            for (int j = 0; j < count; j++)
            {
                if (i == j) continue;

                var to = savedPoints[j];
                float dist = Vector3.Distance(from.position, to.position);

                int step;
                if (dist < 2f)
                {
                    step = 1;
                }
                else
                {
                    step = Mathf.FloorToInt((dist - 2f) / stepSize) + 2;
                }

                if (step <= maxSteps)
                {
                    if (!from.reachableSteps.ContainsKey(step))
                        from.reachableSteps[step] = new List<int>();

                    from.reachableSteps[step].Add(j);
                }
            }
        }

        Debug.LogError($"Generated {savedPoints.Count} points with step-grouped distances.");
        // --- Optional: Find Closest Point to Player on Scene Start ---
#if UNITY_EDITOR
        if (player != null)
        {
            float closestSqrDist = float.MaxValue;
            int closestIndex = -1;

            for (int i = 0; i < savedPoints.Count; i++)
            {
                float sqrDist = (savedPoints[i].position - player.position).sqrMagnitude;
                if (sqrDist < closestSqrDist)
                {
                    closestSqrDist = sqrDist;
                    closestIndex = i;
                }
            }

            nearestPointToPlayer = closestIndex;
            nearestPointTransform = manualSamplePoints[closestIndex];

            Debug.Log($"[Debug] Nearest point to player: index {nearestPointToPlayer} at {savedPoints[nearestPointToPlayer].position}");
        }
        else
        {
            Debug.LogWarning("[Debug] Player reference not set — can't find nearest point.");
        }
#endif
        // Clean up visual cubes after data is generated
        /*foreach (Transform point in manualSamplePoints)
        {
            Destroy(point.gameObject);
        }*/

        // manualSamplePoints.Clear();
    }

    public Vector3 GetRandomPointXStepsFromPlayer(int step)
    {
        if (savedPoints == null || savedPoints.Count == 0)
        {
            Debug.LogWarning("No saved points available.");
            return transform.position;
        }

        if (nearestPointToPlayer < 0 || nearestPointToPlayer >= savedPoints.Count)
        {
            Debug.LogWarning("Nearest point to player not set or invalid.");
            return transform.position;
        }

        if (savedPoints[nearestPointToPlayer].reachableSteps.TryGetValue(step, out var candidates) && candidates.Count > 0)
        {
            int randomIndex = candidates[Random.Range(0, candidates.Count)];
            return savedPoints[randomIndex].position;
        }

        Debug.LogWarning($"No points found {step} steps away from player's nearest point.");
        return savedPoints[nearestPointToPlayer].position;
    }

    /*    public Vector3 GetRandomValidPoint()
        {
            if (savedPoints == null || savedPoints.Count == 0)
            {
                Debug.LogWarning("No saved points available.");
                return transform.position;
            }

            return savedPoints[Random.Range(0, savedPoints.Count)].position;
        }*/


    [ContextMenu("Visualize Step Counts From Selected Cube (Play Mode Only)")]
    public void VisualizeStepsFromSelectedCube()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Debug.LogError("This visualization only works in Play Mode.");
            return;
        }

        Transform selected = Selection.activeTransform;
        if (selected == null)
        {
            Debug.LogError("Please select a cube in the Scene.");
            return;
        }

        int selectedIndex = manualSamplePoints.IndexOf(selected);
        if (selectedIndex == -1)
        {
            Debug.LogError("Selected object is not a registered sample point.");
            return;
        }

        var selectedData = savedPoints[selectedIndex];

        // Clear existing labels first
        foreach (Transform cube in manualSamplePoints)
        {
            var existing = cube.GetComponentInChildren<TextMesh>();
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }
        }

        // Loop through all reachableSteps
        foreach (var kvp in selectedData.reachableSteps)
        {
            int step = kvp.Key;
            foreach (int targetIndex in kvp.Value)
            {
                Transform targetCube = manualSamplePoints[targetIndex];

                GameObject textObj = new GameObject("StepLabel");
                textObj.transform.SetParent(targetCube);
                textObj.transform.localPosition = Vector3.up * 1.2f;

                TextMesh text = textObj.AddComponent<TextMesh>();
                text.text = step.ToString();
                text.characterSize = 0.2f;
                text.fontSize = 32;
                text.color = Color.yellow;
                text.alignment = TextAlignment.Center;
                text.anchor = TextAnchor.MiddleCenter;
            }
        }

        Debug.LogError("Visualized step counts from selected cube.");
#endif
    }


    /*[Header("Manual Sample Points")]
    public List<Transform> manualSamplePoints = new List<Transform>();
    public float cellSize = 1f;
    public float sampleRadius = 2f;
    public float walkabilityThreshold = 0.2f;
    public GameObject markerPrefab;

    private Dictionary<Vector2Int, ZoneCell> cells = new Dictionary<Vector2Int, ZoneCell>();
    public IReadOnlyDictionary<Vector2Int, ZoneCell> Cells => cells;

    private List<GameObject> savedCubes = new List<GameObject>();

    void Start()
    {
        GenerateZoneGridFromManualCubes();
        Debug.Log($"Generated {cells.Count} zone cells.");
    }

    [ContextMenu("Collect Sample Cubes")]
    public void CollectEnabledCubes()
    {
        manualSamplePoints.Clear();

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                manualSamplePoints.Add(child);
            }
        }

        Debug.LogError($"Collected {manualSamplePoints.Count} active sample points.");
    }

    [ContextMenu("Auto Place Sample Cubes on NavMesh")]
    public void AutoPlaceCubesOnNavMesh()
    {
        ClearExistingSampleCubes();

        Renderer rend = GetComponent<Renderer>();
        if (rend == null || markerPrefab == null)
        {
            Debug.LogWarning("Missing Renderer or Marker Prefab");
            return;
        }

        Bounds bounds = rend.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        int numX = Mathf.CeilToInt((max.x - min.x) / cellSize);
        int numZ = Mathf.CeilToInt((max.z - min.z) / cellSize);

        for (int x = 0; x < numX; x++)
        {
            for (int z = 0; z < numZ; z++)
            {
                float posX = min.x + x * cellSize + cellSize * 0.5f;
                float posZ = min.z + z * cellSize + cellSize * 0.5f;
                Vector3 sample = new Vector3(posX, bounds.max.y, posZ);

                if (NavMesh.SamplePosition(sample, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
                {
                    Vector3 fixedPosition = new Vector3(posX, hit.position.y, posZ);
                    GameObject cube = Instantiate(markerPrefab, hit.position, Quaternion.identity);
                    cube.name = $"SampleNav_{x}_{z}";
                    cube.transform.localScale = markerPrefab.transform.localScale;
                    cube.transform.SetParent(transform);
                    manualSamplePoints.Add(cube.transform);
                    savedCubes.Add(cube);
                }
            }
        }

        Debug.Log($"Placed {manualSamplePoints.Count} sample cubes on NavMesh.");
    }

    [ContextMenu("Clear Sample Cubes")]
    public void ClearExistingSampleCubes()
    {
        foreach (Transform child in transform)
        {
            savedCubes.Add(child.gameObject);
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        manualSamplePoints.Clear();
        Debug.Log("Cleared sample cubes and manual points, but saved them in memory.");
    }

    [ContextMenu("Recalculate Grid Relations")]
    public void RecalculateCellData()
    {
        if (manualSamplePoints.Count == 0)
        {
            Debug.LogWarning("No manual sample points found. Place or collect them first.");
            return;
        }

        GenerateZoneGridFromManualCubes();
        Debug.Log("Recalculated reachability and visibility between cells.");
    }

    [ContextMenu("Visualize Visibility From Selected Cube")]
    public void VisualizeVisibilityFromSelectedCube()
    {
#if UNITY_EDITOR
        if (Selection.activeTransform == null)
        {
            Debug.LogWarning("Select a cube (sample point) in the scene.");
            return;
        }

        Transform selected = Selection.activeTransform;
        ZoneCell selectedCell = GetZoneFromPosition(selected.position);
        if (selectedCell == null)
        {
            Debug.LogWarning("Selected object is not inside any ZoneCell.");
            return;
        }

        foreach (Transform point in manualSamplePoints)
        {
            ZoneCell cell = GetZoneFromPosition(point.position);
            if (cell == null || point == null) continue;

            Renderer rend = point.GetComponent<Renderer>();
            if (rend == null) continue;

            if (selectedCell.VisibleCells.Contains(cell))
            {
                rend.sharedMaterial.color = Color.green;
            }
            else
            {
                rend.sharedMaterial.color = Color.red;
            }
        }

        Debug.Log($"Visualized visibility from selected cell at {selectedCell.Center}.");
#endif
    }

    void GenerateZoneGridFromManualCubes()
    {
        cells.Clear();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        foreach (Transform point in manualSamplePoints)
        {
            Vector3 center = point.position;
            Vector2Int grid = new Vector2Int(
                Mathf.RoundToInt(center.x / cellSize),
                Mathf.RoundToInt(center.z / cellSize));

            if (visited.Contains(grid)) continue;
            visited.Add(grid);

            float walkability = CalculateWalkabilityAround(center, cellSize, out float avgY);
            if (walkability >= walkabilityThreshold)
            {
                Rect cellRect = new Rect(new Vector2(center.x, center.z) - Vector2.one * (cellSize / 2f), Vector2.one * cellSize);
                ZoneCell cell = new ZoneCell(cellRect, grid, walkability, avgY);
                cells[grid] = cell;
            }
        }

        PrecomputeReachableCellDistances(30);
        PrecomputeVisibilityBetweenCells();
    }

    void PrecomputeReachableCellDistances(int maxSteps)
    {
        foreach (var sourceCell in cells.Values)
        {
            Queue<(ZoneCell cell, int steps)> queue = new Queue<(ZoneCell, int)>();
            HashSet<ZoneCell> visited = new HashSet<ZoneCell>();
            queue.Enqueue((sourceCell, 0));
            visited.Add(sourceCell);

            while (queue.Count > 0)
            {
                var (current, steps) = queue.Dequeue();
                if (steps > maxSteps) continue;
                if (current != sourceCell)
                    sourceCell.ReachableCellsWithinRange[current] = steps;

                foreach (var neighbor in GetImmediateNeighbors(current))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, steps + 1));
                    }
                }
            }
        }
    }

    IEnumerable<ZoneCell> GetImmediateNeighbors(ZoneCell cell)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(-1, -1),
            new Vector2Int(-1, 1), new Vector2Int(1, -1)
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborKey = cell.GridPos + dir;
            if (cells.TryGetValue(neighborKey, out var neighbor))
            {
                yield return neighbor;
            }
        }
    }

    void PrecomputeVisibilityBetweenCells()
    {
        foreach (var source in cells.Values)
        {
            foreach (var target in cells.Values)
            {
                if (source == target) continue;
                if (source.ReachableCellsWithinRange.ContainsKey(target))
                {
                    if (HasLineOfSight(source.Center, target.Center))
                    {
                        source.VisibleCells.Add(target);
                    }
                }
            }
        }

        Debug.Log("Precomputed visibility between cells.");
    }

    bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        Vector3 eyeFrom = from + Vector3.up * 1.6f;
        Vector3 eyeTo = to + Vector3.up * 1.6f;
        Vector3[] offsets = new Vector3[] { Vector3.zero, Vector3.down * 0.5f, Vector3.down * 1.0f };

        foreach (var offset in offsets)
        {
            if (!Physics.Raycast(eyeFrom, (eyeTo + offset - eyeFrom).normalized, out RaycastHit hit, Vector3.Distance(eyeFrom, eyeTo + offset)))
            {
                return true;
            }
        }

        return false;
    }

    float CalculateWalkabilityAround(Vector3 center, float size, out float surfaceY)
    {
        float half = size / 2f;

        Vector3[] samplePoints = new Vector3[]
        {
            center,
            center + new Vector3(-half, 0, -half),
            center + new Vector3(half, 0, -half),
            center + new Vector3(-half, 0, half),
            center + new Vector3(half, 0, half)
        };

        int walkableCount = 0;
        float totalY = 0f;

        foreach (var point in samplePoints)
        {
            Vector3 fromAbove = new Vector3(point.x, center.y + 2f, point.z);
            if (NavMesh.SamplePosition(fromAbove, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
            {
                walkableCount++;
                totalY += hit.position.y;
            }
        }

        surfaceY = (walkableCount > 0) ? totalY / walkableCount : center.y;
        return walkableCount / (float)samplePoints.Length;
    }

    public ZoneCell GetZoneFromPosition(Vector3 position)
    {
        foreach (var cell in cells.Values)
        {
            if (cell.Contains(position))
                return cell;
        }
        return null;
    }

    void OnDrawGizmosSelected()
    {
        if (cells == null || cells.Count == 0) return;

        foreach (var cell in cells.Values)
        {
            float w = cell.Walkability;
            Vector3 center = cell.Center + Vector3.up * 0.1f;
            Vector3 size = new Vector3(cell.Bounds.width, 0.1f, cell.Bounds.height);

            Gizmos.color = Color.Lerp(Color.red, Color.green, w);
            Gizmos.DrawCube(center, size);
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(center, size);
        }
    }*/
}


/*[System.Serializable]
public class ZoneSource
{
    public GameObject sourceObject;
    public float cellSize = 5f;
}

public class UniformZoneGridManager : MonoBehaviour
{
    [Header("Manual Sample Points")]
    public List<Transform> manualSamplePoints = new List<Transform>();
    public float cellSize = 1f;
    public float sampleRadius = 2f;
    public float walkabilityThreshold = 0.2f;
    public GameObject markerPrefab; // Prefab to instantiate as sample markers

    private Dictionary<Vector2Int, ZoneCell> cells = new Dictionary<Vector2Int, ZoneCell>();
    public IReadOnlyDictionary<Vector2Int, ZoneCell> Cells => cells;

    private List<GameObject> savedCubes = new List<GameObject>();

    void Start()
    {
        GenerateZoneGridFromManualCubes();
        Debug.Log($"Generated {cells.Count} zone cells.");
    }

    [ContextMenu("Collect Sample Cubes")]
    public void CollectEnabledCubes()
    {
        manualSamplePoints.Clear();

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                manualSamplePoints.Add(child);
            }
        }

        Debug.LogError($"Collected {manualSamplePoints.Count} active sample points.");
    }

    [ContextMenu("Auto Place Sample Cubes on NavMesh")]
    public void AutoPlaceCubesOnNavMesh()
    {
        ClearExistingSampleCubes();

        Renderer rend = GetComponent<Renderer>();
        if (rend == null || markerPrefab == null)
        {
            Debug.LogWarning("Missing Renderer or Marker Prefab");
            return;
        }

        Bounds bounds = rend.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        int numX = Mathf.CeilToInt((max.x - min.x) / cellSize);
        int numZ = Mathf.CeilToInt((max.z - min.z) / cellSize);

        for (int x = 0; x < numX; x++)
        {
            for (int z = 0; z < numZ; z++)
            {
                float posX = min.x + x * cellSize + cellSize * 0.5f;
                float posZ = min.z + z * cellSize + cellSize * 0.5f;
                Vector3 sample = new Vector3(posX, bounds.max.y, posZ);

                if (NavMesh.SamplePosition(sample, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
                {
                    Vector3 fixedPosition = new Vector3(posX, hit.position.y, posZ);
                    GameObject cube = Instantiate(markerPrefab, hit.position, Quaternion.identity);
                    cube.name = $"SampleNav_{x}_{z}";
                    cube.transform.localScale = markerPrefab.transform.localScale;
                    cube.transform.SetParent(transform); // Avoid scale distortion
                    manualSamplePoints.Add(cube.transform);
                    savedCubes.Add(cube);
                }
            }
        }

        Debug.Log($"Placed {manualSamplePoints.Count} sample cubes on NavMesh.");

       
    }

    [ContextMenu("Clear Sample Cubes")]
    public void ClearExistingSampleCubes()
    {
        foreach (Transform child in transform)
        {
            savedCubes.Add(child.gameObject);
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        manualSamplePoints.Clear();
        Debug.Log("Cleared sample cubes and manual points, but saved them in memory.");
    }

    void GenerateZoneGridFromManualCubes()
    {
        cells.Clear();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        foreach (Transform point in manualSamplePoints)
        {
            Vector3 center = point.position;
            Vector2Int grid = new Vector2Int(
                Mathf.RoundToInt(center.x / cellSize),
                Mathf.RoundToInt(center.z / cellSize));

            if (visited.Contains(grid)) continue;
            visited.Add(grid);

            float walkability = CalculateWalkabilityAround(center, cellSize, out float avgY);
            if (walkability >= walkabilityThreshold)
            {
                Rect cellRect = new Rect(new Vector2(center.x, center.z) - Vector2.one * (cellSize / 2f), Vector2.one * cellSize);
                ZoneCell cell = new ZoneCell(cellRect, grid, walkability, avgY);
                cells[grid] = cell;
            }
        }

        PrecomputeReachableCellDistances(20);
        PrecomputeVisibilityBetweenCells();
    }

    void PrecomputeReachableCellDistances(int maxSteps)
    {
        foreach (var sourceCell in cells.Values)
        {
            Queue<(ZoneCell cell, int steps)> queue = new Queue<(ZoneCell, int)>();
            HashSet<ZoneCell> visited = new HashSet<ZoneCell>();
            queue.Enqueue((sourceCell, 0));
            visited.Add(sourceCell);

            while (queue.Count > 0)
            {
                var (current, steps) = queue.Dequeue();
                if (steps > maxSteps) continue;
                if (current != sourceCell)
                    sourceCell.ReachableCellsWithinRange[current] = steps;

                foreach (var neighbor in GetImmediateNeighbors(current))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, steps + 1));
                    }
                }
            }
        }
    }

    IEnumerable<ZoneCell> GetImmediateNeighbors(ZoneCell cell)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(-1, -1),
            new Vector2Int(-1, 1), new Vector2Int(1, -1)
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborKey = cell.GridPos + dir;
            if (cells.TryGetValue(neighborKey, out var neighbor))
            {
                yield return neighbor;
            }
        }
    }

    void PrecomputeVisibilityBetweenCells()
    {
        foreach (var source in cells.Values)
        {
            foreach (var target in cells.Values)
            {
                if (source == target) continue;
                if (source.ReachableCellsWithinRange.ContainsKey(target))
                {
                    if (HasLineOfSight(source.Center, target.Center))
                    {
                        source.VisibleCells.Add(target);
                    }
                }
            }
        }

        Debug.Log("Precomputed visibility between cells.");
    }

    bool HasLineOfSight(Vector3 from, Vector3 to)
    {
        Vector3 eyeFrom = from + Vector3.up * 1.6f;
        Vector3 eyeTo = to + Vector3.up * 1.6f;
        Vector3[] offsets = new Vector3[] { Vector3.zero, Vector3.down * 0.5f, Vector3.down * 1.0f };

        foreach (var offset in offsets)
        {
            if (!Physics.Raycast(eyeFrom, (eyeTo + offset - eyeFrom).normalized, out RaycastHit hit, Vector3.Distance(eyeFrom, eyeTo + offset)))
            {
                return true; // No obstruction
            }
        }

        return false;
    }

    float CalculateWalkabilityAround(Vector3 center, float size, out float surfaceY)
    {
        float half = size / 2f;

        Vector3[] samplePoints = new Vector3[]
        {
            center,
            center + new Vector3(-half, 0, -half),
            center + new Vector3(half, 0, -half),
            center + new Vector3(-half, 0, half),
            center + new Vector3(half, 0, half)
        };

        int walkableCount = 0;
        float totalY = 0f;

        foreach (var point in samplePoints)
        {
            Vector3 fromAbove = new Vector3(point.x, center.y + 2f, point.z);
            if (NavMesh.SamplePosition(fromAbove, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
            {
                walkableCount++;
                totalY += hit.position.y;
            }
        }

        surfaceY = (walkableCount > 0) ? totalY / walkableCount : center.y;
        return walkableCount / (float)samplePoints.Length;
    }

    public ZoneCell GetZoneFromPosition(Vector3 position)
    {
        foreach (var cell in cells.Values)
        {
            if (cell.Contains(position))
                return cell;
        }
        return null;
    }

    void OnDrawGizmosSelected()
    {
        if (cells == null || cells.Count == 0) return;

        foreach (var cell in cells.Values)
        {
            float w = cell.Walkability;
            Vector3 center = cell.Center + Vector3.up * 0.1f;
            Vector3 size = new Vector3(cell.Bounds.width, 0.1f, cell.Bounds.height);

            Gizmos.color = Color.Lerp(Color.red, Color.green, w);
            Gizmos.DrawCube(center, size);
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(center, size);
        }
    }
}*/

/*// UniformZoneGridManager.cs - Trigger-Based NavMesh Cell Generator
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BoxCollider))]
public class UniformZoneGridManager : MonoBehaviour
{
    public float cellSize = 5f;
    public float sampleRadius = 2f;
    public float walkabilityThreshold = 0.2f;

    private Dictionary<Vector2Int, ZoneCell> cells = new Dictionary<Vector2Int, ZoneCell>();
    public IReadOnlyDictionary<Vector2Int, ZoneCell> Cells => cells;

    void Start()
    {
        GenerateZoneGridFromTriggerBounds();
        Debug.Log($"Generated {cells.Count} zone cells.");
    }

    void GenerateZoneGridFromTriggerBounds()
    {
        cells.Clear();

        BoxCollider trigger = GetComponent<BoxCollider>();
        Bounds bounds = trigger.bounds;

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        int numX = Mathf.CeilToInt((max.x - min.x) / cellSize);
        int numZ = Mathf.CeilToInt((max.z - min.z) / cellSize);

        for (int x = 0; x < numX; x++)
        {
            for (int z = 0; z < numZ; z++)
            {
                float posX = min.x + x * cellSize + cellSize * 0.5f;
                float posZ = min.z + z * cellSize + cellSize * 0.5f;
                Vector3 samplePos = new Vector3(posX, max.y + 2f, posZ); // Sample from above

                if (NavMesh.SamplePosition(samplePos, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
                {
                    Vector2Int grid = new Vector2Int(x, z);

                    // Calculate walkability using nearby points
                    float walkability = CalculateWalkabilityAround(hit.position, out float avgY);
                    if (walkability >= walkabilityThreshold)
                    {
                        Rect cellRect = new Rect(new Vector2(hit.position.x, hit.position.z) - Vector2.one * (cellSize / 2f), Vector2.one * cellSize);
                        ZoneCell cell = new ZoneCell(cellRect, grid, walkability, avgY);
                        cells[grid] = cell;
                    }
                }
            }
        }
    }

    float CalculateWalkabilityAround(Vector3 center, out float surfaceY)
    {
        float half = cellSize / 2f;

        Vector3[] samplePoints = new Vector3[]
        {
            center,
            center + new Vector3(-half, 0, -half),
            center + new Vector3(half, 0, -half),
            center + new Vector3(-half, 0, half),
            center + new Vector3(half, 0, half)
        };

        int walkableCount = 0;
        float totalY = 0f;

        foreach (var point in samplePoints)
        {
            Vector3 fromAbove = new Vector3(point.x, center.y + 2f, point.z);
            if (NavMesh.SamplePosition(fromAbove, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
            {
                walkableCount++;
                totalY += hit.position.y;
            }
        }

        surfaceY = (walkableCount > 0) ? totalY / walkableCount : center.y;
        return walkableCount / (float)samplePoints.Length;
    }

    public ZoneCell GetZoneFromPosition(Vector3 position)
    {
        foreach (var cell in cells.Values)
        {
            if (cell.Contains(position))
                return cell;
        }
        return null;
    }

    void OnDrawGizmosSelected()
    {
        if (cells == null || cells.Count == 0) return;

        foreach (var cell in cells.Values)
        {
            float w = cell.Walkability;
            Vector3 center = cell.Center + Vector3.up * 0.1f;
            Vector3 size = new Vector3(cell.Bounds.width, 0.1f, cell.Bounds.height);

            Gizmos.color = Color.Lerp(Color.red, Color.green, w);
            Gizmos.DrawCube(center, size);
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
*/