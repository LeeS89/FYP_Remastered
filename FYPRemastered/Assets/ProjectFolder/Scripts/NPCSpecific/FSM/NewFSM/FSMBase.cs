using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class FSMBase : IFSMControl
{
    public Action<StateId> TryRepath { get; protected set; }
    public bool DestinationReached { get; protected set; } = true;
    public bool HasLOS { get; set; }
  //  public StateNotificationProvider Notification { get; set; }
    protected event Action<float> OnTick;
    public void Tick(float dt) => OnTick?.Invoke(dt);
    public Action<float> LateTick { get; protected set; }

    public ITargetable PrimaryTarget { get; set; }
    public IFSMControl.OnNotifyOwner Notification { get; set; }
    public Action<AnimationCue> OnAnimationIntent { get; set; }

    protected IFieldOfViewRunner _fovHandler;

    protected StateId _currentStateId = StateId.None;

    protected IPathResolver _pathFinder;

    protected Coroutine _runningRoutine;

    protected IFSMData _ownerData;
   // protected IFSMNotifications _ownerNotifications;
    protected Vector3 _currentDestination;
    protected Vector3? _currentDestinaationForward = null;
    protected float _targetSpeed = 0f;
    protected float _lerpSpeed = 0f;

    // State enter/ Exit methods
    public abstract void BeginChase(StateId id);
    public abstract void BeginFlank(StateId id);
    public abstract void BeginPatrol(StateId id);
    public abstract void ExitState();
    public abstract void FollowGroup(StateId id);
    public abstract void TakeCover(StateId id);


    // Pathfinding methods
    protected abstract void SetDestination(NavMeshPath path, Vector3 destination, StateId current);
    public abstract void OnPathRequestComplete(in PathResult result);


    // Patrol state specific methods
    public virtual void LookAroundAndContinue() { }
    public virtual bool TrySwitchPatrolZone() => false;

    // 
  //  protected void SendNotification(in NotifyOwnerNPC n) => Notification?.Invoke(n);

   /* public bool TryGetCurrentZone(out int zone)
    {
        if (_pathFinder == null) { zone = 0; return false; }
        return _pathFinder.TryGetCurrentZone(out zone);
    }*/

    public bool IsMoving() => _speedTier != SpeedTier.Idle;

    public abstract void OnInstanceDestroyed();
    

    protected enum SpeedTier
    {
        Idle,
        Walk,
        Sprint
    }
    protected SpeedTier _speedTier = SpeedTier.Idle;

    protected void SetSpeedTier(SpeedTier tier)
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

        SetAgentTargetSpeed(speed, lerp);

    }

    protected void SetAgentTargetSpeed(float speed, float lerpSpeed)
    => (_lerpSpeed, _targetSpeed) = (lerpSpeed, speed);

    protected void ToggleAgent(bool setActive)
    {
        if (_ownerData.Agent.enabled == setActive) return;
        _ownerData.Agent.enabled = setActive;
    }

    public virtual void FieldOfViewSweepResult(FOVResult result, bool withinAttackAngles) { }

    public int? TryGetPatrolZone() => _pathFinder?.TryGetCurrentZone();


    //  public abstract bool TryGetPatrolZone(out int zone);


    // Used when the Agent is currently carving
    // After uncarving, this delays setting a new destination
    // for 1 frame to give the NavMesh enough time to update
    protected struct SetDestinationDelay
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
