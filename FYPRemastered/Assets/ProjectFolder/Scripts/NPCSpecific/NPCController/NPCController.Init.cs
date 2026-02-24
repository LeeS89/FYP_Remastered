using Npc.Internal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public partial class NPCController
{
    [SerializeField] private AgentFsmDeps _fsmDeps;
    // FSMManager Composition - Partly obsolete
    [Header("FOV Data")]
    [SerializeField] private FovData _fovDeps;
    protected AgentEventManager _eManager;
    // FSMManager Composition
    //private IPathResolver _pathFinder;
    private FovRunner _fovRunner;
    private FsmManager _fsmManager;
    //private IFsmControl _fsmManager;
    private Dictionary<StateId, IFsmState> _fsmStates = new(5);
    // end FSMManager Composition

    private INpcAnimationControl _animationControl;
    private ISceneAIServices _aiServices;
    private IPlayerRefService _playerRefService;
    private IAgentAlertService _alertService;
    private Notification _componentNotifications;

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
        _fsmDeps.SetOwner(this);
        _fsmDeps.SetTarget(_primaryTarget);
        _fsmDeps.SetAgentRef(Agent);
        _fsmDeps.SetObstacleRef(Obstacle);
        _fsmDeps.SetPath(new NavMeshPath());

       
        if (_aiServices.TryGetPathService(out var pathService)) _fsmDeps._pathResolver = new PathFinder(pathService);//_pathFinder = new PathFinderNew(pathService);

        // _fovRunner = new NPCFieldOfViewHandler(_fovParams);
        //_fovRunner = new NPCFieldOfViewHandler(_fovParams, _fsmDeps, onSweepComplete: _componentNotifications);

        // _fsmManager = new FSMBaseNew(data: this, resolver: _pathFinder, runner: _fovRunner, _fsmStates);
        var pathNotificationSender = new PathNotificationSender(_componentNotifications);
        var animRequestNotificationSender = new AnimationNotificationSender(_componentNotifications);

        _fsmManager = new FsmManager(deps: _fsmDeps, _fsmStates, pathNotificationSender, animRequestNotificationSender);

        if (_aiServices.TryGetWaypointService(out _fsmDeps._waypointService))
        {
            if (!_aiServices.TryGetAgentAlertService(out _alertService))
            {
#if UNITY_EDITOR
                Debug.LogError("Failed to retrieve alert service");
#endif
            }
            //IFSMState patrolState = new FSMPatrolState(wpService, data: this, resolver: _pathFinder, stateContext: _fsmManager);
            IFsmState patrolState = new FSMPatrolState(deps: _fsmDeps, /*data: this, resolver: _pathFinder,*/ stateEvents: _fsmManager);
            StateId pid = patrolState.GetId();
            _fsmStates.TryAdd(pid, patrolState);
        }

        if (_aiServices.TryGetFlankService(out _fsmDeps._flankService))
        {
            IFsmState flankState = new FSMFlankState(deps: _fsmDeps, /*data: this, _pathFinder,*/ _fsmManager);
            //IFSMState flankState = new FSMFlankState(flankService, data: this, _pathFinder, _fsmManager);
            StateId fid = flankState.GetId();
            _fsmStates.TryAdd(fid, flankState);
        }

        //IFSMState chaseState = new FSMChaseState(PrimaryTarget, data: this, resolver: _pathFinder, stateContext: _fsmManager);
        if (_aiServices.TryGetDistanceService(out var service))
        {
            _fsmDeps.SetDistanceService(service); // Maybe assume it is used by all states instead of just chase
            IFsmState chaseState = new FSMChaseState(deps: _fsmDeps, stateContext: _fsmManager, useRandomStopDistance: true);
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
