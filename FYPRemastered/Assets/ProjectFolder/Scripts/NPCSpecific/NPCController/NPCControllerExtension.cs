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
        bool seenNow = result == FOVResult.ClearFov;

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
    public static void CalculateFOVResultStreakNew(
        this FOVResult result,
        ref uint visibleStreak,
        ref uint notVisibleStreak,
        uint requiredSeenStreak,
        uint requiredNotSeenStreak,
        Action<FOVResult> onResultStable)
    {
        bool seenNow = result == FOVResult.ClearFov;

        if (seenNow)
        {
            visibleStreak++;
            notVisibleStreak = 0;

            if (visibleStreak >= requiredSeenStreak)
            {
                visibleStreak = 0;
                onResultStable?.Invoke(result);
            }
        }
        else
        {
            notVisibleStreak++;
            visibleStreak = 0;

            if (notVisibleStreak >= requiredNotSeenStreak)
            {
                notVisibleStreak = 0;
                onResultStable?.Invoke(result);
            }
        }
    }


   
}
