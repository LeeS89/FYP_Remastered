using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EnemyFSMController : ComponentEvents
{
    private bool _waitForNewDestination = true;
    private Stack<DestinationRequest> _destinationStack = new Stack<DestinationRequest>();
   
    private bool _isWaitingToSetDestination = false;
    
    #region Destination Updates
    private void CarveOnDestinationReached(bool _)
    {
        ToggleAgent(false);
        _obstacle.enabled = true;
    }

    /* private void UpdateAgentDestination(Vector3 newDestination, int stoppingDistance = 0)
     {

         if (!_agent.enabled)
         {
             _obstacle.enabled = false;
             //_agent.stoppingDistance = stoppingDistance;
             StartCoroutine(SetDestinationDelay(newDestination, stoppingDistance));
             return;
         }

         _agent.stoppingDistance = stoppingDistance;
         _agent.SetDestination(LineOfSightUtility.GetClosestPointOnNavMesh(newDestination));
         //_enemyEventManager.DestinationReached(false);

     }*/

    private struct DestinationRequest
    {
        public Vector3 NewDestination;
        public int StoppingDistance;
        public DestinationRequest(Vector3 newDestination, int stoppingDistance)
        {
            NewDestination = newDestination;
            StoppingDistance = stoppingDistance;
        }
    }

    private void UpdateAgentDestination(Vector3 newDestination, int stoppingDistance = 0)
    {
        
        _destinationStack.Push(new DestinationRequest(newDestination, stoppingDistance));
        if (_testCoroutine)
        {
            Debug.LogError("Stack count on Call: "+_destinationStack.Count);
        }

        if (!_isWaitingToSetDestination)
        {
            StartCoroutine(SetDestinationDelay(_destinationStack));
        }

      
        //_agent.stoppingDistance = stoppingDistance;
        //_agent.SetDestination(LineOfSightUtility.GetClosestPointOnNavMesh(newDestination));
        //_enemyEventManager.DestinationReached(false);

    }
    public bool _testCoroutine = false;
    private IEnumerator SetDestinationDelay(Stack<DestinationRequest> pendingDestinations)
    {
        _isWaitingToSetDestination = true;

        if (!_agent.enabled)
        {
            _obstacle.enabled = false;
        }

        yield return new WaitForSeconds(0.15f);

        if (!_agent.enabled)
        {
            ToggleAgent(true);
        }

        while (_destinationStack.Count > 0)
        {
            var latestRequest = _destinationStack.Pop();
            _destinationStack.Clear(); // discard outdated ones
            if (_testCoroutine)
            {
                Debug.LogError("Stack count on clear: " + _destinationStack.Count);
            }
            _agent.stoppingDistance = latestRequest.StoppingDistance;
            _agent.SetDestination(latestRequest.NewDestination);

            // If new requests came in during the delay+SetDestination, loop again
            if (_destinationStack.Count > 0)
            {
                if (_testCoroutine)
                {
                    Debug.LogError("Stack count on re run: " + _destinationStack.Count);
                }
                if (!_agent.enabled)
                {
                    _obstacle.enabled = false;
                }
                yield return new WaitForSeconds(0.15f); // wait again before applying
                if (!_agent.enabled)
                {
                    ToggleAgent(true);
                }
            }
        }

        _isWaitingToSetDestination = false;

    }


    /// <summary>
    /// Used when transitioning from stationary to moving
    /// to give time for disabling carving and re enabling the agent
    /// </summary>
    /// <param name="newDestination"></param>
    /// <param name="stoppingDistance"></param>
    /// <returns></returns>
    //private IEnumerator SetDestinationDelay(Vector3 newDestination, int stoppingDistance)
    //{
    //    //if (!_waitForNewDestination)
    //    //yield return new WaitUntil(() => _waitForNewDestination);
    //    //_waitForNewDestination = false;
    //    //yield return new WaitForEndOfFrame();
    //    yield return new WaitForSeconds(0.15f);
    //    ToggleAgent(true);

    //    _agent.stoppingDistance = stoppingDistance;
    //    _agent.SetDestination(LineOfSightUtility.GetClosestPointOnNavMesh(newDestination));
    //    //_enemyEventManager.DestinationReached(false);
    //    //_waitForNewDestination = true;

    //}

    private void ToggleAgent(bool agentEnabled)
    {
        _agent.enabled = agentEnabled;
    }
    #endregion
}
