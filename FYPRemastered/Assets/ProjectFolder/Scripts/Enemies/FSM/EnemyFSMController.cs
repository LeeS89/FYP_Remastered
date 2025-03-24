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
    
    private float _targetSpeed = 0f;
    private float _lerpSpeed = 0f;
    public bool _movementChanged = false;

    
    
   

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);

    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(eventManager);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animController = new EnemyAnimController(_anim);
        _patrol = new PatrolState(this, _wayPoints, _agent, _stopAndWaitDelay, _walkSpeed);
        //_patrol.OnSpeedChanged += UpdateTargetSpeedValues;
        //_patrol.OnAnimationTriggered += PlayAnimationType;
        _currentState = _patrol;
        _currentState.OnSpeedChanged += UpdateTargetSpeedValues;
        _currentState.OnAnimationTriggered += PlayAnimationType;
        _currentState.EnterState();

        
        //_patrol.EnterState();
       
    }

    public void ChangeState(EnemyState state)
    {
        if(_currentState != null)
        {
            _currentState.OnSpeedChanged -= UpdateTargetSpeedValues;
            _currentState.OnAnimationTriggered -= PlayAnimationType;

            _currentState.ExitState();
        }
        _currentState = state;

        _currentState.OnSpeedChanged += UpdateTargetSpeedValues;
        _currentState.OnAnimationTriggered += PlayAnimationType;

        _currentState.EnterState();
    }

    public bool _testDeath = false;

    // Update is called once per frame
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

        if (Mathf.Abs(_agent.speed - _targetSpeed) <= 0.01f)
        {
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
}
