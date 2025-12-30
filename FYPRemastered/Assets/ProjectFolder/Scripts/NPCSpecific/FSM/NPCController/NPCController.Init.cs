using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public partial class NPCController
{
    // FSMManager Composition - Partly obsolete
    [Header("FOV Data")]
    [SerializeField] protected FOVParameters _fovParams;
    protected AgentEventManager _eManager;
    // FSMManager Composition
    private IPathResolver _pathFinder;
    private ICandidateProvider _destinationResolver;
    private IFieldOfViewRunner _fovRunner;
    private Dictionary<StateId, ICandidateProvider> _destinationProviders;
    private IFSMControlNew _fsmManager;
    private Dictionary<StateId, IFSMState> _fsmStates = new(5);
    // end FSMManager Composition

    private INpcAnimationControl _animationControl;
    private ISceneAIServices _aiServices;
    private IPlayerRefService _playerRefService;



    public override void Init(ISceneAIServices services, AgentEventManager manager)
    {
        SetManagerAndServices(services, manager);
        SetTargetableCollider();
        SetAgentParams();

        var anim = GetComponentsInChildren<MonoBehaviour>(true).OfType<INpcAnimationControl>().FirstOrDefault();
        if (anim != null) _animationControl = anim;

        SetPrimaryTarget();

        OnMeleeRangeCheckCallback = OnMeleeRangeEnter;

        ConstructFSM();

        OnStableFOVResult = StableFOVResultConfirmed;
        OnRequestAgentStoppingDistance = GetAgentStoppingDistance;
    }

    protected void SetPrimaryTarget()
    {
        if (_aiServices == null) return;

        if (_aiServices.TryGetPlayerRefService(out _playerRefService))
            PrimaryTarget = _playerRefService.GetPlayer();
        else
        {
            Debug.LogError("NULL PLAYER REF");
        }
    }

    private void SetTargetableCollider()
    {
        if (_targetCollider == null)
        {
            if (!TryGetComponent<Collider>(out var coll))
            {
                TargetableCollider = gameObject.AddComponent<BoxCollider>();
            }
            else
                TargetableCollider = coll;
        }
        else
        {
            TargetableCollider = _targetCollider;
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
        _sceneService.OnSceneBegin += OnSceneBegin;
    }

    private void ConstructFSM()
    {
        /* _destinationProviders = new()
         {
             [StateId.Patrol] = new WaypointProvider(WaypointRepo.Instance),
             [StateId.Chase] = new TargetPointProvider(PrimaryTarget),
         };

         _destinationResolver = new DestinationResolver(_destinationProviders);*/
        _fovParams.FOVTarget = PrimaryTarget;
        if (_aiServices.TryGetPathService(out var pathService)) _pathFinder = new PathFinderNew(pathService);

        _fovRunner = new NPCFieldOfViewHandler(_fovParams);

        _fsmManager = new FSMBaseNew(data: this, resolver: _pathFinder, runner: _fovRunner, _fsmStates);

        if (_aiServices.TryGetWaypointService(out var wpService))
        {
            IFSMState patrolState = new FSMPatrolState(wpService, data: this, resolver: _pathFinder, stateContext: _fsmManager);
            StateId pid = patrolState.GetId();
            _fsmStates.TryAdd(pid, patrolState);
        }

        if (_aiServices.TryGetFlankService(out var flankService))
        {
            IFSMState flankState = new FSMFlankState(flankService, data: this, _pathFinder, _fsmManager);
            StateId fid = flankState.GetId();
            _fsmStates.TryAdd(fid, flankState);
        }

        IFSMState chaseState = new FSMChaseState(PrimaryTarget, data: this, resolver: _pathFinder, stateContext: _fsmManager);
        StateId cid = chaseState.GetId();
        _fsmStates.TryAdd(cid, chaseState);

        _fsmManager.Notification = Notify;
        _fsmManager.OnAnimationIntent = AnimationIntent;
        _fsmManager.OnMapDestinationToZone = MapDestinationToZone;///// maybe when entering patrol
    }

    private void SetAgentParams()
    {
        if (TryGetComponent<NavMeshAgent>(out var agent)) Agent = agent;

        if (TryGetComponent<NavMeshObstacle>(out var ob)) Obstacle = ob;

        Path = new NavMeshPath();
    }

    public void UnRegisterLocalEvents(EventManager eventManager)
    {
        OnMeleeRangeCheckCallback = null;
        //  base.UnRegisterLocalEvents(_eManager);
        //  UnRegisterGlobalEvents();
    }

    protected override void OnSceneEnd()
    {
        _sceneService.OnSceneEnd -= OnSceneEnd;
        _fsmManager.Notification = null;
        _fsmManager.OnAnimationIntent = null;
        _fsmManager.OnMapDestinationToZone = null;
        OnRequestAgentStoppingDistance = null;
        _eManager = null;
        // OnTargetSeen = null;
        // OnTargetLost = null;
    }

    protected override void OnSceneBegin()
    {
        _sceneService.OnSceneBegin -= OnSceneBegin;
        _animationControl?.SetIKLookTarget(PrimaryTarget?.Transform);
        _fsmManager?.SwitchTo(StateId.Patrol);
    }

    public override void Unload()
    {
        
    }
}
