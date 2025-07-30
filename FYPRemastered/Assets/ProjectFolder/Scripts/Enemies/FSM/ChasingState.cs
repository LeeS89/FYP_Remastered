using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class ChasingState : EnemyState
{
  

    public ChasingState(EnemyEventManager eventManager, GameObject owner, float walkSpeed, float sprintSpeed) : base(eventManager, owner)
    {
       
        _walkSpeed = walkSpeed;
        _sprintSpeed = sprintSpeed;
       
    }


    public override void EnterState()
    {
        if (_coroutine == null)
        {
            _destinationApplied = false;
            _coroutine = CoroutineRunner.Instance.StartCoroutine(ChasePlayerRoutiningNew());
        }
    }


   
   

    private IEnumerator ChasePlayerRoutiningNew()
    {
        _eventManager.DestinationReached(false);
        //SetDestinationReached(false);

        yield return _waitUntilDestinationApplied;
        _destinationApplied = false;

        if (_alertStatus == AlertStatus.Flanking)
        {
            _eventManager.RotateTowardsTarget(true);
        }

        while (!CheckDestinationReached())
        {

            _eventManager.SpeedChanged(_canSeePlayer ? _walkSpeed : _sprintSpeed, 5f);

            if (_alertStatus == AlertStatus.Chasing)
            {
                _eventManager.RotateTowardsTarget(_canSeePlayer);
            }

            if (CheckIfPlayerHasMoved())
            {
                _eventManager.DestinationRequested(AIDestinationType.ChaseDestination);

                yield return _waitUntilDestinationApplied;
                _destinationApplied = false;
                yield return new WaitForSeconds(0.15f);
            }

            yield return null;

        }
        _eventManager.RequestStationaryState(AlertStatus.Alert);

    }

    

  
    public override void ExitState()
    {

        if (_coroutine != null)
        {
            _eventManager.RotateTowardsTarget(false);
            CoroutineRunner.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }

     
    }

    public override void OnStateDestroyed()
    {
        ExitState();
        //_eventManager.OnDestinationReached -= SetDestinationReached;
        base.OnStateDestroyed();
    }

}
