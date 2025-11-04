using Oculus.Interaction.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
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
 
    private IDestinationResolver _pathFinder;

    private Coroutine _runningRoutine;

    private IFSMOwner Owner;
    Vector3 _currentDestination;
    private Vector3? _currentPatrolPoinfForward = null;
    private float _targetSpeed = 0f;
    private float _lerpSpeed = 0f;

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

    private void OnTick(float dt)
    {
        CheckRemainingDistance();
        UpdateAgentSpeed();

        for(int i = 0; i < _timer.Count; i++)
        {
            var t = _timer[i];
            t.RemainingTime -= dt;

            if(t.RemainingTime <= 0)
            {
                t.OnDone?.Invoke(t.Path, t.Destination, t.AgentSpeed, t.Lerp);
                _timer.RemoveAt(i);
                i--;
                continue;
            }
            _timer[i] = t;
        }
    }
    

    

    private StateId ApprovedDestinationStateId;

    private void SendNotification(in NotifyOwnerNPC n) => Notification?.Invoke(n);
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
                ResetAgent();
                bool isStaleDestination = ApprovedDestinationStateId != _currentStateId;
                SendNotification(NotifyOwnerNPC.DestinationReached(_currentStateId, isStaleDestination));

            }

        }
    }

    private void ResetAgent()
    {
        SetAgentTargetSpeed(0f, 10f);
        Owner.Agent.ResetPath();
       // if (_currentStateId == StateId.Patrol) return;
        Owner.Agent.enabled = false;
        Owner.Obstacle.enabled = true;
    }

    public void DestinationApproved(NavMeshPath path, Vector3 newDestination, StateId ApprovalState, float speed, float lerp)
    {
        ApprovedDestinationStateId = ApprovalState;
        //var (speed, lerp) = Owner.GetSpeedAndLerp(ApprovedDestinationStateId);
        NavMeshObstacle o = Owner.Obstacle;
        if (o.enabled && o.carving)
        {
            Owner.Obstacle.enabled = false;
            _timer.Add(new UnCarveDelay(Time.deltaTime + Mathf.Epsilon, newDestination, path, speed, lerp, SetDestination));
          //  CoroutineRunner.Instance.StartCoroutine(DelayEnableroutine(path, newDestination, speed, lerp));
            return;
        }
        SetDestination(path, newDestination, speed, lerp);
    }

    Action<NavMeshPath, Vector3, float, float> UnCarveCB;

    IEnumerator DelayEnableroutine(NavMeshPath path, Vector3 destination, float speed, float lerp)
    {
        Owner.Obstacle.enabled = false;
        yield return null;

        SetDestination(path, destination, speed, lerp);
    }

    private List<UnCarveDelay> _timer = new(2);
    private struct UnCarveDelay
    {
        public float RemainingTime;
        public readonly Vector3 Destination;
        public readonly NavMeshPath Path;
        public readonly float AgentSpeed;
        public readonly float Lerp;
        
        public readonly Action<NavMeshPath, Vector3, float, float> OnDone;

        public UnCarveDelay(float time, Vector3 dest, NavMeshPath p, float speed, float lerp, Action<NavMeshPath, Vector3, float, float> cb)
        {
            RemainingTime = time;
            Destination = dest;
            Path = p;
            AgentSpeed = speed;
            Lerp = lerp;
            OnDone = cb;
        }
    }



    protected void SetDestination(NavMeshPath path, Vector3 destination, float newSpeed, float lerp)
    {
        SetAgentTargetSpeed(newSpeed, lerp);
        ToggleAgent(setActive: true);
        if (!Owner.Agent.SetPath(path))
            if (!Owner.Agent.SetDestination(destination)) Debug.LogError("Failed to Set Destination");
    }

    public void ToggleAgent(bool setActive)
    {
        if (Owner.Agent.enabled == setActive) return;
        Owner.Agent.enabled = setActive;
    }

    #region Path Received & Validation
    public void OnPathRequestComplete(in PathResult result)
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
            _currentPatrolPoinfForward = result.Forward;

            SendNotification(NotifyOwnerNPC.DestinationFound(result.Id, _currentDestination, result.Path));

        }

    }
    #endregion


    #region Tick Region
    private void SetAgentTargetSpeed(float speed, float lerpSpeed)
    => (_lerpSpeed, _targetSpeed) = (lerpSpeed, speed);

    private void UpdateAgentSpeed()
    {
        if (Owner.Agent == null) return;
        float smoothedSpeed = Mathf.Lerp(Owner.Agent.speed, _targetSpeed, _lerpSpeed * Time.deltaTime);
        Owner.Agent.speed = smoothedSpeed;

        float _currentSpeed = Owner.Agent.speed;

        if (Mathf.Approximately(Owner.Agent.speed, _targetSpeed)) Owner.Agent.speed = _targetSpeed;
    }
    #endregion


    #region Patrol Region

 


    public void LookAroundAndContinue()
    {
        if(_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PatrolWaitRoutine(_currentPatrolPoinfForward));
    }

    private IEnumerator PatrolWaitRoutine(Vector3? forward = null)
    {
        if (forward != null)
        {
            Transform t = Owner.Transform;
            Quaternion targetRot = Quaternion.LookRotation(forward.Value);
            while (Quaternion.Angle(t.rotation, targetRot) > 2.0f + Mathf.Epsilon)
            {
                t.rotation = Quaternion.Slerp(t.rotation, targetRot, Time.deltaTime * 2f);
                yield return null;
            }

        }
        Owner.OwnerEM.TriggerAnimation(AnimationCue.Look);

        if (_currentStateId != StateId.Patrol) yield break;

        float _delayTime = Random.Range(Owner.MinWaitTime, Owner.MaxWaitTime);
        float elapsedTime = 0.0f;

        while (elapsedTime < _delayTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (_currentStateId != StateId.Patrol) yield break;
        BeginPatrol(_currentStateId);
   
        _runningRoutine = null;
    }

    private void CancelRunningCoroutine()
    {
        if(_runningRoutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(_runningRoutine);
            _runningRoutine = null;
        }
    }

    public void BeginPatrol(StateId id)
    {
        //  TryRepath = BeginPatrol;
        if (id != StateId.Patrol) return;
        if (_currentStateId != id) _currentStateId = id;

        var request = ValidateDestination.GetPatrolPoint(id, Owner, Owner.Path);
  
        _pathFinder?.TryGetDestination(request);
       
    }

  

    #endregion



    private bool HasReachedDestination() => Owner.Agent.remainingDistance <= (Owner.Agent.stoppingDistance + 0.25f);

 
    public bool TryGetCurrentZone(out int zone)
        => _pathFinder.TryGetCurrentZone(out zone);

    public bool TrySwitchZone() => _pathFinder.TrySwitchZone();
   


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
        CancelRunningCoroutine();
        _pathFinder?.CancelAll();
    }
    

   
}
