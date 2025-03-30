using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFSMController : ComponentEvents
{
    [Header("Agent and Animation Components")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private NavMeshObstacle _obstacle;
    [SerializeField] private Animator _anim;
    [SerializeField, Tooltip("Do Not Change - Synchronized with Walking animation")] private float _walkSpeed;
    private EnemyAnimController _animController;

    private EnemyState _currentState;
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
    [SerializeField] private Collider[] _fovTraceResults;
   
    private float _fovCheckFrequency;

    private float _nextCheckTime = 0f;
    private TraceComponent _fov;

    private float _targetSpeed = 0f;
    private float _lerpSpeed = 0f;
    private bool _movementChanged = false;
    private EnemyEventManager _enemyEventManager;
    private bool _canSeePlayer = false;
    private Action _destinationCheckAction;
    
    private FieldOfViewFrequencyStatus _fieldOfViewStatus = FieldOfViewFrequencyStatus.Normal;
    private AlertStatus _alertStatus = AlertStatus.None;
    

    #region Event Registrations
    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        _enemyEventManager = _eventManager as EnemyEventManager;
        _enemyEventManager.OnDestinationUpdated += UpdateAgentDestination;
        _enemyEventManager.OnDestinationReached += CarveOnDestinationReached;
        _enemyEventManager.OnAnimationTriggered += PlayAnimationType;
        _enemyEventManager.OnSpeedChanged += UpdateTargetSpeedValues;
        
        RegisterGlobalEvents();
       
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _enemyEventManager.OnDestinationUpdated -= UpdateAgentDestination;
        _enemyEventManager.OnDestinationReached -= CarveOnDestinationReached;
        _enemyEventManager.OnAnimationTriggered -= PlayAnimationType;
        _enemyEventManager.OnSpeedChanged -= UpdateTargetSpeedValues;
        base.UnRegisterLocalEvents(eventManager);
        _enemyEventManager = null;
    }

    protected override void RegisterGlobalEvents()
    {
        BaseSceneManager._instance.OnSceneStarted += OnSceneStarted;
        BaseSceneManager._instance.OnSceneEnded += OnSceneComplete;
    }

    protected override void UnRegisterGlobalEvents()
    {
        BaseSceneManager._instance.OnSceneStarted -= OnSceneStarted;
        BaseSceneManager._instance.OnSceneEnded -= OnSceneComplete;
    }
    #endregion


   

    #region FSM Management
    private void SetupFSM()
    {
        _fovCheckFrequency = _patrolFOVCheckFrequency;
        _fov = new TraceComponent(1);
        _fovTraceResults = new Collider[1];
        _animController = new EnemyAnimController(_anim);
        _patrol = new PatrolState(_wayPoints, _agent, _enemyEventManager, _stopAndWaitDelay, _walkSpeed);
        _chasing = new ChasingState(_agent, _enemyEventManager, _walkSpeed, _sprintSpeed);
        _stationary = new StationaryState(_agent, _enemyEventManager);
        

        ChangeState(_patrol);
        
    }


    public void ChangeState(EnemyState state)
    {
        if(_currentState != null)
        {
            _currentState.ExitState();
        }
        _currentState = state;

        
        _destinationCheckAction = _currentState == _stationary ? StopImmediately : MeasurePathToDestination;

        _currentState.EnterState(AlertStatus.Alert);
    }

    private bool CheckIfDestinationIsReached()
    {
        //Debug.LogError("Remainingdistance: "+_agent.remainingDistance);
        return _agent.remainingDistance <= (_agent.stoppingDistance + 0.2f);
    }

    #endregion

    

    #region Updates
    void Update()
    {
        if(_testDeath)
        {
            ChangeState(_chasing);
            //CarveOnDestinationReached(true);
            _testDeath = false;
        }
    }
    

    private void LateUpdate()
    {
       
        UpdateFieldOfViewCheckFrequency();

        /*if (_testDeath)
        {
            //_animController.LookAround();
            //_agent.enabled = false;
            //_animController.EnemyDied();
            CompareDistances(transform.position, _player.position);
            //_testDeath = false;
        }*/
        //_currentState.LateUpdateState();
        //MeasurePathToDestination();
        _destinationCheckAction?.Invoke();
       
        if (!_movementChanged) { return; }

        UpdateAnimatorSpeed();
    }

    private void MeasurePathToDestination()
    {
        if(!_agent.enabled) { return; }
        //DebugExtension.DebugWireSphere(_agent.destination, Color.green);
        if (_currentState != null && _agent.hasPath && !_agent.pathPending)
        {
            if (CheckIfDestinationIsReached())
            {
                
                _agent.ResetPath();
                _enemyEventManager.DestinationReached(true);
                

            }

        }
    }

    /// <summary>
    /// Used when agent enters Stationary state
    /// </summary>
    private void StopImmediately()
    {
        _agent.SetDestination(_agent.transform.position);
        _agent.ResetPath();
        _enemyEventManager.DestinationReached(true);
        

        _destinationCheckAction = null;
    }


    #endregion


    #region Field Of View functions
    private void UpdateFieldOfViewCheckFrequency()
    {
        switch (_fieldOfViewStatus)
        {
            case FieldOfViewFrequencyStatus.Normal:
                if (_fovCheckFrequency != _patrolFOVCheckFrequency)
                {
                    _fovCheckFrequency = _patrolFOVCheckFrequency;
                }
                break;
            case FieldOfViewFrequencyStatus.Heightened:
                if (_fovCheckFrequency != _alertFOVCheckFrequency)
                {
                    _fovCheckFrequency = _alertFOVCheckFrequency;
                }
                break;
            default:
                _fovCheckFrequency = _patrolFOVCheckFrequency;
                break;
        }

        PerformFieldOfViewCheck();
    }


    private void PerformFieldOfViewCheck()
    {
        if (Time.time >= _nextCheckTime && _fov != null)
        {
            _nextCheckTime = Time.time + _fovCheckFrequency;
            bool playerSeen = false;

            // CheckForTarget first performs a Physics.CheckSphere. If check passes, an overlapsphere is performed which returns the targets collider information
            _fov.CheckForTarget(_fovLocation, out _fovTraceResults, _fovTraceRadius, _fovLayerMask, true);

            if (_fovTraceResults.Length > 0)
            {
                playerSeen = LineOfSightUtility.HasLineOfSight(_fovLocation, _fovTraceResults[0].transform, 360f, _fovLayerMask);

                if (_fieldOfViewStatus != FieldOfViewFrequencyStatus.Heightened)
                    _fieldOfViewStatus = FieldOfViewFrequencyStatus.Heightened;
            }
            else
            {
                if (_fieldOfViewStatus != FieldOfViewFrequencyStatus.Normal)
                    _fieldOfViewStatus = FieldOfViewFrequencyStatus.Normal;
            }

            UpdateFieldOfViewResults(playerSeen);


        }
    }

    private void UpdateFieldOfViewResults(bool playerSeen)
    {
        if (_canSeePlayer == playerSeen) { return; } // Already Updated, return early

        _canSeePlayer = playerSeen;

        if(_canSeePlayer)
        {
            _enemyEventManager.PlayerSeen(_canSeePlayer);
            // Alert Group Here (Moon Scene Manager)
            /* if(_currentState != _chasing)
             {
                 ChangeState(_chasing);
             }*/
        }
        else
        {
            _enemyEventManager.PlayerSeen(false);
        }

        //Update Shooting Component Here
    }
    #endregion

    #region Destination Updates
    private void CarveOnDestinationReached(bool _)
    {
        ToggleAgent(false);
        _obstacle.enabled = true;
    }

    private void UpdateAgentDestination(Vector3 newDestination, int stoppingDistance = 0)
    {
        if (!_agent.enabled)
        {
            _obstacle.enabled = false;
            //_agent.stoppingDistance = stoppingDistance;
            StartCoroutine(SetDestinationDelay(newDestination, stoppingDistance));
            return;
        }
        _agent.stoppingDistance = stoppingDistance;
        _agent.SetDestination(newDestination);

    }

    /// <summary>
    /// Used when transitioning from stationary to moving
    /// to give time for disabling carving and re enabling the agent
    /// </summary>
    /// <param name="newDestination"></param>
    /// <param name="stoppingDistance"></param>
    /// <returns></returns>
    private IEnumerator SetDestinationDelay(Vector3 newDestination, int stoppingDistance)
    {
        yield return new WaitForSeconds(0.15f);
        ToggleAgent(true);
        _agent.stoppingDistance = stoppingDistance;
        _agent.SetDestination(newDestination);

    }

    private void ToggleAgent(bool agentEnabled)
    {
        _agent.enabled = agentEnabled;
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
            //Debug.LogError("Speed finished changing");
            _agent.speed = _targetSpeed;
            _animController.UpdateSpeed(_targetSpeed);
            _movementChanged = false;
        }
       
    }

    private void PlayAnimationType(AnimationAction action)
    {
        switch (action)
        {
            case AnimationAction.Look:
                _animController.LookAround();
                break;
            case AnimationAction.Shoot:
                _animController.Shoot();
                break;
            case AnimationAction.Dead:
                _animController.DeadAnimation();
                break;
            default:
                Debug.LogWarning("No Animation Type Selected");
                break;
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
