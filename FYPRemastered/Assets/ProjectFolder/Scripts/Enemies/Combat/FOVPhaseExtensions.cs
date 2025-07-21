using System.Runtime.CompilerServices;
using UnityEngine;

public static class FOVPhaseExtensions
{
    public static bool RunDetectionPhase(this CombatComponent combat, AITraceComponent traceComp, Transform detectionLocation, Collider[] detectionResults, float detectionRadius, LayerMask detectionMask)
    {
        int count = traceComp.CheckTargetProximity(detectionLocation, detectionResults, detectionRadius, detectionMask);

        return count > 0;
    }

    public static bool RunEvaluationPhase(
        this CombatComponent combat, 
        AITraceComponent traceComp, 
        Transform evaluationLocation, 
        Vector3 evaluationTargetPosition, 
        float horizontalViewAngle, 
        float verticalViewangle,
        Vector3 evaluationstart,
        Vector3 evaluationEnd,
        Vector3 evaluationCenter,
        Vector3 evaluationdirection,
        float evaluationRadius,
        LayerMask evaluationMask,
        Vector3[] evaluationHitPoints,
        bool addFallbackPoints = false
        )
    {
        return false;
    }
}
