using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public abstract class FsmBaseState<TDeps> : IFsmState where TDeps : FsmBaseState<TDeps>.FsmBaseStateDeps
{
 //   protected readonly IPathResolver _pathResolver;
    protected readonly IFsmStateEvents _stateEvents;
    //protected readonly IFsmNotificationSource _stateContext;
    protected Coroutine _runningRoutine;
    protected bool _isAtDestination = false;
    protected bool _isInState = false;
    protected DestinationResultCallback _validationCallback;


    protected List<Vector3> _candidateDestinations = new();

    //new
    //protected readonly ITargetable _owner;
    protected TDeps _deps;
    private SharedFsmStateServices _sharedDeps;
   // protected NavMeshPath _path;
    // end new

    public StateId GetId() => _id;
    protected readonly StateId _id = StateId.None;




    public FsmBaseState(TDeps deps, SharedFsmStateServices sharedDeps, IFsmStateEvents stateEvents, StateId id)
    {
        _deps = deps;
        _sharedDeps = sharedDeps;
      //  _owner = deps.Owner;
      //  _path = deps.Path();
      //  _pathResolver = deps.PathResolver;
        _stateEvents = stateEvents;
        _id = id;
        _validationCallback = OnProcessedDestinationsResult;
    }

    private bool SharedDepsIsNull() => _sharedDeps == null; // Maybe new Notification

    // Need to make private once the Distance job is updated to accept transform instead of ITargetable
    /*private*/protected bool TryGetTarget(out ITargetable target)
    {
        if (SharedDepsIsNull()) { Debug.LogError("Shared is null"); target = null; return false; }

        if (_sharedDeps.OnTryGetCurrentTarget?.Invoke(out target) == true) return true;

        Debug.LogError("Delegate is null or returned null");
        target = null;
        //target = _sharedDeps.GetCurrentTarget?.Invoke();
        return false; // Maybe new notification if target is null or Func is not set
    }

    protected bool TryGetTargetPosition(out Vector3 pos)
    {
        ITargetable t;
        if (!TryGetTarget(out t)) { pos = Vector3.zero; return false; }
        pos = t.Position().Value;
        return true;
    }

    private bool OwnerIsNull()
    {
        if (!SharedDepsIsNull()) return _sharedDeps.OwnerTransform == null; // Maybe new Notification
        return true;
    }

    protected bool TryGetOwnerTransform(out Transform t)
    {
        if (OwnerIsNull()) { t = null; return false; }
        t = _sharedDeps.OwnerTransform;
        return true;
    }

    protected bool TryGetPath(out NavMeshPath path)
    {
        if (SharedDepsIsNull()) { path = null; return false; };
        path = _sharedDeps.Path;
        return path != null; // Maybe new notification if path is null
    }

    protected bool TryGetOwnerPosition(out Vector3 pos)
    {
        if (OwnerIsNull()) { pos = Vector3.zero; return false; }

        pos = _sharedDeps.OwnerTransform.position;
        return true;
    }
    
    protected bool TargetIsMoving()
    {
        ITargetable target;
        if (!TryGetTarget(out target)) return false;

        return target.IsMoving();
    }

    

    public virtual bool NeedsNewPath() => false;

    private bool DepsIsNull() => _deps == null;
    protected bool ResolverIsNull() => DepsIsNull() || _deps.PathResolver == null;

    //protected bool OwnerDataNull() => _owner == null || _path == null;
    // protected bool IsStationary() => _stateContext?.HasReachedDestination() ?? true;

    public virtual void EnterState() { _isInState = true; RetrieveCandidateDestinations(); }
    protected abstract void ValidateCandidateDestinations();
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
        _stateEvents?.ProcessDestinationResult(in result);
    }

    

    protected void CancelCurrentPathRequests()
    {
        if (ResolverIsNull()) return;
        _deps.PathResolver.CancelAll();

    }
    
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

    public float GetDesiredStoppingDistance() => _deps?.GetStoppingDistance() ?? 0f;
    

    public abstract class FsmBaseStateDeps 
    { 
        public readonly IPathResolver PathResolver;

        public FsmBaseStateDeps(IPathResolver pathResolver) => PathResolver = pathResolver;

        public virtual float GetStoppingDistance() => 0f; 
    }
}











public class CoTest
{
    public void HIDY() { }
}
public class CoTestTwo : CoTest
{
    public void Howdy() { }
}

public class CoTestThree
{
    CoTest first = new CoTest();
    CoTestTwo second = new CoTestTwo();

    List<CoTest> _list = new List<CoTest>();
    List<CoTestTwo> _list2 = new List<CoTestTwo>();
    List<MonoBehaviour> _ints = new List<MonoBehaviour>();

    public void Begin()
    {
        _list.Add(first);
        _list.Add(second);

//        Testing(_ints);
    }

    public void Testing(IEnumerable<CoTest> _list)
    {
        foreach (var _ in _list)
        {
            _.HIDY();
        }
    }
}















public abstract class FsmBaseStateNew<TService, TContext> : IFsmStateNew<TService> 
  //  where TContext : IContext
    where TService : IFsmStateService<TContext>// where TContext : FsmBaseState<TContext>.FsmBaseStateDeps
    where TContext : IContext
{
 //   protected readonly IPathResolver _pathResolver;
    protected readonly IFsmStateEvents _stateEvents;
    //protected readonly IFsmNotificationSource _stateContext;
    protected Coroutine _runningRoutine;
    protected bool _isAtDestination = false;
    protected bool _isInState = false;
    protected DestinationResultCallback _validationCallback;


    protected readonly List<Vector3> _candidateDestinations = new();

    //new
    //protected readonly ITargetable _owner;
   // protected TContext _deps;
  //  private SharedFsmStateServices _sharedDeps;
   // protected NavMeshPath _path;
    // end new

    public StateId GetId() => _stateId;
    protected readonly StateId _stateId = StateId.None;

    protected readonly ICoroutineHost _host;

    #region New region
    protected TService Service { get; private set; }

   // TService IFsmStateNew<TService>.Context => Context;

    //internal void SetContext(TContext context) => Context = context;
    protected readonly IPathResolver _pathResolver;

    public FsmBaseStateNew(IFsmStateEvents stateController, TService service, IPathResolver pathResolver, ICoroutineHost host, StateId id)
    {
        _stateEvents = stateController;
        Service = service;
        _pathResolver = pathResolver;
        _host = host;
        _stateId = id;
        _validationCallback = OnProcessedDestinationsResult;

        if (_pathResolver == null) DebugLogs.Nre(_pathResolver, "Path resolver", this);
        else DebugLogs.Err("Path resolver was not null", this);

        Service.TryGetDestinationCandidates(_stateEvents, new List<Vector3>());
    }

    #endregion










  /*  public FsmBaseStateNew(TContext deps, SharedFsmStateServices sharedDeps, IFsmStateEvents stateEvents, StateId id)
    {
        _deps = deps;
        _sharedDeps = sharedDeps;
      //  _owner = deps.Owner;
      //  _path = deps.Path();
      //  _pathResolver = deps.PathResolver;
        _stateEvents = stateEvents;
        _id = id;
        _validationCallback = OnProcessedDestinationsResult;

       
    }
*/
   // private bool SharedDepsIsNull() => _sharedDeps == null; // Maybe new Notification

    // Need to make private once the Distance job is updated to accept transform instead of ITargetable
    /*private*//*protected bool TryGetTarget(out ITargetable target)
    {
        if (SharedDepsIsNull()) { Debug.LogError("Shared is null"); target = null; return false; }

        if (_sharedDeps.OnTryGetCurrentTarget?.Invoke(out target) == true) return true;

        Debug.LogError("Delegate is null or returned null");
        target = null;
        //target = _sharedDeps.GetCurrentTarget?.Invoke();
        return false; // Maybe new notification if target is null or Func is not set
    }*/

  /*  protected bool TryGetTargetPosition(out Vector3 pos)
    {
        ITargetable t;
        if (!TryGetTarget(out t)) { pos = Vector3.zero; return false; }
        pos = t.Position().Value;
        return true;
    }*/

   /* private bool OwnerIsNull()
    {
        if (!SharedDepsIsNull()) return _sharedDeps.OwnerTransform == null; // Maybe new Notification
        return true;
    }*/

   /* protected bool TryGetOwnerTransform(out Transform t)
    {
        
        if (OwnerIsNull()) { t = null; return false; }
        t = _sharedDeps.OwnerTransform;
        return true;
    }*/

    protected bool TryGetPath(out NavMeshPath path) => Service.TryGetPath(_stateEvents, out path);
    /*{
        if (SharedDepsIsNull()) { path = null; return false; };
        path = _sharedDeps.Path;
        return path != null; // Maybe new notification if path is null
    }*/

    protected bool TryGetCurrentPosition(out Vector3 pos) =>  Service.TryGetCurrentPosition(_stateEvents, out pos);
   /* {
        if (OwnerIsNull()) { pos = Vector3.zero; return false; }

       

       *//* pos = _sharedDeps.OwnerTransform.position;
        return true;*//*
    }*/
    
   /* protected bool TargetIsMoving()
    {
        ITargetable target;
        if (!TryGetTarget(out target)) return false;

        return target.IsMoving();
    }*/

 /*   protected bool TryGetOwnerTransform(out Transform ownerTransform)
    {
        ownerTransform = null;
        return false;
    }

*/

    protected bool TryGetCurrentPositionAndPath(out Vector3 position, out NavMeshPath path)
        => Service.TryGetCurrentPositionAndPath(_stateEvents, out position, out path);

    public virtual bool NeedsNewPath() => false;

 //   private bool DepsIsNull() => _deps == null;
  //  protected bool ResolverIsNull() => DepsIsNull();// || _deps.PathResolver == null;  => Had error with new setup

    //protected bool OwnerDataNull() => _owner == null || _path == null;
    // protected bool IsStationary() => _stateContext?.HasReachedDestination() ?? true;

    public virtual void EnterState() { DebugLogs.Err($"Entering {_stateId.ToString()} state", this);  _isInState = true; RetrieveCandidateDestinations(); }
    protected abstract void ValidateAndSendCandidateDestinations();
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
        _stateEvents?.ProcessDestinationResult(in result);
    }



    protected void CancelCurrentPathRequests() => _pathResolver.CancelAll();
  /*  {
        if (ResolverIsNull()) return;
       // _deps.PathResolver.CancelAll(); => Had error with new setup

    }*/
    
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

    public float GetDesiredStoppingDistance() => 0.0f;//_deps?.GetStoppingDistance() ?? 0f;  => Had error with new setup



  /*  public abstract class FsmBaseStateDeps 
    { 
        public readonly IPathResolver PathResolver;

        public FsmBaseStateDeps(IPathResolver pathResolver) => PathResolver = pathResolver;

        public virtual float GetStoppingDistance() => 0f; 
    }*/
}

















































public abstract class FsmBaseStateNewest<TProvider> : IFsmStateNewest
 where TProvider : IFsmDataProvider
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

    public FsmBaseStateNewest(IFsmStateContext stateController, IFsmDestinationProvider destP, 
        TProvider dataP, IPathResolver pathResolver, ICoroutineHost host, StateId id)
    {
        _stateContext = stateController;
        
        _destProvider = destP;
        _dataProvider = dataP;
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
    

    protected bool TryGetCurrentPosition(out Vector3? pos) =>  _stateContext.TryGetCurrentPosition(out pos);

    
   /* protected bool TargetIsMoving()
    {
        ITargetable target;
        if (!TryGetTarget(out target)) return false;

        return target.IsMoving();
    }*/

 


    public virtual bool NeedsNewPath() => false;

 
    //protected bool OwnerDataNull() => _owner == null || _path == null;
    // protected bool IsStationary() => _stateContext?.HasReachedDestination() ?? true;

    public virtual void EnterState() { DebugLogs.Err($"Entering {_stateId.ToString()} state", this);  _isInState = true; RetrieveCandidateDestinations(); }
    protected abstract void ValidateAndSendCandidateDestinations();
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

    public float GetDesiredStoppingDistance() => 0.0f;//_deps?.GetStoppingDistance() ?? 0f;  => Had error with new setup


}
