using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class FlankPointGenerator : MonoBehaviour
{
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private List<GameObject> _navMeshObjects;
    [SerializeField] private List<GameObject> _navMeshObjectsBackup;
    [SerializeField] private SamplePointDataSO _samplePointDataSO;

    [SerializeField] private float cellSize = 1.0f;
    [SerializeField] private float sampleRadius = 1.0f;
    [SerializeField] private float stepSize = 1.0f;
    [SerializeField] private int maxSteps = 5;

    [SerializeField] private Transform player;

    private List<Transform> manualSamplePoints = new();

    [ContextMenu("Auto Place Cubes on NavMesh")]
    public void AutoPlaceCubesOnNavMesh()
    {
        if (markerPrefab == null || _samplePointDataSO == null)
        {
            Debug.LogError("Missing Marker Prefab or ScriptableObject.");
            return;
        }

        ClearExistingSampleCubes();

        if (_samplePointDataSO.savedPoints == null || _samplePointDataSO.savedPoints.Count == 0)
        {
            DeduplicateNavMeshObjects();

            if (_navMeshObjects.Count == 0)
            {
                Debug.LogWarning("No NavMesh source objects provided.");
                return;
            }

            _navMeshObjectsBackup = new List<GameObject>(_navMeshObjects);
            GenerateNewCubes(_navMeshObjectsBackup);
        }
        else
        {
            List<GameObject> newObjectsForCubePlacement = new();
            foreach (var obj in _navMeshObjects)
            {
                if (!_navMeshObjectsBackup.Contains(obj))
                {
                    _navMeshObjectsBackup.Add(obj);
                    newObjectsForCubePlacement.Add(obj);
                }
            }

            ReloadStoredCubes();

            if (newObjectsForCubePlacement.Count > 0)
            {
                GenerateNewCubes(newObjectsForCubePlacement);
            }
        }

        CollectSampleCubes();
    }

    [ContextMenu("Clear existing cubes")]
    private void ClearExistingSampleCubes()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        manualSamplePoints.Clear();
    }

    [ContextMenu("Clear existing Data")]
    private void ClearExistingSOData()
    {
        ClearExistingSampleCubes();

        _samplePointDataSO.ClearData();
        SaveScriptableObject();
    }

    private void DeduplicateNavMeshObjects()
    {
        List<GameObject> unique = new();

        foreach (var obj in _navMeshObjects)
        {
            if (!unique.Contains(obj))
                unique.Add(obj);
        }

        _navMeshObjects.Clear();
        _navMeshObjects.AddRange(unique);
    }

    private void GenerateNewCubes(List<GameObject> objectsToScan)
    {
        foreach (GameObject obj in objectsToScan)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend == null)
            {
                Debug.LogWarning($"No Renderer found on {obj.name}, skipping.");
                continue;
            }

            Bounds bounds = rend.bounds;
            int numX = Mathf.CeilToInt(bounds.size.x / cellSize);
            int numZ = Mathf.CeilToInt(bounds.size.z / cellSize);

            for (int x = 0; x < numX; x++)
            {
                for (int z = 0; z < numZ; z++)
                {
                    Vector3 sample = new Vector3(
                        bounds.min.x + x * cellSize + cellSize * 0.5f,
                        bounds.max.y,
                        bounds.min.z + z * cellSize + cellSize * 0.5f
                    );

                    if (NavMesh.SamplePosition(sample, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
                    {
                        GameObject cube = Instantiate(markerPrefab, hit.position, Quaternion.identity, transform);
                        cube.name = $"SampleNav_{x}_{z}";
                        cube.tag = "SampleNavMeshPoint";
                        manualSamplePoints.Add(cube.transform);
                    }
                }
            }
        }

        RemoveOverlappingCubes();
        Debug.Log($"Placed {manualSamplePoints.Count} sample cubes on NavMesh.");
    }

    private void RemoveOverlappingCubes()
    {
        HashSet<Transform> destroyed = new();

        foreach (Transform cube in manualSamplePoints.ToList())
        {
            if (destroyed.Contains(cube)) continue;

            BoxCollider coll = cube.GetComponent<BoxCollider>();
            if (!coll) continue;

            Collider[] overlaps = Physics.OverlapBox(
                coll.bounds.center,
                coll.bounds.extents * 3f,
                cube.rotation
            );

            foreach (Collider overlap in overlaps)
            {
                if (overlap.transform == cube.transform) continue;

                if (overlap.CompareTag("SampleNavMeshPoint") && !destroyed.Contains(overlap.transform))
                {
                    destroyed.Add(overlap.transform);
                    manualSamplePoints.Remove(overlap.transform);
                    DestroyImmediate(overlap.gameObject);
                }
            }
        }

        Debug.Log($"Removed {destroyed.Count} overlapping cubes.");
    }

    private void ReloadStoredCubes()
    {
        if (_samplePointDataSO.savedPoints == null || _samplePointDataSO.savedPoints.Count == 0)
        {
            Debug.LogWarning("No saved points to reload.");
            return;
        }

        foreach (var data in _samplePointDataSO.savedPoints)
        {
            GameObject cube = Instantiate(markerPrefab, data.position, Quaternion.identity, transform);
            manualSamplePoints.Add(cube.transform);
        }

        _samplePointDataSO.savedPoints.Clear();
        SaveScriptableObject();
    }

    public void CollectSampleCubes()
    {
        manualSamplePoints.Clear();

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
                manualSamplePoints.Add(child);
        }

        Debug.Log($"Collected {manualSamplePoints.Count} sample cubes.");
    }

    private void SaveScriptableObject()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(_samplePointDataSO);
            AssetDatabase.SaveAssets();
        }
#endif
    }

    [ContextMenu("Generate and Save Flank Point Data")]
    public void GenerateSamplePointData()
    {
        RefreshManualSamplePointsList();

        List<SamplePointData> flankPoints = new();

        foreach (Transform point in manualSamplePoints)
        {
            flankPoints.Add(new SamplePointData
            {
                position = point.position,
                stepLinks = new List<StepEntry>()
            });
        }

        int count = flankPoints.Count;
        for (int i = 0; i < count; i++)
        {
            var from = flankPoints[i];
            for (int j = 0; j < count; j++)
            {
                if (i == j) continue;

                float dist = Vector3.Distance(from.position, flankPoints[j].position);
                int step = CalculateStep(dist, stepSize);
                if (step > maxSteps) continue;

                StepEntry entry = from.stepLinks.FirstOrDefault(e => e.step == step);
                if (entry == null)
                {
                    entry = new StepEntry { step = step };
                    from.stepLinks.Add(entry);
                }

                entry.reachableIndices.Add(j);
            }
        }

        _samplePointDataSO.savedPoints.Clear();
        _samplePointDataSO.savedPoints.AddRange(flankPoints);

        SaveScriptableObject();

#if UNITY_EDITOR
       // ClearExistingSampleCubes();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif

        Debug.Log($"Generated {flankPoints.Count} flank points with step connections.");
    }

    private void RefreshManualSamplePointsList()
    {
        manualSamplePoints = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
                manualSamplePoints.Add(child);
        }
    }

    private int CalculateStep(float distance, float stepSize)
    {
        if (distance < 2f) return 1;
        return Mathf.FloorToInt((distance - 2f) / stepSize) + 2;
    }

    [ContextMenu("Log Steps From Selected Point")]
    public void LogStepsFromSelected()
    {
#if UNITY_EDITOR
        if (UnityEditor.Selection.activeTransform == null)
        {
            Debug.LogWarning("No cube selected.");
            return;
        }

        Vector3 selectedPos = UnityEditor.Selection.activeTransform.position;

        int index = _samplePointDataSO.savedPoints.FindIndex(p => Vector3.Distance(p.position, selectedPos) < 0.01f);
        if (index == -1)
        {
            Debug.LogWarning("Selected point not found in saved list.");
            return;
        }

        var point = _samplePointDataSO.savedPoints[index];

        Debug.Log($"\n--- Steps from point at {point.position} ---");
        foreach (var entry in point.stepLinks.OrderBy(e => e.step))
        {
            Debug.Log($"Step {entry.step}: {entry.reachableIndices.Count} reachable points");
        }
#endif
    }


    [ContextMenu("Show Step Labels From Selected Point")]
    public void ShowStepLabelsFromSelected()
    {
#if UNITY_EDITOR
        if (UnityEditor.Selection.activeTransform == null)
        {
            Debug.LogWarning("No cube selected.");
            return;
        }

        Vector3 selectedPos = UnityEditor.Selection.activeTransform.position;

        int index = _samplePointDataSO.savedPoints.FindIndex(p => Vector3.Distance(p.position, selectedPos) < 0.01f);
        if (index == -1)
        {
            Debug.LogWarning("Selected point not found in saved list.");
            return;
        }

        var point = _samplePointDataSO.savedPoints[index];

        foreach (Transform cube in manualSamplePoints)
        {
            DestroyImmediate(cube.GetComponentInChildren<TextMesh>()); // Clean previous labels
        }

        foreach (var entry in point.stepLinks)
        {
            foreach (int idx in entry.reachableIndices)
            {
                if (idx >= 0 && idx < manualSamplePoints.Count)
                {
                    Transform cube = manualSamplePoints[idx];
                    GameObject textObj = new GameObject("StepLabel");
                    textObj.transform.SetParent(cube);
                    textObj.transform.localPosition = Vector3.up * 1.5f;

                    TextMesh tm = textObj.AddComponent<TextMesh>();
                    tm.text = entry.step.ToString();
                    tm.characterSize = 0.25f;
                    tm.fontSize = 24;
                    tm.color = Color.red;
                    tm.anchor = TextAnchor.MiddleCenter;
                }
            }
        }
#endif
    }

    [ContextMenu("Clear Step Labels")]
    public void RemoveStepLabels()
    {
#if UNITY_EDITOR
        foreach (Transform cube in manualSamplePoints)
        {
            var label = cube.Find("StepLabel");
            if (label != null)
            {
                DestroyImmediate(label.gameObject);
            }
        }
#endif
    }
}
