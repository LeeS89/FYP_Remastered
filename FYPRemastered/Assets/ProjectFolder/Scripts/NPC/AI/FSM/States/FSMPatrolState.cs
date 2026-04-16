using System.Collections;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;

public sealed class FsmPatrolState : FsmBaseState<IFsmPatrolData>
{

    public FsmPatrolState(/*IFsmStateContext stateController,*/ IFsmPatrolData dataP, IDestinationResolver pathResolver, ICoroutineHost host)
        : base(/*stateController,*/ dataP, pathResolver, host, StateId.Patrol)
    {
        _candidateDestinations.EnsureCapacity(10);
    }


    public override void OnDestinationReached()
    {
        if (!_isInState) return;

        if (_runningRoutine == null)
            _runningRoutine = _host?.StartCoroutine(PatrolWaitRoutineNew());
      
    }

    protected override void RetrieveCandidateDestinations()
    {

        if (!_isInState || _candidateDestinations is null) return;

        if (_candidateDestinations.Count is 0)
        {
            NavMeshPath path;
            if (!TryGetPath(out path)) return;

            DebugLogs.Log("successfully retrieved Path");

            if (_dataProvider is null || !_dataProvider.TryGetDestinationCandidates(_candidateDestinations))
            {
                DebugLogs.Err("Returning Failed Result for patrol", this);
                DestinationResultInfo failedResult = new DestinationResultInfo
                (
                    DestinationRequestReason.ValidatePathForDestination,
                    path,
                    DestinationResult.CandidatesNullOrEmpty,
                    Vector3.zero,
                    _stateId
                );

                base.OnProcessedDestinationsResult(in failedResult);
                return;
            }
        }

        ShuffleList(_candidateDestinations);
        /*if (_candidateDestinations.Count > 1)
        {

            var temp = _candidateDestinations[0];
            _candidateDestinations.RemoveAt(0);
            ShuffleList(_candidateDestinations);
            _candidateDestinations.Add(temp);
        }*/
        CreateDestinationRequest(DestinationRequestReason.ValidatePathForDestination);
       
    }


   

    private IEnumerator PatrolWaitRoutineNew()
    {
        Debug.LogError("Patrol wait routine called");

        float randomAngle = Random.Range(-180, 180);
        bool done = false;
        bool canContinue = false;


        _stateContext?.RequestRotation(randomAngle, _stateId, allowed =>
        {
            done = true;
            canContinue = allowed;
        });

        while (!done)
            yield return null;

        if (!canContinue) yield break;

        _stateContext?.RequestAnimation(AnimationCue.Look, _stateId);
        float _delayTime = _dataProvider.GetIdleTimeSeconds();
        DebugLogs.Err($"Wait Time is: {_delayTime}", this);
        float elapsedTime = 0.0f;

        while (elapsedTime < _delayTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (!_isInState) yield break;
        RetrieveCandidateDestinations();
       
        _runningRoutine = null;

    }



    #region Obsolete code
    /*    protected override async void ValidateAndSendCandidateDestinationsNew()
    {
        if (!_isInState || _candidateDestinations is null || _candidateDestinations.Count is 0) return;

        if (!TryGetCurrentPosition(out var pos) ||
            !TryGetPath(out var path)) return;

        DebugLogs.Err("GOTTEN PATH AND OWNER POS in new await function", this);

        if (_candidateDestinations.Count > 1)
        {

            var temp = _candidateDestinations[0];
            _candidateDestinations.RemoveAt(0);
            ShuffleCandidateList(_candidateDestinations);
            _candidateDestinations.Add(temp);
        }

        DestinationRequest req = new DestinationRequest(_stateId, pos.Value, _candidateDestinations, path,
            DestinationRequestReason.ValidatePathForDestination, _validationCallback);

        var result = await _pathResolver.ProcessCandidates(in req);

        OnProcessedDestinationsResult(in result);
    }*/

    /*    protected override void ValidateAndSendCandidateDestinations()
        {
            if (!_isInState || _candidateDestinations is null) return;


            if (!TryGetCurrentPosition(out var pos) ||
                !TryGetPath(out var path)) return;

            DebugLogs.Err("GOTTEN PATH AND OWNER POS", this);

            if (_candidateDestinations.Count > 1)
            {

                var temp = _candidateDestinations[0];
                _candidateDestinations.RemoveAt(0);
                ShuffleCandidateList(_candidateDestinations);
                _candidateDestinations.Add(temp);
            }

            DestinationRequest req = new DestinationRequest(_stateId, pos.Value, _candidateDestinations, path,
                DestinationRequestReason.ValidatePathForDestination, _validationCallback);

            _pathResolver?.ProcessDestinationCandidates(in req);

        }*/

    #endregion

}
