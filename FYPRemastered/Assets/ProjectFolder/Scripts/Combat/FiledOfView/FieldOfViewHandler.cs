using System;
using UnityEngine;

public class FieldOfViewHandler
{
    private Collider[] _proximityDetectionResults;
    private Vector3[] _evaluationHitPoints;
    private Transform _fovOrigin;
    private Transform _ownerOrigin;
    private Transform _fallbackFOVOrigin;
    private float _horizontalViewAngle;
    private float _verticalViewAngle;
    private float _proximityRadius;
    private float _evaluationCapsuleStartHeight;
    private float _evaluationCapsuleEndHeight;
    private float _evaluationCapsuleRadius;
    private LayerMask _obstructionMask;
    private LayerMask _targetMask;
    private AITraceComponent _aiTraceComponent;

    public FieldOfViewHandler(
        AITraceComponent traceComponent, 
        in FieldOfViewParams fovParams 
       )
    {
        _aiTraceComponent = traceComponent;
        _proximityDetectionResults = new Collider[fovParams.maxTraceTargets];
        _evaluationHitPoints = new Vector3[fovParams.maxTraceTargets];
        _fovOrigin = fovParams.fovOrigin;
        _ownerOrigin = fovParams.ownerOrigin;
        _fallbackFOVOrigin = fovParams.shootOrigin;
        _horizontalViewAngle = fovParams.horizontalViewAngle;
        _verticalViewAngle = fovParams.verticalViewAngle;
        _proximityRadius = fovParams.proximityRadius;
        _evaluationCapsuleStartHeight = fovParams.evaluationCapsuleStartHeight;
        _evaluationCapsuleEndHeight = fovParams.evaluationCapsuleEndHeight;
        _evaluationCapsuleRadius = fovParams.evaluationCapsuleRadius;
        _obstructionMask = fovParams.obstructionMask;
        _targetMask = fovParams.targetMask;
    }


    public int RunDetectionPhase(AITraceComponent traceComp = null, Transform origin = null, Collider[] results = null, float radius = 0.5f, LayerMask targetMask = default)
    {
        if(traceComp == null || origin == null || results == null) { return 0; }

        int count = traceComp.CheckTargetProximity(
            origin,
            results,
            radius,
            targetMask
            );

        return count;
    }

    private void RunEvaluationPhase(Collider targetCollider, out int hitCount, bool addFallbackPoints)
    {
        if (!_aiTraceComponent.IsWithinView(_fovOrigin, targetCollider.bounds.center, _horizontalViewAngle, _verticalViewAngle))
        {
            hitCount = 0;
            return;
        }

        Vector3 waistPos = _ownerOrigin.position + Vector3.up * _evaluationCapsuleStartHeight;
        Vector3 eyePos = _ownerOrigin.position + Vector3.up * _evaluationCapsuleEndHeight;
        Vector3 sweepCenter = (waistPos + eyePos) * 0.5f;
        Vector3 directionTotarget = TargetingUtility.GetDirectionToTarget(targetCollider.bounds.center, sweepCenter);

        hitCount = _aiTraceComponent.EvaluateViewCone(
        waistPos,
        eyePos,
        _evaluationCapsuleRadius,
        directionTotarget,
        _proximityRadius,
        _targetMask,
        _evaluationHitPoints
        );

        if (hitCount == 0)
        {
            return;
        }

        if (addFallbackPoints)
        {
            AddFallbackPoints(targetCollider, _evaluationHitPoints, ref hitCount);
        }


    }

    private bool RunTargetingPhase(int targetCount)
    {
        for (int i = 0; i < targetCount; i++)
        {
            if (!_aiTraceComponent.HasLineOfSight(
                _fovOrigin,
                _evaluationHitPoints[i],
                _obstructionMask,
                _targetMask,
                _ownerOrigin,
                _fallbackFOVOrigin
                ))
            {
                continue;
            }
            return true;
        }


        return false;
    }
    float _shootAngleThreshold = 30f; // Example value, adjust as needed
    public void RunFieldOfViewSweep(Action<bool, bool> onFOVResult, bool addFallbackPoints = false)
    {

        bool seen = false;
        bool inShootAngle = false;

        int detectedCount = RunDetectionPhase(_aiTraceComponent, _fovOrigin, _proximityDetectionResults, _proximityRadius, _targetMask);
        
       
       
        if (detectedCount == 0)
        {
            onFOVResult?.Invoke(seen, inShootAngle);
            //UpdateFOVResults(targetSeen);
            //return false;
        }

        for (int i = 0; i < detectedCount; i++)
        {
            int hitCount;
            
            RunEvaluationPhase(_proximityDetectionResults[i], out hitCount, addFallbackPoints);

            if (hitCount == 0) { continue; }

           // SetTargetingPhaseParams(ref _fovPhaseParams);

            if (RunTargetingPhase(hitCount))
            {
                // UpdateFOVResults(true);
                // bool facingTarget = this.TargetWithinShootingRange(_aiTraceComponent, _fovLocation, _detectionPhaseResults[i].ClosestPointOnBounds(_fovLocation.position), _shootAngleThreshold * 0.5f, _shootAngleThreshold * 1.25f);
                // SetFacingtarget(facingTarget);
                seen = true;
                inShootAngle = TargetWithinShootingRange(_aiTraceComponent, _fovOrigin, _proximityDetectionResults[i].ClosestPointOnBounds(_fovOrigin.position), _shootAngleThreshold * 0.5f, _shootAngleThreshold * 1.25f);
                onFOVResult?.Invoke(seen, inShootAngle);
                return;
                //return true;
            }

        }
        onFOVResult?.Invoke(seen, inShootAngle);
        //return false;

    }


   


    


   

    public bool TargetWithinShootingRange(AITraceComponent traceComp, Transform origin, Vector3 targetPosition, float horizontalangle, float verticalAngle)
    {
        return traceComp.IsWithinView(
            origin,
            targetPosition,
            horizontalangle,
            verticalAngle
            );
        //return false;
    }

    private void AddFallbackPoints(Collider target, Vector3[] hitPoints, ref int startIndex)
    {
        if (target == null) { return; }

        hitPoints[startIndex++] = target.bounds.center + Vector3.up * target.bounds.extents.y;
        hitPoints[startIndex++] = target.bounds.center - Vector3.right * target.bounds.extents.x;
        hitPoints[startIndex++] = target.bounds.center + Vector3.right * target.bounds.extents.x;
    }
}
