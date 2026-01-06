using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[Obsolete("Use FSMBaseNew instead", true)]
public class FSMManagerObsolete : FSMBaseObsolete
{
    private List<SetDestinationDelay> _timer = new(2);
    protected bool _hasValidDestination = false;
    private Action<float> RemainingDistanceAction;
    private Action OnDestinationReached;
    private Action OnPatrolReached;
    private Action OnChaseReached;
    

    public FSMManagerObsolete(IAgentData data, IPathResolver resolver, IFieldOfViewRunner runner)
    {
        if (data == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Must Pass valid Data");
#endif
            return;

        }
        _ownerData = data;

        if (resolver == null)
        {
            Dictionary<StateId, ICandidateProvider> providers = new()
            {
                { StateId.Patrol, new WaypointProvider(WaypointRepo.Instance) }
            };
            ICandidateProvider destResolver = new DestinationResolver(providers);
            _pathFinder = new PathFinderObsolete(destResolver);
        }
        else _pathFinder = resolver;

        if (runner == null)
        {
            FOVParameters fovParams = new FOVParameters();
            fovParams.ownerOrigin = data.Transform;
          //  fovParams.FOVTarget = data.PrimaryTarget;
            // Add FOV origin aswell
            _fovHandler = new NPCFieldOfViewHandler(fovParams);
        }
        else _fovHandler = runner;
        AssignActions();
    }

    private bool StateHasChanged(StateId id)
    {
        if(id != _currentStateId)
        {
            CancelRunningCoroutine();
            return true;
        }
        return false;
    }

    private void AssignActions()
    {
        //_fovHandler.OnFOVSweepComplete = FieldOfViewSweepResult;
        //_pathFinder.Callback = OnPathRequestComplete;// New
        TryRepath = TryGetNextDestination;
        CancelOrContinueRoutine = StateHasChanged;
        LookAroundAction = LookAround; // Obsolete
        RemainingDistanceAction = SetSpeedByDistance; // obsolete
        OnPatrolReached = LookAroundAndContinue;

        OnTick += _fovHandler.Tick;
        OnTick += TimerTicks;
        OnTick += ClassUpdate;
        base.OnLateTick = OnLateTick;
    }

    private void TryResetAgent()
    {
        _hasValidDestination = false;
        SetSpeedTier(SpeedTier.Idle);
        _ownerData.Agent.ResetPath();
        if (_currentStateId == StateId.Patrol) return;
        _ownerData.Agent.enabled = false;
        _ownerData.Obstacle.enabled = true;
    }

    #region Destination Region

    public override void FieldOfViewSweepResult(FOVResult result, bool withinAttackAngles)
    {
       /* if (_currentStateId == StateId.Patrol || _currentStateId == StateId.Search)
        {
            Notification?.Invoke(NotifyOwnerNPC.TargetFound(_currentStateId));
            return;
        }*/
        Notification?.Invoke(NPCNotification.FOVUpdate(/*_currentStateId, */result, withinAttackAngles));
    }

   

    
    protected override void SetDestination(NavMeshPath path, Vector3 destination, StateId current)
    {
        if (current != _currentStateId) return;
        ToggleAgent(setActive: true);
        if (_ownerData.Agent.SetPath(path) ||
            _ownerData.Agent.SetDestination(destination))
        {
         /*   float stopdist = _ownerData.OnRequestAgentStoppingDistance?.Invoke(current) ?? 0f;//_ownerData.GetAgentStoppingDistance(current);
            _ownerData.Agent.stoppingDistance = stopdist;
            _hasValidDestination = true;*/
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Failed to Set Path - Attempting to Re-path");
#endif
            _hasValidDestination = false;
            TryResetAgent();
            TryRepath?.Invoke(current);
        }
    }
    #endregion


    #region Path Received & Validation
    public override void OnPathRequestComplete(in DestinationResult result)
    {

        if (StateHasChanged(result.Id) || result.Reason == ReasonForDestinationCheck.Cancelled) return;
        bool pathFound = result.PathFound;
        StateId id = result.Id;

        if (result.Reason == ReasonForDestinationCheck.ProbePath && pathFound)
        { Notification?.Invoke(NPCNotification.PathToPrimaryAvailable(/*result.Id*/)); return; }
    
        if (!result.PathFound) { Notification?.Invoke(NPCNotification.NoAvailablePath(/*_currentStateId*/)); Debug.LogError("NO Path Found!!"); return; }
        else
        {
            _currentDestination = result.Destination;
            _currentDestinationForward = result.Forward;
            OnMapDestinationToZone?.Invoke(_currentDestination);

            NavMeshObstacle o = _ownerData?.Obstacle;
            if (o !=null && o.enabled && o.carving)
            {
                _ownerData.Obstacle.enabled = false;
                _timer.Add(new SetDestinationDelay(Time.deltaTime + Mathf.Epsilon, _currentDestination, result.Path, id, SetDestination));
                return;
            }
            SetDestination(result.Path, _currentDestination, id);
        }

    }

    #endregion


    #region Tick Region
  
    private void UpdateAgentSpeed()
    {
        /*var a = Owner.Agent; if (!a) return;
        float delta = Mathf.Abs(_targetSpeed - a.speed);
        float rate = (0.5f > 0f) ? Mathf.Max(0.01f, delta / 0.5f) : float.PositiveInfinity;
        a.speed = Mathf.MoveTowards(a.speed, _targetSpeed, rate * Time.deltaTime);*/

        if (_ownerData.Agent == null) return;
        float smoothedSpeed = Mathf.Lerp(_ownerData.Agent.speed, _targetSpeed, _lerpSpeed * Time.deltaTime);
        _ownerData.Agent.speed = smoothedSpeed;

        float _currentSpeed = _ownerData.Agent.speed;

        if (Mathf.Approximately(_ownerData.Agent.speed, _targetSpeed)) _ownerData.Agent.speed = _targetSpeed;
    }



    public void ClassUpdate(float dt)
    {

        if (!_hasValidDestination || _ownerData.Agent == null) return;
        StateId current = _currentStateId;
        NavMeshAgent a = _ownerData.Agent;

        if (a.pathPending) return;

        if (!a.enabled || !a.isOnNavMesh)
        {
            _hasValidDestination = false;
            TryResetAgent();
            TryRepath?.Invoke(current);
            return;
        }

  
        if (!a.hasPath || a.pathStatus != NavMeshPathStatus.PathComplete)
        {
            _hasValidDestination = false;
            TryResetAgent();
            TryRepath?.Invoke(current);

            return;
        }

        var dist = a.remainingDistance;
       
        if (float.IsNaN(dist)) return;

        float stopThreshold = (a.stoppingDistance + 0.25f);
        if (dist <= stopThreshold)
        {
            _hasValidDestination = false;
            TryResetAgent(); // Resets path and Sets speed == 0f
            Debug.LogError("Reached Destination");
            OnDestinationReached?.Invoke();
            return;
        }
        RemainingDistanceAction?.Invoke(dist);
    }
    bool _usesSpeedByDistance = false;

    void SetSpeedByDistance(float remainingDistance)
    {
        if (_ownerData.Agent == null || _ownerData.Agent.isStopped) return;

        if (_usesSpeedByDistance)
        {
            if (remainingDistance > _ownerData.SprintEnterDist) { SetSpeedTier(SpeedTier.Sprint); }
            else if (remainingDistance < _ownerData.SprintExitDist) { SetSpeedTier(SpeedTier.Walk); }
        }
        else SetSpeedTier(SpeedTier.Walk);

    }

    private void OnLateTick(float dt) => UpdateAgentSpeed();



    private void TimerTicks(float dt)
    {
        if(_ownerData is NPCControllerObsolete c) // Delete later
        {
            if (c.TestSprint)
            {
               // SetAgentTargetSpeed(_ownerData.SprintSpeed, 2f);
            }else if (c.TestWalk)
            {
             //   SetAgentTargetSpeed(_ownerData.WalkSpeed, 2f);
            }
        }
    
        for (int i = 0; i < _timer.Count; i++)
        {
            var t = _timer[i];
            t.RemainingTime -= dt;

            if (t.RemainingTime <= 0)
            {
                t.OnDone?.Invoke(t.Path, t.Destination, t.Current);
                _timer.RemoveAt(i);
                i--;
                continue;
            }
            _timer[i] = t;
        }
    }

   

    
    #endregion


    #region Patrol Region
    private Func<StateId, bool> CancelOrContinueRoutine;
    private Action LookAroundAction;
  
    private void RoutineEnd(StateId current)
    {
        CancelRunningCoroutine();
        TryGetNextDestination(current);
    }

    private void TryGetNextDestination(StateId current)
    {
        if (StateHasChanged(current)) return;
      //  if (current != _currentStateId) return;

        ValidateDestination request;

        switch (current)
        {
            case StateId.Patrol:
                request = ValidateDestination.GetPatrolPoint(_ownerData, _ownerData.Path/*, OnWaypointZoneReceived*/);
                break;
            case StateId.Chase:
               // request = ValidateDestination.GetTargetPosition(_ownerData.Path, ReasonForDestinationCheck.ValidatePathForDestination, _ownerData, _ownerData.PrimaryTarget);
                break;
            default:
                return;
        }

      //  _pathFinder?.TryGetDestination(request);
    }


    public override void BeginPatrol(StateId id)
    {
        if (id != StateId.Patrol) return;
        _fovHandler?.SetAlertPhase(AlertPhase.Idle);
        OnDestinationReached = OnPatrolReached;
       // CancelRunningCoroutine();
        
        if (_currentStateId != id) _currentStateId = id;
        TryRepath?.Invoke(id);
    }

    public override void LookAroundAndContinue()
    {
       // if (_runningRoutine == null)
            //_runningRoutine = this.BeginPatrolRoutine(_currentStateId, _ownerData.Transform, _ownerData.MinPatrolPointWaitTime, _ownerData.MaxPatrolPointWaitTime, _currentDestinationForward, OnAnimationIntent, CancelOrContinueRoutine, RoutineEnd);
            //_runningRoutine = CoroutineRunner.Instance.StartCoroutine(PatrolWaitRoutine(_currentPatrolPoinfForward));
    }
  

    // Change to Action
    private void LookAround() { }//=> Owner.OwnerEM.TriggerAnimation(AnimationCue.Look);

    private void CancelRunningCoroutine()
    {
        if(_runningRoutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(_runningRoutine);
            _runningRoutine = null;
        }
    }



    public override bool TrySwitchPatrolZone() => false;//_pathFinder?.TrySwitchPatrolZone() ?? false;
    #endregion





    public override void BeginChase(StateId id)
    {
        if (id != StateId.Chase) return;
        _fovHandler.SetAlertPhase(AlertPhase.Alerted);
        OnDestinationReached = OnChaseReached;
        
        if (_currentStateId != id) _currentStateId = id;
        TryRepath?.Invoke(id);
    }

    public override void BeginFlank(StateId id)
    {
        if (id != StateId.Flank) return;
        _fovHandler.SetAlertPhase(AlertPhase.Alerted);
        if (_currentStateId != id) _currentStateId = id;
        TryRepath?.Invoke(id);
    }

    public override void TakeCover(StateId id)
    {
        throw new NotImplementedException();
    }

    public override void FollowGroup(StateId id)
    {
        throw new NotImplementedException();
    }

    public override void ExitState(StateId id)
    {
        TryResetAgent();
        OnDestinationReached = null;
        CancelRunningCoroutine();
        _pathFinder?.CancelAll();
    }

    public override void OnInstanceDestroyed()
    {
        TryRepath = null;
        CancelOrContinueRoutine = null;
        LookAroundAction = null;
        RemainingDistanceAction = null;
        OnPatrolReached = null;
       // _fovHandler.OnFOVSweepComplete = null;
        OnTick -= TimerTicks;
        OnTick -= ClassUpdate;
        base.OnLateTick = null;
    }

    
}
