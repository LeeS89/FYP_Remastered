using System;
using UnityEngine;

public static class NPCControllerExtension
{
    public static void CalculateFOVResultStreak(
        this FOVResult result,
        ref bool isVisible,
        ref uint visibleStreak,
        ref uint notVisibleStreak,
        uint requiredSeenStreak,
        uint requiredNotSeenStreak,
        Action onSeenStable,
        Action onNotSeenStable)
    {
        bool seenNow = result == FOVResult.TargetSeen;

        if (seenNow)
        {
            visibleStreak++;
            notVisibleStreak = 0;

            if (!isVisible && visibleStreak >= requiredSeenStreak)
            {
                isVisible = true;
                onSeenStable?.Invoke();
            }
        }
        else
        {
            notVisibleStreak++;
            visibleStreak = 0;

            if (isVisible && notVisibleStreak >= requiredNotSeenStreak)
            {
                isVisible = false;
                onNotSeenStable?.Invoke();
            }
        }
    }


    public static void RotateTowardsTarget(this IFSMData controller, Transform target, bool rotate)
    {
        if (controller == null || target == null ||
            controller.Agent == null || controller.Transform == null) return;

        if (!rotate)
        {
            if(!controller.Agent.updateRotation) controller.Agent.updateRotation = true;
            return;
        }
        if (controller.Agent.updateRotation) controller.Agent.updateRotation = false;

        Transform t = controller.Transform;
        Vector3 toTarget = target.position - t.position;
        toTarget.y = 0;

        if (toTarget.sqrMagnitude < 0.0001f) return;

        Vector3 forward = t.forward;
        forward.y = 0;

        float dot = Vector3.Dot(forward.normalized, toTarget.normalized);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        const float precisionThreshold = 1f;
        Quaternion targetRotation = Quaternion.LookRotation(toTarget);

        if(angle < precisionThreshold)
        {
            t.rotation = Quaternion.Slerp(
                t.rotation,
                targetRotation,
                1f);
            return;
        }

        t.rotation = Quaternion.Slerp(
            t.rotation,
            targetRotation,
            Time.deltaTime * 5f);

    }
}
