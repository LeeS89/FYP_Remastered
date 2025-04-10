using UnityEngine;

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

        PerformFieldOfViewCheck();
    }


    private void PerformFieldOfViewCheck()
    {
        if (Time.time >= _nextCheckTime && _fov != null)
        {
            _nextCheckTime = Time.time + _fovCheckFrequency;
            bool playerSeen = false;

            // CheckForTarget first performs a Physics.CheckSphere. If check passes, an overlapsphere is performed which returns the targets collider information
            _fov.CheckForTarget(_fovLocation, out _fovTraceResults, _fovTraceRadius, _fovLayerMask, true);

            if (_fovTraceResults.Length > 0)
            {
                playerSeen = LineOfSightUtility.HasLineOfSight(_fovLocation, _fovTraceResults[0].transform, 360f, _lineOfSightMask);

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

    private void UpdateFieldOfViewResults(bool playerSeen)
    {
        if (_canSeePlayer == playerSeen) { return; } // Already Updated, return early

        _canSeePlayer = playerSeen;

        if (_canSeePlayer)
        {
            if (_currentState != _chasing)
            {
                ChangeState(_chasing);
            }
            //_animController.SetAlertStatus(true);
            _enemyEventManager.PlayerSeen(_canSeePlayer);
            _enemyEventManager.ChangeAnimatorLayerWeight(1, 0, 1, 0.5f, true);
            // Alert Group Here (Moon Scene Manager)

        }
        //else
        //{
        //    //_animController.SetAlertStatus(false);
        //    _enemyEventManager.PlayerSeen(false);
        //    _enemyEventManager.ChangeAnimatorLayerWeight(1, 1, 0, 0.5f, false);
        //}

        //Update Shooting Component Here
    }
    #endregion
}
