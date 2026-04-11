using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class FsmBaseState<TProvider> : IFsmState
 where TProvider : IFsmData
{

    protected Coroutine _runningRoutine;
    protected bool _isAtDestination = false;
    protected bool _isInState = false;
    protected DestinationResultCallback _validationCallback;


    protected readonly List<Vector3> _candidateDestinations = new();

    public StateId GetId() => _stateId;
    protected readonly StateId _stateId = StateId.None;

    protected readonly ICoroutineHost _host;

    #region New region

    protected readonly IFsmDestinationProvider _destProvider;
    protected readonly TProvider _dataProvider;
    protected readonly IFsmStateContext _stateContext;

    protected readonly IPathResolver _pathResolver;

    public FsmBaseState(IFsmStateContext stateController, IFsmDestinationProvider destP,
        TProvider dataProvider, IPathResolver pathResolver, ICoroutineHost host, StateId id)
    {
        _stateContext = stateController;

        _destProvider = destP;
        _dataProvider = dataProvider;
        _pathResolver = pathResolver;
        _host = host;
        _stateId = id;
        _validationCallback = OnProcessedDestinationsResult;

        if (_pathResolver == null) DebugLogs.Nre(_pathResolver, "Path resolver", this);
        else DebugLogs.Err("Path resolver was not null", this);

        //  Service.TryGetDestinationCandidates(_stateEvents, new List<Vector3>());
    }

    #endregion



    protected bool TryGetPath(out NavMeshPath path) => _stateContext.TryGetPath(out path);


    protected bool TryGetCurrentPosition(out Vector3? pos) => _stateContext.TryGetCurrentPosition(out pos);


    /* protected bool TargetIsMoving()
     {
         ITargetable target;
         if (!TryGetTarget(out target)) return false;

         return target.IsMoving();
     }*/




    public virtual bool NeedsNewPath() => false;


    //protected bool OwnerDataNull() => _owner == null || _path == null;
    // protected bool IsStationary() => _stateContext?.HasReachedDestination() ?? true;

    public virtual void EnterState() { DebugLogs.Err($"Entering {_stateId.ToString()} state", this); _isInState = true; RetrieveCandidateDestinations(); }
    protected abstract void ValidateAndSendCandidateDestinations();
    protected virtual void ValidateAndSendCandidateDestinationsNew() { }
    protected abstract void RetrieveCandidateDestinations();
    public void TryRepath()
    {
        if (!_isInState) return;
        RetrieveCandidateDestinations();
    }

    protected virtual void OnProcessedDestinationsResult(in DestinationResultInfo result)
    {
        if (!_isInState) return;
        // Debug.LogError("Sending Dest Result from: "+ _id.ToString());
        DebugLogs.Err("FORWARDING PATH RESULT TO MANAGER", this);
        _stateContext?.ProcessDestinationResult(in result);
    }



    protected void CancelCurrentPathRequests() => _pathResolver?.CancelAll();

    public virtual void ExitState()
    {
        _isInState = false;
        //_pathResolver?.CancelAll();
        if (_runningRoutine != null)
        {
            CoroutineRunner.Instance.StopCoroutine(_runningRoutine);
            _runningRoutine = null;
        }
    }
    public virtual void OnDestinationReached() => _isAtDestination = true;

    public virtual void OnDestinationSet() => _isAtDestination = false;

    public virtual void Tick(float dt) { }

    public virtual void LateTick(float dt) { }

    protected virtual void ShuffleCandidateList<T>(List<T> candidates)
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            int randIndex = Random.Range(i, candidates.Count);
            (candidates[i], candidates[randIndex]) = (candidates[randIndex], candidates[i]);
        }
    }

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    public float GetArrivalThreshold() => _destProvider.GetArrivalThreshold();//0.0f;//_deps?.GetStoppingDistance() ?? 0f;  => Had error with new setup


}

