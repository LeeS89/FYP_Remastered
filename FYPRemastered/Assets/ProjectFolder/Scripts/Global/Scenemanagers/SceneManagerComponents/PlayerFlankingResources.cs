using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerFlankingResources : SceneResources
{
    private SamplePointDataSO _flankPointDataSO;
    private List<SamplePointData> _savedPoints;
    private int _nearestPointToPlayer = 0;

    public override async Task LoadResources()
    {
        try
        {
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
                    SceneEventAggregator.Instance.OnResourceRequested += ResourceRequested; // => Dont forget to unsubscribe
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

            NotifyDependancies(); // => Needs Closest Point Job
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
        Debug.LogError($"Deserialized {_savedPoints.Count} saved points from SamplePointDataSO.");
    }

    protected override void NotifyDependancies()
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

    protected override void ResourceRequested(ResourceRequest request) // => Add as an aevent in SceneEventAggregator
    {
        if(request.resourceType != Resourcetype.FlankPointCandidates) { return; }


        List<Vector3> positions = new List<Vector3>();
        int step = request.numSteps; 

        if (_savedPoints == null || _savedPoints.Count == 0 ||
            _nearestPointToPlayer < 0 || _nearestPointToPlayer >= _savedPoints.Count)
        {
            request.FlankPointCandidatesCallback?.Invoke(null);
            return;
        }

        if (_savedPoints[_nearestPointToPlayer].reachableSteps.TryGetValue(step, out var indices))
        {
            foreach (int i in indices)
            {
                positions.Add(_savedPoints[i].position);
            }
        }
        Debug.LogError($"Flank Point Candidates for step {step}: {positions.Count} positions found.");
        request.FlankPointCandidatesCallback?.Invoke(positions);

    }
}
