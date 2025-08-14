using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class ObsoleteAgentFSMFunctions : MonoBehaviour
{
    public AIDestinationRequestData _resourceRequest;
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
        _resourceRequest.flankTargetMask = _losTargetMask;
        _resourceRequest.flankTargetColliders = GameManager.Instance.GetPlayerTargetPoints();
        //_blockData = BaseSceneManager._instance.RequestWaypointBlock();
        _resourceRequest.resourceType = AIResourceType.WaypointBlock;
        _resourceRequest.waypointCallback = (blockData) =>
        {
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

        SceneEventAggregator.Instance.RequestResource(_resourceRequest);


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
        _destinationManager.RequestNewDestination(_resourceRequest);
    }

    private void AttemptTargetChase()
    {
        //ChasingStateRequested();

        _resourceRequest.destinationType = AIDestinationType.ChaseDestination;
        _resourceRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_agent.transform.position);
        /*Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
        _destinationData.end = LineOfSightUtility.GetClosestPointOnNavMesh(playerPos);*/
        /* _resourceRequest.path = _path;*/

        _resourceRequest.externalCallback = (success, point) =>
        {
            if (IsStaleRequest(AIDestinationType.ChaseDestination)) { return; }
            int _randomStoppingDistance = Random.Range(4, 11);
            DestinationRequestResult(success, point, AlertStatus.Chasing, _randomStoppingDistance);
            //StartCoroutine(DestinationRequestResult(success, point, AlertStatus.Chasing, _randomStoppingDistance));
        };
        _destinationManager.RequestNewDestination(_resourceRequest);

    }

    private void AttemptTargetFlank()
    {
        // First enter the Chasing state here
       // ChasingStateRequested();
        // In chasing, before the wile loop in the coroutine, yield wait until => Destination found


        // Send Destination Request here
        _resourceRequest.destinationType = AIDestinationType.FlankDestination;
        _resourceRequest.start = LineOfSightUtility.GetClosestPointOnNavMesh(_agent.transform.position);

        /*_resourceRequest.path = _path;*/

        _resourceRequest.externalCallback = (success, point) =>
        {
            if (IsStaleRequest(AIDestinationType.FlankDestination)) { return; }
            DestinationRequestResult(success, point, AlertStatus.Flanking);
            //StartCoroutine(DestinationRequestResult(success, point, AlertStatus.Flanking));
        };
        _destinationManager.RequestNewDestination(_resourceRequest);
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
            _destinationManager.StartCarvingRoutine(_resourceRequest); // Move to extension function

            /* yield return new WaitUntil(() => _agent.enabled);
             _agent.SetDestination(destination);
             _enemyEventManager.DestinationApplied();*/
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

      /*  // Only update the direction if it has changed significantly (for example, threshold of 0.1)
        if (Mathf.Abs(normalizedDirection - _previousDirection) > 0.1f)
        {
            _animController.UpdateDirection(normalizedDirection); // Update the animator with the new direction
            //animator.SetFloat("direction", normalizedDirection); // Update the direction parameter
            _previousDirection = normalizedDirection; // Store the new direction
        }*/



    }
    #endregion

    #region Old Flank Resources reques function
    protected /*override*/ void AIResourceRequested(AIDestinationRequestData request)
    {
        if (request.resourceType != AIResourceType.FlankPointCandidates) { return; }

        
        ///// END NEW

        /* int step = request.numSteps; 

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
        */

    }
    #endregion

    #region Redundant Code

    /* private void UpdateFieldOfViewResults(bool playerSeen)
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
 */
    /// <summary>
    /// First checks if the target is within FOV cone, then performs a capsule cast from waist to eye level to check for target colliders
    /// If target collider(s) are hit, performs a line of sight check to see if the target is visible.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    /* private bool EvaluateFieldOfView(Collider target)
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
     }*/

    //private void AddFallbackPoints(Collider target, Vector3[] hitPoints, ref int startIndex)
    //{
    //    hitPoints[startIndex++] = target.bounds.center + Vector3.up * target.bounds.extents.y;
    //    hitPoints[startIndex++] = target.bounds.center - Vector3.right * target.bounds.extents.x;
    //    hitPoints[startIndex++] = target.bounds.center + Vector3.right * target.bounds.extents.x;
    //}

    /*
    private void AlertStatusUpdated(AlertStatus status)
    {
        _alertStatus = status;
        _agentEventManager.AlertStatusChanged(status);
    }*/
    #endregion
}
