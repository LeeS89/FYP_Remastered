using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.ResourceLocations;


public class SceneEventAggregator : MonoBehaviour
{
    public static SceneEventAggregator Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    #region Global Scene Events
    public event Action OnSceneStarted;
    public event Action OnSceneEnded;
   // public event Action<ResourceRequest> OnResourceRequested;
   // public event Action<AIDestinationRequestData> OnAIResourceRequested;
    public event Action<ResourceRequest> OnResourceReleased;
    //public event Action<List<Type>> OnDependanciesAdded;
    public event Action<SceneResources> OnDependancyAdded;
    public event Func<Type, bool> OnCheckDependencyExists;

    // new way
    public delegate void ResourcesRequestedHandler(in ResourceRequests request);
    public event ResourcesRequestedHandler OnResourceRequested;
   // public event Action<ResourceRequests> OnResourcesRequested;

    public void ResourceRequested(in ResourceRequests request) => OnResourceRequested?.Invoke(request);

    //NEW AI
    //public event Action<T> OnAIResourceRequest;
    // END NEW AI

    public void SceneStarted() => OnSceneStarted?.Invoke();


    public void SceneEnded() => OnSceneEnded?.Invoke();


   /* [Obsolete("Use RequestingResource instead")]
    public void RequestResource(ResourceRequest request)
    {
        if (request is AIDestinationRequestData aiRequest)
        {
            OnAIResourceRequested?.Invoke(aiRequest);
        }
        else
        {
            OnResourceRequested?.Invoke(request);
        }
           
    }*/

    public void ReleaseResource(ResourceRequest request) => OnResourceReleased?.Invoke(request);


    /*public void AddDependancies(List<Type> resourceDependancies)
    {
        
        OnDependanciesAdded?.Invoke(resourceDependancies);
    }*/

    public void AddDependancy(SceneResources resource)
    {
        // Invoke the event with the resource
        OnDependancyAdded?.Invoke(resource);
    }

    public bool CheckDependancyExists(Type dependency) => OnCheckDependencyExists?.Invoke(dependency) ?? false;

    #endregion


    #region AI Agent Events

    #region Player Flanking Point Events
    /// <summary>
    /// Events used with Enemy AI system to notify when the closest flanking point to player has changed.
    /// When there is an active Alert status - OnClosestPointToPlayerChanged will be invoked by the player when ever they stop moving
    /// This in turn runs the ClosestPointToPlayerJob to find the closest point to player. Once job completes,
    /// OnClosestPointToPlayerJobComplete notifies all interested parties with the index of the closest point to player.
    /// </summary>
    public event Action OnClosestPointToPlayerChanged;
    public event Action<int> OnClosestFlankPointToPlayerJobComplete;
    public event Action OnRunClosestPointToPlayerJob; 
    //public event Action<ResourceRequest> OnFlankPointsRequested;

    public void ClosestPointToPlayerchanged() // Player will invoke this event, and the scene manager will listen to it
    {
        OnClosestPointToPlayerChanged?.Invoke();
    }

    public void ClosestFlankPointToPlayerJobComplete(int pointIndex)
    {
        OnClosestFlankPointToPlayerJobComplete?.Invoke(pointIndex);
    }

    public void RunClosestPointToPlayerJob()
    {
        OnRunClosestPointToPlayerJob?.Invoke();
    }

    /* public void FlankPointsRequested(ResourceRequest request) // Change
     {
         OnFlankPointsRequested?.Invoke(request);
     }*/
    #endregion Scene Events

    #region Agent Zone Registry Events
    public event Action<FSMControllerBase, int> OnAgentZoneRegistered;
    
    public event Action<FSMControllerBase, int> OnAgentZoneUnRegistered;
    public event Action<int, FSMControllerBase> OnAlertZoneAgents;

    public void RegisterAgentAndZone(FSMControllerBase agent, int zone)
    {
        OnAgentZoneRegistered?.Invoke(agent, zone);
    }

    public void UnRegisterAgentAndZone(FSMControllerBase agent, int zone)
    {
        OnAgentZoneUnRegistered?.Invoke(agent, zone);
    }

    public void AlertZoneAgents(int zone, FSMControllerBase source)
    {
        OnAlertZoneAgents?.Invoke(zone, source);
    }

    // NEW

    public Action<INotificationListener, ZoneId> OnRegisterAgentAndZone;
    public void RegisterAgentAndZone(INotificationListener agent, ZoneId zone)
        => OnRegisterAgentAndZone?.Invoke(agent, zone);

    public Action<INotificationListener, ZoneId> OnUnRegisterAgentAndZone;
    public void UnregisterAgentAndZone(INotificationListener agent, ZoneId zone)
        => OnUnRegisterAgentAndZone?.Invoke(agent, zone);

    public Func<ZoneId, INotificationListener, bool> OnAlertAgentsInZone;
    public bool AlertAgentsInZone(ZoneId zone, INotificationListener listener)
        => OnAlertAgentsInZone?.Invoke(zone, listener) ?? true; // if no subscribers, default to true
    
    // END NEW
    #endregion



    #endregion
}

public class SceneServiceBus : ISceneServiceProvider
{
 
    private readonly string _sceneName;
    public event Action OnSceneBegin;
    public event Action OnSceneEnd;

    private List<ITickable> _tickables = new(5);
    private IWaypointService _waypointService;
    private IAgentAlertService _npcService;
    private IFlankService _flankService;
    private IPoolService _poolService;
    private IPathService _pathService;
    private IGameManager _gameManager;
    private IDistanceService _distanceService;
    private ISceneService _sceneService;

    //private PathRequestManagerNew _pathService;

    public SceneServiceBus(string sceneName, ISceneService sceneService)
    {
        _sceneService = sceneService;
        _sceneName = sceneName;
    }


    public async Task InitialiseServicesAsync()
    {
        _gameManager = GameManager.Instance;
        // Initialise Waypoint Service
        WaypointResourcesNew wp = await ServiceFactory.TryCreateAsync<WaypointBlockData, WaypointResourcesNew>(_sceneName, "Waypoints");
         
        if(wp == null)
            Debug.LogError("Failed to initialise Waypoint Service");
        else
        {
            // _NpcService is only used in scenes containing waypoints
            if(wp is ITickable t) _tickables.Add(t);
            _waypointService = wp;
            Debug.LogError("Waypoint Service Initialised");
        }
            
        /* // Initialise Flank Service
         _flankService = await ServiceFactory.TryCreateAsync<FlankPointBlockData, IFlankService, FlankPointServiceNew>(_sceneName, "FlankPointService")
             ?? throw new Exception("Failed to initialise Flank Point Service");*/
    }

    public void Tick(float dt)
    {
        if (_tickables == null || _tickables.Count == 0) return;

        foreach(var t in _tickables)
            t.Tick(dt);

    }

   
    private bool IsPlayer(ITargetable targetable)
    {
        if (_gameManager == null) return false;
        return _gameManager.IsPlayerRef(targetable);//targetable == _gameManager.TryGetPlayer();
    }

    public void OnTargetableDied(ITargetable targetable)
    {
        if (targetable == null) return;

        if(IsPlayer(targetable))
            _gameManager.PlayerDied();
    }

    public void OnTargetableRespawned(ITargetable targetable)
    {
        if (targetable == null) return;
        if (IsPlayer(targetable))
            _gameManager.PlayerRespawned();
    }

    public bool TryGetPathService(out IPathService pathService)
    {
        if(_pathService == null)
        {
            PathRequestManagerNew ps = new PathRequestManagerNew();
            if(ps is ITickable t) _tickables.Add(t);
            _pathService = ps;
        }
        pathService = _pathService;
        return true;
    }

    public bool TryGetWaypointService(out IWaypointService waypointService)
    {
        waypointService = _waypointService;
        return _waypointService != null;
    }

    public bool TryGetAgentAlertService(out IAgentAlertService npcService)
    {
        if(_npcService == null) _npcService = new AgentZoneRegistryNew();
        npcService = _npcService;
        return _npcService != null;
    }

    public bool TryGetFlankService(out IFlankService flankService)
    {
        flankService = _flankService;
        return _flankService != null;
    }

    public bool TryGetPoolService(out IPoolService poolService)
    {
        if(_poolService == null) _poolService = new PoolLoaderNew();
        poolService = _poolService;
        return true;
    }

    public bool TryGetPlayerRefService(out IPlayerRefService playerRefService)
    {
        playerRefService = _gameManager;
        return _gameManager != null;
    }

    public bool TryGetDistanceService(out IDistanceService distanceService)
    {
        if(_distanceService == null)
        {
            DistanceManagerJob dj = new DistanceManagerJob();

            if(dj is ITickable t) _tickables.Add(t);
            _distanceService = dj;
        }
        distanceService = _distanceService;

        return _distanceService != null;
    }

    public bool TryGetSceneService(out ISceneService sceneService)
    {
        sceneService = _sceneService;
        return _sceneService != null;
    }

    #region Obsolete
    [Obsolete]
    public bool AlertAgentsInZone(ZoneId zone, INotificationListener listener)
       => OnAlertAgentsInZone?.Invoke(zone, listener) ?? true; // if no subscribers, default to true
    [Obsolete]
    public Action<INotificationListener, ZoneId> OnRegisterAgentAndZone;
    [Obsolete]
    public void RegisterAgentAndZone(INotificationListener agent, ZoneId zone)
        => OnRegisterAgentAndZone?.Invoke(agent, zone);

    [Obsolete]
    public Action<INotificationListener, ZoneId> OnUnRegisterAgentAndZone;
    [Obsolete]
    public void UnregisterAgentAndZone(INotificationListener agent, ZoneId zone)
        => OnUnRegisterAgentAndZone?.Invoke(agent, zone);

    [Obsolete]
    public Func<ZoneId, INotificationListener, bool> OnAlertAgentsInZone;

    [Obsolete]
    public void ResourceRequested(in ResourceRequests request) => OnResourceRequested?.Invoke(request);

    [Obsolete]
    public void ReleaseResource(ResourceRequest request) => OnResourceReleased?.Invoke(request);

    [Obsolete]
    public void AddDependancy(SceneResources resource)
    {
        // Invoke the event with the resource
        OnDependancyAdded?.Invoke(resource);
    }
    [Obsolete]
    public bool CheckDependancyExists(Type dependency) => OnCheckDependencyExists?.Invoke(dependency) ?? false;

   

    [Obsolete]
    public event Action<ResourceRequest> OnResourceReleased;
    //public event Action<List<Type>> OnDependanciesAdded;
    [Obsolete]
    public event Action<SceneResources> OnDependancyAdded;
    [Obsolete]
    public event Func<Type, bool> OnCheckDependencyExists;

    // new way
    [Obsolete]
    public delegate void ResourcesRequestedHandler(in ResourceRequests request);
    [Obsolete]
    public event ResourcesRequestedHandler OnResourceRequested;
    // public event Action<ResourceRequests> OnResourcesRequested;

    #endregion 

}





public interface ISceneServiceProvider : ISceneAIServices, IScenePoolServices, IPlaceholderService
{
    bool TryGetSceneService(out ISceneService sceneService);
}

//public interface IService : IGlobalServices { }



public interface ISceneService
{
    event Action OnSceneBegin;
    event Action OnSceneEnd;
    void OnTargetableDied(ITargetable targetable);
    void OnTargetableRespawned(ITargetable targetable);
}

public interface IGameManager : IPlayerRefService
{
    bool IsPlayerRef(ITargetable compareTarget);
    void PlayerDied();
    void PlayerRespawned();
}


public interface IPlayerRefService
{
    event Action OnPlayerDied;
    event Action OnPlayerRespawned;
    bool TryGetPlayer(out ITargetable player);
}

public interface IUtilityServices
{
    bool TryGetDistanceService(out IDistanceService distanceService);
}

public interface ISceneAIServices : IUtilityServices
{
    bool TryGetPlayerRefService(out IPlayerRefService playerRefService);

    bool TryGetPathService(out IPathService pathService);
   // IPathService PathService { get; }

    bool TryGetWaypointService(out IWaypointService waypointService);
   // IWaypointService WaypointService { get; }

    bool TryGetAgentAlertService(out IAgentAlertService npcService);
   // INpcService NpcService { get; }

    bool TryGetFlankService(out IFlankService flankService);
   // IFlankService FlankService { get; }
}

public interface IPlaceholderService { }

public interface IPathService
{
    void RequestPath(Vector3 from, Vector3 to, NavMeshPath path, Action<PathResult> onRequestComplete);
}

public interface IAddressableService
{
    Task InitialiseAsync(IResourceLocation location);
}

public interface IWaypointService : IAddressableService
{
    bool TryGetWaypoints(object requester, List<Vector3> buffer);

    bool TryReleaseWaypoints(object requester, List<Vector3> buffer);
}

public interface IFlankService : IAddressableService
{
    void TryGetFlankCandidates(Vector3 flankTargetPos, int numSteps, List<Vector3> buffer, Action<bool> OnRequestComplete);
}

public interface IClosestFlankPointService
{
    void RequestClosestIndex(int id, Vector3 targetPosition, Action<int, int, bool> OnRequestComplete);
}

public interface IScenePoolServices
{
    bool TryGetPoolService(out IPoolService poolService);
   // IPoolService PoolService { get; }
}

public interface IDistanceService 
{
    int RegisterSubscriber(Vector3 position, ITargetable target/*Vector3 targetPosiiton*/, /*float bufferMultiplier,*/ Action<float/*, float*/> callback);
    bool UnregisterSubscriber(int subscriberId);
}

public interface IAgentAlertService
{
    void RegisterAgentAndZone(INotificationListener agent, ZoneId zone);
    void UnregisterAgentAndZone(INotificationListener agent, ZoneId zone);
    bool TryAlertAgentsInZone(ZoneId zone, INotificationListener listener);
}

public interface  IPoolService
{
    void RequestPool(PoolIdSO poolIdRef, Action<PoolRequestResult, string, IPoolManager> onRequestComplete);
  
}

public interface IServicable
{
    void Init(ISceneServiceProvider provider, EventManagerBase manager);
    void Unload();
}
public interface IServicable<TServices, TManager> : IServicable
{
    void Init(TServices services, TManager manager);
    
}



public interface Test1
{
    void Helping();
}
public interface Test2 : Test1 { }


public class TestClass1
{
    public virtual void TestMethod<T>(T one) { }
}

public class TestClass2 : TestClass1
{
    public override void TestMethod<Test3>(Test3 one)
    {
       // one.Help();
        base.TestMethod(one);
    }
   
}


public interface IBase { }
public interface IChild : IBase { }

public abstract class Handler<T> where T : IBase
{
    public abstract void Handle(T obj);
}

public class ChildHandler : Handler<IChild>
{
    public override void Handle(IChild obj)
    {
        // Can assume IChild here
    }
}

