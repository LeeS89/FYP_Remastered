using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

[Obsolete]
public class PlayerFlankingResourcesObsolete : SceneResources, IUpdateableResource
{
    private AsyncOperationHandle<SamplePointDataSO> _flankPointHandle;
    private SamplePointDataSO _flankPointDataSO;
    private List<FlankPointData> _savedPoints;
    private int _nearestPointToPlayer = 0;
    Collider playerCollider;
    /*Vector3 top;*/

    public override async Task LoadResources()
    {
        try
        {
            playerCollider = GameManager.Instance.GetPlayerCollider(PlayerPart.DefenceCollider);

            // NotifyDependancies(); // => Needs Closest Point Job
            // Load the asset from Addressables
            //var flankPointHandle = Addressables.LoadAssetAsync<ScriptableObject>("SamplePointDataSO");
            _flankPointHandle = Addressables.LoadAssetAsync<SamplePointDataSO>("FlankPointData");

            // Wait for the asset to be loaded
            await _flankPointHandle.Task;

            // Check if the loading succeeded
            if (_flankPointHandle.Status == AsyncOperationStatus.Succeeded)
            {
                // Asset is loaded successfully, cast it to the correct type
                _flankPointDataSO = _flankPointHandle.Result;

                if (_flankPointDataSO != null)
                {
                    SceneEventAggregator.Instance.OnClosestFlankPointToPlayerJobComplete += SetNearestIndexToPlayer; // => Dont forget to unsubscribe
                    SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;
                  //  SceneEventAggregator.Instance.OnAIResourceRequested += AIResourceRequested; // => Dont forget to unsubscribe
                    _savedPoints = new List<FlankPointData>(_flankPointDataSO.savedPoints);
                    //SceneEventAggregator.Instance.OnFlankPointsRequested += ResourceRequested; // => Dont forget to unsubscribe
                    //DeserializeSavedPoints(); // Disabled for new approach
                }
                else
                {
                    Debug.LogError("Loaded flank point data is null.");
                }
            }
            else
            {

                Debug.LogError("Failed to load the flank point data from Addressables.");
            }

            NotifyClassDependancies(); // => Needs Closest Point Job
            // Subscribe to the resource requested event
            /*SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;*/
            //SceneEventAggregator.Instance.OnResourceReleased += ResourceReleased;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading player flanking resources: {e.Message}");
        }

    }

    public override async Task UnLoadResources()
    {
        try
        {
            playerCollider = null;
            _savedPoints.Clear();
            _savedPoints = null;
            _flankPointDataSO = null;
            if (_flankPointHandle.IsValid())
            {
                Addressables.Release(_flankPointHandle);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error unloading player flanking resources: {e.Message}");
        }

        await Task.CompletedTask;
    }

    [Obsolete]
    private void DeserializeSavedPoints()
    {
        foreach (var point in _flankPointDataSO.savedPoints)
        {
            point.reachableSteps = new Dictionary<int, List<int>>();

            foreach (var entry in point.serializedReachableSteps)
            {
                point.reachableSteps[entry.step] = new List<int>(entry.reachableIndices);
            }
        }

        _savedPoints = new List<FlankPointData>(_flankPointDataSO.savedPoints);

    }

    protected override void NotifyClassDependancies()
    {
        bool exists = SceneEventAggregator.Instance.CheckDependancyExists(typeof(ClosestPointToPlayerJob));

        if (exists) { return; } // Already exists, no need to add again

        SceneEventAggregator.Instance.AddDependancy(new ClosestPointToPlayerJob(_flankPointDataSO));

    }

    private void SetNearestIndexToPlayer(int nearestPointIndex)
    {

        _nearestPointToPlayer = nearestPointIndex;
    }

    protected override void ResourceRequested(in ResourceRequests request)
    {
        AIResourceType type = request.AIResourceType;

        switch (type)
        {
            case AIResourceType.FlankPointCandidates:
                ProcessFlankPointCandidatesRequest(in request);
                break;
            case AIResourceType.FlankPointEvaluationMasks:
                ProcessFlankPointEvaluationMaskRequest(in request);
                break;
            default:
#if UNITY_EDITOR
                Debug.Log("Different type of resource requested, return early");
#endif
                return;
        }
    }

   

    private void ProcessFlankPointEvaluationMaskRequest(in ResourceRequests request)
    {
        LayerMask blockingMask = _flankPointDataSO.flankBlockingMask;
        LayerMask targetMask = _flankPointDataSO.flankTargetMask;
        LayerMask secondaryTargetMask = _flankPointDataSO.flankSecondaryTargetMask;
        request.FlankPointTargetAndBlockingMasksCallback?.Invoke(blockingMask, targetMask, secondaryTargetMask);
    }

    private void ProcessFlankPointCandidatesRequest(in ResourceRequests request)
    {
        int step = request.FlankCandidateSteps;
        var buffer = request.FlankCandidates;
        if (_savedPoints == null || _savedPoints.Count == 0 ||
            _nearestPointToPlayer < 0 || _nearestPointToPlayer >= _savedPoints.Count)
        {
            request.FlankCallback?.Invoke(false);
            return;
        }

        FlankPointData playerPoint = _savedPoints[_nearestPointToPlayer];

        StepEntry stepEntry = null;
        for (int i = 0; i < playerPoint.stepLinks.Count; i++)
        {
            if (playerPoint.stepLinks[i].step == step)
            {
                stepEntry = playerPoint.stepLinks[i];
                break;
            }
        }

        if (stepEntry != null)
        {
            foreach (int index in stepEntry.reachableIndices)
            {
                if (index >= 0 && index < _savedPoints.Count)
                {
                    var point = _savedPoints[index];
                    if (!point.inUse)
                    {
                        buffer.Add(point);
                    }

                }
            }
        }

        request.FlankCallback?.Invoke(buffer.Count > 0);
    }

  

    public void UpdateResource()
    {
        return;
        if(_savedPoints == null || _savedPoints.Count == 0) { return; }
        Vector3 top = playerCollider.bounds.center + Vector3.up * playerCollider.bounds.extents.y;
        /*Collider playerPos = GameManager.Instance.GetPlayerCollider(PlayerPart.DefenceCollider);
        center = playerPos.bounds.center + Vector3.up * playerPos.bounds.extents.y;*/
        //Transform playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.DefenceCollider);
        int mask = LayerMask.GetMask("Default", "Water", "PlayerDefence");
        //Debug.LogError("MASK IS: "+mask);
        foreach (var point in _savedPoints)
        {
            if(Physics.Linecast(point.position, top, out RaycastHit hit, mask))
            {
                //Debug.LogError("Hit layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer) + "GameObject name: "+hit.collider.gameObject.name);
               
                if (hit.collider == playerCollider)
                {
                    //Debug.LogError("Hit Player Defence Collider");
                    Debug.DrawLine(point.position, top, Color.green);
                }
                else
                {
                    Debug.DrawLine(point.position, top, Color.red);
                }
            }
            
        }

        
    }
}





























public class PlayerFlankingResourcesNew : SceneResources, IFlankService, ITickable
{
    private AsyncOperationHandle<SamplePointDataSO> _flankPointHandle;
    private SamplePointDataSO _flankPointDataSO;
    private List<FlankPointData> _savedPoints;
   // private int _nearestPointToPlayer = 0;
   // Collider playerCollider;
    /*Vector3 top;*/
    private IClosestFlankPointService _closestFlankPointService;

    public PlayerFlankingResourcesNew(IClosestFlankPointService closestFlankPointService, string sceneName)
    {
        _closestFlankPointService = closestFlankPointService;
    }

    public override async Task LoadResources()
    {
        try
        {
           // playerCollider = GameManager.Instance.GetPlayerCollider(PlayerPart.DefenceCollider);

            // NotifyDependancies(); // => Needs Closest Point Job
            // Load the asset from Addressables
            //var flankPointHandle = Addressables.LoadAssetAsync<ScriptableObject>("SamplePointDataSO");
            _flankPointHandle = Addressables.LoadAssetAsync<SamplePointDataSO>("FlankPointData");

            // Wait for the asset to be loaded
            await _flankPointHandle.Task;

            // Check if the loading succeeded
            if (_flankPointHandle.Status == AsyncOperationStatus.Succeeded)
            {
                // Asset is loaded successfully, cast it to the correct type
                _flankPointDataSO = _flankPointHandle.Result;

                if (_flankPointDataSO != null)
                {
                   // SceneEventAggregator.Instance.OnClosestFlankPointToPlayerJobComplete += SetNearestIndexToPlayer; // => Dont forget to unsubscribe
                   // SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;
                  //  SceneEventAggregator.Instance.OnAIResourceRequested += AIResourceRequested; // => Dont forget to unsubscribe
                    _savedPoints = new List<FlankPointData>(_flankPointDataSO.savedPoints);
                    //SceneEventAggregator.Instance.OnFlankPointsRequested += ResourceRequested; // => Dont forget to unsubscribe
                    //DeserializeSavedPoints(); // Disabled for new approach
                }
                else
                {
                    Debug.LogError("Loaded flank point data is null.");
                }
            }
            else
            {

                Debug.LogError("Failed to load the flank point data from Addressables.");
            }

            //NotifyClassDependancies(); // => Needs Closest Point Job
            // Subscribe to the resource requested event
            /*SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested;*/
            //SceneEventAggregator.Instance.OnResourceReleased += ResourceReleased;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading player flanking resources: {e.Message}");
        }

    }

    public override async Task UnLoadResources()
    {
        try
        {
           // playerCollider = null;
            _savedPoints.Clear();
            _savedPoints = null;
            _flankPointDataSO = null;
            if (_flankPointHandle.IsValid())
            {
                Addressables.Release(_flankPointHandle);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error unloading player flanking resources: {e.Message}");
        }

        await Task.CompletedTask;
    }

    [Obsolete]
    private void DeserializeSavedPoints()
    {
        foreach (var point in _flankPointDataSO.savedPoints)
        {
            point.reachableSteps = new Dictionary<int, List<int>>();

            foreach (var entry in point.serializedReachableSteps)
            {
                point.reachableSteps[entry.step] = new List<int>(entry.reachableIndices);
            }
        }

        _savedPoints = new List<FlankPointData>(_flankPointDataSO.savedPoints);

    }

    [Obsolete]
    protected override void NotifyClassDependancies()
    {
        bool exists = SceneEventAggregator.Instance.CheckDependancyExists(typeof(ClosestPointToPlayerJob));

        if (exists) { return; } // Already exists, no need to add again

        SceneEventAggregator.Instance.AddDependancy(new ClosestPointToPlayerJob(_flankPointDataSO));

    }

    [Obsolete]
    protected override void ResourceRequested(in ResourceRequests request)
    {
        AIResourceType type = request.AIResourceType;

        switch (type)
        {
            case AIResourceType.FlankPointCandidates:
              //  ProcessFlankPointCandidatesRequest(in request);
                break;
            case AIResourceType.FlankPointEvaluationMasks:
                ProcessFlankPointEvaluationMaskRequest(in request);
                break;
            default:
#if UNITY_EDITOR
                Debug.Log("Different type of resource requested, return early");
#endif
                return;
        }
    }


    [Obsolete]
    private void ProcessFlankPointEvaluationMaskRequest(in ResourceRequests request)
    {
        LayerMask blockingMask = _flankPointDataSO.flankBlockingMask;
        LayerMask targetMask = _flankPointDataSO.flankTargetMask;
        LayerMask secondaryTargetMask = _flankPointDataSO.flankSecondaryTargetMask;
        request.FlankPointTargetAndBlockingMasksCallback?.Invoke(blockingMask, targetMask, secondaryTargetMask);
    }

   

    public void TryGetFlankCandidates(Vector3 flankTargetPos, int numSteps, List<Vector3> buffer, Action<bool> OnRequestComplete)
    {
        if(_closestFlankPointService == null) OnRequestComplete?.Invoke(false);

        int id = _nextId++;

        _pendingRequests[id] = new FlankRequest
        {
            FlankSteps = numSteps,
            Buffer = buffer,
            OnRequestComplete = OnRequestComplete
        };

        _closestFlankPointService?.RequestClosestIndex(id, flankTargetPos, OnClosestIndexReceived);
    }

    private void OnClosestIndexReceived(int requestId, int closestIndex, bool success)
    {
        if(_pendingRequests.TryGetValue(requestId, out var req))
        {
            _pendingRequests.Remove(requestId);

            req.Buffer.Clear();

            if(!success || closestIndex < 0)
            {
                req.OnRequestComplete?.Invoke(false);
                return;
            }

            ProcessFlankPointCandidatesRequestNew(closestIndex, in req);

        }

    }

    private void ProcessFlankPointCandidatesRequestNew(int index, in FlankRequest request)
    {
        int step = request.FlankSteps;
        var buffer = request.Buffer;
        if (_savedPoints == null || _savedPoints.Count == 0 ||
            index < 0 || index >= _savedPoints.Count)
        {
            request.OnRequestComplete?.Invoke(false);
            return;
        }

        FlankPointData targetPoint = _savedPoints[index];

        StepEntry stepEntry = null;
        for (int i = 0; i < targetPoint.stepLinks.Count; i++)
        {
            if (targetPoint.stepLinks[i].step == step)
            {
                stepEntry = targetPoint.stepLinks[i];
                break;
            }
        }

        if (stepEntry != null)
        {
            foreach (int i in stepEntry.reachableIndices)
            {
                if (i >= 0 && i < _savedPoints.Count)
                {
                    var point = _savedPoints[i];
                    if (!point.inUse)
                    {
                        buffer.Add(point.position);
                    }

                }
            }
        }

        request.OnRequestComplete?.Invoke(true);
    }

    private struct FlankRequest
    {
        public int FlankSteps;
        public List<Vector3> Buffer;
        public Action<bool> OnRequestComplete;
    }

    private int _nextId = 1;
    private readonly Dictionary<int, FlankRequest> _pendingRequests = new(25);


    

    public void Tick(float dt) { }
    

    public void LateTick(float dt) { }

    public Task InitialiseAsync(IResourceLocation location)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
