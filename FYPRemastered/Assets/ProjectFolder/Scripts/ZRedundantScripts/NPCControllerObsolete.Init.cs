using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public partial class NPCControllerObsolete
{
    // FSMManager Composition - Partly obsolete
    [Header("FOV Data")]
    [SerializeField] protected FOVParameters _fovParams;

    // FSMManager Composition
    private IPathResolver _pathFinder;
    private ICandidateProviderObsolete _destinationResolver;
    private IFieldOfViewRunnerObsolete _fovRunner;
    private Dictionary<StateId, ICandidateProviderObsolete> _destinationProviders;
    //private IFSMControl _fsmManager;
    private Dictionary<StateId, IFsmStateObsolete> _fsmStates = new(5);
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

        ConstructFSM();

        OnStableFOVResult = StableFOVResultConfirmed;
        OnRequestAgentStoppingDistance = GetAgentStoppingDistance;

        base.RegisterLocalEvents(_eManager);
        RegisterGlobalEvents();
    }

    private void ConstructFSM()
    {
        _destinationProviders = new()
        {
            [StateId.Patrol] = new WaypointProviderObsolete(WaypointRepo.Instance),
            [StateId.Chase] = new TargetPointProviderObsolete(PrimaryTarget),
        };

        _destinationResolver = new DestinationResolverObsolete(_destinationProviders);
        _fovParams.FOVTarget = PrimaryTarget;
       // _pathFinder = new PathFinderObsolete(_destinationResolver);
       // _fovRunner = new NPCFieldOfViewHandlerObsolete(_fovParams);

    //    _fsmManager = new FSMBaseNew(data: this, resolver: _pathFinder, runner: _fovRunner, _fsmStates);
       // _fsmStates.TryAdd(StateId.Patrol, new FSMPatrolState(data: this, resolver: _pathFinder, stateContext: _fsmManager));
       // _fsmStates.TryAdd(StateId.Chase, new FSMChaseState(data: this, resolver: _pathFinder, stateContext: _fsmManager));
       // _fsmManager.Notification = OnNotify;
        //_fsmManager.OnAnimationIntent = AnimationIntent;
        //_fsmManager.OnMapDestinationToZone = MapDestinationToZone;///// maybe when entering patrol
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
       /* _fsmManager.Notification = null;
        _fsmManager.OnAnimationIntent = null;
        _fsmManager.OnMapDestinationToZone = null;*/
        OnRequestAgentStoppingDistance = null;
       // OnTargetSeen = null;
       // OnTargetLost = null;
    }

    protected override void OnSceneStarted()
    {
        base.OnSceneStarted();
        _animationControl?.SetIKLookTarget(PrimaryTarget?.Transform);
       // _fsmManager?.SwitchTo(StateId.Patrol);
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


























public partial class NPCControllerNew
{
    // FSMManager Composition - Partly obsolete
    [Header("FOV Data")]
    [SerializeField] protected FOVParameters _fovParams;
    protected AgentEventManager _eManager;
    // FSMManager Composition
    private IPathResolver _pathFinder;
    private ICandidateProviderObsolete _destinationResolver;
    private IFieldOfViewRunnerObsolete _fovRunner;
    private Dictionary<StateId, ICandidateProviderObsolete> _destinationProviders;
  //  private IFSMControl _fsmManager;
    private Dictionary<StateId, IFsmStateObsolete> _fsmStates = new(5);
    // end FSMManager Composition
   
    private INpcAnimationControl _animationControl;
    private ISceneAIServices _services;
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
        if (_services == null) return;

     /*   if(_services.TryGetPlayerRefService(out _playerRefService))
            PrimaryTarget = _playerRefService.TryGetPlayer();*/
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

        _services = services;
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
        if(_services.TryGetPathService(out var pathService)) _pathFinder = new PathFinder(pathService);

       // _fovRunner = new NPCFieldOfViewHandlerObsolete(_fovParams);

       // _fsmManager = new FSMBaseNew(data: this, resolver: _pathFinder, runner: _fovRunner, _fsmStates);

       /* if (_services.TryGetWaypointService(out var wpService))
        { 
            IFSMState patrolState = new FSMPatrolState(wpService, data: this, resolver: _pathFinder, stateContext: _fsmManager);
            StateId pid = patrolState.GetId();
            _fsmStates.TryAdd(pid, patrolState); 
        }

        if(_services.TryGetFlankService(out var flankService))
        {
            IFSMState flankState = new FSMFlankState(flankService, data: this, _pathFinder, _fsmManager);
            StateId fid = flankState.GetId();
            _fsmStates.TryAdd(fid, flankState);
        }
      
        IFSMState chaseState = new FSMChaseState(PrimaryTarget, data: this, resolver: _pathFinder, stateContext: _fsmManager);
        StateId cid = chaseState.GetId();
        _fsmStates.TryAdd(cid, chaseState);*/
       
       /* _fsmManager.Notification = OnNotify;
        _fsmManager.OnAnimationIntent = AnimationIntent;
        _fsmManager.OnMapDestinationToZone = MapDestinationToZone;/*///// maybe when entering patrol
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

    protected void OnSceneComplete()
    {
       // base.OnSceneComplete();
       /* _fsmManager.Notification = null;
        _fsmManager.OnAnimationIntent = null;
        _fsmManager.OnMapDestinationToZone = null;*/
        OnRequestAgentStoppingDistance = null;
       // OnTargetSeen = null;
       // OnTargetLost = null;
    }

    protected void OnSceneStarted()
    {
       // base.OnSceneStarted();
        _animationControl?.SetIKLookTarget(PrimaryTarget?.Transform);
       // _fsmManager?.SwitchTo(StateId.Patrol);
    }

    public override void Unload()
    {
        throw new System.NotImplementedException();
    }
}

