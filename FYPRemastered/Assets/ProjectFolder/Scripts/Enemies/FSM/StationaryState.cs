using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StationaryState : EnemyState
{
    private int _agentId = 0;
    private bool _isStationary = false;
    
    private bool _pathCheckComplete = false;
    /*private bool _shouldUpdateDestination = false;*/
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
                
                break;
            case AlertStatus.Alert:
               
                _eventManager.SpeedChanged(0f, 10f);
               
                /*_stateEntryDistanceFromPlayer = Vector3.Distance(_owner.transform.position, GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position);*/
                //_stateEntryDistanceFromPlayer = (GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position - _owner.transform.position).sqrMagnitude;
               /* Debug.LogError("Distance on entry: " + _stateEntryDistanceFromPlayer);*/
                if (_coroutine == null)
                {
                    _isStationary = true;
                    _coroutine = CoroutineRunner.Instance.StartCoroutine(ChasePlayerRoutine());

                    
                }
               
                break;
            default:
                _eventManager.DestinationUpdated(_owner.transform.position);
                _eventManager.SpeedChanged(0f, 10f);
                break;
        }
        _owner.GetComponent<NavMeshAgent>().updateRotation = false;
       
    }

    private IEnumerator ChasePlayerRoutine()
    {
       
        yield return new WaitForSeconds(0.5f);
        float bufferMultiplier = Random.Range(1.2f, 1.45f);
        _agentId = StationaryChaseManagerJob.Instance.RegisterAgent(_owner.transform.position, bufferMultiplier, this);
        //_stateEntryDistanceFromPlayer = (GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position - _owner.transform.position).sqrMagnitude; //Vector3.Distance(_owner.transform.position, GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position);
        //Debug.LogError("Distance on entry: " + _stateEntryDistanceFromPlayer);

        while (_isStationary)
        {
            /*Vector3 playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
            if (IsTargetMovingAndReachable(LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position)*//*_owner.transform.position*//*, LineOfSightUtility.GetClosestPointOnNavMesh(playerPos)*//* playerPos*//*, _stateEntryDistanceFromPlayer, _path))
            {
          
                _isStationary = false;
                _eventManager.RequestChasingState();
               
                yield break;
            }*/

            yield return _waitUntilPathCheckComplete;
            _pathCheckComplete = false;

            if (_shouldUpdateDestination)
            {
                _isStationary = false;
                _eventManager.RequestChasingState();

                yield break;
            }

            //yield return new WaitForSeconds(1f);

          

           /* if (!_canSeePlayer)
            {
                UniformZoneGridManager _gridManager = GameObject.FindFirstObjectByType<UniformZoneGridManager>();
                Vector3 newPoint = _gridManager.GetRandomPointXStepsFromPlayer(4);
                _eventManager.DestinationUpdated(newPoint);

                yield return new WaitForSeconds(25f);
            }*/
           
            //yield return null;

        }

    }

    public override void SetChaseEligibility(bool canChase, Vector3 playerPosition)
    {
        base.SetChaseEligibility(canChase, playerPosition);

        if (canChase)
        {
           _shouldUpdateDestination = HasClearPathToTarget(LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position), playerPosition, _path);
                  
        }

        _pathCheckComplete = true;
    }



    public override void ExitState()
    {
        if (_coroutine != null)
        {
            StationaryChaseManagerJob.Instance.UnregisterAgent(_agentId);
            _owner.GetComponent<NavMeshAgent>().updateRotation = true;
            //_owner.GetComponent<NavMeshObstacle>().enabled = false;
            //_owner.GetComponent<NavMeshAgent>().enabled = true;
            _isStationary = false;
            _pathCheckComplete = false;
            _shouldUpdateDestination = false;
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
