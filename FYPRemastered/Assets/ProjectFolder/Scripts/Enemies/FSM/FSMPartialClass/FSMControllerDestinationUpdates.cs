using System.Collections;
using UnityEngine;

public partial class EnemyFSMController : ComponentEvents
{
    #region Destination Updates
    private void CarveOnDestinationReached(bool _)
    {
        ToggleAgent(false);
        _obstacle.enabled = true;
    }

    private void UpdateAgentDestination(Vector3 newDestination, int stoppingDistance = 0)
    {
        
        if (!_agent.enabled)
        {
            _obstacle.enabled = false;
            //_agent.stoppingDistance = stoppingDistance;
            StartCoroutine(SetDestinationDelay(newDestination, stoppingDistance));
            return;
        }
        
        _agent.stoppingDistance = stoppingDistance;
        _agent.SetDestination(newDestination);
        _enemyEventManager.DestinationReached(false);

    }

    /// <summary>
    /// Used when transitioning from stationary to moving
    /// to give time for disabling carving and re enabling the agent
    /// </summary>
    /// <param name="newDestination"></param>
    /// <param name="stoppingDistance"></param>
    /// <returns></returns>
    private IEnumerator SetDestinationDelay(Vector3 newDestination, int stoppingDistance)
    {
        yield return new WaitForSeconds(0.15f);
        ToggleAgent(true);
        
        _agent.stoppingDistance = stoppingDistance;
        _agent.SetDestination(newDestination);
        _enemyEventManager.DestinationReached(false);

    }

    private void ToggleAgent(bool agentEnabled)
    {
        _agent.enabled = agentEnabled;
    }
    #endregion
}
