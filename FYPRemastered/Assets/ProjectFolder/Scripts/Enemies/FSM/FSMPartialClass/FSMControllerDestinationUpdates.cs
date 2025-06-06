using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EnemyFSMController : ComponentEvents
{


    #region Destination Updates
    private void CarveOnDestinationReached(bool reached)
    {
        //if(_currentState == _patrol) { return; }
        if(!reached) { return; }
        ToggleAgent(false);
        _obstacle.enabled = true;
    }

    

    private void ToggleAgent(bool agentEnabled)
    {
        if(_agent.enabled == agentEnabled) { return; }

        _agent.enabled = agentEnabled;
    }
    #endregion
}
