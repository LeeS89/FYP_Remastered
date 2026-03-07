using System;
using UnityEngine;
using UnityEngine.AI;

[Obsolete("", true)]
public abstract class FSMControllerBaseNewObsolete : ComponentEvents
{

    // New Functions Needed
    public Action<float> Tick;
    private Action<float> OnPatrol;

    // End New functions



    // NEW stuff for Combining with NPCController
    [Header("AI Components")]
    [SerializeField] protected NavMeshAgent _agent;
    [SerializeField] protected NavMeshObstacle _obstacle;

    [Header("Event Manager & Resource request handler")]
    protected EnemyEventManager _agentEventManager;

    protected PathFinderObsolete _targetResolver;
    public FSMPolicyObsolete? _currentPolicy;
    protected uint _currentPolicyVersion;

    [Header("Agent Speed Params")]
    [SerializeField, Tooltip("Do Not Change - Synchronized with Walking animation")]
    protected float _walkSpeed;
    [SerializeField, Tooltip("Do Not Change - Synchronized with sprinting animation")]
    protected float _sprintSpeed;

    protected virtual void PolicyUpdated(in FSMPolicyObsolete newPolicy) { }

    protected virtual void OnPathEvaluationCompleted2(in FSMPolicyObsolete policyUsed, PolicyIntentResult result, Vector3 destination)
    {
        if (!_currentPolicy.HasValue || _currentPolicy.Value.Version != policyUsed.Version) return;
        MovementIntent currentMoveIntent = _currentPolicy.Value.MoveIntent;

        //  if(currentMoveIntent == policyUsed.MoveIntent)
        //  {
        if (currentMoveIntent == MovementIntent.FollowPrimary)
        {
            //if(result == PathCheckResult.PathToPrimaryAvailable) Setdestination
            if(result == PolicyIntentResult.PathToPrimaryBlocked)
            {
                FSMPolicyResultObsolete haltResult = new FSMPolicyResultObsolete(_currentPolicy.Value, PolicyHaltReason.PathBlocked, true);
                // Notfy NPCController
            }
        }
        else if(currentMoveIntent == MovementIntent.FollowSecondary)
        {
            if(result == PolicyIntentResult.NoAvailableSecondaryToFollow)
            {
                FSMPolicyResultObsolete haltResult = new FSMPolicyResultObsolete(_currentPolicy.Value, PolicyHaltReason.NoAvailableGroupToFollow, true);
                // Notfy NPCController
            }
            else if(result == PolicyIntentResult.PathAvailable)
            {
                // SetDestination
            }
            else if(result == PolicyIntentResult.PathToPrimaryAvailable)
            {
                FSMPolicyResultObsolete haltResult = new FSMPolicyResultObsolete(_currentPolicy.Value, PolicyHaltReason.PathUnAvailable, true);
                // Notfy NPCController
            }
        }
        //}









        /*if (pathCheckIntent == currentMoveIntent) // And Version, and not null
        {
            // if(!pathBlocked)
            // => SetDestination;
            // else
            // if (currentMoveIntent == FollowSecondary) Halt, no available group
            // if (currentMoveIntent == FindFlank) Halt, no available Flank
            // if (currentMoveIntent == FindCover) Halt, no available cover
            // Halted
        }

        switch (pathTarget)
        {
            case PathTarget.Primary:

                break;
        }*/
    }


    protected virtual void OnPathEvaluationCompleted(in FSMPolicyObsolete policyUsed, MovementIntent pathCheckIntent, bool pathBlocked, Vector3 destination)
    {
        if (!_currentPolicy.HasValue || _currentPolicy.Value.Version != policyUsed.Version) return;
        MovementIntent currentMoveIntent = _currentPolicy.Value.MoveIntent;

        if (pathCheckIntent == currentMoveIntent) // And Version, and not null
        {
            // if(!pathBlocked)
            // => SetDestination;
            // else
            // if (currentMoveIntent == FollowSecondary) Halt, no available group
            // if (currentMoveIntent == FindFlank) Halt, no available Flank
            // if (currentMoveIntent == FindCover) Halt, no available cover
            // Halted
        }

        /*switch (pathTarget)
        {
            case PathTarget.Primary:

                break;
        }*/
    }




    protected virtual void OnPathEvaluationComplete(in FSMPolicyObsolete policyUsed, MovementIntent pathCheckIntent, bool pathBlocked, Vector3 destination)
    {
        if (!_currentPolicy.HasValue || _currentPolicy.Value.Version != policyUsed.Version) return;
        MovementIntent currentMoveIntent = _currentPolicy.Value.MoveIntent;

        if (pathCheckIntent == currentMoveIntent) // And Version, and not null
        {
            // if(!pathBlocked)
            // => SetDestination;
            // else
            // if (currentMoveIntent == FollowSecondary) Halt, no available group
            // if (currentMoveIntent == FindFlank) Halt, no available Flank
            // if (currentMoveIntent == FindCover) Halt, no available cover
            // Halted
        }

       /* switch (pathTarget)
        {
            case PathTarget.Primary:

                break;
        }*/
    }

    /// <summary>
    /// A separate PathCheck to the primary target (Player)
    /// Runs separately to the Main PathChecks when the inital path check to the player is blocked
    /// 
    /// </summary>
    /// <param name="policyUsed"></param>
    /// <param name="pathBlocked"></param>
    /// <param name="targetPosition"></param>
    protected virtual void OnPathToPrimaryTargetEvaluationComplete(in FSMPolicyObsolete policyUsed, bool pathBlocked, Vector3 targetPosition)
    {
        if (!_currentPolicy.HasValue || policyUsed.Version != _currentPolicy.Value.Version) return;
        MovementIntent intent = _currentPolicy.Value.MoveIntent;

        if(intent == MovementIntent.FollowPrimary)
        {
            //if(!pathBlocked) => ChasePrimary
            if(pathBlocked)
            {
                FSMPolicyResultObsolete result = new FSMPolicyResultObsolete(_currentPolicy.Value, PolicyHaltReason.PathBlocked, false);
            }
        }
        else if (intent == MovementIntent.FollowSecondary)
        {
            if (!pathBlocked)
            {
                FSMPolicyResultObsolete result = new FSMPolicyResultObsolete(_currentPolicy.Value, PolicyHaltReason.PathUnAvailable, true);
            }
                
        }
    }

    // End New stuff for NPCController


    // To be made Redundant
    [Header("FSM States")]
    protected PatrolStateObsolete _patrol;
    protected StationaryStateObsolete _stationary;
    protected DeathStateObsolete _deathState;
    protected ChasingStateObsolete _chasing;
    protected EnemyStateObsolete _currentState;
    // End redundant







  

    [Header("")]

    [Header("Patrol State - Random number between 0 and stopAndWaitDelay to wait at each way point")]
    protected float _patrolPointWaitDelay;

    [Header("Target Flanking Params")]
    [SerializeField] protected int _maxFlankingSteps = 0;
    [SerializeField] protected GameObject _debugFlankCubes;

    protected DestinationManagerObsolete _destinationManager;

    public int AgentZone { get; protected set; } = 0;

    public bool TargetInView { get; protected set; } = false;
    protected GameObject OwningAgent => gameObject;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _agentEventManager = eventManager as EnemyEventManager;
        base.RegisterLocalEvents(_agentEventManager);
        _agentEventManager.OnTargetSeen += TargetInViewStatusUpdated;
        _agentEventManager.OnRequestStationaryState += StationaryStateRequested;
        _agentEventManager.OnDestinationReached += CarveOnDestinationReached;
        _agentEventManager.OnAgentDeathComplete += ToggleGameObject;
        _agentEventManager.OnSpeedChanged += UpdateAgentSpeedValues;
        _agentEventManager.OnRotateTowardsTarget += ToggleAgentControlledRotationToTarget;

        // NEW events
        _agentEventManager.OnUpdateChaseTarget = SetChaseTarget;
        _agentEventManager.OnChangeState = ChangeStates;
        // end new events

        RegisterGlobalEvents();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(_agentEventManager);
        _agentEventManager.OnRotateTowardsTarget -= ToggleAgentControlledRotationToTarget;
        _agentEventManager.OnSpeedChanged -= UpdateAgentSpeedValues;
        _agentEventManager.OnAgentDeathComplete -= ToggleGameObject;
        _agentEventManager.OnDestinationReached -= CarveOnDestinationReached;
        _agentEventManager.OnRequestStationaryState -= StationaryStateRequested;
        _agentEventManager.OnTargetSeen -= TargetInViewStatusUpdated;

        // NEW events
        _agentEventManager.OnUpdateChaseTarget = null;
        _agentEventManager.OnChangeState = null;
        // end new events

        UnRegisterGlobalEvents();
    }

    protected override void RegisterGlobalEvents()
    {
        base.RegisterGlobalEvents();
        GameManager.OnPlayerDeathStatusChanged += OnPlayerDeathStatusUpdated;
        GameManager.OnPlayerMoved += EnemyStateObsolete.SetPlayerMoved;
    }

    protected override void UnRegisterGlobalEvents()
    {
        base.UnRegisterGlobalEvents();
        GameManager.OnPlayerMoved -= EnemyStateObsolete.SetPlayerMoved;
       // SceneEventAggregator.Instance.UnRegisterAgentAndZone(this, AgentZone);
        GameManager.OnPlayerDeathStatusChanged -= OnPlayerDeathStatusUpdated;
    }

    protected virtual void SetupFSM()
    {
        _patrol = new PatrolStateObsolete(OwningAgent, _agentEventManager, _patrolPointWaitDelay, _walkSpeed);
        _chasing = new ChasingStateObsolete(_agentEventManager, OwningAgent, _walkSpeed, _sprintSpeed);
        _stationary = new StationaryStateObsolete(_agentEventManager, OwningAgent);
        _deathState = new DeathStateObsolete(_agentEventManager, OwningAgent);
        _destinationManager = new DestinationManagerObsolete(_agentEventManager, _maxFlankingSteps, _debugFlankCubes, transform, OnDestinationRequestComplete);
        AgentZone = _destinationManager.GetCurrentWPZone();
      //  SceneEventAggregator.Instance.RegisterAgentAndZone(this, AgentZone);
        Debug.LogError("WP ZOne: " + AgentZone);

    }

    // NEw stuff
    protected abstract StateChangeResult ChangeStates(State newSate, Transform target = null, AlertStatus status = AlertStatus.None);

    protected abstract StateChangeResult ChangeStates(EnemyStateObsolete state, AlertStatus status = AlertStatus.None);

    protected void SetChaseTarget(Transform target)
    {
        if (ChaseTarget == target) return;
        ChaseTarget = target;
    }

    public Transform ChaseTarget { get; protected set; } = null;

    // End new stuff

    protected abstract void ChangeState(EnemyStateObsolete state, AlertStatus status = AlertStatus.None);

    protected virtual void StationaryStateRequested(AlertStatus alertStatus) { }

    protected virtual void OnDestinationRequestComplete(bool success, Vector3 destination, AIDestinationType destType) { }

    protected virtual void CheckIfDestinationReached() { }

    protected virtual void CarveOnDestinationReached(bool reached) { }

    protected virtual void TargetInViewStatusUpdated(bool inView) { }

    protected virtual void CallForBackup() { }

    public virtual void EnterAlertPhase() { }

    protected virtual void ToggleGameObject(bool status) => gameObject.SetActive(status);

    protected virtual void ToggleNMObstacle(bool status) { }

    protected virtual void UpdateAgentSpeedValues(float speed, float lerpSpeed) { }

    protected virtual void ToggleAgentControlledRotationToTarget(bool rotate) { }

    public abstract void ResetFSM(EnemyStateObsolete contextState = null);


    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        if (_currentState != null)
        {
            _currentState.ExitState();
            _currentState = null;
        }
        _patrol?.OnStateDestroyed();
        _chasing?.OnStateDestroyed();
        _stationary?.OnStateDestroyed();
        _deathState?.OnStateDestroyed();

        _patrol = null;
        _chasing = null;
        _stationary = null;
        _deathState = null;
        _agentEventManager = null;
    }
}
