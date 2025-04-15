using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class StationaryState : EnemyState
{
    private GameObject _owner;
    private bool _isStationary = false;
    private NavMeshPath _path;
    private bool _pathCheckComplete = false;
    private bool _shouldUpdateDestination = false;
    private WaitUntil _waitUntilPathCheckComplete;
    private float _alertPhaseStoppingDistance;

    //fgdhfg
    private float _entryDistance;

    public StationaryState(EnemyEventManager eventManager, GameObject owner) : base(eventManager) 
    { 
        _owner = owner;
        _path = new NavMeshPath();
        _waitUntilPathCheckComplete = new WaitUntil(() => _pathCheckComplete);
    }
   

    public override void EnterState(AlertStatus alertStatus = AlertStatus.None, float stoppingDistance = 0)
    {
        base.EnterState(alertStatus);

        switch (_alertStatus)
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
                _alertPhaseStoppingDistance = stoppingDistance;
                _entryDistance = Vector3.Distance(_owner.transform.position, GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position);
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

       /* _eventManager.DestinationUpdated(_owner.transform.position);
        _eventManager.SpeedChanged(0f, 10f);*/
    }

    private IEnumerator ChasePlayerRoutine()
    {
        //Vector3 _playerPos = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position;
        
        yield return new WaitForSeconds(0.5f);
        while (_isStationary)
        {
           

            while (CheckIfPlayerHasMoved())
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

                
            }
           
            yield return null;

        }

    }

    private void CheckIfDestinationShouldUpdate()
    {
        _pathCheckComplete = false;

        float dis = Vector3.Distance(_owner.transform.position, GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position);
        //Debug.LogError("Distance: " + dis + " and alertStop distance: " + _entryDistance);
        _shouldUpdateDestination = dis > (_entryDistance * 1.2f);
        _pathCheckComplete = true;

        /*PathCheckRequest request = new PathCheckRequest
        {
            From = LineOfSightUtility.GetClosestPointOnNavMesh(_owner.transform.position),//_owner.transform.position,
            To = LineOfSightUtility.GetClosestPointOnNavMesh(GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position),
            Path = _path,
            OnComplete = (distance) =>
            {
                
                float dis = Vector3.Distance(_owner.transform.position, GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position);
                Debug.LogError("Distance: " + distance + " and alertStop distance: " + _alertPhaseStoppingDistance + " and vec distance: "+dis);
                _shouldUpdateDestination = dis > (_entryDistance +0.2f);
                _pathCheckComplete = true;
            }
        };

        PathDistanceBatcher.RequestCheck(request);*/
    }

    public override void ExitState()
    {
        if (_coroutine != null)
        {
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
}
