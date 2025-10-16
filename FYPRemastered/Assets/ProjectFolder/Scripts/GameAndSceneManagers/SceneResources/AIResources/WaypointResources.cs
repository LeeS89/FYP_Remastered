using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;



public class WaypointResources : SceneResources
{
    private WaypointBlockData _waypointBlockData;
  

    public override async Task LoadResources()
    {
        try
        {
            //NotifyDependancies();
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
                
                Debug.LogError("Failed to load the waypoint data from Addressables.");
            }

            NotifyClassDependancies();
            // Subscribe to the resource requested event
            // ResourceRequestBus<WaypointBlockRequest>.On += WaypointsRequested; /// NEW WAY to test
            SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;
           // SceneEventAggregator.Instance.OnAIResourceRequested += AIResourceRequested; /////// CURRENT WAY
            SceneEventAggregator.Instance.OnResourceReleased += ResourceReleased;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading waypoint resources: {e.Message}");
        }

    }

    protected override void NotifyClassDependancies()
    {
        bool exists = SceneEventAggregator.Instance.CheckDependancyExists(typeof(PathRequestManager));

        if (!exists)
        {

            SceneEventAggregator.Instance.AddDependancy(new PathRequestManager());
        }

        exists = SceneEventAggregator.Instance.CheckDependancyExists(typeof(AgentZoneRegistry));

        if (!exists)
        {
            SceneEventAggregator.Instance.AddDependancy(new AgentZoneRegistry());
        }

        exists = SceneEventAggregator.Instance.CheckDependancyExists(typeof(PlayerFlankingResources));

        if (!exists)
        {
            SceneEventAggregator.Instance.AddDependancy(new PlayerFlankingResources());
        }
        else
        {
            Debug.LogError("Player Flanking Resources already exists, not adding again.");
        }
        /*List<Type> dependancies = new()
        {
            typeof(PathRequestManager)
        };
        SceneEventAggregator.Instance.AddDependancies(dependancies);*/
    }

    protected override void ResourceRequested(in ResourceRequests request)
    {
        if (request.AIResourceType != AIResourceType.WaypointBlock) return;

        if(_waypointBlockData == null)
        {
            Debug.LogWarning("No Waypoint data exists in the scene, please load the correct SO");
            request.WaypointCallback?.Invoke(null);
        }

        foreach (var blockData in _waypointBlockData.blockDataArray)
        {
            if (!blockData._inUse)
            {
                blockData._inUse = true;
                request.WaypointCallback?.Invoke(blockData);
                return;
            }
        }
  
    }


    protected override void ResourceReleased(ResourceRequest request)
    {
        /*if (request.resourceType != Resourcetype.WaypointBlock) { return; }

        BlockData bd = request.blockData;
        if (!_waypointBlockData.blockDataArray.Contains(bd)) { return; }

        int index = Array.FindIndex(_waypointBlockData.blockDataArray, block => block == bd);
        if (index >= 0)
        {
            _waypointBlockData.blockDataArray[index]._inUse = false;
        }*/

    }

   /* public void ReturnWaypointBlock(BlockData bd)
    {
        if (!_waypointBlockData.blockDataArray.Contains(bd)) { return; }

        int index = Array.FindIndex(_waypointBlockData.blockDataArray, block => block == bd);

        if (index >= 0)
        {
            _waypointBlockData.blockDataArray[index]._inUse = false;
        }

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
