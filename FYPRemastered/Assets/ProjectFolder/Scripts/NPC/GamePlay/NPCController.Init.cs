using Npc.API;
using Npc.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public partial class NpcController
{
    public HashSet<int> _sets;
    public List<int> _listSets;
    private TryGetCombatTarget OnTryGetCurrentTarget;
    private INpcBrain _brain;

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
        _componentNotifications = OnNotificationReceived;

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

                (_fsmManager, _brain) = await factory.CreateFsm(callerId: this, body: this, OnTryGetCurrentTarget, tickHost: this, coroutineHost: this, pathNotificationSenderNew, animRequestNotificationSenderNew);

                if (_fsmManager is null || _brain is null) DebugLogs.Err("Factory returned null FSM manager", this);
                else DebugLogs.Log("Successfully created FSM manager with factory", this);

                OnNotificationReceived(NpcNotification.SceneBegin());
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
    public delegate bool TryGetCombatTarget(out ITargetable t);
}