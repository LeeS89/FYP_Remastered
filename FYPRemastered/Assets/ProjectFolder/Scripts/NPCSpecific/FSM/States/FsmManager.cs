using Npc.API;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class FsmManager : IFsmStateEvents, IFsmController
{
    // Injected Dependancies
    private IReadOnlyDictionary<StateId, IFsmState> _states;
    private FsmManagerServices _deps;
    private SharedFsmStateServices _sharedDeps;

    private IPathNotifications _pathNotifies;
    private IAnimationRequestNotifications _animNotifies;
    // End Injected Dependancies
    //public Notification Notification { get; set; }
    // Used by owning Monobehaviour via interface
    public StateId CurrentState => _currentState?.GetId() ?? StateId.None;
    // End used by owning Monobehaviour
    
    // Actions Invoked from individual states
    public Action<AnimationCue> OnAnimationIntent { get; set; }
   // public Action<Vector3> OnMapDestinationToZone { get; set; }
   // public Vector3? CurrentDestinationForward { get; private set; }
    // End Actions Invoked from individual states

    // Internal Members
    private List<SetDestinationDelay> _timer = new(2);
    private IFsmState _currentState;
    public bool IsInStateTransition { get; private set; } = false;
    private bool _hasValidDestination = false;
    private float _lerpSpeed = 0f;
    private float _targetSpeed = 0f;
    private float _pathCheckTimer;

  //  public bool RotatingToTarget { get; private set; } = false;
    public bool TestPrint { get; set; } = false;

    public int EntityId => throw new NotImplementedException();

    // End internal members

    private const int MaxDestinationAttempts = 5;
    private int _destinationAttemptCounter = 0;

    
    private SpeedTier _currentSpeedTier = SpeedTier.Idle;
    private SpeedOverride _currentSpeedOverride = SpeedOverride.None;
    private RotationOverride _currentRotationOverride = RotationOverride.None;


    public FsmManager(FsmManagerServices deps, SharedFsmStateServices sharedDeps, IReadOnlyDictionary<StateId, IFsmState> states, IPathNotifications pathNotifies, IAnimationRequestNotifications animNotifies = null/*, Notification fsmNotifications*/)
    {
        _deps = deps;
        _sharedDeps = sharedDeps;
        _pathNotifies = pathNotifies;
        _animNotifies = animNotifies;
        //Notification = fsmNotifications;
        _states = states;
        _pathCheckTimer = _deps.Movement.pathStatusInterval;
    }

    public bool StateExists(StateId id) => _states.ContainsKey(id);
   

    #region State Transition & FOV Frequency Updates
    public void SwitchTo(StateId next)
    {
        if (next == CurrentState || next == StateId.None) return; // Allow for none and make current null

        if (_states != null && _states.TryGetValue(next, out var nextstate))
        {
            IsInStateTransition = true;
            _currentState?.ExitState();
            _currentState = nextstate;
            _currentState.EnterState();
            IsInStateTransition = false;
           
        }   // else => Notify state doesnt exist
    }
/*
    private void UpdateFOVFrequency(StateId current)
    {
        AlertPhase phase;
        phase = current switch
        {
            StateId.Patrol => AlertPhase.Idle,
            StateId.Chase or StateId.Flank or StateId.Follow or StateId.Cover => AlertPhase.Alerted,
            StateId.Search => AlertPhase.Suspicious,
            _ => AlertPhase.Idle
        };
        //_fovHandler.SetFOVSweepFrequency(phase);
    }*/

    #endregion

    #region Tick Region

    public void Tick(float dt)
    {
        TimerTicks(dt);
        if (!_hasValidDestination) return;
        _pathCheckTimer -= dt;

        if (_pathCheckTimer <= 0f)
        {
            _pathCheckTimer = _deps.Movement.pathStatusInterval;
            EvaluatePath();
        }
    }
    public void LateTick(float dt)
    {
        UpdateRotation();
        UpdateAgentSpeed();
        _currentState?.LateTick(dt);
    }

    private bool SharedDepsIsNull() => _sharedDeps == null;
    private bool OwnerIsNull() => SharedDepsIsNull() || _sharedDeps.OwnerTransform == null;

    private void UpdateRotation()
    {
        if (_currentRotationOverride == RotationOverride.None
            || /*NullOwnerOrTarget()*/OwnerIsNull()) return;


        if (_sharedDeps.OnTryGetCurrentTarget?.Invoke(out var target) == true)
            _sharedDeps.OwnerTransform.RotateTowards(target.Transform);
        //_sharedDeps.OwnerTransform.RotateTowards(_sharedDeps.GetCurrentTarget?.Invoke().Transform);
    }

    private Vector3[] _corners = new Vector3[64];

  
    public void EvaluatePath()
    {
        if (!_hasValidDestination || _deps == null || _deps.Agent == null) return;
        NavMeshAgent a = _deps.Agent;

        if (a.pathPending) return;
        if (TestPrint)
        {
            Debug.LogError("Running Path Evaluation");
        }
        
        /// Maybe split into 2 separate checks for better clarity
        /// Possibly for !a.isOnNavMesh we could teleport the agent to nearest navmesh point? + Special effects?
        /// if a not enabled, just reset/ repath
        /// if not on navmesh, Send notification - "Lost NavMesh"?
        /// But for now, just repath in both cases
        if (!a.enabled || !a.isOnNavMesh) 
        {
            _hasValidDestination = false;
            TryResetAgent("Reset Called From Not enabled or on navmesh");
            _currentState?.TryRepath(); // Repath instead
            return;
        }

        /// If we enter either above or below block, possibly notify Destination Reached?
        /// Link both blocks to the counter in the SetDestination method?
        /// but only increment counter whenever SetDestination fails 
        if (!a.hasPath || a.pathStatus != NavMeshPathStatus.PathComplete)
        {
            AttemptRepath("Attempt repath called from No path or path not complete");
            return;
        }

       /* // Check if current state needs a new path ( target destination moved, etc)
        if (_current?.NeedsNewPath() ?? false)
        {
            _hasValidDestination = false;
            _current?.TryRepath();
            return;
        }*/

        //var dist = a.remainingDistance;
        var dist = a.GetPathDistance(CurrentState, _corners);
        var rDist = a.remainingDistance;
        if (TestPrint)
        {
            //Debug.LogError($"Path Distance: {dist} | Remaining Distance: {rDist}");
        }
        
       
        float stopThreshold = (a.stoppingDistance + 0.25f);
        if (dist <= stopThreshold)
        {
            DestinationReached();
            return;
        }

        // Check if current state needs a new path ( target destination moved, etc)
        if (_currentState?.NeedsNewPath() ?? false)
        {
            _hasValidDestination = false;
            _currentState?.TryRepath();
            return;
        }

        UpdateSpeedtier(dist);
    }

    private void DestinationReached()
    {
        _hasValidDestination = false;
        TryResetAgent("Reset Called From Destination Reached"); // Resets path and Sets speed == 0f
        //Debug.LogError("Reached Destination");
        _currentState?.OnDestinationReached();

        _pathNotifies?.DestinationReached();
        //Notification?.Invoke(NpcNotification.PathNotifications.DestinationReached());
    }

    private void TimerTicks(float dt)
    {
        if (_timer == null || _timer.Count == 0) return;

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

    #region Destination result & Setting region
    public void RequestAnimation(AnimationCue cue, StateId id)
    {
        if (StateHasChanged(id)) return;
        _animNotifies?.RequestAnimation(cue);
        //Notification?.Invoke(NpcNotification.AnimationNotifications.AnimationIntent(cue));
    }

    private bool StateHasChanged(StateId id) => id != CurrentState;

    public void ProcessDestinationResult(in DestinationResultInfo result)
    {
        Debug.LogError("Destination Result is: "+result.Result.ToString());
        // Debug.LogError("Destination Result Received at source");
        if (StateHasChanged(result.Id) || result.RequestReason == ReasonForDestinationCheck.Cancelled) return;
        DestinationResult pathResult = result.Result;
        //bool pathFound = result.PathFound;
        StateId id = result.Id;

        if (result.RequestReason == ReasonForDestinationCheck.ProbePath && pathResult == DestinationResult.Success)
        { _pathNotifies?.PathToTargetAvailable();/*Notification?.Invoke(NpcNotification.PathNotifications.PathToTargetAvailable());*/ return; }

        if (/*!result.PathFound*/pathResult == DestinationResult.Failed) { NoAvailablePath();/*Notification?.Invoke(NpcNotification.PathNotifications.NoAvailablePath());*/ Debug.LogError("NO Path Found!!"); return; }
        else if(pathResult == DestinationResult.Success)
        {
            Vector3 currentDestination = result.Destination;
           // CurrentDestinationForward = result.Forward;
           // OnMapDestinationToZone?.Invoke(currentDestination);

            NavMeshObstacle o = _deps.Obstacle;
            if (o != null && o.enabled && o.carving)
            {
                _deps.Obstacle.enabled = false;
                _timer.Add(new SetDestinationDelay(Time.deltaTime + Mathf.Epsilon, currentDestination, result.Path, id, SetDestination));
                return;
            }
            SetDestination(result.Path, currentDestination, id);
        }

    }

    protected void SetDestination(NavMeshPath path, Vector3 destination, StateId current)
    {
        if (current != CurrentState) return;
        ToggleAgent(setActive: true);
        if (_deps.Agent.SetPath(path) ||
            _deps.Agent.SetDestination(destination))
        {

            _deps.Agent.stoppingDistance = _currentState?.GetDesiredStoppingDistance() ?? 0f;//_deps.GetAgentStopDistance(true); // NEEDS UPDATING
            DestinationSet(destination);
        }
        else
            AttemptRepath("Attempt repath called from failing to set path or dest");
    }

    private void AttemptRepath(string msg)
    {
#if UNITY_EDITOR
        Debug.LogError("Failed to Set Path - Attempting to Re-path");
        Debug.LogError(msg);
#endif
        // Add counter to prevent infinite loop, and notify if failed after x attempts
        if (++_destinationAttemptCounter <= MaxDestinationAttempts)
        {
            _hasValidDestination = false;
            TryResetAgent("Reset Called From Attempt Repath");
            _currentState?.TryRepath();
        }
        else
        {
            Debug.LogError("Failed to Set Destination after multiple attempts");
            _destinationAttemptCounter = 0;
            NoAvailablePath();
            //NotifyOwner(NpcNotification.PathNotifications.NoAvailablePath());
            //Notification?.Invoke(NpcNotification.PathNotifications.NoAvailablePath());
        }
    }

    private void DestinationSet(Vector3 destination)
    {
        _pathCheckTimer = _deps.Movement.pathStatusInterval;
        _hasValidDestination = true;
        EvaluatePath();
        _currentState?.OnDestinationSet();
        _pathNotifies?.DestinationSet(destination);
        //NotifyOwner(NpcNotification.PathNotifications.DestinationSet(destination));
        //Notification?.Invoke(NpcNotification.PathNotifications.DestinationSet());
    }

    private void NoAvailablePath()
        => _pathNotifies?.NoAvailablePath();

  /*  private void NotifyOwners(in NpcNotification n)
        => Notification?.Invoke(n); */

    protected void ToggleAgent(bool setActive)
    {
        if (_deps.Agent.enabled == setActive) return;
        _deps.Agent.enabled = setActive;
    }
    #endregion

    #region Speed Region
    public bool HasReachedDestination() => !_hasValidDestination || (_deps?.Agent.isStopped ?? true);
    

    private void UpdateAgentSpeed()
    {
        /*var a = Owner.Agent; if (!a) return;
        float delta = Mathf.Abs(_targetSpeed - a.speed);
        float rate = (0.5f > 0f) ? Mathf.Max(0.01f, delta / 0.5f) : float.PositiveInfinity;
        a.speed = Mathf.MoveTowards(a.speed, _targetSpeed, rate * Time.deltaTime);*/

        if (_deps.Agent == null) return;
        float smoothedSpeed = Mathf.Lerp(_deps.Agent.speed, _targetSpeed, _lerpSpeed * Time.deltaTime);
        _deps.Agent.speed = smoothedSpeed;

        float _currentSpeed = _deps.Agent.speed;

        if (Mathf.Approximately(_deps.Agent.speed, _targetSpeed)) _deps.Agent.speed = _targetSpeed;
    }

    public void OverrideSpeed(SpeedOverride overrideTier)
     => _currentSpeedOverride = overrideTier;


    private void UpdateSpeedtier(float remainingDistance)
    {
        if (_deps == null || _deps.Agent == null || _deps.Agent.isStopped) return;

        float speed;
        float lerp;
        SpeedTier tier;

        tier = TryUpdateAgentTargetSpeed(_currentSpeedTier, _currentSpeedOverride, remainingDistance, out speed, out lerp);

        if (tier == _currentSpeedTier) return;

        _currentSpeedTier = tier;
        (_targetSpeed, _lerpSpeed) = (speed, lerp);

    }

    private SpeedTier OverrideSpeed(SpeedOverride speedOverride, out float newSpeed, out float lerp)
    {
        SpeedTier newTier;
        var d = _deps.Movement;

        (newTier, newSpeed, lerp) = speedOverride switch
        {
            SpeedOverride.ForceWalk =>
            (
                SpeedTier.Walk,
                newSpeed = d.walkSpeed,
                lerp = 2f
            ),
            SpeedOverride.ForceSprint =>
            (
                SpeedTier.Sprint,
                newSpeed = d.sprintSpeed,
                lerp = 2f
            ),
            SpeedOverride.ForceIdle =>
            (
                SpeedTier.Idle,
                newSpeed = 0f,
                lerp = 10f
            ),
            _ =>
            (
                SpeedTier.Walk,
                newSpeed = d.walkSpeed,
                lerp = 2f
            )

        };
        return newTier;


    }


    public SpeedTier TryUpdateAgentTargetSpeed(SpeedTier currentTier, SpeedOverride speedOverride, float distanceToDestination, out float newSpeed, out float lerp)
    {
        if (distanceToDestination <= 0.25f)
        {
            newSpeed = 0f;
            lerp = 10f;
            return SpeedTier.Idle;
        }

        if (speedOverride != SpeedOverride.None)
            return OverrideSpeed(speedOverride, out newSpeed, out lerp);

        var d = _deps.Movement;

        if (distanceToDestination > d.sprintEnterDistance)
        {
            newSpeed = d.sprintSpeed;
            lerp = 2f;
            return SpeedTier.Sprint;
        }
        else if (distanceToDestination < d.sprintExitDistance)
        {
            newSpeed = d.walkSpeed;
            lerp = 2f;
            return SpeedTier.Walk;
        }
        else
        {
            if (currentTier == SpeedTier.Idle)
            {
                newSpeed = d.walkSpeed;
                lerp = 2f;
                return SpeedTier.Walk;
            }
            newSpeed = d.sprintSpeed;
            lerp = 2f;
            return currentTier;
        }


    }

    #endregion


    private void TryResetAgent(string msg)
    {
        if (msg != null/* && TestPrint*/)
        {
            Debug.LogError(msg);
        }
        _hasValidDestination = false;
        UpdateSpeedtier(0f);
        _deps?.Agent.ResetPath();
        if (CurrentState == StateId.Patrol) return;
        ToggleAgent(false);
        _deps.Obstacle.enabled = true;
    }

    public void Reset()
    {
        _hasValidDestination = false;
        UpdateSpeedtier(0f);
        _deps?.Agent.ResetPath();
        if (CurrentState == StateId.Patrol) return;
        ToggleAgent(false);
        _deps.Obstacle.enabled = true;
    }



    public void OverrideRotation(RotationOverride rotOverride)
    {
        if (_currentRotationOverride == rotOverride) return;
        _deps.Agent.updateRotation = rotOverride == RotationOverride.None;

        if(TestPrint) Debug.LogError($"Setting Rotation Override to {rotOverride}");
        _currentRotationOverride = rotOverride;
    }

  /*  private bool NullOwnerOrTarget() => _sharedDeps.GetCurrentTarget == null || _sharedDeps.GetCurrentTarget?.Invoke().Transform == null *//*|| _deps.Owner == null*//*
            || _sharedDeps.OwnerTransform == null || _deps.Agent == null;*/

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void RequestRotation(float requestedAngle, StateId id, Action<bool> onComplete)
    {
        throw new NotImplementedException();
    }

    public void Test()
    {
        throw new NotImplementedException();
    }

    public bool TryGetTargetPosition(out Vector3? targetPos)
    {
        throw new NotImplementedException();
    }









    // Used when the Agent is currently carving
    // After uncarving, this delays setting a new destination
    // for 1 frame to give the NavMesh enough time to update
    private struct SetDestinationDelay
    {
        public float RemainingTime;
        public readonly Vector3 Destination;
        public readonly NavMeshPath Path;
        public readonly StateId Current;

        public readonly Action<NavMeshPath, Vector3, StateId> OnDone;

        public SetDestinationDelay(float time, Vector3 dest, NavMeshPath p, StateId c, Action<NavMeshPath, Vector3, StateId> cb)
        {
            RemainingTime = time;
            Destination = dest;
            Path = p;
            Current = c;
            OnDone = cb;
        }
    }

}



public class FsmManagerServices
{
    public NavMeshAgent Agent { get; }
    public NavMeshObstacle Obstacle { get; }
    public MovementConfig Movement { get; }

    public FsmManagerServices(NavMeshAgent agent, NavMeshObstacle obstacle, MovementConfig config)
    {
        Agent = agent;
        Obstacle = obstacle;
        Movement = config;
    }


}

















































public class FsmManagerNew : IFsmStateEvents, IFsmController
{
    // Injected Dependancies
    private IReadOnlyDictionary<StateId, IFsmState> _states;
   // private FsmManagerServices _deps;
    //private SharedFsmStateServices _sharedDeps;

    private IPathNotifications _pathNotifies;
    private IAnimationRequestNotifications _animNotifies;
    // End Injected Dependancies
    //public Notification Notification { get; set; }
    // Used by owning Monobehaviour via interface
    public StateId CurrentState => _currentState?.GetId() ?? StateId.None;
    // End used by owning Monobehaviour

    // Actions Invoked from individual states
    public Action<AnimationCue> OnAnimationIntent { get; set; }
    // public Action<Vector3> OnMapDestinationToZone { get; set; }
    // public Vector3? CurrentDestinationForward { get; private set; }
    // End Actions Invoked from individual states

    // Internal Members
    private List<SetDestinationDelay> _timer = new(2);
    private IFsmStateNew _currentState;
    public bool IsInStateTransition { get; private set; } = false;
    private bool _hasValidDestination = false;
    private float _lerpSpeed = 0f;
    private float _targetSpeed = 0f;
    private float _pathCheckTimer;

    //  public bool RotatingToTarget { get; private set; } = false;
    public bool TestPrint { get; set; } = false;

    private readonly int _instanceId;
    public int EntityId => _instanceId;

    // End internal members

    private const int MaxDestinationAttempts = 5;
    private int _destinationAttemptCounter = 0;


    private SpeedTier _currentSpeedTier = SpeedTier.Idle;
    private SpeedOverride _currentSpeedOverride = SpeedOverride.None;
    private RotationOverride _currentRotationOverride = RotationOverride.None;

    // NEW
    private readonly ICoroutineHost _routineHost;
    private readonly ITickableGroup _tickHost;

   // private readonly IFsmNavigationControl _navControl;
    private readonly IFsmTargetQuery _targetQuery;
    private readonly IReadOnlyDictionary<StateId, IFsmStateNew> _statesNew;

    public FsmManagerNew(int instanceId, IFsmNavigationControl navControl, IFsmTargetQuery targetQuery, IReadOnlyDictionary<StateId, IFsmStateNew> states,
        ICoroutineHost host, ITickableGroup tickHost, IPathNotifications pathNotifies, IAnimationRequestNotifications animNotifies = null)
    {
        _instanceId = instanceId;
       // _navControl = navControl;
        _targetQuery = targetQuery;
        _statesNew = states;

        _routineHost = host;
        _tickHost = tickHost;
        _pathNotifies = pathNotifies;
        _animNotifies = animNotifies;

        
        _tickHost?.Register(this);
    }

    // newest
    private readonly INpcBody _owner;
    private readonly TryGetTarget _ownerTargetGetter;

    public FsmManagerNew(INpcBody owner, TryGetTarget ownerTargetGetter)
    {
        _owner = owner;
        _ownerTargetGetter = ownerTargetGetter;
    }

    // END NEW

   /* public FsmManagerNew(FsmManagerServices deps, SharedFsmStateServices sharedDeps, IReadOnlyDictionary<StateId, IFsmState> states, IPathNotifications pathNotifies, IAnimationRequestNotifications animNotifies = null*//*, Notification fsmNotifications*//*)
    {
        _deps = deps;
        _sharedDeps = sharedDeps;
        _pathNotifies = pathNotifies;
        _animNotifies = animNotifies;
        //Notification = fsmNotifications;
        _states = states;
        _pathCheckTimer = _deps.Movement.pathStatusInterval;
    }*/

    public bool StateExists(StateId id) => _states.ContainsKey(id);


    #region State Transition & FOV Frequency Updates
    public void SwitchTo(StateId next)
    {
        if (next == CurrentState || next == StateId.None) return; // Allow for none and make current null

        if (_statesNew != null && _statesNew.TryGetValue(next, out var nextstate))
        {
            DebugLogs.Err("Calling enter State", this);
            IsInStateTransition = true;
            _currentState?.ExitState();
            _currentState = nextstate;
            _currentState.EnterState();
            IsInStateTransition = false;

        }   // else => Notify state doesnt exist
    }
    /*
        private void UpdateFOVFrequency(StateId current)
        {
            AlertPhase phase;
            phase = current switch
            {
                StateId.Patrol => AlertPhase.Idle,
                StateId.Chase or StateId.Flank or StateId.Follow or StateId.Cover => AlertPhase.Alerted,
                StateId.Search => AlertPhase.Suspicious,
                _ => AlertPhase.Idle
            };
            //_fovHandler.SetFOVSweepFrequency(phase);
        }*/

    #endregion

    #region Tick Region

    public void Tick(float dt)
    {
        TimerTicks(dt);
        if (!_hasValidDestination) return;
        _pathCheckTimer -= dt;

        if (_pathCheckTimer <= 0f)
        {
           // _pathCheckTimer = _deps.Movement.pathStatusInterval; // COMMENTED OUT FOR NOW
            EvaluatePath();
        }
    }
    public void LateTick(float dt)
    {
        UpdateRotation();
        UpdateAgentSpeed();
        _currentState?.LateTick(dt);
    }

  //  private bool SharedDepsIsNull() => _sharedDeps == null;
  //  private bool OwnerIsNull() => SharedDepsIsNull() || _sharedDeps.OwnerTransform == null;

    private void UpdateRotation()
    {
        if (_currentRotationOverride == RotationOverride.None)
            /*|| *//*NullOwnerOrTarget()*//*OwnerIsNull()*/ return;


        if (Transform is null ||
            !TryGetTargetTransform(out var tt)) return;

        Transform.RotateTowards(tt);

       /* if (_sharedDeps.OnTryGetCurrentTarget?.Invoke(out var target) == true)
            _sharedDeps.OwnerTransform.RotateTowards(target.Transform);*/
        //_sharedDeps.OwnerTransform.RotateTowards(_sharedDeps.GetCurrentTarget?.Invoke().Transform);
    }

    public void RequestRotation(float requestedAngle, StateId id, Action<bool> onComplete)
    {
        if (onComplete == null) return;
        if (Transform == null) return;
      //  if (!TryGetOwnerTransform(out var t)) return;
        _routineHost?.StartCoroutine(RotateRoutine(Transform, requestedAngle, id, onComplete));
        //CoroutineRunner.Instance.StartCoroutine(RotateRoutine(t, requestedAngle, id, onComplete));  
    }

    private IEnumerator RotateRoutine(Transform owner, float angle, StateId id, Action<bool> onComplete)
    {
        Vector3 dirOffset = Quaternion.AngleAxis(angle, owner.up) * owner.forward;
        Quaternion targetRot = Quaternion.LookRotation(dirOffset, owner.up);

        while (Quaternion.Angle(owner.rotation, targetRot) > 2.0f + Mathf.Epsilon)
        {
            owner.rotation = Quaternion.Slerp(owner.rotation, targetRot, Time.deltaTime * 2f);

            if (StateHasChanged(id))
            {
                onComplete.Invoke(false);
                yield break;
            }
            yield return null;
        }

        onComplete?.Invoke(true);
    }

    private Vector3[] _corners = new Vector3[64];

   /* private bool TryGetOwnerTransform(out Transform t) => _navControl.TryGetOwnerTransform(this, out t);
    private bool TryGetAgent(out NavMeshAgent agent) => _navControl.TryGetAgent(this, out agent);
    private bool TryGetObstacle(out NavMeshObstacle obstacle) => _navControl.TryGetObstacle(this, out obstacle);*/
    private bool TryGetTargetTransform(out Transform t) => _targetQuery.TryGetTargetTransform(this, out t); 

    private Transform Transform => _owner.Transform;
    private Vector3? Position => _owner.Position() ?? _owner.Transform?.position;
    private NavMeshAgent Agent => _owner.Agent;
    private NavMeshObstacle Obstacle => _owner.Obstacle;
    public NavMeshPath Path => _owner.Path;
    public void EvaluatePath()
    {
        if (!_hasValidDestination) return;// || _deps == null || _deps.Agent == null) return;
        if (Agent == null) return;
        //if (!TryGetAgent(out var a)) return;

      //  NavMeshAgent a = _deps.Agent;

        if (Agent.pathPending) return;
        if (TestPrint)
        {
            Debug.LogError("Running Path Evaluation");
        }

        /// Maybe split into 2 separate checks for better clarity
        /// Possibly for !a.isOnNavMesh we could teleport the agent to nearest navmesh point? + Special effects?
        /// if a not enabled, just reset/ repath
        /// if not on navmesh, Send notification - "Lost NavMesh"?
        /// But for now, just repath in both cases
        if (!Agent.enabled || !Agent.isOnNavMesh)
        {
            _hasValidDestination = false;
            TryResetAgent("Reset Called From Not enabled or on navmesh");
            _currentState?.TryRepath(); // Repath instead
            return;
        }

        /// If we enter either above or below block, possibly notify Destination Reached?
        /// Link both blocks to the counter in the SetDestination method?
        /// but only increment counter whenever SetDestination fails 
        if (!Agent.hasPath || Agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            AttemptRepath("Attempt repath called from No path or path not complete");
            return;
        }

        /* // Check if current state needs a new path ( target destination moved, etc)
         if (_current?.NeedsNewPath() ?? false)
         {
             _hasValidDestination = false;
             _current?.TryRepath();
             return;
         }*/

        //var dist = a.remainingDistance;
        var dist = Agent.GetPathDistance(CurrentState, _corners);
        var rDist = Agent.remainingDistance;
        if (TestPrint)
        {
            //Debug.LogError($"Path Distance: {dist} | Remaining Distance: {rDist}");
        }


        float stopThreshold = (Agent.stoppingDistance + 0.25f);
        if (dist <= stopThreshold)
        {
            DestinationReached();
            return;
        }

        // Check if current state needs a new path ( target destination moved, etc)
        if (_currentState?.NeedsNewPath() ?? false)
        {
            _hasValidDestination = false;
            _currentState?.TryRepath();
            return;
        }

        UpdateSpeedtier(dist, agent: Agent);
    }

    private void DestinationReached()
    {
        _hasValidDestination = false;
        TryResetAgent("Reset Called From Destination Reached"); // Resets path and Sets speed == 0f
        //Debug.LogError("Reached Destination");
        _currentState?.OnDestinationReached();

        _pathNotifies?.DestinationReached();
        //Notification?.Invoke(NpcNotification.PathNotifications.DestinationReached());
    }

    private void TimerTicks(float dt)
    {
        if (_timer == null || _timer.Count == 0) return;

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

    #region Destination result & Setting region
    public void RequestAnimation(AnimationCue cue, StateId id)
    {
        if (StateHasChanged(id)) return;
        _animNotifies?.RequestAnimation(cue);
        //Notification?.Invoke(NpcNotification.AnimationNotifications.AnimationIntent(cue));
    }

    private bool StateHasChanged(StateId id) => id != CurrentState;
    
    public void ProcessDestinationResult(in DestinationResultInfo result)
    {
        Debug.LogError("Destination Result is: " + result.Result.ToString());
        // Debug.LogError("Destination Result Received at source");
        if (StateHasChanged(result.Id) || result.RequestReason == ReasonForDestinationCheck.Cancelled) return;
        DestinationResult pathResult = result.Result;
        //bool pathFound = result.PathFound;
        StateId id = result.Id;

        if (result.RequestReason == ReasonForDestinationCheck.ProbePath && pathResult == DestinationResult.Success)
        { _pathNotifies?.PathToTargetAvailable();/*Notification?.Invoke(NpcNotification.PathNotifications.PathToTargetAvailable());*/ return; }

        if (/*!result.PathFound*/pathResult == DestinationResult.Failed) { NoAvailablePath();/*Notification?.Invoke(NpcNotification.PathNotifications.NoAvailablePath());*/ Debug.LogError("NO Path Found!!"); return; }
        else if (pathResult == DestinationResult.Success)
        {
            Vector3 currentDestination = result.Destination;
            // CurrentDestinationForward = result.Forward;
            // OnMapDestinationToZone?.Invoke(currentDestination);

         //   NavMeshObstacle o = _deps.Obstacle;

            if (Obstacle != null)
            {
                if (Obstacle != null && Obstacle.enabled && Obstacle.carving)
                {
                    ToggleObstacle(false, Obstacle);
                   // o.enabled = false;
                    //_deps.Obstacle.enabled = false;
                    _timer.Add(new SetDestinationDelay(Time.deltaTime + Mathf.Epsilon, currentDestination, result.Path, id, SetDestination));
                    return;
                }
            }
            SetDestination(result.Path, currentDestination, id);
        }

    }

    protected void SetDestination(NavMeshPath path, Vector3 destination, StateId current)
    {
        if (current != CurrentState) return;

        if (Agent is not null)
        {
            ToggleAgent(setActive: true, Agent);
            if (Agent.SetPath(path) ||
                Agent.SetDestination(destination))
            {

                Agent.stoppingDistance = _currentState?.GetDesiredStoppingDistance() ?? 0f;//_deps.GetAgentStopDistance(true); // NEEDS UPDATING
                DestinationSet(destination);
            }
            else
                AttemptRepath("Attempt repath called from failing to set path or dest");
        }
    }

    private void AttemptRepath(string msg)
    {
#if UNITY_EDITOR
        Debug.LogError("Failed to Set Path - Attempting to Re-path");
        Debug.LogError(msg);
#endif
        // Add counter to prevent infinite loop, and notify if failed after x attempts
        if (++_destinationAttemptCounter <= MaxDestinationAttempts)
        {
            _hasValidDestination = false;
            TryResetAgent("Reset Called From Attempt Repath");
            _currentState?.TryRepath();
        }
        else
        {
            Debug.LogError("Failed to Set Destination after multiple attempts");
            _destinationAttemptCounter = 0;
            NoAvailablePath();
            //NotifyOwner(NpcNotification.PathNotifications.NoAvailablePath());
            //Notification?.Invoke(NpcNotification.PathNotifications.NoAvailablePath());
        }
    }

    private void DestinationSet(Vector3 destination)
    {
        //_pathCheckTimer = _deps.Movement.pathStatusInterval; // COMMENTED OUT FOR NOW
        _hasValidDestination = true;
        EvaluatePath();
        _currentState?.OnDestinationSet();
        _pathNotifies?.DestinationSet(destination);
        //NotifyOwner(NpcNotification.PathNotifications.DestinationSet(destination));
        //Notification?.Invoke(NpcNotification.PathNotifications.DestinationSet());
    }

    private void NoAvailablePath()
        => _pathNotifies?.NoAvailablePath();

    /*  private void NotifyOwners(in NpcNotification n)
          => Notification?.Invoke(n); */

    protected void ToggleAgent(bool setActive, NavMeshAgent agent = null)
    {
        if(agent != null)
        {
            if(agent.enabled != setActive) { agent.enabled = setActive; }
            return;
        }

        //if (!TryGetAgent(out var a)) return;
        if (Agent == null) return;
        if (Agent.enabled == setActive) return;
        Agent.enabled = setActive;
        /*if (_deps.Agent.enabled == setActive) return;
        _deps.Agent.enabled = setActive;*/
    }
    #endregion

    #region Speed Region
   // public bool HasReachedDestination() => !_hasValidDestination || (_deps?.Agent.isStopped ?? true);


    private void UpdateAgentSpeed()
    {
        /*var a = Owner.Agent; if (!a) return;
        float delta = Mathf.Abs(_targetSpeed - a.speed);
        float rate = (0.5f > 0f) ? Mathf.Max(0.01f, delta / 0.5f) : float.PositiveInfinity;
        a.speed = Mathf.MoveTowards(a.speed, _targetSpeed, rate * Time.deltaTime);*/

        if (Agent == null) return;

      //  if (_deps.Agent == null) return;
        float smoothedSpeed = Mathf.Lerp(Agent.speed, _targetSpeed, _lerpSpeed * Time.deltaTime);
        Agent.speed = smoothedSpeed;

        float _currentSpeed = Agent.speed;

        if (Mathf.Approximately(Agent.speed, _targetSpeed)) Agent.speed = _targetSpeed;
    }

    public void OverrideSpeed(SpeedOverride overrideTier)
     => _currentSpeedOverride = overrideTier;


    private void UpdateSpeedtier(float remainingDistance, NavMeshAgent agent)
    {
        if (agent == null || agent.isStopped) return;
        //if (_deps == null || _deps.Agent == null || _deps.Agent.isStopped) return;

        float speed;
        float lerp;
        SpeedTier tier;

        tier = TryUpdateAgentTargetSpeed(_currentSpeedTier, _currentSpeedOverride, remainingDistance, out speed, out lerp);

        if (tier == _currentSpeedTier) return;

        _currentSpeedTier = tier;
        (_targetSpeed, _lerpSpeed) = (speed, lerp);

    }

    private SpeedTier OverrideSpeed(SpeedOverride speedOverride, out float newSpeed, out float lerp)
    {
        SpeedTier newTier = SpeedTier.Walk;
        /*  var d = _deps.Movement;

          (newTier, newSpeed, lerp) = speedOverride switch
          {
              SpeedOverride.ForceWalk =>
              (
                  SpeedTier.Walk,
                  newSpeed = d.walkSpeed,
                  lerp = 2f
              ),
              SpeedOverride.ForceSprint =>
              (
                  SpeedTier.Sprint,
                  newSpeed = d.sprintSpeed,
                  lerp = 2f
              ),
              SpeedOverride.ForceIdle =>
              (
                  SpeedTier.Idle,
                  newSpeed = 0f,
                  lerp = 10f
              ),
              _ =>
              (
                  SpeedTier.Walk,
                  newSpeed = d.walkSpeed,
                  lerp = 2f
              )

          };*/
        newSpeed = 0f; // Placeholder
        lerp = 0f; // Placeholder
        return newTier;


    }


    public SpeedTier TryUpdateAgentTargetSpeed(SpeedTier currentTier, SpeedOverride speedOverride, float distanceToDestination, out float newSpeed, out float lerp)
    {
        if (distanceToDestination <= 0.25f)
        {
            newSpeed = 0f;
            lerp = 10f;
            return SpeedTier.Idle;
        }

        if (speedOverride != SpeedOverride.None)
            return OverrideSpeed(speedOverride, out newSpeed, out lerp);

      /*  var d = _deps.Movement;

        if (distanceToDestination > d.sprintEnterDistance)
        {
            newSpeed = d.sprintSpeed;
            lerp = 2f;
            return SpeedTier.Sprint;
        }
        else if (distanceToDestination < d.sprintExitDistance)
        {
            newSpeed = d.walkSpeed;
            lerp = 2f;
            return SpeedTier.Walk;
        }
        else
        {
            if (currentTier == SpeedTier.Idle)
            {
                newSpeed = d.walkSpeed;
                lerp = 2f;
                return SpeedTier.Walk;
            }
            newSpeed = d.sprintSpeed;
            lerp = 2f;
            return currentTier;
        }*/
      newSpeed = 0f; // Placeholder
        lerp = 0f; // Placeholder
        return SpeedTier.Idle;// Placeholder

    }

    #endregion


    private void TryResetAgent(string msg)
    {
        if (msg != null/* && TestPrint*/)
        {
            Debug.LogError(msg);
        }
        
        _hasValidDestination = false;
        

        //TryGetAgent(out var agent);
        UpdateSpeedtier(0f, Agent);
        ResetPath(Agent);
       // if (TryGetAgent(out var a)) a.ResetPath();
        //_deps?.Agent.ResetPath();
        if (CurrentState == StateId.Patrol) return;
        ToggleAgent(false);

        ToggleObstacle(true);
       // if (TryGetObstacle(out var o)) o.enabled = true;
        //_deps.Obstacle.enabled = true;
    }

    private void ToggleObstacle(bool enabled, NavMeshObstacle obstacle = null)
    {
        if(obstacle != null)
        {
            if(obstacle.enabled != enabled) { obstacle.enabled = enabled; }
            return;
        }

        if (Obstacle == null) return;
        if(Obstacle.enabled != enabled) Obstacle.enabled = enabled;
    }

    private void ResetPath(NavMeshAgent agent = null)
    {
        if(agent != null)
        {
            if (agent.hasPath) agent.ResetPath();
            return;
        }

        if (Agent == null) return;
        if(Agent.hasPath) Agent.ResetPath();
    }

    public void Reset()
    {
        _hasValidDestination = false;

    
        UpdateSpeedtier(0f, Agent);

        ResetPath(Agent);
      //  _deps?.Agent.ResetPath();
        if (CurrentState == StateId.Patrol) return;
        ToggleAgent(false, Agent);
        ToggleObstacle(enabled: true);
       // _deps.Obstacle.enabled = true;
    }



    public void OverrideRotation(RotationOverride rotOverride)
    {
        if (_currentRotationOverride == rotOverride) return;
        if (Agent == null) return;

        Agent.updateRotation = rotOverride == RotationOverride.None;
        //_deps.Agent.updateRotation = rotOverride == RotationOverride.None;

        if (TestPrint) Debug.LogError($"Setting Rotation Override to {rotOverride}");
        _currentRotationOverride = rotOverride;
    }

    /*  private bool NullOwnerOrTarget() => _sharedDeps.GetCurrentTarget == null || _sharedDeps.GetCurrentTarget?.Invoke().Transform == null *//*|| _deps.Owner == null*//*
              || _sharedDeps.OwnerTransform == null || _deps.Agent == null;*/

    public void Dispose()
    {
        
        throw new NotImplementedException();
    }

    public bool HasReachedDestination()
    => !_hasValidDestination;

    void IFsmStateEvents.Test()
    {
        throw new NotImplementedException();
    }

    bool IFsmStateEvents.TryGetTargetPosition(out Vector3? targetPos)
    {
        targetPos = null;
        if (!TryGetTarget(out var target)) return false;
        targetPos = target.Position();
        return targetPos is not null;
    }


    private bool TryGetTarget(out ITargetable target)
    {
        target = null;
        if (_ownerTargetGetter is null) return false;
        if (!_ownerTargetGetter.Invoke(out target)) return false;
        return target is not null;
    }








    // Used when the Agent is currently carving
    // After uncarving, this delays setting a new destination
    // for 1 frame to give the NavMesh enough time to update
    private struct SetDestinationDelay
    {

        public float RemainingTime;
        public readonly Vector3 Destination;
        public readonly NavMeshPath Path;
        public readonly StateId Current;

        public readonly Action<NavMeshPath, Vector3, StateId> OnDone;

        public SetDestinationDelay(float time, Vector3 dest, NavMeshPath p, StateId c, Action<NavMeshPath, Vector3, StateId> cb)
        {
            RemainingTime = time;
            Destination = dest;
            Path = p;
            Current = c;
            OnDone = cb;
        }
    }

}

public interface TestingInting
{
    void Howdy();
}

public interface TestingOuting : TestingInting
{

}

public class RunnerOne : TestingOuting
{
    void TestingInting.Howdy()
    {
        throw new NotImplementedException();
    }
}