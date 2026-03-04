using Npc.API;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class FsmFactory : IFsmService
{
    private readonly string _sceneName;
    private List<ITickable> _tickables = new(5);
    private IWaypointService _wpService;
    private IFlankService _flankService;
    private IDistanceService _distService;

    // SO's
    private AgentPatrolData _agentPatrolData;
    private AgentChaseData _agentChaseData;
    // End SO's

    private FsmFactory() { }

    public FsmFactory(string sceneName)
    {
        _sceneName = sceneName;
    }

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    public async Task InitialiseServicesAsync()
    {
        WaypointResources wp = await ServiceFactory.TryCreateAsync<WaypointBlockData, WaypointResources>(_sceneName, "Waypoints");

        if (wp == null)
            DebugLogs.Err("Failed to initialise Waypoint Service", this);
        else
        {

            IResourceLocation apd = await ServiceFactory.TryGetSingleLocationAsync<AgentPatrolData>(_sceneName, "AgentPatrolData");

            if (apd != null)
            {
                var apdHandle = Addressables.LoadAssetAsync<AgentPatrolData>(apd);
                await apdHandle.Task;

                if(apdHandle.Status == AsyncOperationStatus.Succeeded) _agentPatrolData = apdHandle.Result;
                if (_agentPatrolData == null) DebugLogs.Nre(_agentPatrolData, "Agent Patrol Data is null", this);
                else DebugLogs.Err("Agent patrol data loaded", this);
            }

            if (wp is ITickable t) _tickables.Add(t);
            _wpService = wp;
            Debug.LogError("Waypoint Service Initialised");
        }

        /* // Initialise Flank Service
         _flankService = await ServiceFactory.TryCreateAsync<FlankPointBlockData, IFlankService, FlankPointServiceNew>(_sceneName, "FlankPointService")
             ?? throw new Exception("Failed to initialise Flank Point Service");*/
    }

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

public interface IFsmService : ITickable
{
    bool TryCreateAndAddState(StateId id, Dictionary<StateId, IFsmState> _stateDict, NavMeshPath path, Transform ownerTransform, TryGetTarget targetRetrieverFunc);
}
