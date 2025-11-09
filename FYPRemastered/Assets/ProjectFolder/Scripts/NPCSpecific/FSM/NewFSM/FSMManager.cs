using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class FSMManager : FSMBase
{
    private List<SetDestinationDelay> _timer = new(2);
    protected bool _hasValidDestination = false;
    private Action<float> RemainingDistanceAction;
    private Action OnDestinationReached;
    private Action OnPatrolReached;
    private Action OnChaseReached;

    public FSMManager(IFSMOwner owner)
    {
        if (owner == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Must Pass a valid FSMOwner");
#endif
            return;

        }

        Owner = owner;
      
        _pathFinder = new DestinationFinder(this);
        TryRepath = TryGetNextDestination;
        CancelOrContinueRoutine = HasSwitchedState;
        LookAroundAction = LookAround;
        RemainingDistanceAction = SetSpeedByDistance;
        OnPatrolReached = LookAroundAndContinue;
       
        OnTick += TimerTicks;
        OnTick += ClassUpdate;
        LateTick = OnLateTick;
    }

   

    /*private void SetSpeedTier(SpeedTier tier)
    {
        if (tier == _speedTier) return;
        _speedTier = tier;
        var (speed, lerp) = tier switch
        {
            SpeedTier.Idle => (0f, 10f),
            SpeedTier.Walk => (Owner.WalkSpeed, 2f),
            SpeedTier.Sprint => (Owner.SprintSpeed, 2f),
            _=> (0f, 10f)
        };

        SetAgentTargetSpeed(speed, lerp);

    }*/

  /*  public void ToggleAgent(bool setActive)
    {
        if (Owner.Agent.enabled == setActive) return;
        Owner.Agent.enabled = setActive;
    }*/

    private void TryResetAgent()
    {
        SetSpeedTier(SpeedTier.Idle);
        Owner.Agent.ResetPath();
        if (_currentStateId == StateId.Patrol) return;
        Owner.Agent.enabled = false;
        Owner.Obstacle.enabled = true;
    }

    #region Destination Region
  

   

    
    protected override void SetDestination(NavMeshPath path, Vector3 destination, StateId current)
    {
        if (current != _currentStateId) return;
        ToggleAgent(setActive: true);
        if (Owner.Agent.SetPath(path) ||
            Owner.Agent.SetDestination(destination)) _hasValidDestination = true;
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
    public override void OnPathRequestComplete(in PathResult result)
    {

        if (result.Id != _currentStateId || result.Reason == PathCheckReason.Cancelled) return;
        bool pathFound = result.PathFound;
        StateId id = result.Id;

        if (result.Reason == PathCheckReason.ProbePathToPrimaryTarget && pathFound)
        { Owner?.Notify(NotifyOwnerNPC.PathToPrimaryAvailable(result.Id)); return; }
    
        if (!result.PathFound) { Owner.Notify(NotifyOwnerNPC.NoAvailablePath(_currentStateId)); Debug.LogError("NO Path Found!!"); return; }
        else
        {
            _currentDestination = result.Destination;
            _currentDestinaationForward = result.Forward;
            NavMeshObstacle o = Owner?.Obstacle;
            if (o !=null && o.enabled && o.carving)
            {
                Owner.Obstacle.enabled = false;
                _timer.Add(new SetDestinationDelay(Time.deltaTime + Mathf.Epsilon, _currentDestination, result.Path, id, SetDestination));
                return;
            }
            SetDestination(result.Path, _currentDestination, id);
        }

    }

  
   
    #endregion


    #region Tick Region
   /* private void SetAgentTargetSpeed(float speed, float lerpSpeed)
    => (_lerpSpeed, _targetSpeed) = (lerpSpeed, speed);*/

    private void UpdateAgentSpeed()
    {
        /*var a = Owner.Agent; if (!a) return;
        float delta = Mathf.Abs(_targetSpeed - a.speed);
        float rate = (0.5f > 0f) ? Mathf.Max(0.01f, delta / 0.5f) : float.PositiveInfinity;
        a.speed = Mathf.MoveTowards(a.speed, _targetSpeed, rate * Time.deltaTime);*/

        if (Owner.Agent == null) return;
        float smoothedSpeed = Mathf.Lerp(Owner.Agent.speed, _targetSpeed, _lerpSpeed * Time.deltaTime);
        Owner.Agent.speed = smoothedSpeed;

        float _currentSpeed = Owner.Agent.speed;

        if (Mathf.Approximately(Owner.Agent.speed, _targetSpeed)) Owner.Agent.speed = _targetSpeed;
    }



    public void ClassUpdate(float dt)
    {

        if (!_hasValidDestination || Owner.Agent == null) return;
        StateId current = _currentStateId;
        NavMeshAgent a = Owner.Agent;

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
       
        if (/*float.IsInfinity(dist) || */float.IsNaN(dist)) return;

        float stopThreshold = (a.stoppingDistance + 0.25f);
        if (dist <= stopThreshold)
        {
            _hasValidDestination = false;
            TryResetAgent(); // Resets path and Sets speed == 0f
            OnDestinationReached?.Invoke();
            return;
        }
        RemainingDistanceAction?.Invoke(dist);
    }
    bool _usesSpeedByDistance = false;

    void SetSpeedByDistance(float remainingDistance)
    {
        if (Owner.Agent == null || Owner.Agent.isStopped) return;

        if (_usesSpeedByDistance)
        {
            if (remainingDistance > Owner.SprintEnterDist) { SetSpeedTier(SpeedTier.Sprint); }
            else if (remainingDistance < Owner.SprintExitDist) { SetSpeedTier(SpeedTier.Walk); }
        }
        else SetSpeedTier(SpeedTier.Walk);

    }

    private void OnLateTick(float dt) => UpdateAgentSpeed();



    private void TimerTicks(float dt)
    {
        if(Owner is NPCController c)
        {
            if (c.TestSprint)
            {
                SetAgentTargetSpeed(Owner.SprintSpeed, 2f);
            }else if (c.TestWalk)
            {
                SetAgentTargetSpeed(Owner.WalkSpeed, 2f);
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
        if (current != _currentStateId) return;

        ValidateDestination request;

        switch (current)
        {
            case StateId.Patrol:
                request = ValidateDestination.GetPatrolPoint(StateId.Patrol, Owner, Owner.Path);
                break;
            default:
                return;
        }

        _pathFinder?.TryGetDestination(request);
    }


    public override void BeginPatrol(StateId id)
    {
        OnDestinationReached = OnPatrolReached;
        CancelRunningCoroutine();
        if (id != StateId.Patrol) return;
       
        if (_currentStateId != id) _currentStateId = id;
        TryRepath?.Invoke(id);
    }

    public override void LookAroundAndContinue()
    {
        if (_runningRoutine == null)
            _runningRoutine = this.BeginPatrolRoutine(_currentStateId, Owner.Transform, Owner.MinWaitTime, Owner.MaxWaitTime, _currentDestinaationForward, LookAroundAction, CancelOrContinueRoutine, RoutineEnd);
            //_runningRoutine = CoroutineRunner.Instance.StartCoroutine(PatrolWaitRoutine(_currentPatrolPoinfForward));
    }
  

    private bool HasSwitchedState(StateId id)
    {
        if (id != _currentStateId) 
        {
            CancelRunningCoroutine();
            return false; 
        }
        return true;
    }
    

    private void LookAround() => Owner.OwnerEM.TriggerAnimation(AnimationCue.Look);

    private void CancelRunningCoroutine()
    {
        if(_runningRoutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(_runningRoutine);
            _runningRoutine = null;
        }
    }

    

    public override bool TrySwitchPatrolZone() => _pathFinder.TrySwitchPatrolZone();
    #endregion





    public override void BeginChase(StateId id)
    {
       // PathCheckReason reason = PathCheckReason.ValidatePathForDestination;
    }

    public override void BeginFlank(StateId id)
    {
        throw new NotImplementedException();
    }

    public override void TakeCover(StateId id)
    {
        throw new NotImplementedException();
    }

    public override void FollowGroup(StateId id)
    {
        throw new NotImplementedException();
    }

    public override void ExitState()
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

        OnTick -= TimerTicks;
        OnTick -= ClassUpdate;
        LateTick = null;
    }
}
