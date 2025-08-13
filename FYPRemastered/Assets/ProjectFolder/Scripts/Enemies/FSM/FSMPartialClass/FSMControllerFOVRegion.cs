using UnityEngine;
using UnityEngine.UIElements;

public partial class EnemyFSMController : FSMControllerBase
{
    #region Field Of View functions

    public bool TargetInView { get; private set; } = false;

    protected override void TargetInViewStatusUpdated(bool inView)
    {
        if(TargetInView == inView) { return; }

        TargetInView = inView;

        if(_alertStatus != AlertStatus.None) { return;}

        if (TargetInView)
        {
            EnterAlertPhase();
            CallForBackup();
        }
    }

   
    protected override void CallForBackup()
    {
        SceneEventAggregator.Instance.AlertZoneAgents(AgentZone, this);
    }

    public override void EnterAlertPhase()
    {

        if (_currentState != _chasing && _alertStatus == AlertStatus.None) /// Need to change Alert status param to something else
        {
            _alertStatus = AlertStatus.Alert;
            
            _agentEventManager.ChangeAnimatorLayerWeight(EnemyAnimController.AnimationLayer.Alert, 0, 1, 0.5f, true);
           
            //_alertStatus = AlertStatus.Alert;

            _agentEventManager.DestinationRequested(AIDestinationType.ChaseDestination);
           
        }
       
    }

    #endregion


}
