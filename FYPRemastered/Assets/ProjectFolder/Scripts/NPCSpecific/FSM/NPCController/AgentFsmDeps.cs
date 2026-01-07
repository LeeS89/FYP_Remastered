using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class AgentFsmDeps : IFsmControllerDeps, IPatrolDeps, IChaseDeps, IFlankDeps
{
    [Header("Agent Components")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private NavMeshObstacle _obstacle;
    private NavMeshPath _path;

    [Header("Patrol State Params")]
    [Range(0.5f, 15f)]
    [SerializeField] private float _maxTimeAtWaypoint;
    [Min(0.5f)]
    [SerializeField] private float _minTimeAtWaypoint;

    [Header("Flank state Params")]
    [Range(5, 12)]
    [SerializeField] private int _maxFlankSteps;
    [Min(4)]
    [SerializeField] private int _minFlankSteps;

    [Header("(Used in Chasing State only) - If using Random stop distance - the agent will stop moving to its destination\n" +
        "once it reaches its randomly generated stop distance between min and max - otherwise uses min")]
    [SerializeField] private bool _useRandomStopDistance = true;
    [SerializeField] private float _minStoppingdistance;
    [SerializeField] private float _maxStoppingDistance;

    [Header("Speed Params")]
    [SerializeField] private float _sprintSpeed;
    [SerializeField] private float _walkSpeed;

    // FSM Deps
    public IPathResolver _pathResolver;
    private ITargetable _owner;
    private ITargetable _target;
    public IWaypointService _waypointService;
    public IFlankService _flankService;


    public void SetOwner(ITargetable owner) => _owner = owner;
    public void SetPath(NavMeshPath path) => _path = path;
    public void SetAgentRef(NavMeshAgent agent) { if (_agent == null) _agent = agent; }
    public void SetTarget(ITargetable target) => _target = target;

    public void SetObstacleRef(NavMeshObstacle obstacle) { if(_obstacle == null) _obstacle = obstacle; }

    public NavMeshAgent Agent()
    {
        if (_agent == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Must provide a valid NavMesh Agent Component");
            _agent = new NavMeshAgent();
#endif
        }
            return _agent;
    }

    public NavMeshObstacle Obstacle()
    {
        if(_obstacle == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Must provide a valid NavMesh Obstacle Component");
#else
            _obstacle = new NavMeshObstacle();
#endif
        }

        return _obstacle;
    }

    public NavMeshPath Path()
    {
        if (_path == null)
            _path = new NavMeshPath();

        return _path;
    }

    public IWaypointService WaypointService => _waypointService;
    public float MaxTimeAtPatrolPoint => _maxTimeAtWaypoint;
    public float MinTimeAtPatrolPoint => _minTimeAtWaypoint;

    public IFlankService FlankService => _flankService;
    public int MaxFlankSteps => _maxFlankSteps;
    public int MinFlankSteps => _minFlankSteps;

    public float SprintSpeed => _sprintSpeed;
    public float WalkSpeed => _walkSpeed;

    public ITargetable Owner => _owner;

    public IPathResolver PathResolver => _pathResolver;

    public float MinStoppingDistance => _minStoppingdistance;

    public float MaxStoppingDistance => _maxStoppingDistance;

    public ITargetable Target => _target;

    
}
