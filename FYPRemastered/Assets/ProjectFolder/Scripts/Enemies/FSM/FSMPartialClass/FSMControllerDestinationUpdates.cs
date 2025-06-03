using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EnemyFSMController : ComponentEvents
{
   
    
    #region Destination Updates
    private void CarveOnDestinationReached(bool _)
    {
        ToggleAgent(false);
        _obstacle.enabled = true;
    }

    

    private void ToggleAgent(bool agentEnabled)
    {
        _agent.enabled = agentEnabled;
    }
    #endregion
}
