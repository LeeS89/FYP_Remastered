using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// An extenstion of the Destination manager class
/// Retrieves and provides candidate destinations to the Destination manager for processing
/// Flank candidates refer to a specific point in the scene which gives the AI a clear LOS to the player
/// When ever they lose sight of the player from stationary state
/// Flank points are pre calculated and stored in an SO
/// </summary>
public class DestinationManagerHelper
{
    
    private Transform _ownerTransform;

    // Candidate destination container & Resource request instance
    private List<Vector3> _candidatePositions;
  
    // Flank point variables
    private int _maxFlankingSteps = 0;
    private FlankPointData _currentFlankPoint;
    
    private List<int> _stepsToTry; // Flank point Candidates x steps from the nearest point to the player
    
    private Collider[] _flankCandidateLOSColliders;

    // Layermasks for validating potential flank points. A valid flank point means a clear LOS from the flank point to the player
    // With a backup target mask being the owning gameobjects mask - used for extra validation 
    private LayerMask _flankBlockingMask;
    private LayerMask _flankTargetMask;
    private LayerMask _flankBackupTargetMask;

    // Coroutine stuff
    private WaitUntil _waitUntilResultReceived;
    private bool _resultReceived = false;

    // Waypoint Data
    private BlockData _blockData;
    public int CurrentWaypointZone { get; private set; } = 0;
    private List<WaypointPair> _waypointPairs = new();
    private WaypointPair? currentWaypointPair = null;

    // Debug Stuff
    private GameObject _debugCube;

    //NEW STUFF
    private List<FlankPointData> _candidateFlankPointPositions;
    private Action<BlockData> _wayPointCallback;
    private Action<bool> _flankCandidatesCallback;


    public DestinationManagerHelper(Transform owner, int maxflankSteps, GameObject debugCube = null)
    {
        _ownerTransform = owner;
        _maxFlankingSteps = maxflankSteps;
      
        _debugCube = debugCube;

        _stepsToTry = new List<int>();
        _candidateFlankPointPositions = new List<FlankPointData>();
        _candidatePositions = new List<Vector3>();
        _flankCandidateLOSColliders = GameManager.Instance.GetPlayerTargetPoints();
        _waitUntilResultReceived = new WaitUntil(() => _resultReceived);
        _wayPointCallback = SetWayPoints;
        _flankCandidatesCallback = OnReceivedFlankPointCandidates;
        GetFlankEvaluationMasks();  
    }

    private void GetFlankEvaluationMasks() => this.RequestFlankPointEvaluationMasks(OnRetrieveFlankPointEvaluationMasks);


    private void OnRetrieveFlankPointEvaluationMasks(LayerMask blockingMask, LayerMask targetMask, LayerMask secondaryTargetMask)
    {
        _flankBlockingMask = blockingMask;
        _flankTargetMask = targetMask;
        _flankBackupTargetMask = secondaryTargetMask;
    }

    
    private void ShuffleCandidateList<T>(List<T> candidates)
    {

        for (int i = 0; i < candidates.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(i, candidates.Count);
            (candidates[i], candidates[randIndex]) = (candidates[randIndex], candidates[i]);
        }
    }

    public void ClearCandidates()
    {
        if (_candidateFlankPointPositions.Count > 0) _candidateFlankPointPositions.Clear();

        if (_candidatePositions.Count > 0) _candidatePositions.Clear();
    }

    #region Flank Candidate region
    public static Vector3 GetFlankPointCandidatePosition(FlankPointData candidate) => candidate.position;

    

    public static void MarkFlankPointInUse(FlankPointData point) => point.inUse = true;

    public List<FlankPointData> GetFlankCandidates()
    {
        ShuffleCandidateList(_candidateFlankPointPositions);
        return _candidateFlankPointPositions;
    }

    public void SetCurrentFlankPoint(FlankPointData currentFP) => _currentFlankPoint = currentFP;


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

    public void ReleaseCurrentFlankPointIfExists()
    {
        if (_currentFlankPoint != null)
        {
            _currentFlankPoint.inUse = false;
            _currentFlankPoint = null;
        }

    }

    public IEnumerator FlankDestinationRoutine()
    {
        GetStepsToTry();

        foreach (int step in _stepsToTry)
        {
            _resultReceived = false;

            this.RequestFlankPointCandidates(step, _candidateFlankPointPositions, _flankCandidatesCallback);
        
            yield return _waitUntilResultReceived;
        }
//#if UNITY_EDITOR
        if (_debugCube == null) { yield break; }

        foreach (var point in _candidateFlankPointPositions)
        {
            Vector3 pos = point.position;
            GameObject obj = UnityEngine.Object.Instantiate(_debugCube, pos, Quaternion.identity);
        }
//#endif

    }
    #endregion

    #region waypoint Region
    private struct WaypointPair
    {
        public Vector3 position;
        public Vector3 forward;

        public WaypointPair(Vector3 pos, Vector3 fwd)
        {
            position = pos;
            forward = fwd;
        }
    }

    public void InitializeWaypoints() => this.RequestWaypointBlock(callback: _wayPointCallback);


    private void SetWayPoints(BlockData data)
    {
        _blockData = data;
    
        if (_blockData == null)
        {
            Debug.LogError("Waypoint block data is null. Cannot set waypoints.");
            return;
        }

        CurrentWaypointZone = _blockData._blockZone;
        LoadWaypointData(_blockData);
    }

    private void LoadWaypointData(BlockData wpData)
    {

        _waypointPairs.Clear();

        for (int i = 0; i < wpData._waypointPositions.Length; i++)
        {
            _waypointPairs.Add(new WaypointPair(wpData._waypointPositions[i], wpData._waypointForwards[i]));
        }

    }

    public Vector3 GetWaypointForward(Vector3 point)
    {
        for (int i = 0; i < _waypointPairs.Count; i++)
        {
            if (_waypointPairs[i].position == point)
            {
                currentWaypointPair = _waypointPairs[i];
                return _waypointPairs[i].forward;
            }

        }

        return Vector3.forward;

    }
    #endregion

    #region Candidate point retrieval region 

    public static Vector3 ReturnSelf(Vector3 v) => v;

    public List<Vector3> GetCandidates(AIDestinationType destType)
    {
        // when destType == AIDestinationType.FlankDestination, _candidatePositions is populated in the Flank coroutine
        if (destType == AIDestinationType.ChaseDestination)
        {
            AddPlayerDestinationToCandidatePoints();
        }
        else if (destType == AIDestinationType.PatrolDestination)
        {
            AddWaypointsToCandidatePoints();
        }

        return _candidatePositions;
    }

    private void AddWaypointsToCandidatePoints()
    {
        if (currentWaypointPair.HasValue)
        {
            // To prevent picking the current waypoint
            _waypointPairs.Remove(currentWaypointPair.Value);
            ShuffleCandidateList(_waypointPairs);
            _waypointPairs.Add(currentWaypointPair.Value);
        }
        else
        {
            ShuffleCandidateList(_waypointPairs);
        }


        foreach (var pair in _waypointPairs)
        {
            _candidatePositions.Add(pair.position);
        }

    }

    private void AddPlayerDestinationToCandidatePoints()
    {
        Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;

        _candidatePositions.Add(playerPos);
    }

    private void OnReceivedFlankPointCandidates(bool success)
    {

        ///// Later implementation => Based on returned bool => decide what happens when it fails
        // Debug.LogError("Flank Candidates before filtering: " + _candidateFlankPointPositions.Count);
        // foreach (var point in _candidatePoints)
        // {
        // Vector3 startPoint = point + Vector3.up;
        Vector3 losTargetPoint = _ownerTransform.position + Vector3.up * 0.9f;
        // if (!LineOfSightUtility.HasLineOfSight(startPoint, _flankCandidateLOSColliders, _flankBlockingMask, _flankTargetMask)) { continue; }

        _candidateFlankPointPositions.RemoveAll(p => !LineOfSightUtility.HasLineOfSight(p.position + Vector3.up, _flankCandidateLOSColliders, losTargetPoint, _flankBlockingMask, _flankTargetMask, _flankBackupTargetMask));
        //   if (!LineOfSightUtility.HasLineOfSight(startPoint, _flankCandidateLOSColliders, losTargetPoint, _flankBlockingMask, _flankTargetMask, _flankBackupTargetMask)) { continue; }
        // Debug.LogError("Flank Candidates after filtering: " + _candidateFlankPointPositions.Count);

        _resultReceived = true;
    }
    #endregion


   
    public void OnInstanceDestroyed()
    {
        _candidatePositions.Clear();
        _candidatePositions = null;
        _flankCandidateLOSColliders = null;
        _ownerTransform = null;
        _stepsToTry.Clear();
        _stepsToTry = null;
        _currentFlankPoint = null;
        _candidateFlankPointPositions.Clear();
        _candidateFlankPointPositions = null;
        _waitUntilResultReceived = null;
        _blockData = null;
        _debugCube = null;
        currentWaypointPair = null;
        _waypointPairs.Clear();
        _waypointPairs = null;
        _wayPointCallback = null;
        _flankCandidatesCallback = null;
    }
}
