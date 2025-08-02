using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

public class PlayerFlankingResources : SceneResources, IUpdateableResource
{
    private SamplePointDataSO _flankPointDataSO;
    private List<SamplePointData> _savedPoints;
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
            var flankPointHandle = Addressables.LoadAssetAsync<ScriptableObject>("SamplePointDataSO");

            // Wait for the asset to be loaded
            await flankPointHandle.Task;

            // Check if the loading succeeded
            if (flankPointHandle.Status == AsyncOperationStatus.Succeeded)
            {
                // Asset is loaded successfully, cast it to the correct type
                _flankPointDataSO = (SamplePointDataSO)flankPointHandle.Result;

                if (_flankPointDataSO != null)
                {
                    SceneEventAggregator.Instance.OnClosestFlankPointToPlayerJobComplete += SetNearestIndexToPlayer; // => Dont forget to unsubscribe
                    SceneEventAggregator.Instance.OnAIResourceRequested += AIResourceRequested; // => Dont forget to unsubscribe
                    //SceneEventAggregator.Instance.OnFlankPointsRequested += ResourceRequested; // => Dont forget to unsubscribe
                    DeserializeSavedPoints();
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
        catch(Exception e)
        {
            Debug.LogError($"Error loading player flanking resources: {e.Message}");
        }
     
    }

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

        _savedPoints = new List<SamplePointData>(_flankPointDataSO.savedPoints); 
       
    }

    protected override void NotifyClassDependancies()
    {
        bool exists = SceneEventAggregator.Instance.CheckDependancyExists(typeof(ClosestPointToPlayerJob));

        if (exists) { return; } // Already exists, no need to add again

        SceneEventAggregator.Instance.AddDependancy(new ClosestPointToPlayerJob(_flankPointDataSO)); 

       /* List<Type> dependancies = new()
        {
            typeof(ClosestPointToPlayerJob)
        };
        SceneEventAggregator.Instance.AddDependancies(dependancies);*/
    }

    private void SetNearestIndexToPlayer(int nearestPointIndex)
    {
        _nearestPointToPlayer = nearestPointIndex;
    }

    protected override void AIResourceRequested(AIDestinationRequestData request) 
    {
        if(request.resourceType != AIResourceType.FlankPointCandidates) { return; }


       // List<Vector3> positions = new List<Vector3>();
        int step = request.numSteps; 

        if (_savedPoints == null || _savedPoints.Count == 0 ||
            _nearestPointToPlayer < 0 || _nearestPointToPlayer >= _savedPoints.Count)
        {
            //request.FlankPointCandidatesCallback?.Invoke(null);
            request.FlankPointCandidatesCallback?.Invoke(false);
            return;
        }

        if (_savedPoints[_nearestPointToPlayer].reachableSteps.TryGetValue(step, out var indices))
        {
            foreach (int i in indices)
            {
                request.flankPointCandidates.Add(_savedPoints[i].position);
                //positions.Add(_savedPoints[i].position);
            }
        }

        request.FlankPointCandidatesCallback?.Invoke(request.flankPointCandidates.Count > 0);
        //request.FlankPointCandidatesCallback?.Invoke(positions);

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
