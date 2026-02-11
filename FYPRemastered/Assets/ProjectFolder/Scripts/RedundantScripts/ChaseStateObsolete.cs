using System;
using UnityEngine;

[Obsolete]
public sealed class ChaseStateObsolete : IntentStateBaseObsolete
{
    public static readonly ChaseStateObsolete Instance = new();
    private ChaseStateObsolete() { Id = StateId.Chase; }

    public override void Enter(/*IFSMOwner self*/)
    {
       // self.FSM?.BeginChase(Id);
        // public void Chase();
        // Pass a struct containing the following information:
        // Destination Provider, Possible Func<List<Vector3>>
        // bool UseRandomStoppingdistance
        // MinStopDistance
        // MaxStopDistance
    }

   /* public override void Exit(IFSMOwner self)
    {
        // Public void CancelCurrent(token);
    }*/

    public override void Handle(/*IFSMOwner self,*/ NpcNotification n)
    {
        switch (n.Kind)
        {
           // case NotificationKind.TargetLOSLost:
                /*if (n.IsCurrentlyMoving)*/  /*Come back to this, need to know if moving */ //self.SwitchTo(Flank.Instance); // Ensure to wait a few seconds after losing LOS, then check again before sending Notification
                // Plus Notify Zone handler of lost LOS => If all lost LOS switch to Search last known
             //   break;
            case NotificationKind.PathBlocked or NotificationKind.NoAvailablePath:
               // self.SwitchTo(FollowGroup.Instance);
                break;
            case NotificationKind.TargetMoved:
                break;
            default:
              //  base.Handle(self, n);
                //self.SwitchTo(Patrol.Instance);
                break;
        }
    }

    // FSM Functions For ChaseState
   
}


/*[Obsolete("", true)]
public interface IFSMOwner // Maybe Obsolete
{
    // void TryBroadcastAlert(); // Remove, NPCControllerBase will handle this
    void LogUnhandled(IntentStateBaseObsolete state, in NpcNotification notification);
    void SwitchTo(IIntentStateObsolete next);
    void HandleFOVSweepResult(FOVResult result, bool withinAttackAngles);
    IFSMControlObsolete FSM { get; }

}*/