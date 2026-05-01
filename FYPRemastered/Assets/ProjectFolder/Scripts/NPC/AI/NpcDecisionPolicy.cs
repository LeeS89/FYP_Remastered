using Npc.Internal;
using UnityEngine;

public static class NpcDecisionPolicy
{
    public static void Decide(this INpcBrainContext self, in NpcNotification n)
    {
        if (n.Kind == NotificationType.NoCurrentState)
        {
            self.OverrideSpeed(SpeedOverride.ForceWalk);
            self.SwitchState(StateId.Patrol);
            return;
        }

        if(n.Kind == NotificationType.AnimationRequest)
        {
            self.SendAnimationIntent(n.Clip);
            return;
        }

        // If IsDead => return, unless notification is Death related
        var state = self.CurrentFsmState;

        switch (state)
        {
            case StateId.Patrol or StateId.Search:
                DecidePatrol(self, n);
                break;
            case StateId.Chase:
                DecideChase(self, n);
                break;
            case StateId.Flank:
                DecideFlank(self, n);
                break;
            default:
                // Undefined State - Log
                break;
        }
    }

    private static void DecidePatrol(INpcBrainContext self, in NpcNotification n)
    {

        switch (n.Kind)
        {
            case NotificationType.FOVUpdate:
                //Debug.LogError("Receiving FOV update of: "+n.FOVResult.ToString());
                // if (!FOVStatusChanged(self, n.FOVResult)) return;

                // self.UpdateCurrentFovStatus(n.FOVResult);
                if (TargetSeen(n.FOVResult))
                {
                    self.SwitchState(StateId.Chase);
                    self.UpdateAlertPhase(AlertPhase.Alerted);
                    self.OverrideSpeed(SpeedOverride.None);
                    self.TryBroadcastAlert();

                    // eventually => Check current health bracket + Targets Health, and possible nextIntent will be Takecover/ flee
                    return;
                }
                break;
            case NotificationType.ZoneAlert:

                self.SwitchState(StateId.Chase);

                // eventually => Check current health bracket + Targets Health, and possible nextIntent will be Takecover/ flee
                return;
            case NotificationType.DestinationSet:
                self.MapDestinationToZone(n.Destination);
                return;
            default:
                return;// Or Log Unhandled
        }

    }

    private static bool TargetSeen(FOVResult result) => result == FOVResult.ClearFov || result == FOVResult.PartialFov || result == FOVResult.TargetSeenAndWithinMeleeRadius
                    || result == FOVResult.TargetSeenAndWithinShootingAngles;

    private static void DecideChase(INpcBrainContext self, in NpcNotification n)
    {

        switch (n.Kind)
        {
            case NotificationType.FOVUpdate:
                if (!FOVStatusChanged(self, n.FOVResult)) return;
                self.UpdateCurrentFovStatus(n.FOVResult);

                /*if (self.IsMoving())*/
                TryUpdateRotationToTarget(self, TargetSeen(n.FOVResult));

                self.UpdateCombatOrder(DecideNextCombatOrder(self.CurrentComOrder, n.FOVResult));

                break;
            /* case NotificationKind.DestinationReached:
                 //TryUpdateRotationToTarget(self, true); // Always rotate to target on destination reached
                 break;*/
            case NotificationType.DestinationSet:
                //  TryUpdateRotationToTarget(self, TargetSeen(self.CurrentFovState)); // Rotate if target is seen while moving
                break;
            default:
                break;
        }
    }

    /// Update function to implement new enum RotationOverride
    private static void TryUpdateRotationToTarget(INpcBrainContext self, bool shouldRotate)
    {
        RotationOverride rotationOverride = shouldRotate ? RotationOverride.ForceLookAtTarget : RotationOverride.None;
        self.OverrideRotation(rotationOverride);
        //if (shouldRotate != self.IsRotatingToTarget()) self.RotateToTarget(shouldRotate);
    }

    /*  [Obsolete]
      private static void DecideRotateToTarget(INPCBrainContext self, in NpcNotification n)
      {
          NotificationKind kind = n.Kind;
          if (kind != NotificationKind.FOVUpdate || kind != NotificationKind.DestinationReached) return;

          StateId currentState = self.CurrentFsmState;
          if (currentState != StateId.Chase || currentState != StateId.Flank || currentState != StateId.Cover)
          {
              if(self.IsRotatingToTarget()) self.RotateToTarget(false);
              return;
          }

          if(currentState == StateId.Chase)
          {
              if(kind == NotificationKind.DestinationReached && !self.IsRotatingToTarget()) self.RotateToTarget(true);
              else if(kind == NotificationKind.DestinationSet)
              {
                  bool shouldRotate = TargetSeen(self.CurrentFovState);
                  if (shouldRotate != self.IsRotatingToTarget()) self.RotateToTarget(shouldRotate);
              }
              else if (kind == NotificationKind.FOVUpdate && self.IsMoving())
              {
                  bool shouldRotate = TargetSeen(n.FOVResult);
                  if (shouldRotate != self.IsRotatingToTarget()) self.RotateToTarget(shouldRotate);
              }
          }
      }*/


    private static void DecideFlank(INpcBrainContext self, in NpcNotification n)
    {
        return;
    }

    private static bool FOVStatusChanged(INpcBrainContext c, FOVResult r) => c.CurrentFovState != r;


    private static CombatOrder DecideNextCombatOrder(CombatOrder currentOrder, FOVResult newFOVStatus)
    {
        CombatOrder newOrder;
        if (newFOVStatus == FOVResult.TargetSeenAndWithinShootingAngles)
            newOrder = CombatOrder.FireAtWill;
        else if (newFOVStatus == FOVResult.TargetSeenAndWithinMeleeRadius)
            newOrder = CombatOrder.MeleeAttack;
        else newOrder = CombatOrder.HoldFire;

        if (newOrder == currentOrder) return currentOrder;//CombatOrder.None; // Already executing order, do nothing

        return newOrder;
    }
    /* extension(IEnumerable<int> source)
     {
         public IEnumerable<int> WhereGreaterThan(int threshold)
         => source.Where(x => x > threshold);
     }*/
}































public interface INpcBrain
{
   void Decide(INpcBrainContext context, in NpcNotification n);
}







public class NpcDecisionPolicyNew : INpcBrain
{

    public static readonly NpcDecisionPolicyNew Instance = new NpcDecisionPolicyNew();
    private NpcDecisionPolicyNew() { }

    public void Decide(INpcBrainContext self, in NpcNotification n)
    {
        if (n.Kind == NotificationType.NoCurrentState)
        {
            self.OverrideSpeed(SpeedOverride.ForceWalk);
            self.SwitchState(StateId.Patrol);
            return;
        }

        if(n.Kind == NotificationType.AnimationRequest)
        {
            self.SendAnimationIntent(n.Clip);
            return;
        }

        // If IsDead => return, unless notification is Death related
        var state = self.CurrentFsmState;

        switch (state)
        {
            case StateId.Patrol or StateId.Search:
                DecidePatrol(self, n);
                break;
            case StateId.Chase:
                DecideChase(self, n);
                break;
            case StateId.Flank:
                DecideFlank(self, n);
                break;
            default:
                // Undefined State - Log
                break;
        }
    }

    private void DecidePatrol(INpcBrainContext self, in NpcNotification n)
    {

        switch (n.Kind)
        {
            case NotificationType.FOVUpdate:
                //Debug.LogError("Receiving FOV update of: "+n.FOVResult.ToString());
                // if (!FOVStatusChanged(self, n.FOVResult)) return;

                // self.UpdateCurrentFovStatus(n.FOVResult);
                if (TargetSeen(n.FOVResult))
                {
                    self.SwitchState(StateId.Chase);
                    self.UpdateAlertPhase(AlertPhase.Alerted);
                    self.OverrideSpeed(SpeedOverride.None);
                    self.TryBroadcastAlert();

                    // eventually => Check current health bracket + Targets Health, and possible nextIntent will be Takecover/ flee
                    return;
                }
                break;
            case NotificationType.ZoneAlert:

                self.SwitchState(StateId.Chase);

                // eventually => Check current health bracket + Targets Health, and possible nextIntent will be Takecover/ flee
                return;
            case NotificationType.DestinationSet:
                self.MapDestinationToZone(n.Destination);
                return;
            default:
                return;// Or Log Unhandled
        }

    }

    private bool TargetSeen(FOVResult result) => result == FOVResult.ClearFov || result == FOVResult.PartialFov || result == FOVResult.TargetSeenAndWithinMeleeRadius
                    || result == FOVResult.TargetSeenAndWithinShootingAngles;

    private void DecideChase(INpcBrainContext self, in NpcNotification n)
    {

        switch (n.Kind)
        {
            case NotificationType.FOVUpdate:
                if (!FOVStatusChanged(self, n.FOVResult)) return;
                self.UpdateCurrentFovStatus(n.FOVResult);

                /*if (self.IsMoving())*/
                TryUpdateRotationToTarget(self, TargetSeen(n.FOVResult));

                self.UpdateCombatOrder(DecideNextCombatOrder(self.CurrentComOrder, n.FOVResult));

                break;
            /* case NotificationKind.DestinationReached:
                 //TryUpdateRotationToTarget(self, true); // Always rotate to target on destination reached
                 break;*/
            case NotificationType.DestinationSet:
                //  TryUpdateRotationToTarget(self, TargetSeen(self.CurrentFovState)); // Rotate if target is seen while moving
                break;
            default:
                break;
        }
    }

    /// Update function to implement new enum RotationOverride
    private void TryUpdateRotationToTarget(INpcBrainContext self, bool shouldRotate)
    {
        RotationOverride rotationOverride = shouldRotate ? RotationOverride.ForceLookAtTarget : RotationOverride.None;
        self.OverrideRotation(rotationOverride);
        //if (shouldRotate != self.IsRotatingToTarget()) self.RotateToTarget(shouldRotate);
    }

    /*  [Obsolete]
      private static void DecideRotateToTarget(INPCBrainContext self, in NpcNotification n)
      {
          NotificationKind kind = n.Kind;
          if (kind != NotificationKind.FOVUpdate || kind != NotificationKind.DestinationReached) return;

          StateId currentState = self.CurrentFsmState;
          if (currentState != StateId.Chase || currentState != StateId.Flank || currentState != StateId.Cover)
          {
              if(self.IsRotatingToTarget()) self.RotateToTarget(false);
              return;
          }

          if(currentState == StateId.Chase)
          {
              if(kind == NotificationKind.DestinationReached && !self.IsRotatingToTarget()) self.RotateToTarget(true);
              else if(kind == NotificationKind.DestinationSet)
              {
                  bool shouldRotate = TargetSeen(self.CurrentFovState);
                  if (shouldRotate != self.IsRotatingToTarget()) self.RotateToTarget(shouldRotate);
              }
              else if (kind == NotificationKind.FOVUpdate && self.IsMoving())
              {
                  bool shouldRotate = TargetSeen(n.FOVResult);
                  if (shouldRotate != self.IsRotatingToTarget()) self.RotateToTarget(shouldRotate);
              }
          }
      }*/


    private void DecideFlank(INpcBrainContext self, in NpcNotification n)
    {
        return;
    }

    private bool FOVStatusChanged(INpcBrainContext c, FOVResult r) => c.CurrentFovState != r;


    private CombatOrder DecideNextCombatOrder(CombatOrder currentOrder, FOVResult newFOVStatus)
    {
        CombatOrder newOrder;
        if (newFOVStatus == FOVResult.TargetSeenAndWithinShootingAngles)
            newOrder = CombatOrder.FireAtWill;
        else if (newFOVStatus == FOVResult.TargetSeenAndWithinMeleeRadius)
            newOrder = CombatOrder.MeleeAttack;
        else newOrder = CombatOrder.HoldFire;

        if (newOrder == currentOrder) return currentOrder;//CombatOrder.None; // Already executing order, do nothing

        return newOrder;
    }
    /* extension(IEnumerable<int> source)
     {
         public IEnumerable<int> WhereGreaterThan(int threshold)
         => source.Where(x => x > threshold);
     }*/
}
