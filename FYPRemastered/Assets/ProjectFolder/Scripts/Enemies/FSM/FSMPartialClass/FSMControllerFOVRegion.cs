using UnityEngine;
using UnityEngine.UIElements;

public partial class EnemyFSMController : ComponentEvents
{
    #region Field Of View functions
    private void UpdateFieldOfViewCheckFrequency()
    {
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

        RunFieldOfViewCheck();
    }


    private void RunFieldOfViewCheck()
    {
        if (Time.time >= _nextCheckTime && _fov != null)
        {
            _nextCheckTime = Time.time + _fovCheckFrequency;
            bool playerSeen = false;

            // CheckForTarget first performs a Physics.CheckSphere. If check passes, an overlapsphere is performed which returns the targets collider information
            int numTargetsDetected = _fov.CheckTargetProximity(_fovLocation, _fovTraceResults, _fovTraceRadius, _fovLayerMask, true);

            if (numTargetsDetected > 0)
            {

                playerSeen = LineOfSightUtility.HasLineOfSight(_fovLocation, _fovTraceResults, _angle, _shootAngleThreshold, _lineOfSightMask, _fovLayerMask, out _canShootPlayer);


                _enemyEventManager.FacingTarget(_canShootPlayer);
               
                if (_fieldOfViewStatus != FieldOfViewFrequencyStatus.Heightened)
                    _fieldOfViewStatus = FieldOfViewFrequencyStatus.Heightened;
            }
            else
            {

                if (_fieldOfViewStatus != FieldOfViewFrequencyStatus.Normal)
                    _fieldOfViewStatus = FieldOfViewFrequencyStatus.Normal;
            }

            UpdateFieldOfViewResults(playerSeen);


        }
    }


    private bool EvaluateFieldOfView(Collider target)
    {
        if(target == null) { return false; }

        if (!_fov.IsWithinView(_fovLocation, target.bounds.center, _angle * 0.5f, _angle * 0.5f)) { return false; }
        

        return false;
    }



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

            _enemyEventManager.PlayerSeen(_canSeePlayer);
            //_enemyEventManager.ChangeAnimatorLayerWeight(1, 0, 1, 0.5f, true);
            // Alert Group Here (Moon Scene Manager)

        }
        else
        {
        //    //_animController.SetAlertStatus(false);
            _enemyEventManager.PlayerSeen(false);
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
            _enemyEventManager.ChangeAnimatorLayerWeight(1, 0, 1, 0.5f, true);
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
}
