using Npc.API;
using Npc.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public partial class NPCController
{
    private TryGetTarget OnTryGetCurrentTarget;

    [Obsolete]
    [SerializeField] private AgentFsmDepsObsolete _fsmDeps;
    // FSMManager Composition - Partly obsolete
    [Header("FOV Data")]
    [SerializeField] private FovData _fovDeps;
    protected AgentEventManager _eManager;
    // FSMManager Composition
    //private IPathResolver _pathFinder;
    private FovRunner _fovRunner;
    private IFsmController _fsmManager;
    // private FsmManager _fsmManager;
    //private IFsmControl _fsmManager;

    [Obsolete("", true)]
    private Dictionary<StateId, IFsmStateObsolete> _fsmStates = new(5);
    // end FSMManager Composition

    private INpcAnimationControl _animationControl;
    private ISceneAIServices _aiServices;
    private IPlayerRefService _playerRefService;
    private IAgentAlertService _alertService;
    private Notification _componentNotifications;
   // private Func<ITargetable> OnGetCurrentTarget;

    //Latest changes
    [SerializeField] private MovementConfig _moveCfg;
    [SerializeField] private PatrolStateConfig _patrolStateCfg;
    [SerializeField] private ChaseStateConfig _chanceStateCfg;
    [SerializeField] private FlankStateConfig _flankStateCfg;
    // end latest changes

    public override void Init(ISceneAIServices services, AgentEventManager manager)
    {
        SetManagerAndServices(services, manager);
      //  SetTargetableCollider();
        SetAgentParams();
        _componentNotifications = OnNotifies;

        var anim = GetComponentsInChildren<MonoBehaviour>(true).OfType<INpcAnimationControl>().FirstOrDefault();
        if (anim != null) _animationControl = anim;

        SetPrimaryTarget();

        ConstructFovRunner();
        _ = ConstructFSM();

        OnStableFOVResult = StableFOVResultConfirmed;
        
    }

    protected void SetPrimaryTarget()
    {
        if (_aiServices == null) return;

        if (_aiServices.TryGetPlayerRefService(out _playerRefService))
            _playerRefService.TryGetPlayer(out _primaryTarget);
        else
        {
#if UNITY_EDITOR
            Debug.LogError("NULL PLAYER REF");
#endif
        }
    }


    private void SetManagerAndServices(ISceneAIServices services, AgentEventManager manager)
    {
        if (manager == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError(manager + " is null in NPCControllerNew Init");
#endif
            _eManager = gameObject.AddComponent<AgentEventManager>();
        }
        else
            _eManager = manager;

        _aiServices = services;
       
    }

    private void ConstructFovRunner()
    {
        _fovDeps.SetTarget(_primaryTarget); // TESTING NOW

        var fovNotificationSender = new FovNotificationSender(_componentNotifications);
     //   _fovRunner = new NPCFieldOfViewHandlerNew(_fovDeps, onSweepComplete: _componentNotifications);
        _fovRunner = new FovRunner(_fovDeps, onNotify: fovNotificationSender);
    }

    private async Task ConstructFSM()
    {
        if (_aiServices.TryGetFsmFactory(out var factory))
        {
            try
            {
                var pathNotificationSenderNew = new PathNotificationSender(_componentNotifications);
                var animRequestNotificationSenderNew = new AnimationNotificationSender(_componentNotifications);
        
                _fsmManager = await factory.CreateFsm(callerId: this, body: this, OnTryGetCurrentTarget, tickHost: this, coroutineHost: this, pathNotificationSenderNew, animRequestNotificationSenderNew);

                if (_fsmManager is null) DebugLogs.Err("Factory returned null FSM manager", this);
                else DebugLogs.Log("Successfully created FSM manager with factory", this);

                OnNotifies(NpcNotification.SceneBegin());
            }
            catch (Exception ex)
            {
                DebugLogs.Err("Exception occurred while constructing FSM: " + ex.Message, this);
                //   DebugLogs.Throw(ex, "Exception during FSM construction", this);
            }
        }
        else
            DebugLogs.Err("Failed to retrieve Factory", this);


      /*  return;
        ConstructObsolete();*/
    }

    [Obsolete("", true)]
    private async Task ConstructFSMOld()
    {
        if (_aiServices.TryGetFsmFactory(out var factory))
        {
            var pathNotificationSenderNew = new PathNotificationSender(_componentNotifications);
            var animRequestNotificationSenderNew = new AnimationNotificationSender(_componentNotifications);
            /*    if (factory.TryCreateFsm(out var manager, this, this, OnTryGetCurrentTarget, tickHost: this, coroutineHost: this, pathNotificationSenderNew, animRequestNotificationSenderNew))
                {
                    _fsmManager = manager;
                }
                else
                    DebugLogs.Err("Failed to create FSM manager", this);*/
            _fsmManager = await factory.CreateFsm(this, this, OnTryGetCurrentTarget, this, this, pathNotificationSenderNew, animRequestNotificationSenderNew);

            if(_fsmManager is null) DebugLogs.Err("Factory returned null FSM manager", this);
            else DebugLogs.Err("Successfully created FSM manager with factory", this);

            OnNotifies(NpcNotification.SceneBegin());
        }
        else
            DebugLogs.Err("Failed to retrieve Factory", this);


      /*  return;
        ConstructObsolete();*/
    }

    [Obsolete("", true)]
    private void ConstructObsolete()
    {
        // Obsolete
        _fsmDeps.SetOwner(this);
        _fsmDeps.SetTarget(_primaryTarget);
        _fsmDeps.SetAgentRef(Agent);
        _fsmDeps.SetObstacleRef(Obstacle);
        _fsmDeps.SetPath(new NavMeshPath());
        // Obsolete end
        OnTryGetCurrentTarget = TryGetCurrentTarget;
        // if (_aiServices.TryGetPathService(out var pathService)) _fsmDeps._pathResolver = new PathFinder(pathService);//_pathFinder = new PathFinderNew(pathService);
        if (!_aiServices.TryGetPathService(out var pathService)) return;

        //  _fsmDeps._pathResolver = new PathFinder(pathService);
        IDestinationResolver pathResolver = null;// new PathFinder(pathService);

        // _fovRunner = new NPCFieldOfViewHandler(_fovParams);
        //_fovRunner = new NPCFieldOfViewHandler(_fovParams, _fsmDeps, onSweepComplete: _componentNotifications);

        // _fsmManager = new FSMBaseNew(data: this, resolver: _pathFinder, runner: _fovRunner, _fsmStates);
        var pathNotificationSender = new PathNotificationSender(_componentNotifications);
        var animRequestNotificationSender = new AnimationNotificationSender(_componentNotifications);

        FsmManagerServices s = new FsmManagerServices(Agent, Obstacle, _moveCfg);
        SharedFsmStateServices shared = new SharedFsmStateServices(Path, transform, OnTryGetCurrentTarget);

        _fsmManager = new FsmManagerObsoleteO(deps: s, shared, _fsmStates, pathNotificationSender, animRequestNotificationSender);
        //_fsmManager = new FsmManager(deps: _fsmDeps, _fsmStates, pathNotificationSender, animRequestNotificationSender);
        IPatrolService wps;
        if (_aiServices.TryGetWaypointService(out wps/*_fsmDeps._waypointService*/))
        {
            if (!_aiServices.TryGetAgentAlertService(out _alertService))
            {
#if UNITY_EDITOR
                Debug.LogError("Failed to retrieve alert service");
#endif
            }

            PatrolDeps pDeps = new PatrolDeps(wps, pathResolver, _patrolStateCfg);
            //IFSMState patrolState = new FSMPatrolState(wpService, data: this, resolver: _pathFinder, stateContext: _fsmManager);
            IFsmStateObsolete patrolState = new FSMPatrolStateObsolete(deps: pDeps, shared, /*data: this, resolver: _pathFinder,*/ stateEvents: /*_fsmManager*/null);
            // IFsmState patrolState = new FSMPatrolState(deps: _fsmDeps, /*data: this, resolver: _pathFinder,*/ stateEvents: _fsmManager);
            StateId pid = patrolState.GetId();
            _fsmStates.TryAdd(pid, patrolState);
        }
        else
            DebugLogs.Err("Failed to retrieve WaypointService", this);

        IFlankService fService;
        if (_aiServices.TryGetFlankService(out fService/*_fsmDeps._flankService*/))
        {
            FlankDeps fDeps = new FlankDeps(fService, pathResolver, _flankStateCfg);

            IFsmStateObsolete flankState = new FsmFlankStateObsolete(deps: fDeps, shared, /*data: this, _pathFinder,*/ /*_fsmManager*/null);
            // IFsmState flankState = new FsmFlankState(deps: _fsmDeps, /*data: this, _pathFinder,*/ _fsmManager);
            //IFSMState flankState = new FSMFlankState(flankService, data: this, _pathFinder, _fsmManager);
            StateId fid = flankState.GetId();
            _fsmStates.TryAdd(fid, flankState);
        }

        //IFSMState chaseState = new FSMChaseState(PrimaryTarget, data: this, resolver: _pathFinder, stateContext: _fsmManager);
        if (_aiServices.TryGetDistanceService(out var service))
        {
            /*Obsolete line*/
            _fsmDeps.SetDistanceService(service); // Maybe assume it is used by all states instead of just chase

            ChaseDeps cDeps = new ChaseDeps(service, pathResolver, _chanceStateCfg);
            IFsmStateObsolete chaseState = new FSMChaseStateObsolete(deps: cDeps, shared, stateContext: /*_fsmManager*/null);
            // IFsmState chaseState = new FSMChaseState(deps: _fsmDeps, stateContext: _fsmManager);
            StateId cid = chaseState.GetId();
            _fsmStates.TryAdd(cid, chaseState);
        }

        //_fsmManager.Notification = Notify;
        //_fsmManager.OnAnimationIntent = AnimationIntent;
        // _fsmManager.OnMapDestinationToZone = MapDestinationToZone;///// maybe when entering patrol
    }

    private void SetAgentParams()
    {
        if (TryGetComponent<NavMeshAgent>(out var agent)) Agent = agent;

        if (TryGetComponent<NavMeshObstacle>(out var ob)) Obstacle = ob;

        Path = new NavMeshPath();
    }

    private bool TryGetCurrentTarget(out ITargetable t)
    {
        t = _primaryTarget;
        return t != null;
    }

  
    protected override void OnSceneEnd()
    {
        //_fsmManager.Notification = null;
       // _fsmManager.OnAnimationIntent = null;
      //  _fsmManager.OnMapDestinationToZone = null;
      
        _eManager = null;
        // OnTargetSeen = null;
        // OnTargetLost = null;
    }

    protected override void OnSceneBegin()
    {
        //_animationControl?.SetIKLookTarget(_primaryTarget?.Transform);
       // _fsmManager?.SwitchTo(StateId.Patrol);
        
        //OnNotifies(NpcNotification.SceneBegin()); Remember to Un comment
    }

    public override void Unload()
    {
        
    }
}

namespace Npc.API
{
    public delegate bool TryGetTarget(out ITargetable t);
}



















































































public partial class NPCControllerNewest
{
    private TryGetTarget OnTryGetCurrentTarget;

   // [SerializeField] private AgentFsmDepsObsolete _fsmDeps;
    // FSMManager Composition - Partly obsolete
    [Header("FOV Data")]
    [SerializeField] private FovData _fovDeps;
    protected AgentEventManager _eManager;
    // FSMManager Composition
    //private IPathResolver _pathFinder;
    private FovRunner _fovRunner;
    private IFsmController _fsmManager;
    // private FsmManager _fsmManager;
    //private IFsmControl _fsmManager;

    [Obsolete("", true)]
    private Dictionary<StateId, IFsmStateObsolete> _fsmStates = new(5);
    // end FSMManager Composition

    private INpcAnimationControl _animationControl;
    private ISceneAIServices _aiServices;
    private IPlayerRefService _playerRefService;
    private IAgentAlertService _alertService;
    private Notification _componentNotifications;
    // private Func<ITargetable> OnGetCurrentTarget;

    //Latest changes
    [SerializeField] private MovementConfig _moveCfg;
    [SerializeField] private PatrolStateConfig _patrolStateCfg;
    [SerializeField] private ChaseStateConfig _chanceStateCfg;
    [SerializeField] private FlankStateConfig _flankStateCfg;
    // end latest changes

    public override void Init(ISceneAIServices services, AgentEventManager manager)
    {
        SetManagerAndServices(services, manager);
        //  SetTargetableCollider();
        SetAgentParams();
        _componentNotifications = OnNotifies;

        var anim = GetComponentsInChildren<MonoBehaviour>(true).OfType<INpcAnimationControl>().FirstOrDefault();
        if (anim != null) _animationControl = anim;

        SetPrimaryTarget();

        ConstructFovRunner();
        ConstructFSM();

        OnStableFOVResult = StableFOVResultConfirmed;

    }

    protected void SetPrimaryTarget()
    {
        if (_aiServices == null) return;

        if (_aiServices.TryGetPlayerRefService(out _playerRefService))
            _playerRefService.TryGetPlayer(out _primaryTarget);
        else
        {
#if UNITY_EDITOR
            Debug.LogError("NULL PLAYER REF");
#endif
        }
    }


    private void SetManagerAndServices(ISceneAIServices services, AgentEventManager manager)
    {
        if (manager == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError(manager + " is null in NPCControllerNew Init");
#endif
            _eManager = gameObject.AddComponent<AgentEventManager>();
        }
        else
            _eManager = manager;

        _aiServices = services;

    }

    private void ConstructFovRunner()
    {
        _fovDeps.SetTarget(_primaryTarget); // TESTING NOW

        var fovNotificationSender = new FovNotificationSender(_componentNotifications);
        //   _fovRunner = new NPCFieldOfViewHandlerNew(_fovDeps, onSweepComplete: _componentNotifications);
        _fovRunner = new FovRunner(_fovDeps, onNotify: fovNotificationSender);
    }

    private void ConstructFSM()
    {
        OnTryGetCurrentTarget = TryGetCurrentTarget;
        if (_aiServices.TryGetFsmFactory(out var factory))
        {
            var pathNotificationSenderNew = new PathNotificationSender(_componentNotifications);
            var animRequestNotificationSenderNew = new AnimationNotificationSender(_componentNotifications);
            if (factory.TryCreateFsm(out var manager, this, this, OnTryGetCurrentTarget, tickHost: this, coroutineHost: this, pathNotificationSenderNew, animRequestNotificationSenderNew))
            {
                _fsmManager = manager;
            }
            else
                DebugLogs.Err("Failed to create FSM manager", this);
        }
        else
            DebugLogs.Err("Failed to retrieve Factory", this);

        
    }

   

    private void SetAgentParams()
    {
        if (TryGetComponent<NavMeshAgent>(out var agent)) Agent = agent;

        if (TryGetComponent<NavMeshObstacle>(out var ob)) Obstacle = ob;

        Path = new NavMeshPath();
    }

    private bool TryGetCurrentTarget(out ITargetable t)
    {
        t = _primaryTarget;
        return t != null;
    }


    protected override void OnSceneEnd()
    {
        //_fsmManager.Notification = null;
        // _fsmManager.OnAnimationIntent = null;
        //  _fsmManager.OnMapDestinationToZone = null;

        _eManager = null;
        // OnTargetSeen = null;
        // OnTargetLost = null;
    }

    protected override void OnSceneBegin()
    {
        //_animationControl?.SetIKLookTarget(_primaryTarget?.Transform);
        // _fsmManager?.SwitchTo(StateId.Patrol);
        OnNotifies(NpcNotification.SceneBegin());
    }

    public override void Unload()
    {

    }
}