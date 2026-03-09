using Services.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class WaypointResources : SceneResources, IWaypointService, IAddressableService
{
    private AsyncOperationHandle<WaypointBlockData>? _wpHandle;

    private WaypointBlockData _waypointBlockData;
    private Dictionary<object, BlockData> _inUseBlockTracker = new(20);

    [Obsolete]
    public async Task<bool> TryInitialiseAsync(IResourceLocation location)
    {
        throw new NotImplementedException();
    }

   

    public async Task<bool> TryInitialiseAsync(string addressKey)
    {
        if (string.IsNullOrWhiteSpace(addressKey)) { DebugLogs.RequireNotNull(addressKey, "addressKey", this); return false; }
        
        // Load the asset from Addressables
        _wpHandle = await AddressableLoader.TryLoadAssetAsync<WaypointBlockData>(addressKey);

        if (!_wpHandle.HasValue)
        {
            DebugLogs.Nre(_wpHandle, "_wpHandle", this);
           // Dispose();
            return false;
        }

        _waypointBlockData = _wpHandle.Value.Result;

        if (_waypointBlockData == null)
        {
           // Dispose();

            DebugLogs.Nre(_waypointBlockData, "_waypointBlockData", this);
            return false;
        }

        var blocks = _waypointBlockData.blockDataArray;

        if (blocks == null || blocks.Length == 0)
        {
            DebugLogs.Err("Waypoint block data array is null or contains no elements", this);
            //Dispose();
            return false;
        }

        foreach (var block in _waypointBlockData.blockDataArray)
            block._inUse = false;

        DebugLogs.Log("Successfully initialized waypoint blocks", this);
        return true;

    }

    public bool TryGetWaypoints(object requester, List<Vector3> buffer)
    {
        if (requester == null || requester.GetType().IsValueType || buffer == null) return false;

        if (_waypointBlockData != null)
        {
            foreach (var blockData in _waypointBlockData.blockDataArray)
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

    public void Dispose()
    {
        if (_wpHandle.Value.IsValid())
            Addressables.Release(_wpHandle.Value);
    }

    [Obsolete]
    public async Task InitialiseAsyncOldToKeepItRunningForNow(IResourceLocation location)
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
        bool exists = SceneEventAggregatorObsolete.Instance.CheckDependancyExists(typeof(PathRequestManagerObsolete));

        if (!exists)
        {

            SceneEventAggregatorObsolete.Instance.AddDependancy(new PathRequestManagerObsolete());
        }

        exists = SceneEventAggregatorObsolete.Instance.CheckDependancyExists(typeof(AgentZoneRegistryNew));

        if (!exists)
        {
            SceneEventAggregatorObsolete.Instance.AddDependancy(new AgentZoneRegistryNew());
        }

        exists = SceneEventAggregatorObsolete.Instance.CheckDependancyExists(typeof(PlayerFlankingResourcesObsolete));

        if (!exists)
        {
            SceneEventAggregatorObsolete.Instance.AddDependancy(new PlayerFlankingResourcesObsolete());
        }
        else
        {
            Debug.LogError("Player Flanking Resources already exists, not adding again.");
        }

    }

    protected override void ResourceRequested(in ResourceRequests request)
    {
        if (request.AIResourceType != AIResourceType.WaypointBlock) return;

        if (_waypointBlockData == null)
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


  

    public bool TryReleaseWaypoints(object requester, List<Vector3> buffer)
    {
        if (requester == null || requester.GetType().IsValueType) return false;

        if (_inUseBlockTracker.Remove(requester, out var block))
        {
            block._inUse = false;
            if (buffer != null) buffer.Clear();
            return true;
        }

        return false;
    }

   




    //END NEW SETUP

}
