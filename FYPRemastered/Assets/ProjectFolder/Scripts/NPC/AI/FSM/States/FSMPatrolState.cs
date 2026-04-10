using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;

public sealed class FsmPatrolState : FsmBaseState<IFsmPatrolData>
{

    public FsmPatrolState(IFsmStateContext stateController, IFsmDestinationProvider destP, IFsmPatrolData dataP, IPathResolver pathResolver, ICoroutineHost host)
        : base(stateController, destP, dataP, pathResolver, host, StateId.Patrol)
    {
        _candidateDestinations.EnsureCapacity(10);
    }


    public override void OnDestinationReached()
    {
        if (!_isInState) return;

        Transform ownerTransform;
        // if (!TryGetOwnerTransform(out ownerTransform)) return;


        if (_runningRoutine == null)
            _runningRoutine = _host?.StartCoroutine(PatrolWaitRoutineNew(/*0.5f, 7f)*/));//CoroutineRunner.Instance.StartCoroutine(PatrolWaitRoutineNew(
                                                                                         // 0.5f, 7f));
        /*if (_runningRoutine == null)
            _runningRoutine = CoroutineRunner.Instance.StartCoroutine(PatrolWaitRoutineNew(
                ownerTransform, _deps.MinTimeAtPatrolPoint, _deps.MaxTimeAtPatrolPoint));*/

    }

    protected override void RetrieveCandidateDestinations()
    {

        if (!_isInState || _candidateDestinations is null) return;

        if (_candidateDestinations.Count is 0)
        {
            NavMeshPath path;
            if (!TryGetPath(out path)) return;

            Debug.LogError("successfully retrieved Path");

            if (_destProvider is null || !_destProvider.TryGetDestinationCandidates(_candidateDestinations))
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
            ReasonForDestinationCheck.ValidatePathForDestination, _validationCallback);

        _pathResolver?.ProcessDestinationCandidates(in req);

    }



    private IEnumerator PatrolWaitRoutineNew(/*float minWait, float maxWait*/)
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
        ValidateAndSendCandidateDestinations();

        _runningRoutine = null;

    }



}
