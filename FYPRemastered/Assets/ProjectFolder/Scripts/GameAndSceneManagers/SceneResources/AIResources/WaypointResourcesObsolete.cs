using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;


[Obsolete]
public class WaypointResourcesObsolete : SceneResources, IWaypointService
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
        bool exists = SceneEventAggregator.Instance.CheckDependancyExists(typeof(PathRequestManagerObsolete));

        if (!exists)
        {

            SceneEventAggregator.Instance.AddDependancy(new PathRequestManagerObsolete());
        }

        exists = SceneEventAggregator.Instance.CheckDependancyExists(typeof(AgentZoneRegistryNew));

        if (!exists)
        {
            SceneEventAggregator.Instance.AddDependancy(new AgentZoneRegistryNew());
        }

        exists = SceneEventAggregator.Instance.CheckDependancyExists(typeof(PlayerFlankingResourcesObsolete));

        if (!exists)
        {
            SceneEventAggregator.Instance.AddDependancy(new PlayerFlankingResourcesObsolete());
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

    //NEW SETUP
    private Dictionary<object, BlockData> _inUseBlockTracker = new(20);

    public bool TryGetWaypoints(object requester, List<Vector3> buffer)
    {
        if (requester == null || requester.GetType().IsValueType || buffer == null) return false;

        if(_waypointBlockData != null)
        {
            foreach(var blockData in _waypointBlockData.blockDataArray)
            {
                if (!blockData._inUse)
                {
                    TryReleaseWaypoints(requester, buffer);
                    blockData._inUse = true;
                    buffer.AddRange(blockData._waypointPositions);
                    _inUseBlockTracker[requester] = blockData;
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryReleaseWaypoints(object requester, List<Vector3> buffer)
    {
        if (requester == null || requester.GetType().IsValueType) return false;
        
        if(_inUseBlockTracker.Remove(requester, out var block))
        {
            block._inUse = false;
            if(buffer != null) buffer.Clear();
            return true;
        }

        return false;
    }
    //END NEW SETUP


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

    public Task InitialiseAsync(IResourceLocation location)
    {
        throw new NotImplementedException();
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


public class WaypointResourcesNew : SceneResources, IWaypointService
{
    private WaypointBlockData _waypointBlockData;
    private Dictionary<object, BlockData> _inUseBlockTracker = new(20);

    public async Task InitialiseAsync(IResourceLocation location)
    {
        try
        {
  
            // Load the asset from Addressables
            var waypointHandle = Addressables.LoadAssetAsync<WaypointBlockData>(location);

            // Wait for the asset to be loaded
            await waypointHandle.Task;

            // Check if the loading succeeded
            if (waypointHandle.Status == AsyncOperationStatus.Succeeded)
            {
                // Asset is loaded successfully, cast it to the correct type
                _waypointBlockData = waypointHandle.Result;

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

            //NotifyClassDependancies();
            // Subscribe to the resource requested event
            // ResourceRequestBus<WaypointBlockRequest>.On += WaypointsRequested; /// NEW WAY to test
            // SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;
            // SceneEventAggregator.Instance.OnAIResourceRequested += AIResourceRequested; /////// CURRENT WAY
            //   SceneEventAggregator.Instance.OnResourceReleased += ResourceReleased;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading waypoint resources: {e.Message}");
        }
    }

    public override async Task LoadResources()
    {
        try
        {
         /*   var waypointHandleNew = Addressables.LoadAssetAsync<WaypointBlockData>("MoonSceneWP");

            await waypointHandleNew.Task;
            _waypointBlockData = waypointHandleNew.Result;*/
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

            //NotifyClassDependancies();
            // Subscribe to the resource requested event
            // ResourceRequestBus<WaypointBlockRequest>.On += WaypointsRequested; /// NEW WAY to test
           // SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;
           // SceneEventAggregator.Instance.OnAIResourceRequested += AIResourceRequested; /////// CURRENT WAY
         //   SceneEventAggregator.Instance.OnResourceReleased += ResourceReleased;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading waypoint resources: {e.Message}");
        }

    }

    [Obsolete]
    protected override void NotifyClassDependancies()
    {
        bool exists = SceneEventAggregator.Instance.CheckDependancyExists(typeof(PathRequestManagerObsolete));

        if (!exists)
        {

            SceneEventAggregator.Instance.AddDependancy(new PathRequestManagerObsolete());
        }

        exists = SceneEventAggregator.Instance.CheckDependancyExists(typeof(AgentZoneRegistryNew));

        if (!exists)
        {
            SceneEventAggregator.Instance.AddDependancy(new AgentZoneRegistryNew());
        }

        exists = SceneEventAggregator.Instance.CheckDependancyExists(typeof(PlayerFlankingResourcesObsolete));

        if (!exists)
        {
            SceneEventAggregator.Instance.AddDependancy(new PlayerFlankingResourcesObsolete());
        }
        else
        {
            Debug.LogError("Player Flanking Resources already exists, not adding again.");
        }
       
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

    //NEW SETUP
    

    public bool TryGetWaypoints(object requester, List<Vector3> buffer)
    {
        if (requester == null || requester.GetType().IsValueType || buffer == null) return false;

        if(_waypointBlockData != null)
        {
            foreach(var blockData in _waypointBlockData.blockDataArray)
            {
                if (!blockData._inUse)
                {
                    TryReleaseWaypoints(requester, buffer);
                    blockData._inUse = true;
                    buffer.AddRange(blockData._waypointPositions);
                    _inUseBlockTracker[requester] = blockData;
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryReleaseWaypoints(object requester, List<Vector3> buffer)
    {
        if (requester == null || requester.GetType().IsValueType) return false;
        
        if(_inUseBlockTracker.Remove(requester, out var block))
        {
            block._inUse = false;
            if(buffer != null) buffer.Clear();
            return true;
        }

        return false;
    }

   
    //END NEW SETUP

}
