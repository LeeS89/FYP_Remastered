using Services.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.ResourceLocations;

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

    private FsmFactory _fsmFactory;

    //private PathRequestManagerNew _pathService;

    public SceneServiceBus(string sceneName, ISceneService sceneService)
    {
        _sceneService = sceneService;
        _sceneName = sceneName;
    }

    // NEW with meta data
    public async Task NewInit()
    {
        var meta = await ServiceFactory.LoadMetaAsync(_sceneName);

        if(meta == null)
        {
            DebugLogs.Err("No Scene meta data found", this);
            return;
        }

        if (meta.FsmFeatures.UsedInScene) _fsmFactory = new FsmFactory(_sceneName);
        
    }
    // END NEW with meta data


    public async Task InitialiseServicesAsync()
    {
        _gameManager = GameManager.Instance;
        // Initialise Waypoint Service
        WaypointResources wp = await ServiceFactory.TryCreateAsync<WaypointBlockData, WaypointResources>(_sceneName, "Waypoints");

        if (wp == null)
            Debug.LogError("Failed to initialise Waypoint Service");
        else
        {
            // _NpcService is only used in scenes containing waypoints
            if (wp is ITickable t) _tickables.Add(t);
            _waypointService = wp;
            Debug.LogError("Waypoint Service Initialised");
        }
       // await Task.CompletedTask;

       // FsmFactory fsmF = new FsmFactory(_sceneName);
        //await fsmF.InitialiseServicesAsync();
        /* // Initialise Flank Service
         _flankService = await ServiceFactory.TryCreateAsync<FlankPointBlockData, IFlankService, FlankPointServiceNew>(_sceneName, "FlankPointService")
             ?? throw new Exception("Failed to initialise Flank Point Service");*/
    }


    private async Task TryInitializeFsmService()
    {
        bool exists = await ServiceFactory.ExistsInSceneAsync<WaypointBlockData>(_sceneName, "Waypoints");
        if (!exists) return;

        _fsmFactory = new FsmFactory(_sceneName);
        await _fsmFactory.InitialiseServicesAsync();

    }

    public void Tick(float dt)
    {
        if (_tickables == null || _tickables.Count == 0) return;

        foreach (var t in _tickables)
            t.Tick(dt);

    }

    private List<ISceneService> _services = new(5);

    public bool TryGetService<T>(out T service) where T : class, ISceneService // Placeholder interface
    {
        foreach(var s in _services)
        {
            if(s is T typed)
            {
                service = typed;
                return true;
            }
        }
        service = null;
        return false;
    }
    

    private bool IsPlayer(ITargetable targetable)
    {
        if (_gameManager == null) return false;
        return _gameManager.IsPlayerRef(targetable);//targetable == _gameManager.TryGetPlayer();
    }

    public void OnTargetableDied(ITargetable targetable)
    {
        if (targetable == null) return;

        if (IsPlayer(targetable))
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
        if (_pathService == null)
        {
            PathRequestManagerNew ps = new PathRequestManagerNew();
            if (ps is ITickable t) _tickables.Add(t);
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
        if (_npcService == null) _npcService = new AgentZoneRegistryNew();
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
        if (_poolService == null) _poolService = new PoolLoaderNew();
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
        if (_distanceService == null)
        {
            DistanceManagerJob dj = new DistanceManagerJob();

            if (dj is ITickable t) _tickables.Add(t);
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
    void RequestPath(Vector3 from, Vector3 to, NavMeshPath path, Action<DestinationResult> onRequestComplete);
}

public interface IAddressableServiceObsolete
{
    Task InitialiseAsync(IResourceLocation location);
}
public interface IAddressableService
{
    Task<bool> TryInitialiseAsync(IResourceLocation location);
    Task InitialiseAsyncOldToKeepItRunningForNow(IResourceLocation location);
}

public interface IWaypointService// : IAddressableServiceObsolete
{
    bool TryGetWaypoints(object requester, List<Vector3> buffer);

    bool TryReleaseWaypoints(object requester, List<Vector3> buffer);
}

public interface IFlankService : IAddressableServiceObsolete
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
    bool TryRegisterAgentAndZone(INotificationListener agent, ZoneId zone);
    void UnregisterAgentAndZone(INotificationListener agent, ZoneId zone);
    bool TryAlertAgentsInZone(ZoneId zone, INotificationListener listener);
}

public interface IPoolService
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

