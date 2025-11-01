using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FSMManager : IFSMEvents
{
    public Action<float> Tick { get; private set; }
    public StateNotificationProvider Notification { get; set; }
    public ITargetable PrimaryTarget { get; set; }

    public bool HasLOS { get; set; }

    public bool DestinationReached { get; private set; } = true;

    //public uint CurrentZone { get; set; }

    // public Transform Transform { get; set; }

    // private DestinationProviderOld _destinationProvider;
    //  private DestinationService _destService;
    private IDestinationResolver _pathFinder;

    private Action<float> OnPatrol;

   // private NavMeshAgent _agent;
   // private NavMeshObstacle _obstacle;
  //  private NavMeshPath _path;
    private uint _stateTransitionId;
   // private EnemyEventManager _eventManager;
    private IFSMOwner Owner; 
    //private bool _isInStateTransition = false;
   
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
        Tick = OnTick;
    }

    private void OnTick(float dt) => CheckRemainingDistance();
    

    public void OnPathRequestComplete(in PathResult result)
    {
        
        // Blocks Destination Setting while transitioning to new state
        if (result.Id != _stateTransitionId) return;
        bool pathFound = result.PathFound;
        DestinationKind kind = result.Kind;

        if (result.Reason == PathCheckReason.ProbePathToPrimaryTarget && pathFound) 
        { SendNotification(NotificationKind.PathToPrimaryAvailable, false); return; }

        if (!result.PathFound) { SendNotification(NotificationKind.NoAvailablePath, false); Debug.LogError("NO Path Found!!"); return; }
        else
        {
            if (kind == DestinationKind.Patrol) Owner.OwnerEM.SpeedChanged(Owner.WalkSpeed, 2f);

            Owner.Agent.SetDestination(result.Position);
        }

    }

    private void CheckRemainingDistance()
    {
        if (!Owner.Agent.enabled) return;
        if(Owner.Agent.hasPath && !Owner.Agent.pathPending)
        {
            bool reached = HasReachedDestination();
            if (DestinationReached == reached) return;
            DestinationReached = reached;
            if (DestinationReached)
            {
                Owner.OwnerEM.SpeedChanged(0f, 10f);
                Owner.Agent.ResetPath();
                Owner.Agent.enabled = false;
                Owner.Obstacle.enabled = true;
            }
                
        }
    }

    private bool HasReachedDestination() => Owner.Agent.remainingDistance <= (Owner.Agent.stoppingDistance + 0.25f);

 
    public bool TryGetCurrentZone(out int zone)
        => _pathFinder.TryGetCurrentZone(out zone);

    public bool TrySwitchZone() => _pathFinder.TrySwitchZone();
   

    private void SendNotification(NotificationKind kind, bool destinationReached)
    {
        StateNotification n = new StateNotification(kind, destinationReached);
        Notification?.Invoke(n);
    }

    private Vector3 GetOwnerPos() => LineOfSightUtility.GetClosestPointOnNavMesh(Owner.GetPosition());

    private bool IsDestinationReached() => false;

    public void BeginPatrol()
    {
        Debug.LogError("Trying To Patrol");
   /*     PathCheckReason reason = PathCheckReason.ValidatePathForDestination;*/
        var request = ValidateDestination.GetPatrolPoint(_stateTransitionId, Owner, Owner.Path);
        /*List<(Vector3,Vector3?)> points = */_pathFinder?.TryGetDestination(request);
     //   PathRequestInfo info = new PathRequestInfo(points, GetOwnerPos(), reason, Owner.Path, _stateTransitionId);
       // _pathFinder.TryGetPath(info);
       // Tick = OnPatrol;
    }

  

    public void BeginChase()
    {
        PathCheckReason reason = PathCheckReason.ValidatePathForDestination;
    }

    public void BeginFlank()
    {
        throw new NotImplementedException();
    }

    public void TakeCover()
    {
        throw new NotImplementedException();
    }

    public void FollowGroup()
    {
        throw new NotImplementedException();
    }

    public void ClearState() => _stateTransitionId++;

   
}
