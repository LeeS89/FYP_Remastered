using System;
using UnityEngine;
using UnityEngine.AI;

public abstract class FSMBase : IFSMEvents
{
    public Action<StateId> TryRepath { get; protected set; }
    public bool DestinationReached { get; protected set; } = true;
    public bool HasLOS { get; set; }
    public StateNotificationProvider Notification { get; set; }
    protected event Action<float> OnTick;

    public void Tick(float dt) => OnTick?.Invoke(dt);
    
    public Action<float> LateTick { get; protected set; }

    protected Action<float> OnDistanceTick;

   

    public ITargetable PrimaryTarget { get; set; }

    

    protected StateId _currentStateId = StateId.None;

    protected IDestinationResolver _pathFinder;

    protected Coroutine _runningRoutine;

    protected IFSMOwner Owner;
    protected Vector3 _currentDestination;
    protected Vector3? _currentDestinaationForward = null;
    protected float _targetSpeed = 0f;
    protected float _lerpSpeed = 0f;

    public abstract void BeginChase(StateId id);
    public abstract void BeginFlank(StateId id);
    public abstract void BeginPatrol(StateId id);
  //  public abstract void DestinationApproval(bool approved, NavMeshPath path, Vector3 newDestination, StateId ApprovalState, float newAgentpeed, float lerp);
    public abstract void ExitState();
    public abstract void FollowGroup(StateId id);
    public abstract void TakeCover(StateId id);
    protected abstract void SetDestination(NavMeshPath path, Vector3 destination, StateId current);
    public abstract void OnPathRequestComplete(in PathResult result);
    


    public virtual void LookAroundAndContinue() { }
    public virtual bool TrySwitchPatrolZone() => false;

    protected void SendNotification(in NotifyOwnerNPC n) => Notification?.Invoke(n);

    public bool TryGetCurrentZone(out int zone)
    {
        if (_pathFinder == null) { zone = 0; return false; }
        return _pathFinder.TryGetCurrentZone(out zone);
    }

    public bool IsMoving() => _speedTier != SpeedTier.Idle;
   
    protected enum SpeedTier
    {
        Idle,
        Walk,
        Sprint
    }
    protected SpeedTier _speedTier = SpeedTier.Idle;
}
