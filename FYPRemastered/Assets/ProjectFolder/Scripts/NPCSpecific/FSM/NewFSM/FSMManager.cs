using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class FSMManager : FSMBase
{
    private StateId ApprovedDestinationStateId;
    private List<SetDestinationDelay> _timer = new(2);
    private float _currentSpeed;

   

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
        _currentSpeed = Owner?.WalkSpeed ?? 0f;
        _pathFinder = new DestinationFinder(this);
        OnLookAroundComplete = BeginPatrol;
        CancelOrContinueRoutine = CheckState;
        LookAroundAction = LookAround;
        OnChaseTick = ChaseTick;
        OnTick += FSMTick;
     //   OnTick += OnChaseTick;
        LateTick = OnLateTick;
    }

   

    private void SetSpeedTier(SpeedTier tier)
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

    }

    public void ToggleAgent(bool setActive)
    {
        if (Owner.Agent.enabled == setActive) return;
        Owner.Agent.enabled = setActive;
    }

    private void TryResetAgent()
    {
        SetSpeedTier(SpeedTier.Idle);
        //SetAgentTargetSpeed(0f, 10f);
        Owner.Agent.ResetPath();
       // if (_currentStateId == StateId.Patrol) return;
        Owner.Agent.enabled = false;
        Owner.Obstacle.enabled = true;
    }

    #region Destination Region
  

    private SpeedTier ResolveNewSpeed(StateId id, bool destinationreached)
    {
        SpeedTier newtier;
        switch (id)
        {
            case StateId.Patrol or StateId.Flank:
                newtier = SpeedTier.Walk;
                break;
            case StateId.Cover or StateId.Flee or StateId.Follow:
                newtier = SpeedTier.Sprint;
                break;
            case StateId.Chase:
                newtier = SpeedTier.Sprint;
                break;
            default:
                newtier = SpeedTier.Idle;
                break;
        }
        return newtier;
    }

    
    protected override void SetDestination(NavMeshPath path, Vector3 destination, SpeedTier tier)
    {
        SetSpeedTier(tier);
        //SetAgentTargetSpeed(newSpeed, lerp);
        ToggleAgent(setActive: true);
        if (!Owner.Agent.SetPath(path))
            if (!Owner.Agent.SetDestination(destination)) Debug.LogError("Failed to set path");
                //SetSpeedTier(SpeedTier.Idle); // Later => Send Notification i.e. Stuck
    }
    #endregion


    #region Path Received & Validation
    public override void OnPathRequestComplete(in PathResult result)
    {

        if (result.Id != _currentStateId || result.Reason == PathCheckReason.Cancelled) return;
        bool pathFound = result.PathFound;
        StateId id = result.Id;

        if (result.Reason == PathCheckReason.ProbePathToPrimaryTarget && pathFound)
        { SendNotification(NotifyOwnerNPC.PathToPrimaryAvailable(result.Id)); return; }
    
        if (!result.PathFound) { SendNotification(NotifyOwnerNPC.NoAvailablePath(_currentStateId)); Debug.LogError("NO Path Found!!"); return; }
        else
        {

            _currentDestination = result.Destination;
            _currentDestinaationForward = result.Forward;

            SendNotification(NotifyOwnerNPC.DestinationFound(result.Id, _currentDestination, result.Path));

        }

    }

    public void TrySetDestination(NavMeshPath path, Vector3 newDestination, StateId destinationState)
    {
        if (destinationState != _currentStateId) { SetSpeedTier(SpeedTier.Idle); return; }

        NavMeshObstacle o = Owner.Obstacle;
        if (o.enabled && o.carving)
        {
            Owner.Obstacle.enabled = false;
            // _timer.Add(new SetDestinationDelay(Time.deltaTime + Mathf.Epsilon, newDestination, path, speed, lerp, SetDestination));
            //  CoroutineRunner.Instance.StartCoroutine(DelayEnableroutine(path, newDestination, speed, lerp));
            return;
        }
      //  SetDestination(path, newDestination, speed, lerp);
    }

    public override void DestinationApproval(bool approved, NavMeshPath path, Vector3 newDestination, StateId ApprovalState, float speed, float lerp)
    {
        if (!approved)
        {
            SetAgentTargetSpeed(speed, lerp);
            return;
        }
        ApprovedDestinationStateId = ApprovalState;
        //var (speed, lerp) = Owner.GetSpeedAndLerp(ApprovedDestinationStateId);
        NavMeshObstacle o = Owner.Obstacle;
        if (o.enabled && o.carving)
        {
            Owner.Obstacle.enabled = false;
           // _timer.Add(new SetDestinationDelay(Time.deltaTime + Mathf.Epsilon, newDestination, path, SetDestination));
            //  CoroutineRunner.Instance.StartCoroutine(DelayEnableroutine(path, newDestination, speed, lerp));
            return;
        }
     //   SetDestination(path, newDestination, speed, lerp);
    }
   
    #endregion


    #region Tick Region
    private void SetAgentTargetSpeed(float speed, float lerpSpeed)
    => (_lerpSpeed, _targetSpeed) = (lerpSpeed, speed);

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

    private void CheckRemainingDistance()
    {
        //if (!Owner.Agent.enabled) return;
        if (HasPathAndMoving())
        {
            bool reached = HasReachedDestination();
            if (DestinationReached == reached) return;
            DestinationReached = reached;
            if (DestinationReached)
            {
                TryResetAgent();
                
                
                bool isStaleDestination = ApprovedDestinationStateId != _currentStateId;
                SendNotification(NotifyOwnerNPC.DestinationReached(_currentStateId, isStaleDestination));

            }
            
        }
    }

    protected bool HasPathAndMoving()
    {
        if (Owner.Agent == null || !Owner.Agent.enabled) return false;
        return Owner.Agent.hasPath && !Owner.Agent.pathPending && !Owner.Agent.isStopped;
    }

    void NewChasetick(float remainingDistance)
    {
        if (DestinationReached) return;
       
        if (remainingDistance > Owner.SprintEnterDist) { SetSpeedTier(SpeedTier.Sprint); }
        else if (remainingDistance < Owner.SprintExitDist) { SetSpeedTier(SpeedTier.Walk); }
        else if (_speedTier == SpeedTier.Idle) SetSpeedTier(SpeedTier.Walk);

    }
  
    protected override void ChaseTick(float dt)
    {
        if (!HasPathAndMoving()) return; 
        float remainingDistance = RemainingDistance();

    }

    private float RemainingDistance() => Owner?.Agent?.remainingDistance ?? float.MaxValue;
    
    /*private bool RemainingDistance(out float remaining)
    {
        if (Owner.Agent == null || !Owner.Agent.enabled) { remaining = float.MaxValue; return false; }
        if (Owner.Agent.hasPath && !Owner.Agent.pathPending)
        {
            remaining = Owner.Agent.remainingDistance;
            return true;
        }
            remaining = float.MaxValue;
            return false;
    }*/

    private bool HasReachedDestination() => Owner.Agent.remainingDistance <= (Owner.Agent.stoppingDistance + 0.25f);

    private void FSMTick(float dt)
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
        CheckRemainingDistance();
     
        for (int i = 0; i < _timer.Count; i++)
        {
            var t = _timer[i];
            t.RemainingTime -= dt;

            if (t.RemainingTime <= 0)
            {
                t.OnDone?.Invoke(t.Path, t.Destination, t.SpeedTier);
                _timer.RemoveAt(i);
                i--;
                continue;
            }
            _timer[i] = t;
        }
    }

    private void OnLateTick(float dt) => UpdateAgentSpeed();

    // Used when the Agent is currently carving
    // After uncarving, this delays setting a new destination
    // for 1 frame to give the NavMesh enough time to update
    private struct SetDestinationDelay
    {
        public float RemainingTime;
        public readonly Vector3 Destination;
        public readonly NavMeshPath Path;
        public readonly SpeedTier SpeedTier;
        //public readonly float AgentSpeed;
        //public readonly float Lerp;

        public readonly Action<NavMeshPath, Vector3, SpeedTier> OnDone;

        public SetDestinationDelay(float time, Vector3 dest, NavMeshPath p, SpeedTier tier, Action<NavMeshPath, Vector3, SpeedTier> cb)
        {
            RemainingTime = time;
            Destination = dest;
            Path = p;
            SpeedTier = tier;
            OnDone = cb;
        }
    }
    #endregion


    #region Patrol Region
    private Func<StateId, bool> CancelOrContinueRoutine;
    private Action LookAroundAction;
    private Action<StateId> OnLookAroundComplete;

    public override void LookAroundAndContinue()
    {
        if (_runningRoutine == null)
            _runningRoutine = this.BeginPatrolRoutine(_currentStateId, Owner.Transform, Owner.MinWaitTime, Owner.MaxWaitTime, _currentDestinaationForward, LookAroundAction, CancelOrContinueRoutine, OnLookAroundComplete);
            //_runningRoutine = CoroutineRunner.Instance.StartCoroutine(PatrolWaitRoutine(_currentPatrolPoinfForward));
    }
    private void LookAroundAndContinuePatrol()
    {
        if (_runningRoutine == null)
            _runningRoutine = this.BeginPatrolRoutine(_currentStateId, Owner.Transform, Owner.MinWaitTime, Owner.MaxWaitTime, _currentDestinaationForward, LookAroundAction, CancelOrContinueRoutine, OnLookAroundComplete);
            //_runningRoutine = CoroutineRunner.Instance.StartCoroutine(PatrolWaitRoutine(_currentPatrolPoinfForward));
    }

    private bool CheckState(StateId id)
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

    public override void BeginPatrol(StateId id)
    {
        CancelRunningCoroutine();
        if (id != StateId.Patrol) return;
        if (_currentStateId != id) _currentStateId = id;

        var request = ValidateDestination.GetPatrolPoint(id, Owner, Owner.Path);
  
        _pathFinder?.TryGetDestination(request);
       
    }

    public override bool TrySwitchZone() => _pathFinder.TrySwitchZone();
    #endregion





    public override void BeginChase(StateId id)
    {
        PathCheckReason reason = PathCheckReason.ValidatePathForDestination;
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
        CancelRunningCoroutine();
        _pathFinder?.CancelAll();
    }
    
}
