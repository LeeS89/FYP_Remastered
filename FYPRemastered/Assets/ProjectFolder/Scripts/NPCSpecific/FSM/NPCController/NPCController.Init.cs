using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public partial class NPCController
{
    // FSMManager Composition - Partly obsolete
    [Header("FOV Data")]
    [SerializeField] protected FOVParameters _fovParams;

    // FSMManager Composition
    private IPathResolver _pathFinder;
    private ICandidateProvider _destinationResolver;
    private IFieldOfViewRunner _fovRunner;
    private Dictionary<StateId, ICandidateProvider> _destinationProviders;
    private IFSMControlNew _fsmManager;
    private Dictionary<StateId, IFSMState> _fsmStates = new(5);
    // end FSMManager Composition
    //   protected IIntentState _state;
   
    private INpcAnimationControl _animationControl;
  

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _eManager = eventManager as EnemyEventManager;

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

        var anim = GetComponentsInChildren<MonoBehaviour>(true).OfType<INpcAnimationControl>().FirstOrDefault();
        if (anim != null) _animationControl = anim;

        SetPrimaryToPlayer();
        SetNavMeshAgentParams();
        OnMeleeRangeCheckCallback = OnMeleeRangeEnter;

        SetupFSM();

        OnStableFOVResult = StableFOVResultConfirmed;
        OnRequestAgentStoppingDistance = GetAgentStoppingDistance;

        base.RegisterLocalEvents(_eManager);
        RegisterGlobalEvents();
    }

    private void SetupFSM()
    {
        _destinationProviders = new()
        {
            [StateId.Patrol] = new WaypointProvider(WaypointRepo.Instance),
            [StateId.Chase] = new TargetPointProvider(PrimaryTarget),
        };

        _destinationResolver = new DestinationResolver(_destinationProviders);
        _fovParams.FOVTarget = PrimaryTarget;
        _pathFinder = new PathFinder(_destinationResolver);
        _fovRunner = new NPCFieldOfViewHandler(_fovParams);

        _fsmManager = new FSMBaseNew(data: this, resolver: _pathFinder, runner: _fovRunner, _fsmStates);
        _fsmStates.TryAdd(StateId.Patrol, new FSMPatrolState(data: this, resolver: _pathFinder, stateContext: _fsmManager));
        _fsmStates.TryAdd(StateId.Chase, new FSMChaseState(data: this, resolver: _pathFinder, stateContext: _fsmManager));
        _fsmManager.Notification = Notify;
        _fsmManager.OnAnimationIntent = AnimationIntent;
        _fsmManager.OnMapDestinationToZone = MapDestinationToZone;///// maybe when entering patrol
    }

    private void SetNavMeshAgentParams()
    {
        if (TryGetComponent<NavMeshAgent>(out var agent)) Agent = agent;
      
        if (TryGetComponent<NavMeshObstacle>(out var ob)) Obstacle = ob;
       
        Path = new NavMeshPath();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        OnMeleeRangeCheckCallback = null;
        base.UnRegisterLocalEvents(_eManager);
        UnRegisterGlobalEvents();
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        _fsmManager.Notification = null;
        _fsmManager.OnAnimationIntent = null;
        _fsmManager.OnMapDestinationToZone = null;
        OnRequestAgentStoppingDistance = null;
       // OnTargetSeen = null;
       // OnTargetLost = null;
    }

    protected override void OnSceneStarted()
    {
        base.OnSceneStarted();
        _animationControl?.SetIKLookTarget(PrimaryTarget?.Transform);
        _fsmManager?.SwitchTo(StateId.Patrol);
    }


    protected void SetPrimaryToPlayer()
    {
        if (!GameManager.Instance.TryGetPlayer(out var player))
        {
#if UNITY_EDITOR
            Debug.LogError("Failed to retrieve player ref");
#endif
            return;
        }
        PrimaryTarget = player;
    }
}
/*public partial class NPCController
{
    // FSMManager Composition
    [Header("FOV Data")]
    [SerializeField] protected FOVParameters _fovParams;
    public IFSMControl FSM { get; protected set; }
    private IPathResolver _pathFinder;
    private ICandidateProvider _destinationResolver;
    private IFieldOfViewRunner _fovRunner;
    private Dictionary<StateId, ICandidateProvider> _destinationProviders;
    protected IIntentState _state;
    // end FSMManager Composition

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _eManager = eventManager as EnemyEventManager;

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

        SetPrimaryToPlayer();
        SetNavMeshAgentParams();
        OnMeleeRangeCheckCallback = OnMeleeRangeEnter;

        _destinationProviders = new()
        {
            [StateId.Patrol] = new WaypointProvider(WaypointRepo.Instance),
            [StateId.Chase] = new TargetPointProvider(PrimaryTarget),
        };

        _destinationResolver = new DestinationResolver(_destinationProviders);
        _fovParams.FOVTarget = PrimaryTarget;
        _pathFinder = new PathFinder(_destinationResolver);
        _fovRunner = new NPCFieldOfViewHandler(_fovParams);

        FSM = new FSMManager(data: this, resolver: _pathFinder, runner: _fovRunner);
        FSM.Notification = Notify;
        FSM.OnAnimationIntent = AnimationIntent;
        FSM.OnMapDestinationToZone = MapDestinationToZone;

        OnTargetSeen = TargetSeen;
        OnTargetLost = TargetLost;

        base.RegisterLocalEvents(_eManager);
        RegisterGlobalEvents();
    }

    private void SetNavMeshAgentParams()
    {
        if (TryGetComponent<NavMeshAgent>(out var agent)) Agent = agent;
      
        if (TryGetComponent<NavMeshObstacle>(out var ob)) Obstacle = ob;
       
        Path = new NavMeshPath();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        OnMeleeRangeCheckCallback = null;
        base.UnRegisterLocalEvents(_eManager);
        UnRegisterGlobalEvents();
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        FSM.OnMapDestinationToZone = null;
        FSM.Notification = null;
        FSM.OnAnimationIntent = null;
        OnTargetSeen = null;
        OnTargetLost = null;
    }

    protected override void OnSceneStarted()
    {
        base.OnSceneStarted();
        _eManager.SetLookTarget(PrimaryTarget?.Transform);
        SwitchTo(Patrol.Instance);

    }


    protected void SetPrimaryToPlayer()
    {
        if (!GameManager.Instance.TryGetPlayer(out var player))
        {
#if UNITY_EDITOR
            Debug.LogError("Failed to retrieve player ref");
#endif
            return;
        }
        PrimaryTarget = player;
    }
}*/
