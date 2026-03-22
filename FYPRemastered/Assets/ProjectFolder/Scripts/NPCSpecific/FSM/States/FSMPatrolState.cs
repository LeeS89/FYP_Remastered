using Npc.API;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public sealed class FSMPatrolState : FsmBaseState<PatrolDeps>
{
    private readonly IPatrolService _waypointService;
  //  private readonly IPatrolDeps _patrolDeps;

    /*public FSMPatrolState(IWaypointService waypointService, IAgentData data, IPathResolver resolver, IFSMStateContext stateContext) 
        : base(data, resolver, stateContext, StateId.Patrol)
    {
        _waypointService = waypointService;
        _candidateDestinations.EnsureCapacity(10);
    }*/
    public FSMPatrolState(PatrolDeps deps, SharedFsmStateServices sharedDeps, IFsmStateEvents stateEvents) 
        : base(deps, sharedDeps, stateEvents, StateId.Patrol)
    {
       // _patrolDeps = deps;
        _waypointService = _deps.WaypointService;
        //_waypointService = _patrolDeps.WaypointService;
        _candidateDestinations.EnsureCapacity(10);
    }


    public override void OnDestinationReached()
    {
        if (!_isInState /*|| OwnerIsNull()*//*_sharedDeps.OwnerTransform == null*/) return;

        Transform ownerTransform;
        if (!TryGetOwnerTransform(out ownerTransform)) return;


        if (_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PatrolWaitRoutine(
                ownerTransform, _deps.MinTimeAtPatrolPoint, _deps.MaxTimeAtPatrolPoint));
    }

    protected override void RetrieveCandidateDestinations()
    {
      
        if (!_isInState || _candidateDestinations == null) return;

        if (_candidateDestinations.Count == 0)
        {
            NavMeshPath path;
            if (!TryGetPath(out path)) return;

            Debug.LogError("successfully retrieved Path");

            if (_waypointService == null /*|| !_waypointService.TryGetWaypoints(this, _candidateDestinations)*/)
            {
                Debug.LogError("Returning Failed Result for patrol");
                DestinationResultInfo failedResult = new DestinationResultInfo
                (
                    ReasonForDestinationCheck.ValidatePathForDestination,
                    path,
                    DestinationResult.CandidatesNullOrEmpty,
                    Vector3.zero,
                    _id
                );

                base.OnProcessedDestinationsResult(in failedResult);
                return;
            }
        }
        ValidateCandidateDestinations();
    }

    protected override void ValidateCandidateDestinations()
    {
        if (!_isInState || _candidateDestinations == null || ResolverIsNull()) return;


        Vector3 ownerPos;
        if (!TryGetOwnerPosition(out ownerPos)) return;

        NavMeshPath path;
        if (!TryGetPath(out path)) return;

        if (_candidateDestinations.Count > 1)
        {
         
            var temp = _candidateDestinations[0];
            _candidateDestinations.RemoveAt(0);
            ShuffleCandidateList(_candidateDestinations);
            _candidateDestinations.Add(temp);
        }
        //ContinueRoutine = true;
     

        DestinationRequest req = new DestinationRequest(_id, ownerPos, _candidateDestinations, path, 
            ReasonForDestinationCheck.ValidatePathForDestination, _validationCallback);
        _deps.PathResolver.ProcessDestinationCandidates(in req);


       /* _pathResolver?.ProcessDestinationCandidates(_id, ReasonForDestinationCheck.ValidatePathForDestination,
            _candidateDestinations, _path, _owner.Position(), _validationCallback);*/

    }

  

    private IEnumerator PatrolWaitRoutine(Transform t, float minWait, float maxWait)
    {
        Debug.LogError("Patrol wait routine called");
       // if (forward != null)
     //   {
            float randomAngle = Random.Range(-180, 180);
            Vector3 dirOffset = Quaternion.AngleAxis(randomAngle, t.up) * t.forward;
            Quaternion targetRot = Quaternion.LookRotation(dirOffset, t.up);
            //Quaternion targetRot = Quaternion.LookRotation(forward.Value);
            while (Quaternion.Angle(t.rotation, targetRot) > 2.0f + Mathf.Epsilon)
            {
                t.rotation = Quaternion.Slerp(t.rotation, targetRot, Time.deltaTime * 2f);
                yield return null;
            }

       // }
        if (!_isInState) yield break;

        _stateEvents?.RequestAnimation(AnimationCue.Look, _id);
        //_stateContext?.OnAnimationIntent?.Invoke(AnimationCue.Look);
      
        float _delayTime = Random.Range(minWait, maxWait);
        float elapsedTime = 0.0f;

        while (elapsedTime < _delayTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (!_isInState) yield break;
        ValidateCandidateDestinations();

        _runningRoutine = null;
    }


    
}

public sealed class PatrolDeps : FsmBaseState<PatrolDeps>.FsmBaseStateDeps
{
    public IPatrolService WaypointService { get; private set; }
    public float MaxTimeAtPatrolPoint { get; private set; }
    public float MinTimeAtPatrolPoint { get; private set; }

    public PatrolDeps(IPatrolService waypointService, IPathResolver resolver, PatrolStateConfig config) : base(resolver)
    {
        WaypointService = waypointService;
        MaxTimeAtPatrolPoint = config?.maxTimeAtWaypoint ?? 10f;
        MinTimeAtPatrolPoint = config?.minTimeAtWaypoint ?? 0.5f;
    }
}

































public sealed class FSMPatrolStateNew : FsmBaseStateNew<IPatrolService>
{
 //   private readonly IPatrolService _waypointService;
    //  private readonly IPatrolDeps _patrolDeps;

    /*public FSMPatrolState(IWaypointService waypointService, IAgentData data, IPathResolver resolver, IFSMStateContext stateContext) 
        : base(data, resolver, stateContext, StateId.Patrol)
    {
        _waypointService = waypointService;
        _candidateDestinations.EnsureCapacity(10);
    }*/
    public FSMPatrolStateNew(IFsmStateEvents stateController, IPatrolService service, IPathResolver pathResolver, ICoroutineHost host)
        : base(stateController, service, pathResolver, host, StateId.Patrol)
    {
        // _patrolDeps = deps;
        //_waypointService = _deps.WaypointService;
        //_waypointService = _patrolDeps.WaypointService;
        _candidateDestinations.EnsureCapacity(10);

     
    }


    public override void OnDestinationReached()
    {
        if (!_isInState /*|| OwnerIsNull()*//*_sharedDeps.OwnerTransform == null*/) return;

        Transform ownerTransform;
        // if (!TryGetOwnerTransform(out ownerTransform)) return;


        if (_runningRoutine == null)
            _runningRoutine = _host?.StartCoroutine(PatrolWaitRoutineNew(/*0.5f, 7f)*/));//CoroutineRunner.Instance.StartCoroutine(PatrolWaitRoutineNew(
                                                                                         // 0.5f, 7f));
        /*if (_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PatrolWaitRoutineNew(
                ownerTransform, _deps.MinTimeAtPatrolPoint, _deps.MaxTimeAtPatrolPoint));*/

        _stateEvents.Test();
    }

    protected override void RetrieveCandidateDestinations()
    {

        if (!_isInState || _candidateDestinations == null) return;

        if (_candidateDestinations.Count == 0)
        {
            NavMeshPath path;
            if (!TryGetPath(out path)) return;

            Debug.LogError("successfully retrieved Path");

            if (Context == null || !Context.TryGetDestinationCandidates(_stateEvents, _candidateDestinations)/*!_waypointService.TryGetWaypoints(this, _candidateDestinations)*/)
            {
                Debug.LogError("Returning Failed Result for patrol");
                DestinationResultInfo failedResult = new DestinationResultInfo
                (
                    ReasonForDestinationCheck.ValidatePathForDestination,
                    path,
                    DestinationResult.CandidatesNullOrEmpty,
                    Vector3.zero,
                    _stateId
                );

                base.OnProcessedDestinationsResult(in failedResult);
                return;
            }
        }
        ValidateAndSendCandidateDestinations();
    }

    protected override void ValidateAndSendCandidateDestinations()
    {
        if (!_isInState || _candidateDestinations == null/* || ResolverIsNull()*/) return;


        /*Vector3 ownerPos;
        if (!TryGetCurrentPosition(out ownerPos)) return;

        NavMeshPath path;
        if (!TryGetPath(out path)) return;*/
        Vector3 ownerPos;
        NavMeshPath path;
        if (!TryGetCurrentPositionAndPath(out ownerPos, out path)) return;

        DebugLogs.Err("GOTTEN PATH AND OWNER POS", this);

        if (_candidateDestinations.Count > 1)
        {

            var temp = _candidateDestinations[0];
            _candidateDestinations.RemoveAt(0);
            ShuffleCandidateList(_candidateDestinations);
            _candidateDestinations.Add(temp);
        }
        //ContinueRoutine = true;


        DestinationRequest req = new DestinationRequest(_stateId, ownerPos, _candidateDestinations, path,
            ReasonForDestinationCheck.ValidatePathForDestination, _validationCallback);

        _pathResolver?.ProcessDestinationCandidates(in req);
       // _deps.PathResolver.ProcessDestinationCandidates(in req);


        /* _pathResolver?.ProcessDestinationCandidates(_id, ReasonForDestinationCheck.ValidatePathForDestination,
             _candidateDestinations, _path, _owner.Position(), _validationCallback);*/

    }



    /*private IEnumerator PatrolWaitRoutine(Transform t, float minWait, float maxWait)
    {
        Debug.LogError("Patrol wait routine called");
        // if (forward != null)
        //   {
        float randomAngle = Random.Range(-180, 180);
        Vector3 dirOffset = Quaternion.AngleAxis(randomAngle, t.up) * t.forward;
        Quaternion targetRot = Quaternion.LookRotation(dirOffset, t.up);
        //Quaternion targetRot = Quaternion.LookRotation(forward.Value);
        while (Quaternion.Angle(t.rotation, targetRot) > 2.0f + Mathf.Epsilon)
        {
            t.rotation = Quaternion.Slerp(t.rotation, targetRot, Time.deltaTime * 2f);
            yield return null;
        }
  
        // }
        if (!_isInState) yield break;

        _stateEvents?.RequestAnimation(AnimationCue.Look, _stateId);
        //_stateContext?.OnAnimationIntent?.Invoke(AnimationCue.Look);

        float _delayTime = Random.Range(minWait, maxWait);
        float elapsedTime = 0.0f;

        while (elapsedTime < _delayTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (!_isInState) yield break;
        ValidateAndSendCandidateDestinations();

        _runningRoutine = null;
    }*/

    private IEnumerator PatrolWaitRoutineNew(/*float minWait, float maxWait*/)
    {
        Debug.LogError("Patrol wait routine called");
       
        float randomAngle = Random.Range(-180, 180);
        bool done = false;
        bool canContinue = false;
      

        _stateEvents.RequestRotation(randomAngle, _stateId, allowed =>
        {
            done = true;
            canContinue = allowed;
        });

        while (!done)
            yield return null;

        if (!canContinue) yield break;

        _stateEvents?.RequestAnimation(AnimationCue.Look, _stateId);
        float _delayTime = Context.GetIdleTimeSeconds();//Random.Range(minWait, maxWait);
        float elapsedTime = 0.0f;

        while (elapsedTime < _delayTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (!_isInState) yield break;
        ValidateAndSendCandidateDestinations();

        _runningRoutine = null;

    }



}
