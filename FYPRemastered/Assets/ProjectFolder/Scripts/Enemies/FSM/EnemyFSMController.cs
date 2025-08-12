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
    [SerializeField, Tooltip("Do Not Change - Synchronized with Walking animation")] private float _walkSpeed; // In base

    private EnemyAnimController _animController;
    private Action _destinationCheckAction;
    private EnemyState _currentState;
    
   // private bool _playerIsDead = false;
    private bool _rotatingTowardsTarget = false;

    [Header("Patrol State - Random number between 0 and stopAndWaitDelay to wait at each way point")]
    [SerializeField] private float _patrolPointWaitDelay; /// In base
    [SerializeField] private List<Vector3> _wayPoints;
    [SerializeField] private List<Vector3> _wayPointForwards;
   
    private BlockData _blockData;
    public int _blockZone = 0;
    private PatrolState _patrol; // In base

    [Header("Chasing State Params")]
    [SerializeField, Tooltip("Do Not Change - Synchronized with sprinting animation")] private float _sprintSpeed; // In base
    private ChasingState _chasing; // In base

    [Header("Stationary State Params")]
    private StationaryState _stationary; /// In base
    [SerializeField] private int _maxFlankingSteps = 0;

   


    [SerializeField] private LayerMask _losTargetMask; // Delete later once I fix Flank FOV
    [SerializeField] private LayerMask _losBlockingMask;
    [SerializeField] private LayerMask _losBackupTargetMask;



    [Header("Death State")]
    private DeathState _deathState;

    [Header("Gun Parameters")]
    [SerializeField] private Transform _bulletSpawnPoint;
    [SerializeField] private GameObject _owningGameObject; // In base
    //private GunBase _gun;


    [Header("Agent and animation speed values")]
    private float _targetSpeed = 0f;
    private float _lerpSpeed = 0f;
   // private bool _movementChanged = false;
    private float _previousDirection = 0f;
    private EnemyEventManager _agentEventManager; // In base

    private DestinationManager _destinationManager;
    
    private AlertStatus _alertStatus = AlertStatus.None;

   // private bool _agentIsAlive = false;

    public bool AgentIsAlive { get; private set; }




    #region Event Registrations
    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        _agentEventManager = _eventManager as EnemyEventManager;
        _owningGameObject = gameObject;

        _agentEventManager.OnRequestChasingState += ChasingStateRequested;
        _agentEventManager.OnRequestStationaryState += StationaryStateRequested;
      
        _agentEventManager.OnOwnerDeathStatusUpdated += OnDeathStatusUpdated;
        _agentEventManager.OnAgentDeathComplete += ToggleGameObject;
        _agentEventManager.OnAgentRespawn += ToggleGameObject;
       
        _agentEventManager.OnDestinationReached += CarveOnDestinationReached;
        _agentEventManager.OnRotateTowardsTarget += ToggleRotationToTarget;
        _agentEventManager.OnSpeedChanged += UpdateAnimatorSpeedValues;


        _agentEventManager.OnTargetSeen += TargetInViewStatusUpdated;


        RegisterGlobalEvents();
        SetupFSM();

    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _agentEventManager.OnRequestChasingState -= ChasingStateRequested;
        _agentEventManager.OnRequestStationaryState -= StationaryStateRequested;
       
        _agentEventManager.OnTargetSeen -= TargetInViewStatusUpdated;
      
        _agentEventManager.OnDestinationReached -= CarveOnDestinationReached;
        _agentEventManager.OnOwnerDeathStatusUpdated -= OnDeathStatusUpdated;
        _agentEventManager.OnAgentDeathComplete -= ToggleGameObject;
        _agentEventManager.OnAgentRespawn -= ToggleGameObject;
        _agentEventManager.OnRotateTowardsTarget -= ToggleRotationToTarget;
        _agentEventManager.OnSpeedChanged -= UpdateAnimatorSpeedValues;

       
        base.UnRegisterLocalEvents(eventManager);
        _agentEventManager = null;
    }

    protected override void RegisterGlobalEvents()
    {
        GameManager.OnPlayerDeathStatusChanged += OnPlayerDeathStatusUpdated;
        
        GameManager.OnPlayerMoved += EnemyState.SetPlayerMoved;
        BaseSceneManager._instance.OnSceneStarted += OnSceneStarted;
        BaseSceneManager._instance.OnSceneEnded += OnSceneComplete;
    }

    protected override void UnRegisterGlobalEvents()
    {
        GameManager.OnPlayerDeathStatusChanged -= OnPlayerDeathStatusUpdated;
       
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

       // if (!_movementChanged) { return; }
        float smoothedSpeed = Mathf.Lerp(_agent.speed, _targetSpeed, _lerpSpeed * Time.deltaTime);
        _agent.speed = smoothedSpeed;

        float _currentSpeed = _agent.speed;

        if (Mathf.Approximately(_agent.speed, _targetSpeed))
        {
            _agent.speed = _targetSpeed;
        }
      /*  if (Mathf.Abs(_agent.speed - _targetSpeed) <= 0f)
        {
            _movementChanged = false;
        }*/

        // _animController.UpdateSpeed(smoothedSpeed);

        /* if (Mathf.Abs(_agent.speed - _targetSpeed) <= 0.001f)
         {

             _agent.speed = _targetSpeed;
             //_animController.UpdateSpeed(_targetSpeed);
            // _movementChanged = false;
         }*/

    }

    private void UpdateAnimator()
    {
        
        Vector3 moveDir = _agent.velocity;
        moveDir.y = 0f;
        moveDir.Normalize();
        Vector3 forward = transform.forward;
        float directionAngle = Vector3.SignedAngle(forward, moveDir, Vector3.up);
        float normalizedDirection = directionAngle / 90f; // Normalize to -1 to 1 range

        float dot = Vector3.Dot(forward, moveDir);
        float speed = _agent.speed;
       /* if(dot < 0)
        {
            speed *= -1;
        }*/

        _animController.UpdateBlendTreeParams(speed, 0f);
    }

  // bool _movementChanged = false;
    private void UpdateAnimatorSpeedValues(float speed, float lerpSpeed)
    {
        _lerpSpeed = lerpSpeed;
        _targetSpeed = speed;
        
      //  _movementChanged = true;
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


    #region Death Region
    private void OnDeathStatusUpdated(bool isDead)
    {
        if (AgentIsAlive) { AgentIsAlive = false; }

        ResetFSM(_deathState);
    }

    private void DisableAgent()
    {
        if (_agent.enabled)
        {
            _agent.enabled = false;
        }
        if (_obstacle.enabled)
        {
            _obstacle.enabled = false;
        }
    }

    private void ToggleGameObject(bool status)
    {
        gameObject.SetActive(status);
    }

    #endregion



    #region Global Events

    protected override void OnSceneStarted()
    {
        //InitializeWeapon();
        PatrolStateRequested();
        AgentIsAlive = true;
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
     
        _animController = null;
        _deathState = null;
    }

    protected override void OnPlayerDeathStatusUpdated(bool isDead)
    {
        base.OnPlayerDeathStatusUpdated(isDead);
        
        //_playerIsDead = true;
        if (!AgentIsAlive || !PlayerIsDead) { return; }

        
        ResetFSM(_patrol);
        
    }

    /*protected override void OnPlayerRespawned()
    {
        _playerIsDead = false;
    }*/
    #endregion

   
}
