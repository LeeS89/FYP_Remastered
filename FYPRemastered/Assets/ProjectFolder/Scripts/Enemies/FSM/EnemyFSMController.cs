using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    private bool _agentIsActive = true;
    private bool _playerIsDead = false;

    [Header("Patrol State - Random number between 0 and stopAndWaitDelay to wait at each way point")]
    [SerializeField] private float _stopAndWaitDelay;
    [SerializeField] private List<Transform> _wayPoints;
    private PatrolState _patrol;

    [Header("Chasing State Params")]
    [SerializeField, Tooltip("Do Not Change - Synchronized with sprinting animation")] private float _sprintSpeed;
    private ChasingState _chasing;

    [Header("Stationary State Params")]
    private StationaryState _stationary;

    [Header("Field of View Component Parameters")]
    [SerializeField] private Transform _fovLocation;
    [SerializeField] private float _patrolFOVCheckFrequency = 1f;
    [SerializeField] private float _alertFOVCheckFrequency = 0.1f;
    [SerializeField] private float _fovTraceRadius = 5f;
    [SerializeField] private LayerMask _fovLayerMask;
    [SerializeField] private LayerMask _lineOfSightMask;
    [SerializeField] private Collider[] _fovTraceResults;
    private float _fovCheckFrequency;
    private float _nextCheckTime = 0f;
    private TraceComponent _fov;
    private FieldOfViewFrequencyStatus _fieldOfViewStatus = FieldOfViewFrequencyStatus.Normal;
    private bool _canSeePlayer = false;

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
    private EnemyEventManager _enemyEventManager;
    
    
    //private AlertStatus _alertStatus = AlertStatus.None;
    

    #region Event Registrations
    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        _enemyEventManager = _eventManager as EnemyEventManager;
        _owningGameObject = gameObject;

        _enemyEventManager.OnRequestChasingState += ChasingStateRequested;
        _enemyEventManager.OnRequestStationaryState += StationaryStateRequested;
        _enemyEventManager.OnOwnerDied += OnDeath;
        _enemyEventManager.OnAgentDeathComplete += ToggleGameObject;
        _enemyEventManager.OnAgentRespawn += ToggleGameObject;
        _enemyEventManager.OnDestinationUpdated += UpdateAgentDestination;
        _enemyEventManager.OnDestinationReached += CarveOnDestinationReached;
        
        _enemyEventManager.OnSpeedChanged += UpdateTargetSpeedValues;
        
        RegisterGlobalEvents();
       
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _enemyEventManager.OnRequestChasingState -= ChasingStateRequested;
        _enemyEventManager.OnRequestStationaryState -= StationaryStateRequested;
        _enemyEventManager.OnDestinationUpdated -= UpdateAgentDestination;
        _enemyEventManager.OnDestinationReached -= CarveOnDestinationReached;
        _enemyEventManager.OnOwnerDied -= OnDeath;
        _enemyEventManager.OnAgentDeathComplete -= ToggleGameObject;
        _enemyEventManager.OnAgentRespawn -= ToggleGameObject;
        
        _enemyEventManager.OnSpeedChanged -= UpdateTargetSpeedValues;
        base.UnRegisterLocalEvents(eventManager);
        _enemyEventManager = null;
    }

    protected override void RegisterGlobalEvents()
    {
        GameManager.OnPlayerDied += OnPlayerDied;
        GameManager.OnPlayerRespawn += OnPlayerRespawned;
        BaseSceneManager._instance.OnSceneStarted += OnSceneStarted;
        BaseSceneManager._instance.OnSceneEnded += OnSceneComplete;
    }

    protected override void UnRegisterGlobalEvents()
    {
        GameManager.OnPlayerDied -= OnPlayerDied;
        GameManager.OnPlayerRespawn -= OnPlayerRespawned;
        BaseSceneManager._instance.OnSceneStarted -= OnSceneStarted;
        BaseSceneManager._instance.OnSceneEnded -= OnSceneComplete;
    }
    #endregion


    #region Animation Updates
    private void UpdateAnimatorSpeed()
    {
      
        float smoothedSpeed = Mathf.Lerp(_agent.speed, _targetSpeed, _lerpSpeed * Time.deltaTime);
        _agent.speed = smoothedSpeed;

       
        // Update animator 
        _animController.UpdateSpeed(smoothedSpeed);

        if (Mathf.Abs(_agent.speed - _targetSpeed) <= 0.001f)
        {
           
            _agent.speed = _targetSpeed;
            _animController.UpdateSpeed(_targetSpeed);
            _movementChanged = false;
        }
       
    }

   
    private void UpdateTargetSpeedValues(float speed, float lerpSpeed)
    {
        _targetSpeed = speed;
        _lerpSpeed = lerpSpeed;
        _movementChanged = true;
    }
    #endregion

   

    #region Global Events

    protected override void OnSceneStarted()
    {
        Invoke(nameof(SetupFSM), 1f);
        //SetupFSM();
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
