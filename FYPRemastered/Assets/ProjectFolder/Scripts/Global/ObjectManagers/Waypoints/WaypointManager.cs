
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{

    [SerializeField] private GameObject _waypointBlockPrefab;
    [SerializeField] private int _blockZone = 0;
    [SerializeField] private Vector3 blockPosition = new Vector3(0, 0, 0);

    [SerializeField] public int _numWaypoints = 0;
    [SerializeField]private List<BlockData> _waypointBlocks = new List<BlockData>();

    public WaypointBlockData waypointBlockData;



    // Create block via context menu
    [ContextMenu("Create Block")]
    public void CreateWaypointBlock()
    {
        // Instantiate the block at the desired position
        GameObject block = Instantiate(_waypointBlockPrefab, blockPosition, Quaternion.identity);
        block.name = "WaypointBlock_" + _waypointBlocks.Count + 1;

   
        WaypointBlock waypointBlock = GetComponent<WaypointBlock>();
        waypointBlock._numberOfWaypoints = _numWaypoints;  // Set the number of waypoints
        waypointBlock.InstantiateWaypoints(block);

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
        return waypointBlockData;
    }

    [ContextMenu("Confirm Waypoint List")]
    public void UpdateWayPoints()
    {
        for(int i = 0; i < _waypointBlocks.Count; i++)
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

            data._waypointPositions = waypointPositions.ToArray();
            data._waypointForwards = waypointForwards.ToArray();

        }
    }

    [ContextMenu("Store Waypoints in SO")]
    public void StoreWaypointsInSO()
    {
        List<BlockData> storedBlocks = new List<BlockData>();

        // Store the waypoint blocks data into the ScriptableObject
        foreach (var block in _waypointBlocks)
        {
            BlockData newBlockData = new BlockData
            {
                _blockPosition = block._blockPosition,             // Store position of the block
                _blockZone = block._blockZone,                     // Store zone
                _waypointPositions = block._waypointPositions,     // Store waypoint positions
                _waypointForwards = block._waypointForwards,       // Store waypoint forwards (if needed)
                _inUse = block._inUse                              // Store whether the block is in use
            };

            // Add the block data (without _block reference)
            storedBlocks.Add(newBlockData);

        }

        // Update the ScriptableObject with the data
        waypointBlockData.blockDataArray = storedBlocks.ToArray();

        if (Application.isEditor && !Application.isPlaying)
        {
            // Iterate through _waypointBlocks list and destroy the blocks' children (the waypoints)
            foreach (var blockData in _waypointBlocks)
            {
                if (blockData._block != null)
                {
                    // Destroy the waypoints (children of each block)
                    foreach (Transform waypoint in blockData._block.transform)
                    {
                        DestroyImmediate(waypoint.gameObject); // Destroy each waypoint child
                    }

                    // Now destroy the block itself
                    DestroyImmediate(blockData._block); // Destroy the block
                }
            }
        }
        else
        {
            // Iterate through _waypointBlocks list and destroy the blocks' children (the waypoints)
            foreach (var blockData in _waypointBlocks)
            {
                if (blockData._block != null)
                {
                    // Destroy the waypoints (children of each block)
                    foreach (Transform waypoint in blockData._block.transform)
                    {
                        Destroy(waypoint.gameObject); // Destroy each waypoint child
                    }

                    // Now destroy the block itself
                    Destroy(blockData._block); // Destroy the block
                }
            }
        }
        // Clear the waypoint blocks from the scene and list
        _waypointBlocks.Clear();
        
    }


    [ContextMenu("Test Count")]
    public void PrintCount()
    {
        int waypoints = _waypointBlocks[0]._block.transform.childCount;
        /*foreach (Transform t in _waypointBlocks[0]._block.transform)
        {
            waypoints++;
        }*/
       // int waypoints = _waypointBlocks[0]._block.GetComponentsInChildren<Transform>().Length;
        Debug.LogError("Count is: "+waypoints);
    }

    [ContextMenu("Reload Waypoints from SO")]
    public void ReloadWaypointsFromSO()
    {
        // Repopulate the _waypointBlocks list with data from the ScriptableObject
        foreach (var blockData in waypointBlockData.blockDataArray)
        {
            // Instantiate the block prefab and set its position
            GameObject block = Instantiate(_waypointBlockPrefab, blockData._blockPosition, Quaternion.identity);
            block.name = "WaypointBlock_" + blockData._blockZone;

            // Assign the _block reference in BlockData (so it holds the instantiated block)
            blockData._block = block;  // Set the block reference here

            // Instantiate the waypoints for this block
            WaypointBlock waypointBlock = block.GetComponent<WaypointBlock>();
            waypointBlock._numberOfWaypoints = blockData._waypointPositions.Length;
            waypointBlock.InstantiateWaypoints(block);  // Method to instantiate waypoints for this block

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
    }


    [ContextMenu("Refresh Waypoint List")]
    public void UpdateList()
    {
        for (int i = _waypointBlocks.Count - 1; i >= 0; i--)
        {
            // Check if the position of the block is invalid or if other conditions apply.
            // In this case, we'll check if any of the waypoints are invalid (you could also check other conditions).
            //bool isValidBlock = _waypointBlocks[i]._waypointPositions != null && _waypointBlocks[i]._waypointPositions.Length > 0;
            //bool validBlock = _waypointBlocks[i]._blockPosition != null;
            GameObject obj = _waypointBlocks[i]._block;
            if (obj == null)
            {
                // Remove the block from the list if it's not valid
                _waypointBlocks.RemoveAt(i);
                Debug.Log("Removed invalid block at index: " + i);
            }
        }
    }


    // Function to request a block
    public BlockData RequestWaypointBlock()
    {
        foreach (var blockData in _waypointBlocks)
        {
            if (!blockData._inUse)
            {
                blockData._inUse = true;
                return blockData;
            }
        }
        return null; // No available blocks
    }

    /*// Function to return a block
    public void ReturnWaypointBlock(GameObject block)
    {
        foreach (var blockData in _waypointBlocks)
        {
            if (blockData._block == block)
            {
                blockData._inUse = false;
                break;
            }
        }
    }*/

    /* 

     public void ReturnWaypointBlock(GameObject block)
     {
         if (_waypointBlockStatus.ContainsKey(block))
         {
             // Mark the block as available again
             BlockData blockData = _waypointBlockStatus[block];
             blockData.isInUse = false;
             _waypointBlockStatus[block] = blockData; // Update the status
         }
     }*/

   

    
}
