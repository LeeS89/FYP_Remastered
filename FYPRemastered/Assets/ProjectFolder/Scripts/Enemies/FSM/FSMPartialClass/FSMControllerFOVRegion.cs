using UnityEngine;
using UnityEngine.UIElements;

public partial class EnemyFSMController : ComponentEvents
{
    #region Field Of View functions

    public bool _capsuleResultTest = false;

   



    private static bool _testAlert = false;
    private void UpdateFieldOfViewResults(bool playerSeen)
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
    }

    public void EnterAlertPhase()
    {

        if (!_testAlert)
        {
            _testAlert = true;
            //BaseSceneManager._instance.AlertZoneAgents(_blockZone, this);
            SceneEventAggregator.Instance.AlertZoneAgents(_blockZone, this);


        }

        if (_currentState != _chasing && _alertStatus == AlertStatus.None)
        {
            //BaseSceneManager._instance.AlertZoneAgents(_blockZone, this);
            _enemyEventManager.ChangeAnimatorLayerWeight(EnemyAnimController.AnimationLayer.Alert, 0, 1, 0.5f, true);
            AlertStatusUpdated(AlertStatus.Alert);
            //_alertStatus = AlertStatus.Alert;

            PursuitTargetRequested(AIDestinationType.ChaseDestination);
            //ChangeState(_chasing);
        }
        /*if (!_testAlert)
        {
            _testAlert = true;
            BaseSceneManager._instance.AlertZoneAgents(_blockZone, this);
            

        }

        if (_currentState != _chasing && _alertStatus == AlertStatus.None)
        {
           //BaseSceneManager._instance.AlertZoneAgents(_blockZone, this);
            _enemyEventManager.ChangeAnimatorLayerWeight(1, 0, 1, 0.5f, true);
            AlertStatusUpdated(AlertStatus.Alert);
            //_alertStatus = AlertStatus.Alert;
           
            ChangeState(_chasing);
        }*/


    }

    private void AlertStatusUpdated(AlertStatus status)
    {
        _alertStatus = status;
        _enemyEventManager.AlertStatusChanged(status);
    }
    #endregion







    #region Redundant Code
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
    #endregion
}
