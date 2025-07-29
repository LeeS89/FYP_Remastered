
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(WaypointBlock))]
public class WaypointManager : MonoBehaviour
{

    [SerializeField] private GameObject _waypointBlockPrefab;
    [SerializeField] private int _blockZone = 0;
    [SerializeField] private Vector3 blockPosition = new Vector3(0, 0, 0);

    [SerializeField] private int _numWaypoints = 0;
    [SerializeField] private List<BlockData> _waypointBlocks = new List<BlockData>();

    [SerializeField] private WaypointBlockData _waypointBlockData;
    [SerializeField] private WaypointBlock _waypointBlock;

    public static WaypointManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Create block via context menu
    [ContextMenu("Create Block")]
    public void CreateWaypointBlock()
    {
        if(_waypointBlockData.blockDataArray.Length > 0)
        {
            ReloadWaypointsFromSO();
        }


        RefreshList();

        // Instantiate the block at the desired position
        GameObject block = Instantiate(_waypointBlockPrefab, blockPosition, Quaternion.identity);
        block.name = "WaypointBlock_" + _waypointBlocks.Count + 1;

   
        //WaypointBlock waypointBlock = GetComponent<WaypointBlock>();
        _waypointBlock._numberOfWaypoints = _numWaypoints;  // Set the number of waypoints
        _waypointBlock.InstantiateWaypoints(block);

        // Create a new BlockData entry and populate the list
        BlockData newBlock = new BlockData
        {
            _block = block,  // Store the block's GameObject
            _blockPosition = block.transform.position,
            _blockZone = this._blockZone,
            _inUse = false  // Initially, the block is not in use
        };

  
        // Add the block data to the list
        _waypointBlocks.Add(newBlock);

        

        // Optionally, log or update something
        Debug.LogError("Block created in Zone: " + _blockZone + ", Total Blocks: " + _waypointBlocks.Count);
    }


    public WaypointBlockData RetreiveWaypointData()
    {
        return _waypointBlockData;
    }

    [ContextMenu("Confirm Waypoint List")]
    public void UpdateWayPoints()
    {
        RefreshList();

        for (int i = 0; i < _waypointBlocks.Count; i++)
        {
            BlockData data = _waypointBlocks[i];

            List<Vector3> waypointPositions = new List<Vector3>();
            List<Vector3> waypointForwards = new List<Vector3>();

            foreach (Transform waypoint in data._block.transform)
            {
                // Assuming each waypoint is a child of the block
                waypointPositions.Add(waypoint.position);
                waypointForwards.Add(waypoint.forward);
            }
            data._blockPosition = data._block.transform.position;
            data._waypointPositions = waypointPositions.ToArray();
            data._waypointForwards = waypointForwards.ToArray();

        }

        StoreWaypointsInSO();
    }

   
    private void StoreWaypointsInSO()
    {
#if UNITY_EDITOR
        if (_waypointBlockData == null) return;

        // Optional: clear existing data
        if (_waypointBlockData.blockDataArray != null && _waypointBlockData.blockDataArray.Length > 0)
        {
            _waypointBlockData.ClearData();
        }

        List<BlockData> storedBlocks = new List<BlockData>();

        foreach (var block in _waypointBlocks)
        {
            BlockData newBlockData = new BlockData
            {
                _blockPosition = block._blockPosition,
                _blockZone = block._blockZone,
                _waypointPositions = block._waypointPositions,
                _waypointForwards = block._waypointForwards,
                _inUse = block._inUse
                // _block is intentionally excluded
            };

            storedBlocks.Add(newBlockData);
        }

        // 🔄 Assign new data
        _waypointBlockData.blockDataArray = storedBlocks.ToArray();

        // ✅ Persist the change
        UnityEditor.EditorUtility.SetDirty(_waypointBlockData);
        UnityEditor.AssetDatabase.SaveAssets();

        // 🧹 Clean up waypoint objects from scene
        foreach (var blockData in _waypointBlocks)
        {
            if (blockData._block != null)
            {
                foreach (Transform waypoint in blockData._block.transform)
                {
                    Object.DestroyImmediate(waypoint.gameObject);
                }

                Object.DestroyImmediate(blockData._block);
            }
        }

        _waypointBlocks.Clear();
#endif

#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif

    }



    [ContextMenu("Reload Waypoints from SO")]
    public void ReloadWaypointsFromSO()
    {
        if(_waypointBlockData.blockDataArray.Length == 0)
        {
            //Debug.LogWarning("No waypoint data found in the ScriptableObject.");
            return;
        }

        // Repopulate the _waypointBlocks list with data from the ScriptableObject
        foreach (var blockData in _waypointBlockData.blockDataArray)
        {
            // Instantiate the block prefab and set its position
            GameObject block = Instantiate(_waypointBlockPrefab, blockData._blockPosition, Quaternion.identity);
            block.name = "WaypointBlock_" + blockData._blockZone;

            // Assign the _block reference in BlockData (so it holds the instantiated block)
            blockData._block = block;  // Set the block reference here

            // Instantiate the waypoints for this block
            
            _waypointBlock._numberOfWaypoints = blockData._waypointPositions.Length;
            _waypointBlock.InstantiateWaypoints(block);  // Method to instantiate waypoints for this block

            // Now reposition the waypoints using the data from ScriptableObject
            int waypoints = block.transform.childCount;

            // Ensure waypoints exist and their count matches
            if (waypoints == blockData._waypointPositions.Length)
            {
                for (int i = 0; i < waypoints; i++)
                {
                    Transform waypointTransform = block.transform.GetChild(i);
                    // Reposition each waypoint to match the stored position
                    waypointTransform.position = blockData._waypointPositions[i];

                    // Optionally update the forward direction if you have stored that data
                    if (blockData._waypointForwards.Length > i)
                    {
                        waypointTransform.forward = blockData._waypointForwards[i];
                    }
                }
            }
            else
            {
                Debug.LogWarning("Mismatch between waypoint count and positions array.");
            }

            _waypointBlocks.Add(blockData);
        }
        _waypointBlockData.ClearData(); // Clear the data in the ScriptableObject after loading

        SaveScriptableObject();
    }

    private void SaveScriptableObject()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(_waypointBlockData);  // Mark the ScriptableObject as modified
            AssetDatabase.SaveAssets();  // Force Unity to save the changes
#endif
        }
    }


    [ContextMenu("Refresh Waypoint List")]
    public void RefreshList()
    {
        if(_waypointBlocks.Count == 0)
        {
            return;
        }

        _waypointBlocks.RemoveAll(block => block._block == null); // Remove any blocks that are null


    }


}
