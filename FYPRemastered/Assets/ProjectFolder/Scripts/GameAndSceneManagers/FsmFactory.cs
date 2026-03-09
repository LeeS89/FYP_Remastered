using Npc.API;
using Services.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.XR.CoreUtils.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;


public class FsmFactory : ServiceBundle, IFsmService, ITickable
{
    [Obsolete("")]
    private readonly string _sceneName;

    
    private List<ITickable> _tickables = new(5);
    private IAddressableService _wpService; // Split interfaces in WaypointResources class so only correct interface is used by relevant classes i.e. => This class need only store it as an IAddressableService
    private IFlankService _flankService;
    private IDistanceService _distService;

    // SO's
    private AgentPatrolData _agentPatrolData;
    private AgentChaseData _agentChaseData;
    // End SO's

    private readonly List<AsyncOperationHandle> _handles = new(5);

    //private FsmFactory() { }

    /* public FsmFactory(string sceneName*//*, SceneMetadata data*//*)
     {
         _sceneName = sceneName;
     }*/

    // public FsmFactory(SceneMetaData data) => _metaData = data;
    public FsmFactory(SceneMetaData data) : base(data) { }

    public Task<bool> TryInitialiseAsync(IResourceLocation location)
    {
        throw new NotImplementedException();
    }

  

    public Task InitialiseAsyncOldToKeepItRunningForNow(IResourceLocation location)
    {
        throw new NotImplementedException();
    }
    // NEW META SETUP


    public IWaypointService GetWService() => _wpService as IWaypointService;


    public override async Task InitialiseAsync()
    {
 
        if (_metaData == null) 
        {
            DebugLogs.RequireNotNull(_metaData, "SceneMetaData", this);
            return;
        } //=> Possibly just switch all to manual systems

        var waypointFeature = _metaData.FsmFeatures.Waypoints;
        if (waypointFeature.enabled)
        {
            _wpService = await TryLoadStateServiceAndInitialize<WaypointBlockData, WaypointResources>(waypointFeature.addressKey);
            if (_wpService == null) DebugLogs.Nre(_wpService, "WaypointService");
            else DebugLogs.Err("Found Waypoint service was not null", this);
           
        }

     
        // Later, I will have an SO for scene Metadata which will show the services the current scene uses, so the TryGetLocation need only return locations
      //  var (waypointsUsedInScene, location) = await ServiceFactory.TryGetLocationAsync<WaypointBlockData>(sceneLabel: _sceneName, featureLabel: "Waypoints");

      /*  if (waypointsUsedInScene)
        {
            if (location != null)
            {
                _wpService = new WaypointResources();
                bool serviceInitSuccess = await _wpService.TryInitialiseAsync(location);

                if (!serviceInitSuccess)
                {
                    DebugLogs.LoadFail(_wpService, "Wp service Waypoint data", this);
                    // At this point, dispose of the _wpService class, but first ensure it release anything it has loaded
                    // Plus, this will switch to use of manual waypoint system when request is received
                }

            }
            else
            {
                DebugLogs.RequireNotNull(location, "waypointDataLocation", this);
                // Switch to yet to be implemented manual waypoint system when request is received
                return;// Instead of returning, let it continue to still try and load the state data since they can be still used in manual wp system
            }

            IResourceLocation apd = await ServiceFactory.TryGetSingleLocationAsync<AgentPatrolData>(_sceneName, "AgentPatrolData");

            if (apd != null)
            {
                var apdHandle = Addressables.LoadAssetAsync<AgentPatrolData>(apd);
                _agentPatrolData = await apdHandle.Task;
                _handles.Add(apdHandle);
            }
            else
                DebugLogs.LoadFail(apd, "Agent Patrol data location", this); // If the address load fails or the asset ends up being null,
                                                                             // Switch to manually defined data when request comes in


        }*/



    }

    private async Task<TConcrete> TryLoadStateServiceAndInitialize<TAsset, TConcrete>(string addressKey) where TConcrete : class, IAddressableService, new()
    {
        if (string.IsNullOrWhiteSpace(addressKey)) { DebugLogs.RequireNotNull(addressKey, "addressKey", this); return null; }
       
        var svc = new TConcrete();
        bool serviceInitSuccess = await svc.TryInitialiseAsync(addressKey);

        if (!serviceInitSuccess)
        {
            DebugLogs.LoadFail(svc, $"(The Service of {typeof(TConcrete).Name})", this);
            svc.Dispose();
            return null;
            // Call Dispose on scv and return null;
        }

        return svc;
    }
    // END NEW META SETUP


    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

   
    


/*    private async Task<bool> TryLoadPatrolServices()
    {
        var (waypointsUsedInScene, location) = await ServiceFactory.TryGetLocationAsync<WaypointBlockData>(sceneLabel: _sceneName, featureLabel: "Waypoints");

        if (waypointsUsedInScene)
        {
            if (location != null)
            {
                _wpService = new WaypointResources();
                bool serviceInitSuccess = await _wpService.TryInitialiseAsync(location);

                if (!serviceInitSuccess)
                {
                    DebugLogs.LoadFail(_wpService, "Wp service Waypoint data", this);
                    // At this point, dispose of the _wpService class, but first ensure it release anything it has loaded
                    // Plus, this will switch to use of manual waypoint system when request is received
                    return false;
                }

            }
            else
            {
                DebugLogs.RequireNotNull(location, "waypointDataLocation", this);
                // Switch to yet to be implemented manual waypoint system when request is received
                return false;// Instead of returning, let it continue to still try and load the state data since they can be still used in manual wp system
            }

            IResourceLocation apd = await ServiceFactory.TryGetSingleLocationAsync<AgentPatrolData>(_sceneName, "AgentPatrolData");

            if (apd != null)
            {
                var apdHandle = Addressables.LoadAssetAsync<AgentPatrolData>(apd);
                _agentPatrolData = await apdHandle.Task;
                _handles.Add(apdHandle);
                return true;
            }
            else
            {
                DebugLogs.LoadFail(apd, "Agent Patrol data location", this);
                return false;
            }
                // If the address load fails or the asset ends up being null,
                                                                             // Switch to manually defined data when request comes in
            

        }
        return false;
    } */


    public void LateTick(float dt) { }
   

    public void Tick(float dt)
    {
        if (_tickables == null || _tickables.Count == 0) return;

        foreach (var t in _tickables)
            t.Tick(dt);
    }

    /// <summary>
    /// Attempts to create a new state with the specified identifier and add it to the provided state dictionary.
    /// </summary>
    /// <param name="id">The unique identifier for the state to create and add.</param>
    /// <param name="_stateDict">A dictionary that maps state identifiers to their corresponding state instances. The new state will be added to
    /// this dictionary if creation succeeds.</param>
    /// <param name="path">The navigation path to associate with the new state. Cannot be null.</param>
    /// <param name="ownerTransform">The transform representing the owner of the state. Used to initialize the new state.</param>
    /// <param name="targetRetrieverFunc">A delegate used to retrieve the target for the state. The function is invoked whenever a state needs to know its targets position</param>
    /// <returns>true if the state was successfully created and added to the dictionary; otherwise, false.</returns>
    public bool TryCreateAndAddState(StateId id, Dictionary<StateId, IFsmState> _stateDict, NavMeshPath path, Transform ownerTransform, TryGetTarget targetRetrieverFunc)
    {
        if (id == StateId.None || _stateDict == null || path == null ||
            ownerTransform == null || targetRetrieverFunc == null) return false;

        if (_stateDict.ContainsKey(id)) return false;


        return id switch
        {
            StateId.Patrol => TryCreatePatrol(_stateDict, path, ownerTransform, targetRetrieverFunc),
            _ => false

        };

       /* switch (id)
        {
            case StateId.Patrol:
              //  FSMPatrolState ps = new FSMPatrolState();
               // _stateDict[id] = ps;
                return true;
            default:
                return false;
        }*/

    }

    private bool TryCreatePatrol(Dictionary<StateId, IFsmState> _dict, NavMeshPath path, Transform t, TryGetTarget tgt)
        => _dict.TryAdd(StateId.Patrol, new FSMPatrolState(null, null, null));

   
}

public interface IFsmService
{
    bool TryCreateAndAddState(StateId id, Dictionary<StateId, IFsmState> _stateDict, NavMeshPath path, Transform ownerTransform, TryGetTarget targetRetrieverFunc);
}
