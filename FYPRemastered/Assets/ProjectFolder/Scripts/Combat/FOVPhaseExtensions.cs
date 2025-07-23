using System.Runtime.CompilerServices;
using UnityEngine;

public static class FOVPhaseExtensions
{
    public static int RunDetectionPhase(this CombatComponent combat, in FOVPhaseParams detectionParams)
    {
      
        int count = detectionParams.traceComponent.CheckTargetProximity(
            detectionParams.detectionOrigin, 
            detectionParams.detectionResults, 
            detectionParams.detectionRadius, 
            detectionParams.detectionTargetMask
            );

        return count;
    }



    public static void RunEvaluationPhase(this CombatComponent combat, in FOVPhaseParams evaluationParams, out int hitCount)
    {
        if (!evaluationParams.traceComponent.IsWithinView(evaluationParams.detectionOrigin, evaluationParams.evaluationTargetPosition, evaluationParams.horizontalViewAngle, evaluationParams.verticalViewAngle))
        {
            hitCount = 0;
            return;
        }

            hitCount = evaluationParams.traceComponent.EvaluateViewCone(
            evaluationParams.evaluationStart,
            evaluationParams.evaluationEnd,
            evaluationParams.evaluationRadius,
            evaluationParams.evaluationDirection,
            evaluationParams.evaluationDistance,
            evaluationParams.detectionTargetMask,
            evaluationParams.evaluationHitPoints
            );

        if (hitCount == 0)
        {
            return;
        }

        if (evaluationParams.addFallbackPoints)
        {
            AddFallbackPoints(evaluationParams.targetCollider, evaluationParams.evaluationHitPoints, ref hitCount);
        }

       
    }


    public static bool RunTargetingPhase(this CombatComponent combat, in FOVPhaseParams targetParams, int targetCount)
    {
        for(int i = 0; i < targetCount; i++)
        {
            if (!targetParams.traceComponent.HasLineOfSight(
                targetParams.detectionOrigin,
                targetParams.evaluationHitPoints[i], 
                targetParams.targetingBlockingMask, 
                targetParams.detectionTargetMask,
                targetParams.ownerOrigin,
                targetParams.shootOrigin
                ))
            {
                continue;
            }
            return true;
        }


        return false;
    }

    public static bool TargetWithinShootingRange(this CombatComponent combat, AITraceComponent traceComp, Transform origin, Vector3 targetPosition, float horizontalangle, float verticalAngle)
    {
        return traceComp.IsWithinView(
            origin,
            targetPosition,
            horizontalangle,
            verticalAngle
            );
        //return false;
    }

    private static void AddFallbackPoints(Collider target, Vector3[] hitPoints, ref int startIndex)
    {
        if(target == null) { return; }

        hitPoints[startIndex++] = target.bounds.center + Vector3.up * target.bounds.extents.y;
        hitPoints[startIndex++] = target.bounds.center - Vector3.right * target.bounds.extents.x;
        hitPoints[startIndex++] = target.bounds.center + Vector3.right * target.bounds.extents.x;
    }
}
