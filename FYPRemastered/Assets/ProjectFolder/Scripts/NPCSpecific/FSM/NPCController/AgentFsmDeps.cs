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

    [Header("If using Random stop distance (Set in states CTR) - the agent will stop moving to its destination\n" +
        "once it reaches its randomly generated stop distance between min and max - otherwise defaults to 0f")]
    [SerializeField] private float _minStoppingDistance;
    [SerializeField] private float _maxStoppingDistance;

    [Header("Speed Params")]
    [SerializeField] private float _sprintSpeed;
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _sprintEnterDistance;
    [SerializeField] private float _sprintExitDistance;

    // FSM Deps
    public IPathResolver _pathResolver;
    private ITargetable _owner;
    private ITargetable _target;
    public IWaypointService _waypointService;
    public IFlankService _flankService;

    private IDistanceService _distanceService;
    public IDistanceService DistanceService => _distanceService;

    public float GetAgentStopDistance(bool useRandomDistance)
        => useRandomDistance ? Random.Range(_minStoppingDistance, _maxStoppingDistance) : 0f;

    public void SetOwner(ITargetable owner) => _owner = owner;
    public void SetPath(NavMeshPath path) => _path = path;
    public void SetAgentRef(NavMeshAgent agent) { if (_agent == null) _agent = agent; }
    public void SetTarget(ITargetable target) => _target = target;

    public void SetObstacleRef(NavMeshObstacle obstacle) { if(_obstacle == null) _obstacle = obstacle; }

    public void SetDistanceService(IDistanceService distanceService) => _distanceService = distanceService;

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

   // public float MinStoppingDistance => _minStoppingDistance;

  //  public float MaxStoppingDistance => _maxStoppingDistance;

    public ITargetable Target => _target;

    public static bool TryGetEndpoint(string env, out string host, out int port)
    {
        bool ok;
        (ok, host, port) = env switch
        {
            "dev" => (true, "localhost", 8080),
            "prod" => (true, "api.myapp.com", 443),
            _ => (false, default!, 0)
        };

        return ok;
    }
    private SpeedTier OverrideSpeed(SpeedOverride speedOverride, out float newSpeed, out float lerp)
    {
        SpeedTier newTier;

        (newTier, newSpeed, lerp) = speedOverride switch
        {
            SpeedOverride.ForceWalk => 
            (
                SpeedTier.Walk,
                newSpeed = WalkSpeed,
                lerp = 2f
            ),
            SpeedOverride.ForceSprint => 
            (
                SpeedTier.Sprint,
                newSpeed = _sprintSpeed,
                lerp = 2f
            ),
            SpeedOverride.ForceIdle => 
            (
                SpeedTier.Idle,
                newSpeed = 0f,
                lerp = 10f
            ),
            _ => 
            (
                SpeedTier.Walk,
                newSpeed = WalkSpeed,
                lerp = 2f
            )

        };
        return newTier;

        
    }
    

    public SpeedTier TryUpdateAgentTargetSpeed(SpeedTier currentTier, SpeedOverride speedOverride, float distanceToDestination, out float newSpeed, out float lerp)
    {
        if (distanceToDestination <= 0.25f)
        {
            newSpeed = 0f;
            lerp = 10f;
            return SpeedTier.Idle;
        }

        if (speedOverride != SpeedOverride.None)
            return OverrideSpeed(speedOverride, out newSpeed, out lerp);

        /*  

          if (!usesSpeedByDistance)
          {
              newSpeed = WalkSpeed;
              lerp = 2f;
              return SpeedTier.Walk;
          }*/

        if (distanceToDestination > _sprintEnterDistance)
        {
            newSpeed = _sprintSpeed;
            lerp = 2f;
            return SpeedTier.Sprint;
        }
        else if (distanceToDestination < _sprintExitDistance)
        {
            newSpeed = WalkSpeed;
            lerp = 2f;
            return SpeedTier.Walk;
        }
        else
        {
            if(currentTier == SpeedTier.Idle)
            {
                newSpeed = WalkSpeed;
                lerp = 2f;
                return SpeedTier.Walk;
            }
            newSpeed = _sprintSpeed;
            lerp = 2f;
            return currentTier;
        }

          
    }

}


public enum SpeedTier
{
    Idle,
    Walk,
    Sprint
}

public enum SpeedOverride
{
    None,
    ForceIdle,
    ForceWalk,
    ForceSprint
}

public enum RotationOverride
{
    None,
    ForceLookAtTarget
}