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

    // public Transform Transform { get; set; }

    private DestinationProviderOld _destinationProvider;
    private PathFinder _pathFinder;

    private Action<float> OnPatrol;

   // private NavMeshAgent _agent;
   // private NavMeshObstacle _obstacle;
  //  private NavMeshPath _path;
    private uint _stateTransitionId;
   // private EnemyEventManager _eventManager;
    private FSMOwner Owner; 
    //private bool _isInStateTransition = false;
   
    public FSMManager(FSMOwner owner)
    {
        if (Owner == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Must Pass a valid FSMOwner");
#endif
            return;

        }

        Owner = owner;
        _pathFinder = new(this);
        _destinationProvider = new();
    }

    public FSMManager(EnemyEventManager em, Transform owner, NavMeshAgent agt, NavMeshObstacle ob)
    {
        _pathFinder = new(this);
       // Transform = owner;
       // _path = new();
        _destinationProvider = new();
       // _eventManager = em;
       // _agent = agt;
      //  _obstacle = ob;
    }

    public void OnPathRequestComplete(in PathResult result)
    {
        // Blocks Destination Setting while transitioning to new state
        if (result.Id != _stateTransitionId) return;
        bool pathFound = result.PathFound;

        if (result.Reason == PathCheckReason.ProbePathToPrimaryTarget && pathFound) 
        { SendNotification(NotificationKind.PathToPrimaryAvailable, false); return; }

        if (!result.PathFound) { SendNotification(NotificationKind.NoAvailablePath, false); return; }
        else
        {
           

            Owner.Agent.SetDestination(result.Position);
        }

           /* switch (result.Kind)
            {
                case DestinationKind.Patrol:
                    if (!pathFound) SendNotification(NotificationKind.NoAvailablePath, false);
                    return;
                case DestinationKind.ProbeToTarget:
                    if (pathFound) SendNotification(NotificationKind.PathToPrimaryAvailable, false);
                    return;
                case DestinationKind.ChaseTarget:
                    if (!pathFound) SendNotification(NotificationKind.NoAvailablePath, false);
                    return;
                case DestinationKind.Flank:
                    if (!pathFound) SendNotification(NotificationKind.NoAvailablePath, false);
                    return;
            }*/

        /*switch (target)
        {
            *//*case PathPurpose.CheckPrimary:
                break;*//*
        }

        if (!pathFound)
        {
            StateNotification n = new StateNotification(NotificationKind.NoAvailablePath, false);
            Notification?.Invoke(n);
            return;
        }*/
    }

    private void SendNotification(NotificationKind kind, bool destinationReached)
    {
        StateNotification n = new StateNotification(kind, destinationReached);
        Notification?.Invoke(n);
    }

    private Vector3 GetOwnerPos() => LineOfSightUtility.GetClosestPointOnNavMesh(Owner.Transform.position);

    private bool IsDestinationReached() => false;

    public void BeginPatrol()
    {
        PathCheckReason reason = PathCheckReason.ValidatePathForDestination;
        List<(Vector3,Vector3?)> points = _destinationProvider?.TryGetDestinations(DestinationKind.Patrol);
        PathRequestInfo info = new PathRequestInfo(points, GetOwnerPos(), reason, Owner.Path, _stateTransitionId);
        _pathFinder.TryGetPath(info);
        Tick = OnPatrol;
    }

    private void TryGetDestinations(PathCheckReason reason)
    {

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
