using UnityEngine;

public struct FOVPhaseParams
{
    // shared FOV Phase Parameters
    public AITraceComponent traceComponent;
    public Transform detectionOrigin;
    public LayerMask detectionTargetMask;

    // Detection Phase Parameters
    public Collider[] detectionResults;
    public float detectionRadius;
   // public LayerMask detectionMask;

    // Evaluation Phase Parameters
    public Vector3 evaluationTargetPosition;
    public float horizontalViewAngle;
    public float verticalViewAngle;
    public Vector3 evaluationStart;
    public Vector3 evaluationEnd;
    public Vector3 evaluationCenter;
    public Vector3 evaluationDirection;
    public float evaluationDistance;
    public float evaluationRadius;
    public Collider targetCollider;
    //public LayerMask evaluationMask;
    // public Vector3 evaluationTargetPosition;
    public Vector3[] evaluationHitPoints;

    // Targeting Phase Parameters
    public Transform shootOrigin;
    public Transform ownerOrigin;
    public float shootHorizontalAngle;
    public float shootVerticalAngle;
    public LayerMask targetingBlockingMask;
    
    // Optional Toggles
    public bool addFallbackPoints; // For Evaluation Phase
    public bool debugMode; // For Debugging

}
