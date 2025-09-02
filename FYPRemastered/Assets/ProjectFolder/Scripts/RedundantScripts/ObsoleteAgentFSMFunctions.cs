/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;
using Random = UnityEngine.Random;

[Obsolete]
public class ObsoleteAgentFSMFunctions : MonoBehaviour
{
    //public AIDestinationRequestData _resourceRequest;
    [SerializeField] private NavMeshObstacle _obstacle;
    private NavMeshAgent _agent;
    private BlockData _blockData;
    public LayerMask _losTargetMask;
    private int _blockZone = 0;
    private DestinationManager _destinationManager;
    private EnemyEventManager _enemyEventManager;

    #region Obsolete
    private WaypointData _wpData;
    private void InitializeWeapon()
    {
        Transform playerTransform = GameManager.Instance.GetPlayerPosition(PlayerPart.DefenceCollider);
        if (playerTransform == null)
        {
            Debug.LogError("Player transform not found!");
            return;
        }

        // _eventManager.SetupGun(_owningGameObject, _enemyEventManager, _bulletSpawnPoint, 5, playerTransform);
        //_gun = new GunBase(_bulletSpawnPoint, playerTransform, _enemyEventManager, _owningGameObject);

    }

    private IEnumerator SetNewDestination(Vector3 destination)
    {
        if (_obstacle.enabled)
        {
            yield return new WaitUntil(() => !_obstacle.enabled);
        }
        if (!_agent.enabled)
        {
            //ToggleAgent(true);
        }

        _agent.SetDestination(destination);
    }

    private void PursuitTargetRequested(AIDestinationType chaseType)
    {
        switch (chaseType)
        {
            case AIDestinationType.ChaseDestination:
                AttemptTargetChase();
                break;
            case AIDestinationType.FlankDestination:
                AttemptTargetFlank();
                break;
            case AIDestinationType.PatrolDestination:

                AttemptPatrol();

                break;
            default:
                Debug.LogError("Invalid chase type requested: " + chaseType);
                break;
        }
    }

    private void ChangeWaypoints()
    {
        BlockData temp = _blockData;
        InitializeWaypoints();

        // BaseSceneManager._instance.ReturnWaypointBlock(temp);

    }

    private void InitializeWaypoints()
    {
        //_resourceRequest.flankBlockingMask = _lineOfSightMask;
       // _resourceRequest.flankTargetMask = _losTargetMask;
       /// _resourceRequest.flankTargetColliders = GameManager.Instance.GetPlayerTargetPoints();
        //_blockData = BaseSceneManager._instance.RequestWaypointBlock();
      //  _resourceRequest.resourceType = AIResourceType.WaypointBlock;
       // _resourceRequest.waypointCallback = (blockData) =>
        *//*{
            if (blockData != null)
            {
                _blockData = blockData;
                SetWayPoints();
            }
            else
            {
                Debug.LogError("No valid waypoint block data found!");

            }
        };
*//*
       // SceneEventAggregator.Instance.RequestResource(_resourceRequest);


    }


    private void SetWayPoints()
    {
        if (_blockData != null)
        {
            _blockZone = _blockData._blockZone;
            //Debug.LogError("Block Zone for enemy: " + _blockZone);

            _wpData.UpdateData(_blockData._waypointPositions.ToList(), _blockData._waypointForwards.ToList());

            //_destinationManager.LoadWaypointData(_wpData);
            //_enemyEventManager.WaypointsUpdated(_wpData);

           // SceneEventAggregator.Instance.RegisterAgentAndZone(this, _blockZone);
          


        }
    }

    private void AttemptPatrol()
    {


        _resourceRequest.destinationType = AIDestinationType.PatrolDestination;
        _resourceRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_agent.transform.position);

        //_resourceRequest.path = _path;

        _resourceRequest.externalCallback = (success, point) =>
        {
            if (IsStaleRequest(AIDestinationType.PatrolDestination)) { return; }
            DestinationRequestResult(success, point, AlertStatus.None);

        };
       // _destinationManager.RequestNewDestination(_resourceRequest);
    }

    private void AttemptTargetChase()
    {
        //ChasingStateRequested();

        _resourceRequest.destinationType = AIDestinationType.ChaseDestination;
        _resourceRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_agent.transform.position);
        *//*Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
        _destinationData.end = LineOfSightUtility.GetClosestPointOnNavMesh(playerPos);*/
        /* _resourceRequest.path = _path;*//*

        _resourceRequest.externalCallback = (success, point) =>
        {
            if (IsStaleRequest(AIDestinationType.ChaseDestination)) { return; }
            int _randomStoppingDistance = Random.Range(4, 11);
            DestinationRequestResult(success, point, AlertStatus.Chasing, _randomStoppingDistance);
            //StartCoroutine(DestinationRequestResult(success, point, AlertStatus.Chasing, _randomStoppingDistance));
        };
       // _destinationManager.RequestNewDestination(_resourceRequest);

    }

    private void AttemptTargetFlank()
    {
        // First enter the Chasing state here
       // ChasingStateRequested();
        // In chasing, before the wile loop in the coroutine, yield wait until => Destination found


        // Send Destination Request here
        _resourceRequest.destinationType = AIDestinationType.FlankDestination;
        _resourceRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_agent.transform.position);

        *//*_resourceRequest.path = _path;*//*

        _resourceRequest.externalCallback = (success, point) =>
        {
            if (IsStaleRequest(AIDestinationType.FlankDestination)) { return; }
            DestinationRequestResult(success, point, AlertStatus.Flanking);
            //StartCoroutine(DestinationRequestResult(success, point, AlertStatus.Flanking));
        };
       // _destinationManager.RequestNewDestination(_resourceRequest);
        // If request fails => Request Player destination

        // If 2nd request fails => Later implement a fallback 
    }

    private void ToggleAgent(bool agentEnabled)
    {
        if (_agent.enabled == agentEnabled) { return; }

        _agent.enabled = agentEnabled;
    }

    private void DestinationRequestResult(bool success, Vector3 destination, AlertStatus status = AlertStatus.None, int stoppingDistance = 0)
    {
        if (success)
        {

            _resourceRequest.carvingCallback = () =>
            {
                if (_obstacle.enabled)
                {
                    _obstacle.enabled = false;
                }
            };

            _resourceRequest.agentActiveCallback = () =>
            {
                if (!_agent.enabled)
                {
                    ToggleAgent(true);

                }
              //  AlertStatusUpdated(status);
                _agent.stoppingDistance = stoppingDistance;
                _agent.SetDestination(destination);
                _enemyEventManager.DestinationApplied();
            };
           // _destinationManager.StartCarvingRoutine(_resourceRequest); // Move to extension function

            *//* yield return new WaitUntil(() => _agent.enabled);
             _agent.SetDestination(destination);
             _enemyEventManager.DestinationApplied();*//*
        }
    }

    private bool IsStaleRequest(AIDestinationType expectedType)
    {
        return expectedType != _resourceRequest.destinationType;
    }
    #endregion

    #region Test Functions
    public bool _testDeath = false;

    public float GetStraightLineDistance(Vector3 from, Vector3 to)
    {
        return Vector3.Distance(from, to);
    }

    public float GetNavMeshPathDistance(Vector3 from, Vector3 to)
    {
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
        {
            float distance = 0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return distance;
        }
        return float.PositiveInfinity; // No valid path
    }


    void CompareDistances(Vector3 enemyPosition, Vector3 playerPosition)
    {
        if (_agent.hasPath && !_agent.pathPending)
        {
            float disRem = _agent.remainingDistance;
            float straightLineDist = GetStraightLineDistance(enemyPosition, _agent.destination);
            float navMeshDist = GetNavMeshPathDistance(enemyPosition, _agent.destination);

            Debug.LogError($"Straight-Line Distance: {straightLineDist}");
            Debug.LogError($"NavMesh Path Distance: {navMeshDist}");
            Debug.LogError($"remaining Distance: {disRem}");
        }
    }


    private void UpdateAnimatorDirection()
    {
        // Get the normalized movement direction
        Vector3 movementDirection = _agent.velocity.normalized;

        // Calculate the angle between the agent's forward direction and the movement direction
        float angle = Vector3.SignedAngle(transform.forward, movementDirection, Vector3.up);

        // Normalize the angle to a value between -1 and 1
        float normalizedDirection = angle / 90f;

      *//*  // Only update the direction if it has changed significantly (for example, threshold of 0.1)
        if (Mathf.Abs(normalizedDirection - _previousDirection) > 0.1f)
        {
            _animController.UpdateDirection(normalizedDirection); // Update the animator with the new direction
            //animator.SetFloat("direction", normalizedDirection); // Update the direction parameter
            _previousDirection = normalizedDirection; // Store the new direction
        }*//*



    }
    #endregion

    #region Old Flank Resources reques function
   *//* protected *//*override*//* void AIResourceRequested(AIDestinationRequestData request)
    {
        if (request.resourceType != AIResourceType.FlankPointCandidates) { return; }

        
        ///// END NEW

        *//* int step = request.numSteps; 

         if (_savedPoints == null || _savedPoints.Count == 0 ||
             _nearestPointToPlayer < 0 || _nearestPointToPlayer >= _savedPoints.Count)
         {

             request.FlankPointCandidatesCallback?.Invoke(false);
             return;
         }

         if (_savedPoints[_nearestPointToPlayer].reachableSteps.TryGetValue(step, out var indices))
         {
             foreach (int i in indices)
             {
                 request.flankPointCandidates.Add(_savedPoints[i].position);

             }
         }

         request.FlankPointCandidatesCallback?.Invoke(request.flankPointCandidates.Count > 0);
        *//*

    }*//*
    #endregion

    #region Redundant Code

    *//* private void UpdateFieldOfViewResults(bool playerSeen)
    {
        if (_canSeePlayer == playerSeen) { return; } // Already Updated, return early

        _canSeePlayer = playerSeen;

        if (_canSeePlayer)
        {
            //if (_alertStatus == AlertStatus.None)
            // {
            //_alertStatus = AlertStatus.Alert;
            EnterAlertPhase();

            //_enemyEventManager.TargetSeen(_canSeePlayer);
            //_enemyEventManager.ChangeAnimatorLayerWeight(1, 0, 1, 0.5f, true);
            // Alert Group Here (Moon Scene Manager)

        }
        else
        {
        //    //_animController.SetAlertStatus(false);
            //_enemyEventManager.TargetSeen(false);
            //_enemyEventManager.ChangeAnimatorLayerWeight(1, 1, 0, 0.5f, false);
        }

        //Update Shooting Component Here
    }*/

    /* private void UpdateFieldOfViewCheckFrequency()
     {
         return;
         switch (_fieldOfViewStatus)
         {
             case FieldOfViewFrequencyStatus.Normal:
                 if (_fovCheckFrequency != _patrolFOVCheckFrequency)
                 {
                     _fovCheckFrequency = _patrolFOVCheckFrequency;
                 }
                 break;
             case FieldOfViewFrequencyStatus.Heightened:
                 if (_fovCheckFrequency != _alertFOVCheckFrequency)
                 {
                     _fovCheckFrequency = _alertFOVCheckFrequency;
                 }
                 break;
             default:
                 _fovCheckFrequency = _patrolFOVCheckFrequency;
                 break;
         }

         //RunFieldOfViewCheck();
     }
 */

    /*private void RunFieldOfViewCheck()
    {
        if (Time.time >= _nextCheckTime && _fov != null)
        {
            _nextCheckTime = Time.time + _fovCheckFrequency;
            bool playerSeen = false;

            // CheckForTarget first performs a Physics.CheckSphere. If check passes, an overlapsphere is performed which returns the targets collider information
            int numTargetsDetected = _fov.CheckTargetProximity(_fovLocation, _fovTraceResults, _fovTraceRadius, _fovLayerMask, true);

            if (numTargetsDetected > 0)
            {
                for (int i = 0; i < numTargetsDetected; i++)
                {
                    if (EvaluateFieldOfView(_fovTraceResults[i]))
                    {
                        playerSeen = true;
                        CheckIfTargetWithinShootAngle(_fovTraceResults[i]);
                        break;
                    }

                }

                if (_fieldOfViewStatus != FieldOfViewFrequencyStatus.Heightened)
                    _fieldOfViewStatus = FieldOfViewFrequencyStatus.Heightened;
            }
            else
            {

                if (_fieldOfViewStatus != FieldOfViewFrequencyStatus.Normal)
                    _fieldOfViewStatus = FieldOfViewFrequencyStatus.Normal;
            }

            //  UpdateFieldOfViewResults(playerSeen);


        }
    }*/

    /* private void CheckIfTargetWithinShootAngle(Collider target)
     {
         if (target == null) { return; }

        // bool canShootTarget = _fov.IsWithinView(_fovLocation, target.ClosestPointOnBounds(_fovLocation.position), _shootAngleThreshold * 0.5f, _shootAngleThreshold * 1.25f);


         //_enemyEventManager.FacingTarget(canShootTarget);
     }
 *//*
    /// <summary>
    /// First checks if the target is within FOV cone, then performs a capsule cast from waist to eye level to check for target colliders
    /// If target collider(s) are hit, performs a line of sight check to see if the target is visible.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    *//* private bool EvaluateFieldOfView(Collider target)
     {
         if (target == null) { return false; }

        *//* if (!_fov.IsWithinView(_fovLocation, target.bounds.center, _angle * 0.5f, _angle * 0.75f))
         {
             if (_capsuleResultTest)
             {
                 Debug.LogError("Failed Angle Check");
             }
             return false;
         }*//*

         Vector3 waistPos = transform.position + Vector3.up * _waistHeight;
         Vector3 eyePos = transform.position + Vector3.up * _eyeHeight;
         Vector3 sweepCenter = (waistPos + eyePos) * 0.5f;
         Vector3 directionTotarget = TargetingUtility.GetDirectionToTarget(target.bounds.center, sweepCenter);

         int hitCount = 0;//_fov.EvaluateViewCone(waistPos, eyePos, 0.4f, directionTotarget, _fovTraceRadius, _fovLayerMask, _traceHitPoints);
         if (hitCount == 0)
         {
             if (_capsuleResultTest)
             {
                 Debug.LogError("Capsule cast failed");
             }
             return false;
         }

         AddFallbackPoints(target, _traceHitPoints, ref hitCount);

         for (int i = 0; i < hitCount; i++)
         {
            // if (!_fov.HasLineOfSight(_fovLocation, _traceHitPoints[i], _lineOfSightMask, _fovLayerMask, transform, _bulletSpawnPoint, _capsuleResultTest)) { continue; }
             return true;
         }

         return false;
     }*//*

    //private void AddFallbackPoints(Collider target, Vector3[] hitPoints, ref int startIndex)
    //{
    //    hitPoints[startIndex++] = target.bounds.center + Vector3.up * target.bounds.extents.y;
    //    hitPoints[startIndex++] = target.bounds.center - Vector3.right * target.bounds.extents.x;
    //    hitPoints[startIndex++] = target.bounds.center + Vector3.right * target.bounds.extents.x;
    //}

    *//*
    private void AlertStatusUpdated(AlertStatus status)
    {
        _alertStatus = status;
        _agentEventManager.AlertStatusChanged(status);
    }*//*
    #endregion

    *//*if (_interactor != null && _interactor.HasSelectedInteractable)
       {
           Debug.LogError("Interactor has interactable: ");
       }*/
    /* if (_testtrace)
     {
         GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
         sphere.transform.position = _grabbableCheckAnchor.position;
         sphere.transform.localScale = Vector3.one * (_grabbableCheckRadius * 2f);

         GameObject.Destroy(sphere, 2f);
         int grabbablesDetected = TraceComp.CheckTargetProximity(_grabbableCheckAnchor, _grabbableCheckResults, _grabbableCheckRadius, _grabbableMask);

         for (int i = 0; i < grabbablesDetected; i++)
         {
             if (_grabbableCheckResults[i].TryGetComponent<Lightsaber>(out Lightsaber ls))
             {
                 _currentInteractable = ls.Testgrab(_side*//*, _interactor*//*);
                 _interactor.ForceSelect(_currentInteractable);
                // _interactable2 = _interactor.Interactable;
                // Debug.LogError("Interactable Name is: " + _interactable2.name);
                 //ls.OnGrabbed();
             }
         }
         _testtrace = false;
     }*//*


















    #region Obsolete 
    *//*private bool ValidateTargetVisibilityFromPoint(Vector3 point)
    {
        int mask = LayerMask.GetMask("Default", "Water", "PlayerDefence", "Player");
        Collider playerCollider = GameManager.Instance.GetPlayerCollider(PlayerPart.Position);
        Vector3[] testPoints = new Vector3[]
        {
            playerCollider.bounds.center,
            playerCollider.bounds.center + Vector3.up * playerCollider.bounds.extents.y, // top
            playerCollider.bounds.center - Vector3.up * playerCollider.bounds.extents.y, // bottom
            playerCollider.bounds.center + Vector3.right * playerCollider.bounds.extents.x, // right shoulder
            playerCollider.bounds.center - Vector3.right * playerCollider.bounds.extents.x  // left shoulder
        };

        foreach (var colPoint in testPoints)
        {
            if (Physics.Linecast(point, colPoint, out RaycastHit hit, mask))
            {
                if (hit.collider == playerCollider)
                {
                    Debug.DrawLine(point, colPoint, Color.green, 25f);
                    return true;
                }
            }
        }

        return false;
    }

    public void RequestNewDestination(AIDestinationRequestData destinationRequest)
    {

        switch (destinationRequest.destinationType)
        {
            case AIDestinationType.ChaseDestination:
                RequestPlayerDestination(destinationRequest);
                break;
            case AIDestinationType.FlankDestination:
                CoroutineRunner.Instance.StartCoroutine(RequestFlankDestination(destinationRequest));
                break;
            case AIDestinationType.PatrolDestination:
                RequestPatrolPointDestination(destinationRequest);
                break;
            default:
                Debug.LogError($"Unknown destination type: {destinationRequest.destinationType}");

                break;
        }
    }

    public void LoadWaypointData(WaypointData wpData)
    {

        _waypointPairs.Clear();

        for (int i = 0; i < wpData._waypointPositions.Count; i++)
        {
            _waypointPairs.Add(new WaypointPair(wpData._waypointPositions[i], wpData._waypointForwards[i]));
        }

    }

    private void RequestPatrolPointDestination(AIDestinationRequestData destinationRequest)
    {
        if (currentWaypointPair.HasValue)
        {
            // If there's a previously selected waypoint, remove it, shuffle the list, and then add it back at the end
            _waypointPairs.Remove(currentWaypointPair.Value);
            ShuffleWaypointPairs();  // Shuffle the remaining list
            _waypointPairs.Add(currentWaypointPair.Value);  // Add the selected waypoint to the end
        }
        else
        {
            ShuffleWaypointPairs();  // Shuffle if no waypoint has been selected yet
        }

        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationRequest, GetWaypointPositions));
    }

    private IEnumerator RequestFlankDestination(AIDestinationRequestData destinationRequest)
    {
        GetStepsToTry();
        //_waypoints.Clear();

        destinationRequest.resourceType = AIResourceType.FlankPointCandidates; // Set the resource type for the request
        foreach (int step in _stepsToTry)
        {
            _resultReceived = false;
            destinationRequest.numSteps = step; // Set the step for the request

            *//*     destinationRequest.FlankPointCandidatesCallback = (points) =>
                 {
                     if (points != null && points.Count > 0)
                     {
                         foreach (var point in points)
                         {
                             Vector3 startPoint = point + Vector3.up;
                             if (!LineOfSightUtility.HasLineOfSight(startPoint, destinationRequest.flankTargetColliders, destinationRequest.flankBlockingMask, destinationRequest.flankTargetMask)) { continue; }

                             _candidatePoints.Add(point);
                         }


                         //_points.AddRange(points.OrderBy(p => Random.value));
                     }
                     _resultReceived = true;

                 };*//*

            SceneEventAggregator.Instance.RequestResource(destinationRequest);

            yield return _waitUntilResultReceived;
        }

        foreach (var point in _candidatePoints)
        {

            GameObject obj = UnityEngine.Object.Instantiate(testCube, point, Quaternion.identity);
        }

        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationRequest, GetFlankPoints));

    }

    private void RequestPlayerDestination(AIDestinationRequestData destinationRequest)
    {
        CoroutineRunner.Instance.StartCoroutine(AttemptDestinationRoutine(destinationRequest, GetPlayerPoint));

    }

    private IEnumerator AttemptDestinationRoutine(AIDestinationRequestData destinationRequest, Func<List<Vector3>> candidatePointProvider)
    {
        var candidates = candidatePointProvider.Invoke();


        foreach (var point in candidates)
        {


            _resultReceived = false;
            _isValid = false;

            destinationRequest.end = LineOfSightUtility.GetClosestPointOnNavMesh(point);

            destinationRequest.internalCallback = (success) =>
            {
                _isValid = success;
                _resultReceived = true;

            };


            SceneEventAggregator.Instance.PathRequested(destinationRequest);

            yield return _waitUntilResultReceived;

            if (!_isValid) continue;

            if (destinationRequest.destinationType == AIDestinationType.PatrolDestination)
            {
                var match = _waypointPairs.FirstOrDefault(p => p.position == point);
                _eventManager.RotateAtPatrolPoint(match.forward);

                currentWaypointPair = match;
            }
            destinationRequest.resourceType = AIResourceType.None;
            destinationRequest.externalCallback?.Invoke(true, point);
            yield break;
        }

        destinationRequest.externalCallback?.Invoke(false, Vector3.zero);
    }
    private List<Vector3> GetWaypointPositions()
    {
        return _waypointPairs.Select(p => p.position).ToList();
    }






    private List<Vector3> GetPlayerPoint()
    {
        Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
        return new List<Vector3> { playerPos };
    }

    private List<Vector3> GetFlankPoints()
    {
        return _candidatePoints;
    }*//*
    #endregion









*//*    #region Backup imp[lementaiton but now Obsolete
    private LayerMask _flankBlockingMask; // OLD
    private LayerMask _flankTargetMask; // OLD
    private LayerMask _flankBackupTargetMask; //OLD

    private int _maxFlankingSteps; // OLD

    // Waypoint Data => OLD
    private BlockData _blockData;
    // public int CurrentWaypointZone { get; private set; } = 0; // OLD
    private List<WaypointPair> _waypointPairs = new(); // OLD
    private WaypointPair? currentWaypointPair = null; // OLD
    ///private int _currentWaypointZone = 0;

    private List<Vector3> _candidatePoints = new List<Vector3>(); // OLD
    private Collider[] _flankCandidateLOSColliders; // OLD
    private List<int> _stepsToTry; ///// Remove

    #region Waypoint Initialization
    private void InitializeWaypoints() // OLD => Call helper
    {
        _destinationRequest.flankPointCandidates = _candidatePoints; // OLD
        _destinationRequest.path = _path;
        _destinationRequest.resourceType = AIResourceType.WaypointBlock; // OLD
        _destinationRequest.waypointCallback = SetWayPoints; // OLD
        SceneEventAggregator.Instance.RequestResource(_destinationRequest); // OLd
    }

    private void SetWayPoints(BlockData data) // OLD
    {
        _blockData = data;
        _destinationRequest.resourceType = AIResourceType.None;
        if (_blockData == null)
        {
            Debug.LogError("Waypoint block data is null. Cannot set waypoints.");
            return;
        }

        // CurrentWaypointZone = _blockData._blockZone;
        //  _currentWaypointZone = _blockData._blockZone;
        LoadWaypointData(_blockData);
    }



    public void LoadWaypointData(BlockData wpData)
    {

        _waypointPairs.Clear();

        for (int i = 0; i < wpData._waypointPositions.Length; i++)
        {
            _waypointPairs.Add(new WaypointPair(wpData._waypointPositions[i], wpData._waypointForwards[i]));
        }

    }

    private struct WaypointPair // OLD
    {
        public Vector3 position;
        public Vector3 forward;

        public WaypointPair(Vector3 pos, Vector3 fwd)
        {
            position = pos;
            forward = fwd;
        }
    }




    private void ShuffleWaypointPairs() // OLD
    {
        for (int i = 0; i < _waypointPairs.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(i, _waypointPairs.Count);
            (_waypointPairs[i], _waypointPairs[randIndex]) = (_waypointPairs[randIndex], _waypointPairs[i]);
        }
    }

    #endregion
    #region Populating Candidate Points for processing
    private void AddWaypointsToCandidatePoints() // OLD
    {
        if (currentWaypointPair.HasValue)
        {

            _waypointPairs.Remove(currentWaypointPair.Value);
            ShuffleWaypointPairs();
            _waypointPairs.Add(currentWaypointPair.Value);
        }
        else
        {
            ShuffleWaypointPairs();
        }


        foreach (var pair in _waypointPairs)
        {
            _candidatePoints.Add(pair.position);
        }

    }

    private void AddPlayerDestinationToCandidatePoints() // OLD
    {
        Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;

        _candidatePoints.Add(playerPos);
    }

    private List<Vector3> GetCandidatePoints(AIDestinationType destType)
    {

        // when destType == AIDestinationType.FlankDestination, _candidatePoints is populated in the Flank coroutine
        if (destType == AIDestinationType.ChaseDestination)
        {
            AddPlayerDestinationToCandidatePoints();
        }
        else if (destType == AIDestinationType.PatrolDestination)
        {
            AddWaypointsToCandidatePoints();
        }

        return _candidatePoints;

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
    #endregion

    #region Destination Request Processing
    private IEnumerator ProcessDestinationQueueOld()
    {
        while (_destinationQueue.Count > 0)
        {
            var request = _destinationQueue.Dequeue();

            if (request == AIDestinationType.FlankDestination)
            {
                yield return CoroutineRunner.Instance.StartCoroutine(FlankDestinationRoutine());
                yield return CoroutineRunner.Instance.StartCoroutine(DestinationRoutine(request));
            }
            else
            {
                yield return CoroutineRunner.Instance.StartCoroutine(DestinationRoutine(request));
            }
            _candidatePoints.Clear();
        }
        _runningRoutine = null;
    }

    private IEnumerator DestinationRoutine(AIDestinationType destType)
    {

        var candidates = GetCandidatePoints(destType);


        foreach (var point in candidates)
        {

            _resultReceived = false;
            _isValid = false;

            _destinationRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.position);
            _destinationRequest.end = LineOfSightUtility.GetClosestPointOnNavMesh(point);


            _destinationRequest.internalCallback = PathRequestInternalCallback;

            SceneEventAggregator.Instance.PathRequested(_destinationRequest);

            yield return _waitUntilResultReceived;


            if (destType != _globalDestinationType)
            {
                yield break;
            }

            if (!_isValid) continue;


            if (destType == AIDestinationType.PatrolDestination)
            {
                var match = _waypointPairs.FirstOrDefault(p => p.position == point);
                _eventManager.RotateAtPatrolPoint(match.forward);

                currentWaypointPair = match;
            }



            _destinationRequest.resourceType = AIResourceType.None;
            _onRequestComplete?.Invoke(true, point, destType);

            yield break;
        }

        if (destType != _globalDestinationType)
        {
            yield break;
        }

        _onRequestComplete?.Invoke(false, Vector3.zero, destType);


    }

    private IEnumerator FlankDestinationRoutine()
    {
        GetStepsToTry();

        _destinationRequest.resourceType = AIResourceType.FlankPointCandidates;
        foreach (int step in _stepsToTry)
        {
            _resultReceived = false;
            _destinationRequest.numSteps = step; // Set the step for the request

            _destinationRequest.FlankPointCandidatesCallback = OnReceivedFlankPointCandidates; *//*(points) =>
            {
                if (points != null && points.Count > 0)
                {
                    foreach (var point in points)
                    {
                        Vector3 startPoint = point + Vector3.up;
                        if (!LineOfSightUtility.HasLineOfSight(startPoint, _flankCandidateLOSColliders, _flankBlockingMask, _flankTargetMask)) { continue; }

                        _candidatePoints.Add(point);
                    }


                    //_points.AddRange(points.OrderBy(p => Random.value));
                }
                _resultReceived = true;

            };*//*

            SceneEventAggregator.Instance.RequestResource(_destinationRequest);

            yield return _waitUntilResultReceived;
        }

        foreach (var point in _candidatePoints)
        {

            GameObject obj = UnityEngine.Object.Instantiate(testCube, point, Quaternion.identity);
        }

    }

    private void OnReceivedFlankPointCandidates(*//*List<Vector3> points*//*bool success)
    {
        ///// Later implementation => Based on returned bool => decide what happens when it fails
        Debug.LogError("Flank Candidates before filtering: " + _candidatePoints.Count);
        // foreach (var point in _candidatePoints)
        // {
        // Vector3 startPoint = point + Vector3.up;
        Vector3 losTargetPoint = _owner.position + Vector3.up * 0.9f;
        // if (!LineOfSightUtility.HasLineOfSight(startPoint, _flankCandidateLOSColliders, _flankBlockingMask, _flankTargetMask)) { continue; }

        _candidatePoints.RemoveAll(p => !LineOfSightUtility.HasLineOfSight(p + Vector3.up, _flankCandidateLOSColliders, losTargetPoint, _flankBlockingMask, _flankTargetMask, _flankBackupTargetMask));
        //   if (!LineOfSightUtility.HasLineOfSight(startPoint, _flankCandidateLOSColliders, losTargetPoint, _flankBlockingMask, _flankTargetMask, _flankBackupTargetMask)) { continue; }
        Debug.LogError("Flank Candidates after filtering: " + _candidatePoints.Count);
        // _candidatePoints.Add(point);
        // }

        //_points.AddRange(points.OrderBy(p => Random.value));


        *//* if (points != null && points.Count > 0)
         {
             foreach (var point in points)
             {
                 Vector3 startPoint = point + Vector3.up;
                 if (!LineOfSightUtility.HasLineOfSight(startPoint, _flankCandidateLOSColliders, _flankBlockingMask, _flankTargetMask)) { continue; }

                 _candidatePoints.Add(point);
             }

             //_points.AddRange(points.OrderBy(p => Random.value));
         }*//*
        _resultReceived = true;
    }


    #endregion

    #endregion*//*
}
*/