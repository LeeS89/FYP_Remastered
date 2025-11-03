using Oculus.Interaction.Editor;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;


public class FSMManager : IFSMEvents
{
    public Action<float> Tick { get; private set; }
    public StateNotificationProvider Notification { get; set; }
    public ITargetable PrimaryTarget { get; set; }

    public bool HasLOS { get; set; }

    public bool DestinationReached { get; private set; } = true;

    public Action TryRepath { get; private set; }

    private StateId _currentStateId = StateId.None;
 
    //public uint CurrentZone { get; set; }

    // public Transform Transform { get; set; }

    // private DestinationProviderOld _destinationProvider;
    //  private DestinationService _destService;
    private IDestinationResolver _pathFinder;

    private Action<float> OnPatrol;

   // private NavMeshAgent _agent;
   // private NavMeshObstacle _obstacle;
  //  private NavMeshPath _path;
  //  private uint _stateId;
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
        if (result.Id != _currentStateId) return;
        bool pathFound = result.PathFound;
        DestinationKind kind = result.Kind;

        if (result.Reason == PathCheckReason.ProbePathToPrimaryTarget && pathFound)
        { SendNotification(NotifyOwnerNPC.PathToPrimaryAvailable(result.Id)); return; }
        //{ SendNotification(NotificationKind.PathToPrimaryAvailable, false); return; }

       // if (!result.PathFound) { SendNotification(NotificationKind.NoAvailablePath, false); Debug.LogError("NO Path Found!!"); return; }
        if (!result.PathFound) { SendNotification(NotifyOwnerNPC.NoAvailablePath(_currentStateId)); Debug.LogError("NO Path Found!!"); return; }
        else
        {

            /*float newSpeed;
            Vector3 destination = result.Destination;
            switch (result.Id)
            {
                case StateId.Patrol or StateId.Flank or StateId.Chase or StateId.Search:
                    newSpeed = Owner.WalkSpeed;
                    break;
                case StateId.Flee or StateId.Follow or StateId.Cover:
                    newSpeed = Owner.SprintSpeed;
                    break;
                default:
                    newSpeed = 0f;
                    destination = Vector3.zero;
                    break;
            }*/
            SendNotification(NotifyOwnerNPC.DestinationFound(result.Id, result.Destination/*, newSpeed, 2f*/));
           // if (kind == DestinationKind.Patrol) Owner.OwnerEM.SpeedChanged(Owner.WalkSpeed, 2f);

           // Owner.Agent.SetDestination(result.Position);
        }

    }

    private StateId ApprovedDestinationStateId;

    private void SendNotification(in NotifyOwnerNPC n) => Notification?.Invoke(n);

    #region Patrol Region
    private void CheckRemainingDistance()
    {
        if (!Owner.Agent.enabled) return;
        if (Owner.Agent.hasPath && !Owner.Agent.pathPending)
        {
            bool reached = HasReachedDestination();
            if (DestinationReached == reached) return;
            DestinationReached = reached;
            if (DestinationReached)
            {
                // SendNot(StateNotification.DestinationReached(DestinationKind.Patrol, _id));
                bool isStaleDestination = ApprovedDestinationStateId != _currentStateId;
                SendNotification(NotifyOwnerNPC.DestinationReached(_currentStateId, isStaleDestination));
               /* Owner.OwnerEM.SpeedChanged(0f, 10f);
                Owner.Agent.ResetPath();
                Owner.Agent.enabled = false;
                Owner.Obstacle.enabled = true;*/
            }

        }
    }

    public void DestinationApproved(Vector3 newDestination, StateId ApprovalState)
    {
        ApprovedDestinationStateId = ApprovalState;
        Owner.Agent.SetDestination(newDestination);
    }

    private IEnumerator PatrolWaitRoutine(Vector3? forward = null)
    {
        if (forward != null)
        {
            Quaternion ownerRot = Owner.GetRotation();
            Quaternion targetRot = Quaternion.LookRotation(forward.Value);
            while (Quaternion.Angle(ownerRot, targetRot) > 2.0f + Mathf.Epsilon)
            {
                ownerRot = Quaternion.Slerp(ownerRot, targetRot, Time.deltaTime * 2f);
                yield return null;
            }

        }
        Owner.OwnerEM.TriggerAnimation(AnimationCue.Look);

        if (_currentStateId != StateId.Patrol) yield break;

        float _delayTime = Random.Range(0, 5f);
        float elapsedTime = 0.0f;

        while (elapsedTime < _delayTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (_currentStateId != StateId.Patrol) yield break;
        //FSM.TryGetNextDestination(_state.Id);
        //StartCoroutine(DelayEnableRoutine(0, _currentDestination, WalkSpeed, 2f));
        //TrySetDestination(0, _currentDestination, WalkSpeed, 2f);
        //FSM.TryRepath?.Invoke();
    }



    public void BeginPatrol(StateId id)
    {
        //  TryRepath = BeginPatrol;

        if (_currentStateId != id) _currentStateId = id;
        Debug.LogError("BeginPatrol Passed id: " + id.ToString() + ", and after setting: " + _currentStateId.ToString());
        // _stateId = stateId;
        /*     PathCheckReason reason = PathCheckReason.ValidatePathForDestination;*/
        var request = ValidateDestination.GetPatrolPoint(_currentStateId, Owner, Owner.Path);
        /*List<(Vector3,Vector3?)> points = */
        _pathFinder?.TryGetDestination(request);
        //   PathRequestInfo info = new PathRequestInfo(points, GetOwnerPos(), reason, Owner.Path, _stateTransitionId);
        // _pathFinder.TryGetPath(info);
        // Tick = OnPatrol;
    }

    public void OnPathRequestCompleted(in PathResult result)
    {

        // Blocks Destination Setting while transitioning to new state
        if (result.Id != _currentStateId) return;
        bool pathFound = result.PathFound;
       // DestinationKind kind = result.Kind;
        
        if (result.Reason == PathCheckReason.ProbePathToPrimaryTarget && pathFound)
        { SendNotification(NotifyOwnerNPC.PathToPrimaryAvailable(_currentStateId)); return; }
        //{ SendNotification(NotificationKind.PathToPrimaryAvailable, false); return; }

        // if (!result.PathFound) { SendNotification(NotificationKind.NoAvailablePath, false); Debug.LogError("NO Path Found!!"); return; }
        if (!result.PathFound) { SendNotification(NotifyOwnerNPC.NoAvailablePath(_currentStateId)); Debug.LogError("NO Path Found!!"); return; }
        else
        {
          //  SendNotification(NotifyOwnerNPC.DestinationFound(_currentStateId, result.Destination, 0, 0));
            // if (kind == DestinationKind.Patrol) Owner.OwnerEM.SpeedChanged(Owner.WalkSpeed, 2f);
            
            // Owner.Agent.SetDestination(result.Position);
        }

    }

    #endregion
















    public void TryGetNextDestination(StateId currentStateId)
    {
        if (_currentStateId != currentStateId) return;

        switch (currentStateId)
        {
            case StateId.Patrol:
                BeginPatrol(currentStateId);
                break;
        }
    }


    private bool HasReachedDestination() => Owner.Agent.remainingDistance <= (Owner.Agent.stoppingDistance + 0.25f);

 
    public bool TryGetCurrentZone(out int zone)
        => _pathFinder.TryGetCurrentZone(out zone);

    public bool TrySwitchZone() => _pathFinder.TrySwitchZone();
   

   // private void SendNot(in StateNotification n) => Notification?.Invoke(n);

    /*private void SendNotification(NotificationKind kind, bool destinationReached)
    {
        StateNotification n = new StateNotification(kind, destinationReached);
        Notification?.Invoke(n);
    }*/

    private Vector3 GetOwnerPos() => LineOfSightUtility.GetClosestPointOnNavMesh(Owner.GetPosition());

    private bool IsDestinationReached() => false;

    
  

    public void BeginChase(StateId id)
    {
        PathCheckReason reason = PathCheckReason.ValidatePathForDestination;
    }

    public void BeginFlank(StateId id)
    {
        throw new NotImplementedException();
    }

    public void TakeCover(StateId id)
    {
        throw new NotImplementedException();
    }

    public void FollowGroup(StateId id)
    {
        throw new NotImplementedException();
    }

    public void ExitState()
    {
       
    }

   
}
