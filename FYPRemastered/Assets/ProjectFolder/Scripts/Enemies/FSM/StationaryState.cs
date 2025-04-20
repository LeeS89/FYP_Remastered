using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StationaryState : EnemyState
{

    private bool _isStationary = false;
    
    private bool _pathCheckComplete = false;
    private bool _shouldUpdateDestination = false;
    private WaitUntil _waitUntilPathCheckComplete;
    //private float _alertPhaseStoppingDistance;

    //
    private float _stateEntryDistanceFromPlayer;

    public StationaryState(EnemyEventManager eventManager, GameObject owner, NavMeshPath path) : base(eventManager, path, owner) 
    { 
      
        _waitUntilPathCheckComplete = new WaitUntil(() => _pathCheckComplete);
    }
   

    public override void EnterState(Vector3? destination = null, AlertStatus alertStatus = AlertStatus.None, float stoppingDistance = 0)
    {
        //base.EnterState(alertStatus);

        switch (alertStatus)
        {
            case AlertStatus.None:
                //_eventManager.DestinationUpdated(_owner.transform.position);
                //_eventManager.SpeedChanged(0f, 10f);
                //Debug.LogError("In Stationary State, no alert status");
                break;
            case AlertStatus.Alert:
                //_eventManager.DestinationUpdated(_owner.transform.position);
                _eventManager.SpeedChanged(0f, 10f);
                //GameManager.OnPlayerMoved += SetPlayerMoved;
                //_alertPhaseStoppingDistance = stoppingDistance;
                _stateEntryDistanceFromPlayer = Vector3.Distance(_owner.transform.position, GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position);
                if (_coroutine == null)
                {
                    _isStationary = true;
                    _coroutine = CoroutineRunner.Instance.StartCoroutine(ChasePlayerRoutine());
                }
                /*Debug.LogError("Player moved is: " + _playerHasMoved);
                Debug.LogError("Alert stopping distance is: "+_alertPhaseStoppingDistance);
                Debug.LogError("In Stationary State, with alert status");*/
                break;
            default:
                _eventManager.DestinationUpdated(_owner.transform.position);
                _eventManager.SpeedChanged(0f, 10f);
                break;
        }
        _owner.GetComponent<NavMeshAgent>().enabled = false;
        /*_owner.GetComponent<NavMeshObstacle>().enabled = true;*/
        //_owner.GetComponent<NavMeshAgent>().updateRotation = false;
        /* _eventManager.DestinationUpdated(_owner.transform.position);
         _eventManager.SpeedChanged(0f, 10f);*/
    }

    private IEnumerator ChasePlayerRoutine()
    {
        //Vector3 _playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
        //_owner.GetComponent<NavMeshAgent>().enabled = false;
        yield return new WaitForSeconds(0.5f);
        //_owner.GetComponent<NavMeshObstacle>().enabled = true;
        while (_isStationary)
        {
            Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
            if (IsTargetMovingAndReachable(/*LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position)*/_owner.transform.position, playerPos, _stateEntryDistanceFromPlayer, _path))
            {
                _owner.GetComponent<NavMeshAgent>().enabled = true;
                _isStationary = false;
                _eventManager.RequestChasingState();
                _coroutine = null;
                yield break;
            }
            yield return new WaitForSeconds(1f);

           /* while (CheckIfPlayerHasMoved())
            {
                CheckIfDestinationShouldUpdate();

                yield return _waitUntilPathCheckComplete;


                if (_shouldUpdateDestination)
                {
                    /// Change state to chasing
                    _eventManager.RequestChasingState();
                    _shouldUpdateDestination = false;
                    _isStationary = false;
                    yield return new WaitForEndOfFrame();
                    //SetDestinationReached(false);
                }

                
            }*/

           /* if (!_canSeePlayer)
            {
                UniformZoneGridManager _gridManager = GameObject.FindFirstObjectByType<UniformZoneGridManager>();
                Vector3 newPoint = _gridManager.GetRandomPointXStepsFromPlayer(4);
                _eventManager.DestinationUpdated(newPoint);

                yield return new WaitForSeconds(25f);
            }*/
           
            yield return null;

        }

    }

    private void CheckIfDestinationShouldUpdate()
    {
        _pathCheckComplete = false;

        float dis = Vector3.Distance(_owner.transform.position, GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position);
        //Debug.LogError("Distance: " + dis + " and alertStop distance: " + _entryDistance);
        _shouldUpdateDestination = dis > (_stateEntryDistanceFromPlayer * 1.2f);
        _pathCheckComplete = true;

        PathCheckRequest request = new PathCheckRequest
        {
            From = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position),//_owner.transform.position,
            To = LineOfSightUtility.GetClosestPointOnNavMesh(GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position),
            Path = _path,
            OnComplete = (distance) =>
            {
                
                float dis = Vector3.Distance(_owner.transform.position, GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position);
                //Debug.LogError("Distance: " + distance + " and alertStop distance: " + _alertPhaseStoppingDistance + " and vec distance: "+dis);
                _shouldUpdateDestination = dis > (_stateEntryDistanceFromPlayer +0.2f);
                _pathCheckComplete = true;
            }
        };

        PathDistanceBatcher.RequestCheck(request);
    }

    public override void ExitState()
    {
        if (_coroutine != null)
        {
            _owner.GetComponent<NavMeshAgent>().updateRotation = true;
            //_owner.GetComponent<NavMeshObstacle>().enabled = false;
            //_owner.GetComponent<NavMeshAgent>().enabled = true;
            _isStationary = false;
            CoroutineRunner.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }
        //GameManager.OnPlayerMoved -= SetPlayerMoved;
    }

    public override void OnStateDestroyed()
    {
        base.OnStateDestroyed();
        _path = null;
    }

    public override void UpdateState()
    {
        throw new System.NotImplementedException();
    }

    public override void LateUpdateState()
    {
        if (_owner == null || GameManager.Instance == null)
            return;

        Transform player = GameManager.Instance.GetPlayerPosition(PlayerPart.Position);
        if (player == null)
            return;

        Vector3 toPlayer = player.position - _owner.transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.001f)
            return;

        Vector3 forward = _owner.transform.forward;
        forward.y = 0f;

        // ✅ Check if agent is already facing the player (dot ~ 1 = same direction)
        float dot = Vector3.Dot(forward.normalized, toPlayer.normalized);
        const float facingThreshold = 0.995f; // ~cos(5°)

        if (dot >= facingThreshold)
            return; // ✅ Already facing the player closely enough

        // ✅ Apply smooth rotation toward player
        Quaternion targetRotation = Quaternion.LookRotation(toPlayer.normalized);
        _owner.transform.rotation = Quaternion.Slerp(
            _owner.transform.rotation,
            targetRotation,
            Time.deltaTime * 3f);
    }
}
