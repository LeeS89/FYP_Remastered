using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DestinationManagerHelper
{
    private FlankPointData _currentFlankPoint;
    private AIDestinationRequestData _requestData;
    private List<FlankPointData> _candidateFlankPointDestinations;
    private List<Vector3> _candidateDestinations;

    private Transform _ownerTransform;
    private List<int> _stepsToTry;
    private bool _resultReceived = false;
    private LayerMask _flankBlockingMask;
    private LayerMask _flankTargetMask;
    private LayerMask _flankBackupTargetMask;
    private Collider[] _flankCandidateLOSColliders;

    private int _maxFlankingSteps = 0;
    private WaitUntil _waitUntilResultReceived;
    private GameObject _debugCube;

    public DestinationManagerHelper(AIDestinationRequestData requestData, Transform owner, int maxflankSteps, LayerMask flankBlockingMask, LayerMask flankTargetMask, LayerMask flankBackupTargetMask, GameObject debugCube = null)
    {
        _requestData = requestData;
        _ownerTransform = owner;
        _maxFlankingSteps = maxflankSteps;
        _flankBackupTargetMask = flankBlockingMask;
        _flankTargetMask = flankTargetMask;
        _flankBackupTargetMask = flankBackupTargetMask;

        _debugCube = debugCube;

        _stepsToTry = new List<int>();
        _candidateFlankPointDestinations = new List<FlankPointData>();
        _candidateDestinations = new List<Vector3>();
        _flankCandidateLOSColliders = GameManager.Instance.GetPlayerTargetPoints();
        _waitUntilResultReceived = new WaitUntil(() => _resultReceived);
        _requestData.flankCandidates = _candidateFlankPointDestinations;
    }

    public static Vector3 GetFlankPointCandidatePosition(FlankPointData candidate) => candidate.position;

    public static Vector3 ReturnSelf(Vector3 v) => v;

    public static void MarkFlankPointInUse(FlankPointData point) => point.inUse = true;

    public List<FlankPointData> GetFlankCandidates() => _candidateFlankPointDestinations;

    public void ReleaseFlankPoint()
    {
        if(_currentFlankPoint != null)
        {
            _currentFlankPoint.inUse = false;
            _currentFlankPoint = null;
        }

    }
   
    public void ClearCandidates()
    {

    }


    public IEnumerator FlankDestinationRoutine()
    {
        GetStepsToTry();

        _requestData.resourceType = AIResourceType.FlankPointCandidates;
        foreach (int step in _stepsToTry)
        {
            _resultReceived = false;
            _requestData.numSteps = step; // Set the step for the request

            _requestData.FlankPointCandidatesCallback = OnReceivedFlankPointCandidates; 


            SceneEventAggregator.Instance.RequestResource(_requestData);

            yield return _waitUntilResultReceived;
        }
#if UNITY_EDITOR
        if(_debugCube == null) { yield break; }

        foreach (var point in _candidateFlankPointDestinations)
        {
            Vector3 pos = point.position;
            GameObject obj = UnityEngine.Object.Instantiate(_debugCube, pos, Quaternion.identity);
        }
#endif

    }

    private void OnReceivedFlankPointCandidates(/*List<Vector3> points*/bool success)
    {
        ///// Later implementation => Based on returned bool => decide what happens when it fails
        Debug.LogError("Flank Candidates before filtering: " + _candidateFlankPointDestinations.Count);
        // foreach (var point in _candidatePoints)
        // {
        // Vector3 startPoint = point + Vector3.up;
        Vector3 losTargetPoint = _ownerTransform.position + Vector3.up * 0.9f;
        // if (!LineOfSightUtility.HasLineOfSight(startPoint, _flankCandidateLOSColliders, _flankBlockingMask, _flankTargetMask)) { continue; }

        _candidateFlankPointDestinations.RemoveAll(p => !LineOfSightUtility.HasLineOfSight(p.position + Vector3.up, _flankCandidateLOSColliders, losTargetPoint, _flankBlockingMask, _flankTargetMask, _flankBackupTargetMask));
        //   if (!LineOfSightUtility.HasLineOfSight(startPoint, _flankCandidateLOSColliders, losTargetPoint, _flankBlockingMask, _flankTargetMask, _flankBackupTargetMask)) { continue; }
        Debug.LogError("Flank Candidates after filtering: " + _candidateFlankPointDestinations.Count);
       
        _resultReceived = true;
    }

    private void GetStepsToTry()
    {
        _stepsToTry.Clear();

        int randomIndex = Random.Range(4, _maxFlankingSteps + 1);
        int temp = randomIndex;
        while (temp >= 4) // 4 will eventually be changed to a passed minSteps parameter
        {
            _stepsToTry.Add(temp);
            temp--;
        }
        temp = randomIndex + 1;
        while (temp <= _maxFlankingSteps)
        {
            _stepsToTry.Add(temp);
            temp++;
        }
    }


    public void OnInstanceDestroyed()
    {
        _candidateDestinations.Clear();
        _candidateDestinations = null;
        _flankCandidateLOSColliders = null;
        _ownerTransform = null;
        _stepsToTry.Clear();
        _stepsToTry = null;
        _requestData = null;
        _currentFlankPoint = null;
        _candidateFlankPointDestinations.Clear();
        _candidateFlankPointDestinations = null;
        _waitUntilResultReceived = null;

        _debugCube = null;
    }
}
