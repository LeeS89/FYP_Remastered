using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class FSMPatrolState : FSMBaseState
{
    private readonly IWaypointService _waypointService;

    public FSMPatrolState(IWaypointService waypointService, IAgentData data, IPathResolver resolver, IFSMStateContext stateContext) 
        : base(data, resolver, stateContext, StateId.Patrol)
    {
        _waypointService = waypointService;
        _candidateDestinations.EnsureCapacity(10);
    }



    public override void EnterState() { base.EnterState(); RetrieveCandidateDestinations(); }


    public override void OnDestinationReached()
    {
        if (!_isInState || _ownerData == null) return;

        if (_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PatrolWaitRoutine(
                _ownerData.Transform, _ownerData.MinPatrolPointWaitTime, _ownerData.MaxPatrolPointWaitTime, 
                _stateContext.CurrentDestinationForward));
    }

    protected override void RetrieveCandidateDestinations()
    {
        if (_candidateDestinations.Count == 0)
        {
            if (_waypointService == null || !_waypointService.TryGetWaypoints(this, _candidateDestinations))
            {
                DestinationResultNew failedResult = new DestinationResultNew
                (
                    ReasonForDestinationCheck.ValidatePathForDestination,
                    _ownerData.Path,
                    PathResult.CandidatesNullOrEmpty,
                    Vector3.zero,
                    _id
                );

                base.OnPathResultReceived(in failedResult);
                return;
            }
        }
        ValidateCandidateDestinations();
    }

    public override void ValidateCandidateDestinations()
    {
        if(_candidateDestinations.Count > 1)
        {
         
            var temp = _candidateDestinations[0];
            _candidateDestinations.RemoveAt(0);
            ShuffleCandidateList(_candidateDestinations);
            _candidateDestinations.Add(temp);
        }
        //ContinueRoutine = true;
        _pathFinder?.ProcessDestinationCandidates(_id, ReasonForDestinationCheck.ValidatePathForDestination,
            _candidateDestinations, _ownerData.Path, _ownerData.Position(), _validationCallback);

        /*var request = ValidateDestination.GetPatrolPoint(_ownerData, _ownerData.Path);
        _pathFinder?.TryGetDestination(request);*/
    }

  

    private IEnumerator PatrolWaitRoutine(Transform t, float minWait, float maxWait, Vector3? forward)
    {
        Debug.LogError("Patrol wait routine called");
        if (forward != null)
        {
            Quaternion targetRot = Quaternion.LookRotation(forward.Value);
            while (Quaternion.Angle(t.rotation, targetRot) > 2.0f + Mathf.Epsilon)
            {
                t.rotation = Quaternion.Slerp(t.rotation, targetRot, Time.deltaTime * 2f);
                yield return null;
            }

        }
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
