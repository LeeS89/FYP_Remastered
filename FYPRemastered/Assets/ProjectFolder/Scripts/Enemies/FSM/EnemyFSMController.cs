using System;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.AI;


public sealed class EnemyFSMController : FSMControllerBase
{
    [Header("Animation Components")]
    [SerializeField] private Animator _anim;
    private EnemyAnimController _animController;

    // Depending on wether _currentState is == _startionary or a moving state,
    // this action will either stop the agent immediately or measure the path to the destination
    private Action _destinationCheckAction;
    
    // When true, the agent will aloways face its target
    private bool _rotatingTowardsTarget = false;

    [Header("Agent and animation speed values")]
    private float _targetSpeed = 0f;
    private float _lerpSpeed = 0f;
    private AlertStatus _alertStatus = AlertStatus.None;


    #region Agent and FSM Setup / Scene Initialization
    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
    
        RegisterGlobalEvents();
        SetupFSM();

    }

    protected override void SetupFSM()
    {
        base.SetupFSM();
        _animController = new EnemyAnimController(_anim, _agentEventManager);

    }

    protected override void OnSceneStarted()
    {
        //InitializeWeapon();
        ChangeState(_patrol);

        OwnerIsDead = false;
        //AgentIsAlive = true;
        //_agent.ResetPath();
    }

    #endregion

    #region Scene End/ Agent & Component Cleanup
    protected override void OnSceneComplete()
    {
        _animController = null;
        base.OnSceneComplete();
    }
    #endregion

    #region Agent speed Updates
    protected override void UpdateAgentSpeedValues(float speed, float lerpSpeed)
    {
        _lerpSpeed = lerpSpeed;
        _targetSpeed = speed;

    }

    private void UpdateAgentSpeed()
    {

        float smoothedSpeed = Mathf.Lerp(_agent.speed, _targetSpeed, _lerpSpeed * Time.deltaTime);
        _agent.speed = smoothedSpeed;

        float _currentSpeed = _agent.speed;

        if (Mathf.Approximately(_agent.speed, _targetSpeed))
        {
            _agent.speed = _targetSpeed;
        }

    }

    #endregion

    #region Agent/ GameObject Toggles
    private void DisableAgentAndObstacle()
    {
        ToggleAgent(false);
        ToggleNMObstacle(false);
    }

    protected override void ToggleNMObstacle(bool status)
    {
        if (_obstacle.enabled == status) { return; }
        _obstacle.enabled = status;
    }

    protected override void CarveOnDestinationReached(bool reached)
    {
        //if(_currentState == _patrol) { return; }
        if (!reached) { return; }
        ToggleAgent(false);
        ToggleNMObstacle(true);
    }

    private void ToggleAgent(bool agentEnabled)
    {
        if (_agent.enabled == agentEnabled) { return; }

        _agent.enabled = agentEnabled;
    }

    protected override void ToggleAgentControlledRotationToTarget(bool rotate)
    {
        if (_rotatingTowardsTarget == rotate) return;

        _agent.updateRotation = !rotate;
        _rotatingTowardsTarget = rotate;
    }
    #endregion

    #region Status Updates
    protected override void OwnerDeathStatusChanged(bool isDead)
    {
        base.OwnerDeathStatusChanged(isDead);

        if (OwnerIsDead)
        {
            ResetFSM(_deathState);
        }
        // Implement Respawn here
        //if (AgentIsAlive) { AgentIsAlive = false; }


    }

    protected override void OnPlayerDeathStatusUpdated(bool isDead)
    {
        base.OnPlayerDeathStatusUpdated(isDead);

        //_playerIsDead = true;
        if (OwnerIsDead || !PlayerIsDead) { return; }


        ResetFSM(_patrol);

    }

    private void HandleAgentRespawn()
    {
        ToggleGameObject(true);
        ToggleAgent(true);
        //_agent.enabled = true;
        OwnerIsDead = false;
        _agentEventManager.ChangeAnimatorLayerWeight(EnemyAnimController.AnimationLayer.Combat, 0, 1, 0.5f);
        OwnerDeathStatusChanged(false);
        _agentEventManager.AgentRespawn();
        _alertStatus = AlertStatus.None;
        //AgentIsAlive = true;
    }
    #endregion

    #region Component Updates
    private void UpdateAnimator()
    {
        if (_animController == null || _agent == null)
            return;

        _animController.UpdateAnimator(_agent.velocity, transform.forward);

    }
    #endregion

    #region FSM State Management
    protected override void ChangeState(EnemyState state, AlertStatus status = AlertStatus.None)
    {

        if (_currentState == state) { return; }

        if (_currentState != null)
        {
            _currentState.ExitState();
        }
        _currentState = state;

        _destinationCheckAction = _currentState == _stationary ? StopImmediately : MeasurePathToDestination;

        _currentState.EnterState(status);

    }

    protected override void StationaryStateRequested(AlertStatus alertStatus)
    {
        if (_currentState == _stationary)
        {
            return;
        }
        ChangeState(_stationary, alertStatus);
    }

    protected override void OnDestinationRequestComplete(bool success, Vector3 destination, AIDestinationType destType)
    {
        if (!success)
        {
            ChangeState(_stationary);
            return;
        }

        int stoppingDistance = 0;

        switch (destType)
        {
            case AIDestinationType.PatrolDestination:
                break;
            case AIDestinationType.ChaseDestination:
                ChangeState(_chasing, AlertStatus.Chasing);
                stoppingDistance = Random.Range(4, 11);
                break;
            case AIDestinationType.FlankDestination:
                ChangeState(_chasing, AlertStatus.Flanking);
                break;
            default:
                ChangeState(_stationary);
                return;
        }
        _resourceRequest.carvingCallback = () =>
        {
            if (_obstacle.enabled)
            {
                _obstacle.enabled = false;
            }
        };

        _resourceRequest.agentActiveCallback = () =>
        {
            if (!_agent.enabled)
            {
                ToggleAgent(true);

            }

            _agent.stoppingDistance = stoppingDistance;
            _agent.SetDestination(destination);
            _agentEventManager.DestinationApplied();
        };
        _destinationManager.StartCarvingRoutine(_resourceRequest);


    }

    public override void ResetFSM(EnemyState contextState = null)
    {
        if (TargetInView)
        {
            TargetInViewStatusUpdated(false);
            //Debug.LogError("Target in view reset to false");
            // UpdateFieldOfViewResults(false);

        }

        _agentEventManager.ChangeAnimatorLayerWeight(EnemyAnimController.AnimationLayer.Alert, 1, 0, 0.5f, true);

        switch (contextState)
        {
            case PatrolState: // ResetFSM called when the player dies, or this agent is respawning

                break;
            case DeathState: // ResetFSM called when this agent dies
                DisableAgentAndObstacle();
                break;
            default: // ResetFSM called when this agent is respawning
                contextState = _patrol;
                if (OwnerIsDead)
                {
                    HandleAgentRespawn();
                }
                break;
        }
#if UNITY_EDITOR
        Debug.LogError("Resetting FSM to state: " + contextState.GetType().Name);
#endif
        if (_currentState != contextState)
        {
            ChangeState(contextState);
        }
    }

   
    #endregion

    #region Path Validity & Remaining Distance Check
    private void CheckCurrentPathValidity()
    {
        if (_agent.hasPath)
        {
            if (_agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                //_enemyEventManager.PathInvalid();
                //Debug.LogError("Path is invalid, resetting path.");
            }
        }
    }

    private void MeasurePathToDestination()
    {
        if (!_agent.enabled) { return; }
        //DebugExtension.DebugWireSphere(_agent.destination, Color.green);
        if (_currentState != null && _agent.hasPath && !_agent.pathPending)
        {
            if (CheckIfDestinationIsReached())
            {

                _agent.ResetPath();
                _agentEventManager.DestinationReached(true);


            }

        }
    }

    private bool CheckIfDestinationIsReached()
    {
        //Debug.LogError("Remainingdistance: "+_agent.remainingDistance);
        return _agent.remainingDistance <= (_agent.stoppingDistance + 0.25f);
    }

    /// <summary>
    /// Used when agent enters Stationary state
    /// </summary>
    private void StopImmediately()
    {

        if (_agent.hasPath)
        {
            _agent.ResetPath();
        }

        _destinationCheckAction = null;
    }
    #endregion

    #region Field of view responses
    protected override void TargetInViewStatusUpdated(bool inView)
    {
        if (TargetInView == inView) { return; }

        TargetInView = inView;

        if (_alertStatus != AlertStatus.None) { return; }

        if (TargetInView)
        {
            EnterAlertPhase();
            CallForBackup();
        }
    }

    protected override void CallForBackup()
    {
        SceneEventAggregator.Instance.AlertZoneAgents(AgentZone, this);
    }

    public override void EnterAlertPhase()
    {

        if (_currentState != _chasing && _alertStatus == AlertStatus.None) /// Need to change Alert status param to something else
        {
            _alertStatus = AlertStatus.Alert;

            _agentEventManager.ChangeAnimatorLayerWeight(EnemyAnimController.AnimationLayer.Alert, 0, 1, 0.5f, true);

            //_alertStatus = AlertStatus.Alert;

            _agentEventManager.DestinationRequested(AIDestinationType.ChaseDestination);

        }

    }

    private void RotateTowardsPlayer()
    {
        Transform player = GameManager.Instance.GetPlayerPosition(PlayerPart.Position);
        if (player == null)
            return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.001f)
            return;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        float dot = Vector3.Dot(forward.normalized, toPlayer.normalized);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        const float precisionThreshold = 1f; // degrees

        Quaternion targetRotation = Quaternion.LookRotation(toPlayer);

        if (angle < precisionThreshold)
        {
            // Close enough — complete the rotation smoothly
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                1f); // forces it to land exactly, still smooth
            //_enemyEventManager.FacingTarget(true);
            return;
        }
        /*else
        {
            _enemyEventManager.FacingTarget(false);
        }*/
        // _agentEventManager.FacingTarget(false);

        transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 5f); // adjust speed as needed
    }
    #endregion


    public bool testRespawn = false;

    public bool _testDeath = false;


    void Update()
    {
        if (_testDeath)
        {
            GameManager.Instance.CharacterDeathStatusChanged(CharacterType.Player);
            //_animController.DeadAnimation();
            //ChangeState(_chasing);
            //CarveOnDestinationReached(true);
            _testDeath = false;
        }

        if (testRespawn)
        {
            GameManager.Instance.PlayerDeathStatusChanged(false);
            //_playerIsDead = false;
            testRespawn = false;
        }

        /*if (_testWaypoint)
        {
            ChangeWaypoints();
            _testWaypoint = false;
        }*/

        // UpdateAgentSpeed();
    }

    private void LateUpdate()
    {
        if (_currentState != null)
        {
            _currentState.LateUpdateState();
        }

        if (OwnerIsDead) { return; }

        CheckCurrentPathValidity();


        //MeasurePathToDestination();
        _destinationCheckAction?.Invoke();

        if (_rotatingTowardsTarget)
        {

            RotateTowardsPlayer();
            // UpdateAnimatorDirection();
        }
        // UpdateAnimatorDirection();
        UpdateAgentSpeed();
        UpdateAnimator();
        //if (!_movementChanged) { return; }

        //UpdateAgentSpeed();
    }
}
