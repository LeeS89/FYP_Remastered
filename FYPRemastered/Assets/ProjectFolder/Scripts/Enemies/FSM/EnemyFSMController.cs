using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


public partial class EnemyFSMController : ComponentEvents
{
    [Header("Agent and Animation Components")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private NavMeshObstacle _obstacle;
    [SerializeField] private Animator _anim;
    [SerializeField, Tooltip("Do Not Change - Synchronized with Walking animation")] private float _walkSpeed;
    private NavMeshPath _path;
    private EnemyAnimController _animController;
    private Action _destinationCheckAction;
    private EnemyState _currentState;
    private bool _agentIsActive = false;
    private bool _playerIsDead = false;
    private bool _rotatingTowardsTarget = false;

    [Header("Patrol State - Random number between 0 and stopAndWaitDelay to wait at each way point")]
    [SerializeField] private float _stopAndWaitDelay;
    [SerializeField] private List<Vector3> _wayPoints;
    [SerializeField] private List<Vector3> _wayPointForwards;
    //[SerializeField] private WaypointManager _waypointManager;
    //private GameObject _waypointBlock;
    private BlockData _blockData;
    public int _blockZone = 0;
    private PatrolState _patrol;

    [Header("Chasing State Params")]
    [SerializeField, Tooltip("Do Not Change - Synchronized with sprinting animation")] private float _sprintSpeed;
    private ChasingState _chasing;

    [Header("Stationary State Params")]
    private StationaryState _stationary;
    [SerializeField] private int _maxFlankingSteps = 0;

    [Header("Field of View Component Parameters")]
    [SerializeField] public Transform _fovLocation; // Make Private Later
    [SerializeField] private float _patrolFOVCheckFrequency = 1f;
    [SerializeField] private float _alertFOVCheckFrequency = 0.1f;
    [SerializeField] public float _fovTraceRadius = 5f; // Make Private Later
    [Range(0, 360)]  public float _angle; // Make Private Later
    [Range(0, 360)]  [SerializeField] private float _shootAngleThreshold = 30f;
    [SerializeField] private LayerMask _fovLayerMask;
    [SerializeField] private LayerMask _lineOfSightMask;
    [SerializeField] private Collider[] _fovTraceResults;
    [SerializeField] private Vector3[] _traceHitPoints;
    [SerializeField] private int _maxFovTraceResults = 5;
    private float _fovCheckFrequency;
    private float _nextCheckTime = 0f;
    private AITraceComponent _fov;
    private FieldOfViewFrequencyStatus _fieldOfViewStatus = FieldOfViewFrequencyStatus.Normal;
    public bool _canSeePlayer = false; // Make Private Later
    private bool _canShootPlayer;

    [Header("Death State")]
    private DeathState _deathState;

    [Header("Gun Parameters")]
    [SerializeField] private Transform _bulletSpawnPoint;
    [SerializeField] private GameObject _owningGameObject;
    private Gun _gun;


    [Header("Agent and animation speed values")]
    private float _targetSpeed = 0f;
    private float _lerpSpeed = 0f;
    private bool _movementChanged = false;
    private float _previousDirection = 0f;
    private EnemyEventManager _enemyEventManager;
    
    private DestinationManager _destinationManager;
    
    private AlertStatus _alertStatus = AlertStatus.None;
    //[SerializeField] private UniformZoneGridManager _gridManager;


    #region Event Registrations
    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        _enemyEventManager = _eventManager as EnemyEventManager;
        _owningGameObject = gameObject;

        _enemyEventManager.OnRequestChasingState += ChasingStateRequested;
        _enemyEventManager.OnRequestStationaryState += StationaryStateRequested;
        _enemyEventManager.OnRequestTargetPursuit += PursuitTargetRequested;
        _enemyEventManager.OnOwnerDied += OnDeath;
        _enemyEventManager.OnAgentDeathComplete += ToggleGameObject;
        _enemyEventManager.OnAgentRespawn += ToggleGameObject;
        //_enemyEventManager.OnDestinationUpdated += UpdateAgentDestination;
        _enemyEventManager.OnDestinationReached += CarveOnDestinationReached;
        _enemyEventManager.OnRotateTowardsTarget += ToggleRotationToTarget;
        _enemyEventManager.OnSpeedChanged += UpdateAnimatorSpeedValues;

        RegisterGlobalEvents();
        SetupFSM();

    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _enemyEventManager.OnRequestChasingState -= ChasingStateRequested;
        _enemyEventManager.OnRequestStationaryState -= StationaryStateRequested;
        //_enemyEventManager.OnDestinationUpdated -= UpdateAgentDestination;

        _enemyEventManager.OnRequestTargetPursuit -= PursuitTargetRequested;
        _enemyEventManager.OnDestinationReached -= CarveOnDestinationReached;
        _enemyEventManager.OnOwnerDied -= OnDeath;
        _enemyEventManager.OnAgentDeathComplete -= ToggleGameObject;
        _enemyEventManager.OnAgentRespawn -= ToggleGameObject;
        _enemyEventManager.OnRotateTowardsTarget -= ToggleRotationToTarget;
        _enemyEventManager.OnSpeedChanged -= UpdateAnimatorSpeedValues;

       
        base.UnRegisterLocalEvents(eventManager);
        _enemyEventManager = null;
    }

    protected override void RegisterGlobalEvents()
    {
        GameManager.OnPlayerDied += OnPlayerDied;
        GameManager.OnPlayerRespawn += OnPlayerRespawned;
        
        GameManager.OnPlayerMoved += EnemyState.SetPlayerMoved;
        BaseSceneManager._instance.OnSceneStarted += OnSceneStarted;
        BaseSceneManager._instance.OnSceneEnded += OnSceneComplete;
    }

    protected override void UnRegisterGlobalEvents()
    {
        GameManager.OnPlayerDied -= OnPlayerDied;
        GameManager.OnPlayerRespawn -= OnPlayerRespawned;
        GameManager.OnPlayerMoved -= EnemyState.SetPlayerMoved;
       
        BaseSceneManager._instance.OnSceneStarted -= OnSceneStarted;
        BaseSceneManager._instance.OnSceneEnded -= OnSceneComplete;
    }
    #endregion


    #region Animation Updates

   

    private void ToggleRotationToTarget(bool rotate)
    {
        if(_rotatingTowardsTarget == rotate)
        {
            return;
        }
        _agent.updateRotation = !rotate;
        _rotatingTowardsTarget = rotate;
    }

    private void UpdateAgentSpeed()
    {
      
        float smoothedSpeed = Mathf.Lerp(_agent.speed, _targetSpeed, _lerpSpeed * Time.deltaTime);
        _agent.speed = smoothedSpeed;

       
        
        //_animController.UpdateSpeed(smoothedSpeed);

        if (Mathf.Abs(_agent.speed - _targetSpeed) <= 0.001f)
        {
           
            _agent.speed = _targetSpeed;
           // _animController.UpdateSpeed(_targetSpeed);
            _movementChanged = false;
        }
       
    }

    private void UpdateAnimator()
    {
        Vector3 moveDir = _agent.velocity.normalized;
        Vector3 forward = transform.forward;
        float directionAngle = Vector3.SignedAngle(forward, moveDir, Vector3.up);
        float normalizedDirection = directionAngle / 90f; // Normalize to -1 to 1 range

        float dot = Vector3.Dot(forward, moveDir);
        float speed = _agent.speed;
        if(dot < 0)
        {
            speed *= -1;
        }

        _animController.UpdateBlendTreeParams(speed, normalizedDirection);
    }

   
    private void UpdateAnimatorSpeedValues(float speed, float lerpSpeed)
    {
        _targetSpeed = speed;
        _lerpSpeed = lerpSpeed;
        _movementChanged = true;
    }

    private void UpdateAnimatorDirection()
    {
        // Get the normalized movement direction
        Vector3 movementDirection = _agent.velocity.normalized;

        // Calculate the angle between the agent's forward direction and the movement direction
        float angle = Vector3.SignedAngle(transform.forward, movementDirection, Vector3.up);

        // Normalize the angle to a value between -1 and 1
        float normalizedDirection = angle / 90f;

        // Only update the direction if it has changed significantly (for example, threshold of 0.1)
        if (Mathf.Abs(normalizedDirection - _previousDirection) > 0.1f)
        {
            _animController.UpdateDirection(normalizedDirection); // Update the animator with the new direction
            //animator.SetFloat("direction", normalizedDirection); // Update the direction parameter
            _previousDirection = normalizedDirection; // Store the new direction
        }

       
       
    } 
    #endregion

   

    #region Global Events

    protected override void OnSceneStarted()
    {
        PatrolStateRequested();
        _agentIsActive = true;
        //_agent.ResetPath();
    }

    protected override void OnSceneComplete()
    {
        if (_currentState != null)
        {
            _currentState.ExitState();
            _currentState = null;
        }
        _patrol = null;
        _chasing = null;
        _stationary = null;
        _fov = null;
        _fovTraceResults = null;
        _animController = null;
        _deathState = null;
    }

    protected override void OnPlayerDied()
    {
        _playerIsDead = true;
        if (!_agentIsActive) { return; }

        
        ResetFSM(_patrol);
        
    }

    protected override void OnPlayerRespawned()
    {
        _playerIsDead = false;
    }
    #endregion

    #region Test Functions
    public bool _testDeath = false;

    public float GetStraightLineDistance(Vector3 from, Vector3 to)
    {
        return Vector3.Distance(from, to);
    }

    public float GetNavMeshPathDistance(Vector3 from, Vector3 to)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
        {
            float distance = 0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return distance;
        }
        return float.PositiveInfinity; // No valid path
    }


    void CompareDistances(Vector3 enemyPosition, Vector3 playerPosition)
    {
        if (_agent.hasPath && !_agent.pathPending)
        {
            float disRem = _agent.remainingDistance;
            float straightLineDist = GetStraightLineDistance(enemyPosition, _agent.destination);
            float navMeshDist = GetNavMeshPathDistance(enemyPosition, _agent.destination);

            Debug.LogError($"Straight-Line Distance: {straightLineDist}");
            Debug.LogError($"NavMesh Path Distance: {navMeshDist}");
            Debug.LogError($"remaining Distance: {disRem}");
        }
    }
    #endregion
}
