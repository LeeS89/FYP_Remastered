// UniformZoneGridManager.cs - NavMesh Aware Grid with Sample Cube Centers as Cells
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif
using TMPro;



public class UniformZoneGridManager : MonoBehaviour
{
  

    [Header("Cube Placement")]
    public float cellSize = 1f;
    public float sampleRadius = 2f;
    public GameObject markerPrefab;

    [Header("NavMesh Source Objects")]
    public List<GameObject> _navMeshObjects = new List<GameObject>();
    [SerializeField] private List<GameObject> _navMeshObjectsBackup = new List<GameObject>();

    [Header("Step Logic")]
    public float stepSize = 1f; // Step bands: step 2 = 2–2.99, etc.
    public int maxSteps = 30;

    public SamplePointDataSO _samplePointDataSO; // ScriptableObject to save data

    [HideInInspector] public List<Transform> manualSamplePoints = new List<Transform>();
    public List<SamplePointData> savedPoints = new List<SamplePointData>();


    //public ClosestPointToPlayerJob _playerJob;


    /// <summary>
    /// Scans the navmesh objects and places cubes at regular intervals.
    /// </summary>
    [ContextMenu("Auto Place Sample Cubes on NavMesh")]
    public void AutoPlaceCubesOnNavMesh()
    {
        if (markerPrefab == null || _samplePointDataSO == null)
        {
            Debug.LogError("Missing Marker Prefab.");
            return;
        }

        ClearExistingSampleCubes();

        //CheckForDuplicates();

        if (_samplePointDataSO.savedPoints.Count == 0)
        {
            if(_navMeshObjects.Count == 0) { return; }
            CheckForDuplicates();
            _navMeshObjectsBackup = new List<GameObject>(_navMeshObjects);
            GenerateNewCubes(_navMeshObjectsBackup);
        }
        else
        {
            List<GameObject> newObjectsForCubePlacement = new List<GameObject>();
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

    private void CheckForDuplicates()
    {
        List<GameObject> temp = new List<GameObject>();

        foreach (var obj in _navMeshObjects)
        {
            if (!temp.Contains(obj))
            {
                temp.Add(obj);
            }
        }

        _navMeshObjects.Clear();
        _navMeshObjects.AddRange(temp);
    }

    private void ReloadStoredCubes()
    {
        if (_samplePointDataSO == null || _samplePointDataSO.savedPoints.Count == 0)
        {
            Debug.LogError("No saved points available in the ScriptableObject.");
            return;
        }

        foreach (var pos in _samplePointDataSO.savedPoints)
        {
            GameObject cube = Instantiate(markerPrefab, pos.position, Quaternion.identity);
            cube.transform.SetParent(transform);
            manualSamplePoints.Add(cube.transform);
        }
        _samplePointDataSO.ClearData();
        SaveScriptableObject(); // Clear the data in the ScriptableObject after loading
    }

    private void SaveScriptableObject()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(_samplePointDataSO);  // Mark the ScriptableObject as modified
            AssetDatabase.SaveAssets();  // Force Unity to save the changes
#endif
        }
    }

    private void GenerateNewCubes(List<GameObject> objetcsToScan)
    {
        foreach (GameObject obj in objetcsToScan)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend == null)
            {
                Debug.LogWarning($"No Renderer found on {obj.name}, skipping.");
                continue;
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
                        cube.tag = "SampleNavMeshPoint"; // Optional: tag for easier identification
                        cube.transform.SetParent(transform);
                        manualSamplePoints.Add(cube.transform);
                    }
                }
            }
        }

        RemoveOverlappingCubes();
        

        Debug.LogError($"Placed {manualSamplePoints.Count} sample cubes on NavMesh.");
    }

    private void RemoveOverlappingCubes()
    {
#if UNITY_EDITOR
        HashSet<Transform> destroyed = new HashSet<Transform>();

        foreach (Transform cube in manualSamplePoints.ToList())
        {
            if (destroyed.Contains(cube)) { continue; }

            BoxCollider coll = cube.GetComponent<BoxCollider>();
            if (!coll) { continue; }

            Collider[] overlaps = Physics.OverlapBox(
                coll.bounds.center,
                coll.bounds.extents * 3f,
                cube.rotation
                );

            foreach(Collider overlap in overlaps)
            {
                if(overlap.transform == cube.transform) {  continue; }

                if (overlap.CompareTag("SampleNavMeshPoint") && !destroyed.Contains(overlap.transform))
                {
                    destroyed.Add(overlap.transform);
                    manualSamplePoints.Remove(overlap.transform);
                    DestroyImmediate(overlap.gameObject);
                }
            }
        }
        Debug.Log($"Removed {destroyed.Count} overlapping cubes.");
#endif
    }

    /// <summary>
    /// Once cubes are generated and edited, this function collects them into the manualSamplePoints list.
    /// </summary>
    //[ContextMenu("Collect Sample Cubes")]
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

    /// <summary>
    /// Removes all existing sample cubes from the scene.
    /// </summary>
    [ContextMenu("Clear Sample Cubes")]
    private void ClearExistingSampleCubes()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        manualSamplePoints.Clear();
        //Debug.LogError("Cleared existing sample cubes.");
    }

   /* void Start()
    {


        //DeserializeSavedPoints();

        //_playerJob.AddSamplePointData(_samplePointDataSO);
        *//*
        if(manualSamplePoints.Count == 0)
        {
            return;
        }

        GenerateSamplePointData();*//*
    }*/

    

    public SamplePointDataSO InitializeGrid()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return null;
#endif
        BaseSceneManager._instance.OnClosestPointToPlayerJobComplete += SetNearestIndexToPlayer;

        DeserializeSavedPoints();

        return _samplePointDataSO;
    }

    private void DeserializeSavedPoints()
    {
        foreach (var point in _samplePointDataSO.savedPoints)
        {
            point.reachableSteps = new Dictionary<int, List<int>>();

            foreach (var entry in point.serializedReachableSteps)
            {
                point.reachableSteps[entry.step] = new List<int>(entry.reachableIndices);
            }
        }

        savedPoints = new List<SamplePointData>(_samplePointDataSO.savedPoints); // Optional: sync to local list
    }

    [Header("Runtime Info (Debug / Optional)")]
    public Transform player; // Assign in inspector

    public int _nearestPointToPlayer = -1;

    public Transform nearestPointTransform;

    /// <summary>
    /// Generates and processes sample point data based on the positions of manually defined points.
    /// </summary>
    /// <remarks>This method clears any previously saved points and generates a new set of sample point data 
    /// by iterating through the provided manual sample points. It calculates step-grouped distances  between points and
    /// determines reachable steps for each point based on the specified step size  and maximum step limit. Debug
    /// information about the generated points is logged.  If a player reference is set in the Unity Editor, the method
    /// also identifies the nearest point  to the player and logs its index and position. If no player reference is set,
    /// a warning is logged.</remarks>
    [ContextMenu("Generate and save data")]
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

        foreach (var point in savedPoints)
        {
            var copy = new SamplePointData
            {
                position = point.position,
                serializedReachableSteps = new List<StepEntry>()
            };

            foreach (var kvp in point.reachableSteps)
            {
                copy.serializedReachableSteps.Add(new StepEntry
                {
                    step = kvp.Key,
                    reachableIndices = new List<int>(kvp.Value)
                });
            }

            _samplePointDataSO.savedPoints.Add(copy);
        }


        SaveScriptableObject();

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

            _nearestPointToPlayer = closestIndex;
            //nearestPointTransform = manualSamplePoints[closestIndex];

            Debug.Log($"[Debug] Nearest point to player: index {_nearestPointToPlayer} at {savedPoints[_nearestPointToPlayer].position}");
        }
        else
        {
            Debug.LogWarning("[Debug] Player reference not set — can't find nearest point.");
        }

        ClearExistingSampleCubes(); // Clear existing cubes after data generation
        // Clean up visual cubes after data is generated
        //foreach (Transform point in manualSamplePoints)
        //{
        //    DestroyImmediate(point.gameObject);
        //}

        // manualSamplePoints.Clear();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif
    }

    private void SetNearestIndexToPlayer(int nearestPointIndex)
    {
        _nearestPointToPlayer = nearestPointIndex;
        //nearestPointTransform = manualSamplePoints[_nearestPointToPlayer]; // Delete Later
    }


    /// <summary>
    /// Retrieves a random point that is exactly the specified number of steps away from the player's nearest point.
    /// </summary>
    /// <remarks>This method assumes that the player's nearest point and the saved points have been properly
    /// initialized.  If no saved points are available or the nearest point to the player is invalid, the method returns
    /// the  current position of the object.</remarks>
    /// <param name="step">The number of steps away from the player's nearest point. Must be a positive integer.</param>
    /// <returns>A <see cref="Vector3"/> representing the position of a random point that is <paramref name="step"/> steps away 
    /// from the player's nearest point. If no such point exists, returns the position of the player's nearest point.</returns>
    public Vector3 GetRandomPointXStepsFromPlayer(int step)
    {
        if (savedPoints == null || savedPoints.Count == 0)
        {
            Debug.LogWarning("No saved points available.");
            return transform.position;
        }

        if (_nearestPointToPlayer < 0 || _nearestPointToPlayer >= savedPoints.Count)
        {
            Debug.LogWarning("Nearest point to player not set or invalid.");
            return transform.position;
        }

        if (savedPoints[_nearestPointToPlayer].reachableSteps.TryGetValue(step, out var candidates) && candidates.Count > 0)
        {
            int randomIndex = candidates[Random.Range(0, candidates.Count)];
            return savedPoints[randomIndex].position;
        }

        Debug.LogWarning($"No points found {step} steps away from player's nearest point.");
        return savedPoints[_nearestPointToPlayer].position;
    }

    public List<Vector3> GetCandidatePointsAtStep(int step)
    {
        List<Vector3> positions = new List<Vector3>();

        if (savedPoints == null || savedPoints.Count == 0 ||
            _nearestPointToPlayer < 0 || _nearestPointToPlayer >= savedPoints.Count)
        {
            return positions;
        }

        if (savedPoints[_nearestPointToPlayer].reachableSteps.TryGetValue(step, out var indices))
        {
            foreach (int i in indices)
            {
                positions.Add(savedPoints[i].position);
            }
        }

        return positions;
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

        var selectedData = _samplePointDataSO.savedPoints[selectedIndex];

        if (_samplePointDataSO.savedPoints.Count == 0)
        {
            Debug.LogError("No sample points available to visualize.");
            return;
        }

        // Clear existing labels first
        foreach (Transform cube in manualSamplePoints)
        {
            var existing = cube.GetComponentInChildren<TextMesh>();
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }
        }
        Debug.LogError($"Selected index: {selectedIndex}, Manual Points Count: {manualSamplePoints.Count}");

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

   

    
}