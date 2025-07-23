using UnityEngine;

public readonly struct FieldOfViewParams
{
    public readonly Transform fovOrigin;
    public readonly Transform ownerOrigin;
    public readonly Transform shootOrigin;
    public readonly float proximityRadius;
    public readonly float evaluationCapsuleStartHeight;
    public readonly float evaluationCapsuleEndHeight;
    public readonly float evaluationCapsuleRadius;
    public readonly float horizontalViewAngle;
    public readonly float verticalViewAngle;

    public readonly LayerMask obstructionMask;
    public readonly LayerMask targetMask;
    public readonly int maxTraceTargets;

    public FieldOfViewParams(
        Transform fovOrigin,
        Transform ownerOrigin,
        Transform shootOrigin,
        float proximityRadius,
        float evaluationCapsuleStartHeight,
        float evaluationCapsuleEndHeight,
        float evaluationCapsuleRadius,
        float horizontalViewAngle,
        float verticalViewAngle,
        LayerMask obstructionMask,
        LayerMask targetMask,
        int maxTraceTargets
    )
    {
        this.fovOrigin = fovOrigin;
        this.ownerOrigin = ownerOrigin;
        this.shootOrigin = shootOrigin;
        this.proximityRadius = proximityRadius;
        this.evaluationCapsuleStartHeight = evaluationCapsuleStartHeight;
        this.evaluationCapsuleEndHeight = evaluationCapsuleEndHeight;
        this.evaluationCapsuleRadius = evaluationCapsuleRadius;
        this.horizontalViewAngle = horizontalViewAngle;
        this.verticalViewAngle = verticalViewAngle;
        this.obstructionMask = obstructionMask;
        this.targetMask = targetMask;
        this.maxTraceTargets = maxTraceTargets;
    }

}
