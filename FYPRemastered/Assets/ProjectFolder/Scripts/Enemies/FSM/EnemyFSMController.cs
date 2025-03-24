using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFSMController : ComponentEvents
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent _agent;
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

    
    
    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        _enemyEventManager = _eventManager as EnemyEventManager;
        _enemyEventManager.OnAnimationTriggered += PlayAnimationType;
        _enemyEventManager.OnSpeedChanged += UpdateTargetSpeedValues;
        RegisterGlobalEvents();

        //SetupFSM();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
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

    public bool _testDeath = false;

    
    void Update()
    {
        if (_testDeath)
        {
            _animController.LookAround();
            //_agent.enabled = false;
            //_animController.EnemyDied();
            _testDeath = false;
        }

        
    }

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

        if (!_movementChanged) { return; }

        UpdateAnimatorSpeed();
    }

    public void TesReset()
    {
        _animController.ResetLook();
    }

    private void UpdateAnimatorSpeed()
    {
      
        float smoothedSpeed = Mathf.Lerp(_agent.speed, _targetSpeed, _lerpSpeed * Time.deltaTime);
        _agent.speed = smoothedSpeed;

       
        // Update animator (assumes 1D blend tree for walk/run)
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
}
