using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public partial class FSMBaseNew : IFSMControlNew
{
    // Injected Dependancies
    private IReadOnlyDictionary<StateId, IFSMState> _states;
    private IAgentData _ownerData;
    private IPathResolver _pathFinder;
    private IFieldOfViewRunner _fovHandler;
    // End Injected Dependancies

    // Used by owning Monobehaviour via interface
    public StateId CurrentStateId => _current?.Id ?? StateId.None;
    public IFSMControlNew.OnNotifyOwner Notification { get; set; }
    // End used by owning Monobehaviour
    
    // Actions Invoked from individual states
    public Action<AnimationCue> OnAnimationIntent { get; set; }
    public Action<Vector3> OnMapDestinationToZone { get; set; }
    public Vector3? CurrentDestinationForward { get; private set; }
    // End Actions Invoked from individual states

    // Internal Members
    private event Action<float> OnTick;
    private List<SetDestinationDelay> _timer = new(2);
    private IFSMState _current;
    public bool IsInStateTransition { get; private set; } = false;
    private bool _hasValidDestination = false;
    private float _lerpSpeed = 0f;
    private float _targetSpeed = 0f;
    private bool _usesSpeedByDistance = false;
    // End internal members


    private enum SpeedTier
    {
        Idle,
        Walk,
        Sprint
    }
    private SpeedTier _speedTier = SpeedTier.Idle;


    public FSMBaseNew(IAgentData data, IPathResolver resolver, IFieldOfViewRunner runner, IReadOnlyDictionary<StateId, IFSMState> states)
    {
        _ownerData = data;
        _pathFinder = resolver;
        _fovHandler = runner;
        _states = states;
        _pathFinder.Callback = OnPathRequestComplete;
        _fovHandler.OnFOVSweepComplete = FieldOfViewSweepResult;
        OnTick += _fovHandler.Tick;
        OnTick += TimerTicks;
        OnTick += ClassUpdate;
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
            UpdateFOVFrequency(CurrentStateId);
        }   // else => Notify state doesnt exist
    }

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
        _fovHandler.SetFOVSweepFrequency(phase);
    }

    private void FieldOfViewSweepResult(FOVResult result, bool withinAttackAngles)
        => Notification?.Invoke(NPCNotification.FOVUpdate(/*CurrentStateId, */result, withinAttackAngles));
    #endregion

    #region Tick Region

    public void Tick(float dt) => OnTick?.Invoke(dt);
    public void LateTick(float dt)
    {
        UpdateAgentSpeed();
        _current?.LateTick(dt);
    }


    public void ClassUpdate(float dt)
    {
        if (!_hasValidDestination || _ownerData.Agent == null) return;
        NavMeshAgent a = _ownerData.Agent;

        if (a.pathPending) return;

        if (!a.enabled || !a.isOnNavMesh)
        {
            _hasValidDestination = false;
            TryResetAgent();
            _current?.TryGetNewDestination();
            return;
        }


        if (!a.hasPath || a.pathStatus != NavMeshPathStatus.PathComplete)
        {
            _hasValidDestination = false;
            TryResetAgent();
            _current?.TryGetNewDestination();
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
            _current?.OnDestinationReached();
            return;
        }
        SetAgentSpeedByDistance(dist);
    }

    private void TimerTicks(float dt)
    {
        if (_ownerData is NPCController c) // Delete later
        {
            if (c.TestSprint)
                (_lerpSpeed, _targetSpeed) = (_ownerData.SprintSpeed, 2f);
            else if (c.TestWalk)
                (_lerpSpeed, _targetSpeed) = (_ownerData.WalkSpeed, 2f);
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

    #region Destination result & Setting region
    private bool StateHasChanged(StateId id) => id != CurrentStateId;

    public void OnPathRequestComplete(in PathResult result)
    {

        if (StateHasChanged(result.Id) || result.Reason == PathCheckReason.Cancelled) return;
        bool pathFound = result.PathFound;
        StateId id = result.Id;

        if (result.Reason == PathCheckReason.ProbePath && pathFound)
        { Notification?.Invoke(NPCNotification.PathToPrimaryAvailable(/*result.Id*/)); return; }

        if (!result.PathFound) { Notification?.Invoke(NPCNotification.NoAvailablePath(/*CurrentStateId*/)); Debug.LogError("NO Path Found!!"); return; }
        else
        {
            Vector3 currentDestination = result.Destination;
            CurrentDestinationForward = result.Forward;
            OnMapDestinationToZone?.Invoke(currentDestination);

            NavMeshObstacle o = _ownerData?.Obstacle;
            if (o != null && o.enabled && o.carving)
            {
                _ownerData.Obstacle.enabled = false;
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
        if (_ownerData.Agent.SetPath(path) ||
            _ownerData.Agent.SetDestination(destination))
        {
            float stopdist = _ownerData?.OnRequestAgentStoppingDistance?.Invoke(current) ?? 0f;
            _ownerData.Agent.stoppingDistance = stopdist;
            _hasValidDestination = true;
            _current?.OnDestinationSet();
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("Failed to Set Path - Attempting to Re-path");
#endif
            _hasValidDestination = false;
            TryResetAgent();
            _current?.TryGetNewDestination();
        }
    }

    protected void ToggleAgent(bool setActive)
    {
        if (_ownerData.Agent.enabled == setActive) return;
        _ownerData.Agent.enabled = setActive;
    }
    #endregion

    #region Speed Region
    public bool IsStationary() => _speedTier == SpeedTier.Idle;
    private void SetSpeedTier(SpeedTier tier)
    {
        if (tier == _speedTier) return;
        _speedTier = tier;
        var (speed, lerp) = tier switch
        {
            SpeedTier.Idle => (0f, 10f),
            SpeedTier.Walk => (_ownerData.WalkSpeed, 2f),
            SpeedTier.Sprint => (_ownerData.SprintSpeed, 2f),
            _ => (0f, 10f)
        };

        (_targetSpeed, _lerpSpeed) = (speed, lerp);
    }

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

    
    private void SetAgentSpeedByDistance(float remainingDistance)
    {
        if (_ownerData.Agent == null || _ownerData.Agent.isStopped) return;

        if (_usesSpeedByDistance)
        {
            if (remainingDistance > _ownerData.SprintEnterDist) { SetSpeedTier(SpeedTier.Sprint); }
            else if (remainingDistance < _ownerData.SprintExitDist) { SetSpeedTier(SpeedTier.Walk); }
        }
        else SetSpeedTier(SpeedTier.Walk);
    }
    #endregion


    private void TryResetAgent()
    {
        _hasValidDestination = false;
        SetSpeedTier(SpeedTier.Idle);
        _ownerData.Agent.ResetPath();
        if (CurrentStateId == StateId.Patrol) return;
        ToggleAgent(false);
        _ownerData.Obstacle.enabled = true;
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
