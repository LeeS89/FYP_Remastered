using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class FSMPatrolState : FsmBaseState
{
    private readonly IWaypointService _waypointService;
    private readonly IPatrolDeps _patrolDeps;

    /*public FSMPatrolState(IWaypointService waypointService, IAgentData data, IPathResolver resolver, IFSMStateContext stateContext) 
        : base(data, resolver, stateContext, StateId.Patrol)
    {
        _waypointService = waypointService;
        _candidateDestinations.EnsureCapacity(10);
    }*/
    public FSMPatrolState(IPatrolDeps deps, IFSMStateContext stateContext, bool useRandomStopDistance = false) 
        : base(deps, stateContext, useRandomStopDistance, StateId.Patrol)
    {
        _patrolDeps = deps;
        _waypointService = _patrolDeps.WaypointService;
        _candidateDestinations.EnsureCapacity(10);
    }


    public override void OnDestinationReached()
    {
        if (!_isInState || _owner == null) return;

        if (_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PatrolWaitRoutine(
                _owner.Transform, _patrolDeps.MinTimeAtPatrolPoint, _patrolDeps.MaxTimeAtPatrolPoint));
    }

    protected override void RetrieveCandidateDestinations()
    {
        if (_candidateDestinations.Count == 0)
        {
            if (_waypointService == null || !_waypointService.TryGetWaypoints(this, _candidateDestinations))
            {
                DestinationResultInfo failedResult = new DestinationResultInfo
                (
                    ReasonForDestinationCheck.ValidatePathForDestination,
                    _path,
                    DestinationResult.CandidatesNullOrEmpty,
                    Vector3.zero,
                    _id
                );

                base.OnPathResultReceived(in failedResult);
                return;
            }
        }
        ValidateCandidateDestinations();
    }

    protected override void ValidateCandidateDestinations()
    {
        if(_candidateDestinations.Count > 1)
        {
         
            var temp = _candidateDestinations[0];
            _candidateDestinations.RemoveAt(0);
            ShuffleCandidateList(_candidateDestinations);
            _candidateDestinations.Add(temp);
        }
        //ContinueRoutine = true;
        DestinationRequest req = new DestinationRequest(_id, _owner.Position(), _candidateDestinations, _path, 
            ReasonForDestinationCheck.ValidatePathForDestination, _validationCallback);
        _pathResolver?.ProcessDestinationCandidates(in req);


       /* _pathResolver?.ProcessDestinationCandidates(_id, ReasonForDestinationCheck.ValidatePathForDestination,
            _candidateDestinations, _path, _owner.Position(), _validationCallback);*/

    }

  

    private IEnumerator PatrolWaitRoutine(Transform t, float minWait, float maxWait)
    {
        Debug.LogError("Patrol wait routine called");
       // if (forward != null)
     //   {
            float randomAngle = Random.Range(-180, 180);
            Vector3 dirOffset = Quaternion.AngleAxis(randomAngle, _owner.Transform.up) * _owner.Transform.forward;
            Quaternion targetRot = Quaternion.LookRotation(dirOffset, _owner.Transform.up);
            //Quaternion targetRot = Quaternion.LookRotation(forward.Value);
            while (Quaternion.Angle(t.rotation, targetRot) > 2.0f + Mathf.Epsilon)
            {
                t.rotation = Quaternion.Slerp(t.rotation, targetRot, Time.deltaTime * 2f);
                yield return null;
            }

       // }
        if (!_isInState) yield break;

        _stateContext?.OnAnimationIntent?.Invoke(AnimationCue.Look);
      
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
