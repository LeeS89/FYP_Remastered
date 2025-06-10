using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static ClosestPointToPlayerJob;


public class WaypointResources : SceneResources
{
    private WaypointBlockData _waypointBlockData;
    private WaypointManager _waypointManager;

    public override async Task LoadResources()
    {
        try
        {
            // Load the asset from Addressables
            var waypointHandle = Addressables.LoadAssetAsync<ScriptableObject>("MoonSceneWP");

            // Wait for the asset to be loaded
            await waypointHandle.Task;

            // Check if the loading succeeded
            if (waypointHandle.Status == AsyncOperationStatus.Succeeded)
            {
                // Asset is loaded successfully, cast it to the correct type
                _waypointBlockData = (WaypointBlockData)waypointHandle.Result;

                if (_waypointBlockData != null)
                {
                    // Perform logic if the object is not null
                    foreach (var blockData in _waypointBlockData.blockDataArray)
                    {
                        blockData._inUse = false;
                    }
                }
                else
                {
                    Debug.LogError("Loaded waypoint block data is null.");
                }
            }
            else
            {
                // Log failure if the operation didn't succeed
                Debug.LogError("Failed to load the waypoint data from Addressables.");
            }

            // Subscribe to the resource requested event
            SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading waypoint resources: {e.Message}");
        }

    }

    protected override void ResourceRequested(ResourceRequest request)
    {
        if(request.resourceType != Resourcetype.WaypointBlock) { return; }

        if (_waypointBlockData == null)
        {
            Debug.LogWarning("No Waypoints exist in the scene, please update waypoint manager");

            request.waypointCallback?.Invoke(null);
            
        }

        foreach (var blockData in _waypointBlockData.blockDataArray)
        {
            if (!blockData._inUse)
            {
                blockData._inUse = true;
                request.waypointCallback?.Invoke(blockData);
            }
        }
        
    }

   /* private void RequestWaypointBlock(ResourceRequest request)
    {
        

        if (_waypointBlockData == null)
        {
            Debug.LogWarning("No Waypoints exist in the scene, please update waypoint manager");

            request.waypointCallback?.Invoke(null);
            //return null;
        }

        foreach (var blockData in _waypointBlockData.blockDataArray)
        {
            if (!blockData._inUse)
            {
                blockData._inUse = true;
                return blockData;
            }
        }
        return null;
    }
*/
   /* private BlockData RequestWaypointBlock()
    {
        if (_waypointBlockData == null)
        {
            Debug.LogWarning("No Waypoints exist in the scene, please update waypoint manager");
            return null;
        }

        foreach (var blockData in _waypointBlockData.blockDataArray)
        {
            if (!blockData._inUse)
            {
                blockData._inUse = true;
                return blockData;
            }
        }
        return null;
    }*/

    /* protected void LoadWaypoints() // Create WaypointManager Component Later
     {
         if (_waypointManager == null)
         {
             _waypointManager = WaypointManager.Instance;

         }
         if (_waypointManager != null)
         {

             _waypointBlockData = _waypointManager.RetreiveWaypointData();
         }

         if (_waypointBlockData != null)
         {
             foreach (var blockData in _waypointBlockData.blockDataArray)
             {
                 blockData._inUse = false;
             }
         }
     }*/
}
