using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFSMController : ComponentEvents
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private NavMeshObstacle _obstacle;
    [SerializeField] private Animator _anim;
    private EnemyAnimController _animController;

    private EnemyState _currentState;
    [Header("Patrol State - Random number between 0 and stopAndWaitDelay to wait at each way point")]
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _stopAndWaitDelay;
    [SerializeField] private List<Transform> _wayPoints;
    private PatrolState _patrol;


    [Header("Trace Component Parameters")]
    [SerializeField] private Transform _fovLocation;
    [SerializeField] private float _fovTraceRadius = 5f;
    [SerializeField] private LayerMask _fovLayerMask;
    [SerializeField] private Collider[] _fovTraceresults;
    [SerializeField] private float _patrolCheckFrequency = 1f;
    [SerializeField] private float _alertcheckFrequency = 0.1f;
    private float _fovCheckFrequency;

    private float _nextCheckTime = 0f;
    private TraceComponent _fov;

    private float _targetSpeed = 0f;
    private float _lerpSpeed = 0f;
    public bool _movementChanged = false;
    private EnemyEventManager _enemyEventManager;


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
    public Transform _player;

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

    #region FSM Management
    private void SetupFSM()
    {
        _fovCheckFrequency = _patrolCheckFrequency;
        _fov = new TraceComponent(1);
        _fovTraceresults = new Collider[1];
        _animController = new EnemyAnimController(_anim);
        _patrol = new PatrolState(_wayPoints, _agent, _enemyEventManager, _stopAndWaitDelay, _walkSpeed);
        
        _currentState = _patrol;
       
        _currentState.EnterState();
    }


    public void ChangeState(EnemyState state)
    {
        if(_currentState != null)
        {
            _currentState.ExitState();
        }
        _currentState = state;


        _currentState.EnterState();
    }

    private bool CheckIfDestinationIsReached()
    {
        return _agent.remainingDistance <= 0.2f;
    }

    #endregion

    

    #region Updates
    void Update() { }
    

    private void LateUpdate()
    {
        //if (_fov != null)
        //{
        //    _fov.CheckForFreezeable(_fovLocation, out _fovTraceresults, _fovTraceRadius, _fovLayerMask, true);
        //    if (_fovTraceresults.Length > 0)
        //    {
        //        LineOfSightUtility.HasLineOfSight(_test, _fovTraceresults[0].transform, 360f, _plqayer);
        //        //_fovCheckFrequency = _alertcheckFrequency;
        //    }
        //}
        if (Time.time >= _nextCheckTime && _fov != null)
        {
            _nextCheckTime = Time.time + _fovCheckFrequency;

            _fov.CheckForFreezeable(_fovLocation, out _fovTraceresults, _fovTraceRadius, _fovLayerMask, true);
            if (_fovTraceresults.Length > 0)
            {
                _fovCheckFrequency = _alertcheckFrequency;
            }

        }

        if (_testDeath)
        {
            //_animController.LookAround();
            //_agent.enabled = false;
            //_animController.EnemyDied();
            CompareDistances(transform.position, _player.position);
            //_testDeath = false;
        }
        if (_currentState != null && _agent.hasPath && !_agent.pathPending)
        {
            if (CheckIfDestinationIsReached()) 
            { 
                _enemyEventManager.DestinationReached(true);
                _agent.ResetPath();
                //_agent.path = null;
            }

        }


        if (!_movementChanged) { return; }

        UpdateAnimatorSpeed();
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

    private void CarveOnDestinationReached(bool _)
    {
        _agent.enabled = false;
        _obstacle.enabled = true;
    }

    private void UpdateAgentDestination(Vector3 newDestination)
    {
        _obstacle.enabled = false;
        StartCoroutine(SetDestinationDelay(newDestination));
    }

    private IEnumerator SetDestinationDelay(Vector3 newDestination)
    {
        yield return new WaitForSeconds(0.15f);
        _agent.enabled = true;
        _agent.SetDestination(newDestination);
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
        SetupFSM();
    }

    protected override void OnSceneComplete()
    {
        if (_currentState != null)
        {
            _currentState.ExitState();
            _currentState = null;
        }
        _patrol = null;
        _fov = null;
        _fovTraceresults = null;
        _animController = null;
    }
    #endregion

    #region Test Functions
    public bool _testDeath = false;
   
    #endregion
}
