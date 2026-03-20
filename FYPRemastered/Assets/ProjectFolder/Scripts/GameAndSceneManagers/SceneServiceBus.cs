using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Services.Internal
{

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
        private IDistanceMonitoringService _distanceService;
        private ISceneService _sceneService;

        private FsmFactory _fsmFactory;
        private SceneMetaData metaData;

        //private PathRequestManagerNew _pathService;

        public SceneServiceBus(string sceneName, ISceneService sceneService)
        {
            _sceneService = sceneService;
            _sceneName = sceneName;
        }

        // NEW with meta data
        public async Task NewInit()
        {

            metaData = await AddressableLoader.LoadMetaAsyncNew<SceneMetaData>(_sceneName);
            _gameManager = GameManager.Instance;
            if (metaData == null)
            {
                DebugLogs.Err("No Scene meta data found", this);
                return;
            }

            DebugLogs.Err("Meta successfully loaded", this);

            if (metaData.FsmFeatures.UsedInScene)
            {
                _fsmFactory = new FsmFactory(metaData.FsmFeatures);
                await _fsmFactory.InitialiseAsync();
            }

        }
        // END NEW with meta data



        public void Tick(float dt)
        {
            if (_tickables == null || _tickables.Count == 0) return;

            foreach (var t in _tickables)
                t.Tick(dt);

        }

        private List<ISceneService> _services = new(5);

        public bool TryGetService<T>(out T service) where T : class, ISceneService // Placeholder interface
        {
            foreach (var s in _services)
            {
                if (s is T typed)
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
                PathRequestManager ps = new PathRequestManager();
                if (ps is ITickable t) _tickables.Add(t);
                _pathService = ps;
            }
            pathService = _pathService;
            return true;
        }

        public bool TryGetWaypointService(out IWaypointService waypointService)
        {
            waypointService = _fsmFactory?.GetWService();//_waypointService;



            return waypointService != null;//_waypointService != null;
        }

        public bool TryGetAgentAlertService(out IAgentAlertService npcService)
        {
            if (_npcService == null) _npcService = new AgentZoneRegistry();
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

        public bool TryGetDistanceService(out IDistanceMonitoringService distanceService)
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
    bool TryGetDistanceService(out IDistanceMonitoringService distanceService);
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
public interface IAddressableService : IDisposable
{
    Task<bool> TryInitialiseAsync(FeatureMeta data);

}

public interface IFsmStateService
{
    bool TryGetOwnerPosition(IInstanceIdentifiable id, out Vector3 pos);
    bool TryGetPath(IInstanceIdentifiable id, out NavMeshPath path);

    bool TryGetDestinationCandidates(IInstanceIdentifiable id, List<Vector3> buffer);
    void ReleaseDestinationCandidates(IInstanceIdentifiable id, List<Vector3> buffer);
}

public interface IChaseService : IFsmStateService
{
    //bool TryGetDestinationCandidates(IInstanceIdentifiable id, List<Vector3> buffer);
    bool TargetIsMoving(IInstanceIdentifiable id);
    bool TryGetSqrDistanceToTarget(IInstanceIdentifiable id, Vector3 from, out float sqrDistance);

    bool TryRegisterDistanceToTargetMonitoring(IInstanceIdentifiable id, Action<float> onDistanceUpdate, out float initialDistance);
    bool TryUnregisterDistanceToTargetMonitoring(IInstanceIdentifiable id);
}

public interface IWaypointService : IFsmStateService// : IAddressableServiceObsolete
{
    bool TryGetWaypoints(object requester, List<Vector3> buffer);

    bool TryReleaseWaypoints(object requester, List<Vector3> buffer);
}

public interface IFlankService : IFsmStateService// : IAddressableServiceObsolete
{
    void TryGetFlankCandidates(Vector3 flankTargetPos, int numSteps, List<Vector3> buffer, Action<bool> OnRequestComplete);
}

public interface IClosestIndexService
{
    
    void RequestClosestIndex(int id, Vector3 targetPosition, Action<int, int, bool> OnRequestComplete);
}

public interface IInitializable { }

public interface IListInitializable<T> : IInitializable
{
    bool TryInit(IReadOnlyList<T> data);
}


public interface IScenePoolServices
{
    bool TryGetPoolService(out IPoolService poolService);
    // IPoolService PoolService { get; }
}

public interface IDistanceMonitoringService
{
    int RegisterSubscriber(Vector3 position, ITargetable target/*Vector3 targetPosiiton*/, /*float bufferMultiplier,*/ Action<float/*, float*/> callback);
    bool TryRegisterSubscriber(IInstanceIdentifiable id, Vector3 currentPosition, ITargetable targetToCompare, Action<float> callback);
    bool TryUnregisterSubscriber(IInstanceIdentifiable id);
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

