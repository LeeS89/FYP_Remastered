using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public /*partial*/ class FSMBaseNew : IFSMControl
{
    // Injected Dependancies
    private IReadOnlyDictionary<StateId, IFSMState> _states;
    private IFsmControllerDeps _deps;
    // End Injected Dependancies
    public Notification Notification { get; set; }
    // Used by owning Monobehaviour via interface
    public StateId CurrentStateId => _current?.GetId() ?? StateId.None;
    // End used by owning Monobehaviour
    
    // Actions Invoked from individual states
    public Action<AnimationCue> OnAnimationIntent { get; set; }
    public Action<Vector3> OnMapDestinationToZone { get; set; }
    public Vector3? CurrentDestinationForward { get; private set; }
    // End Actions Invoked from individual states

    // Internal Members
    private List<SetDestinationDelay> _timer = new(2);
    private IFSMState _current;
    public bool IsInStateTransition { get; private set; } = false;
    private bool _hasValidDestination = false;
    private float _lerpSpeed = 0f;
    private float _targetSpeed = 0f;
    private float _pathCheckTimer;

  //  public bool RotatingToTarget { get; private set; } = false;
    public bool TestPrint { get; set; } = false;

    // End internal members

    private const int MaxDestinationAttempts = 5;
    private int _destinationAttemptCounter = 0;

    
    private SpeedTier _currentSpeedTier = SpeedTier.Idle;
    private SpeedOverride _currentSpeedOverride = SpeedOverride.None;
    private RotationOverride _currentRotationOverride = RotationOverride.None;


    public FSMBaseNew(IFsmControllerDeps deps, IReadOnlyDictionary<StateId, IFSMState> states, Notification fsmNotifications)
    {
        _deps = deps;
        Notification = fsmNotifications;
        _states = states;
        _pathCheckTimer = _deps.PathStatusInterval;
    }

    #region State Transition & FOV Frequency Updates
    public void SwitchTo(StateId next)
    {
        if (next == CurrentStateId || next == StateId.None) return; // Allow for none and make current null

        if (_states != null && _states.TryGetValue(next, out var nextstate))
        {
            IsInStateTransition = true;
            _current?.ExitState();
            _current = nextstate;
            _current.EnterState();
            IsInStateTransition = false;
           // UpdateFOVFrequency(CurrentStateId);
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
/*
    private void FieldOfViewSweepResult(FOVResult result, bool withinAttackAngles)
        => Notification?.Invoke(NPCNotification.FOVUpdate(*//*CurrentStateId, *//*result, withinAttackAngles));*/
    #endregion

    #region Tick Region

    public void Tick(float dt)
    {
        TimerTicks(dt);
        if (!_hasValidDestination) return;
        _pathCheckTimer -= dt;

        if (_pathCheckTimer <= 0f)
        {
            _pathCheckTimer = _deps.PathStatusInterval;
            EvaluatePath();
        }
    }
    public void LateTick(float dt)
    {
        UpdateRotation();
        UpdateAgentSpeed();
        _current?.LateTick(dt);
    }

    private void UpdateRotation()
    {
        if (_currentRotationOverride == RotationOverride.None
            || NullOwnerOrTarget()) return;

        _deps.Owner.RotateTowards(_deps.Target);
    }

    private Vector3[] _corners = new Vector3[64];

  
    public void EvaluatePath()
    {
        if (!_hasValidDestination || _deps == null || _deps.Agent() == null) return;
        NavMeshAgent a = _deps.Agent();

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
            _current?.TryRepath(); // Repath instead
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
        var dist = a.GetPathDistance(CurrentStateId, _corners);
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
        if (_current?.NeedsNewPath() ?? false)
        {
            _hasValidDestination = false;
            _current?.TryRepath();
            return;
        }

        UpdateSpeedtier(dist);
    }

    private void DestinationReached()
    {
        _hasValidDestination = false;
        TryResetAgent("Reset Called From Destination Reached"); // Resets path and Sets speed == 0f
        //Debug.LogError("Reached Destination");
        _current?.OnDestinationReached();
        Notification?.Invoke(NpcNotification.DestinationReached());
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
    private bool StateHasChanged(StateId id) => id != CurrentStateId;

    public void OnDestinationResultReceived(in DestinationResultNew result)
    {
       // Debug.LogError("Destination Result Received at source");
        if (StateHasChanged(result.Id) || result.Reason == ReasonForDestinationCheck.Cancelled) return;
        PathResult pathResult = result.PathResult;
        //bool pathFound = result.PathFound;
        StateId id = result.Id;

        if (result.Reason == ReasonForDestinationCheck.ProbePath && pathResult == PathResult.Success)
        { Notification?.Invoke(NpcNotification.PathToPrimaryAvailable(/*result.Id*/)); return; }

        if (/*!result.PathFound*/pathResult == PathResult.Failed) { Notification?.Invoke(NpcNotification.NoAvailablePath(/*CurrentStateId*/)); Debug.LogError("NO Path Found!!"); return; }
        else if(pathResult == PathResult.Success)
        {
            Vector3 currentDestination = result.Destination;
            CurrentDestinationForward = result.Forward;
            OnMapDestinationToZone?.Invoke(currentDestination);

            NavMeshObstacle o = _deps.Obstacle();
            if (o != null && o.enabled && o.carving)
            {
                _deps.Obstacle().enabled = false;
                _timer.Add(new SetDestinationDelay(Time.deltaTime + Mathf.Epsilon, currentDestination, result.Path, id, SetDestination));
                return;
            }
            SetDestination(result.Path, currentDestination, id);
        }

    }

    protected void SetDestination(NavMeshPath path, Vector3 destination, StateId current)
    {
        if (current != CurrentStateId) return;
        ToggleAgent(setActive: true);
        if (_deps.Agent().SetPath(path) ||
            _deps.Agent().SetDestination(destination))
        {

            _deps.Agent().stoppingDistance = _deps.GetAgentStopDistance(_current.UsesRandomAgentStopDistance);
            DestinationSet();
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
            _current?.TryRepath();
        }
        else
        {
            Debug.LogError("Failed to Set Destination after multiple attempts");
            _destinationAttemptCounter = 0;
            Notification?.Invoke(NpcNotification.NoAvailablePath());
        }
    }

    private void DestinationSet()
    {
        _pathCheckTimer = _deps.PathStatusInterval;
        _hasValidDestination = true;
        EvaluatePath();
        _current?.OnDestinationSet();
        Notification?.Invoke(NpcNotification.DestinationSet());
    }

    protected void ToggleAgent(bool setActive)
    {
        if (_deps.Agent().enabled == setActive) return;
        _deps.Agent().enabled = setActive;
    }
    #endregion

    #region Speed Region
    public bool HasReachedDestination() => !_hasValidDestination || (_deps?.Agent().isStopped ?? true);//_speedTier == SpeedTier.Idle;
    

    private void UpdateAgentSpeed()
    {
        /*var a = Owner.Agent; if (!a) return;
        float delta = Mathf.Abs(_targetSpeed - a.speed);
        float rate = (0.5f > 0f) ? Mathf.Max(0.01f, delta / 0.5f) : float.PositiveInfinity;
        a.speed = Mathf.MoveTowards(a.speed, _targetSpeed, rate * Time.deltaTime);*/

        if (_deps.Agent() == null) return;
        float smoothedSpeed = Mathf.Lerp(_deps.Agent().speed, _targetSpeed, _lerpSpeed * Time.deltaTime);
        _deps.Agent().speed = smoothedSpeed;

        float _currentSpeed = _deps.Agent().speed;

        if (Mathf.Approximately(_deps.Agent().speed, _targetSpeed)) _deps.Agent().speed = _targetSpeed;
    }

    public void OverrideSpeed(SpeedOverride overrideTier)
     => _currentSpeedOverride = overrideTier;


    private void UpdateSpeedtier(float remainingDistance)
    {
        if (_deps == null || _deps.Agent() == null || _deps.Agent().isStopped) return;

        float speed;
        float lerp;
        SpeedTier tier;

        tier = _deps.TryUpdateAgentTargetSpeed(_currentSpeedTier, _currentSpeedOverride, remainingDistance, out speed, out lerp);

        if (tier == _currentSpeedTier) return;

        _currentSpeedTier = tier;
        (_targetSpeed, _lerpSpeed) = (speed, lerp);

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
        _deps?.Agent().ResetPath();
        if (CurrentStateId == StateId.Patrol) return;
        ToggleAgent(false);
        _deps.Obstacle().enabled = true;
    }

 
    public void OverrideRotation(RotationOverride rotOverride)
    {
        if (_currentRotationOverride == rotOverride) return;
        _deps.Agent().updateRotation = rotOverride == RotationOverride.None;

        if(TestPrint) Debug.LogError($"Setting Rotation Override to {rotOverride}");
        _currentRotationOverride = rotOverride;
    }

    private bool NullOwnerOrTarget() => _deps.Target == null || _deps.Target.Transform == null || _deps.Owner == null
            || _deps.Owner.Transform == null || _deps.Agent() == null;

    public void Dispose()
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
