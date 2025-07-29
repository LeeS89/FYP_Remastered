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
    private float _horizontalShootAngle;
    private float _verticalShootAngle;
    private float _proximityRadius;
    private float _evaluationCapsuleStartHeight;
    private float _evaluationCapsuleEndHeight;
    private float _evaluationCapsuleRadius;
    private LayerMask _obstructionMask;
    private LayerMask _targetMask;
    private AITraceComponent _aiTraceComponent;
    private float _fovCheckFrequency;
    private FieldOfViewFrequencyStatus _fieldOfViewStatus = FieldOfViewFrequencyStatus.Normal;
    [SerializeField] private float _normalFOVCheckFrequency = 1f;
    [SerializeField] private float _heightenedFOVCheckFrequency = 0.1f;
    private Action<bool, bool> _onFOVResultCallback;
    private bool _addFallbackPoints = false;
    private float _nextCheckTime = 0f;


    public FieldOfViewHandler(
        AITraceComponent traceComponent,
        Action<bool, bool> onFOVResultCallback,
        in FieldOfViewParams fovParams,
        bool addFallbackPoints = false
       )
    {
        _aiTraceComponent = traceComponent;
        _onFOVResultCallback = onFOVResultCallback;
        _addFallbackPoints = addFallbackPoints;

        _proximityDetectionResults = new Collider[fovParams.maxTraceTargets];
        _evaluationHitPoints = new Vector3[fovParams.maxTraceTargets];
        _fovOrigin = fovParams.fovOrigin;
        _ownerOrigin = fovParams.ownerOrigin;
        _fallbackFOVOrigin = fovParams.shootOrigin;
        _horizontalViewAngle = fovParams.horizontalViewAngle;
        _verticalViewAngle = fovParams.verticalViewAngle;
        _horizontalShootAngle = fovParams.horizontalShootAngle;
        _verticalShootAngle = fovParams.verticalShootAngle;
        _proximityRadius = fovParams.proximityRadius;
        _evaluationCapsuleStartHeight = fovParams.evaluationCapsuleStartHeight;
        _evaluationCapsuleEndHeight = fovParams.evaluationCapsuleEndHeight;
        _evaluationCapsuleRadius = fovParams.evaluationCapsuleRadius;
        _obstructionMask = fovParams.obstructionMask;
        _targetMask = fovParams.targetMask;
    }


    public void UpdateFieldOfView()
    {
       
        switch (_fieldOfViewStatus)
        {
            case FieldOfViewFrequencyStatus.Normal:
                if (_fovCheckFrequency != _normalFOVCheckFrequency)
                {
                    _fovCheckFrequency = _normalFOVCheckFrequency;
                }
                break;
            case FieldOfViewFrequencyStatus.Heightened:
                if (_fovCheckFrequency != _heightenedFOVCheckFrequency)
                {
                    _fovCheckFrequency = _heightenedFOVCheckFrequency;
                }
                break;
            default:
                _fovCheckFrequency = _normalFOVCheckFrequency;
                break;
        }
        if (Time.time >= _nextCheckTime)
        {
            _nextCheckTime = Time.time + _fovCheckFrequency;
            RunFieldOfViewSweep();
        }
       // RunFieldOfViewCheck();
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


   

   // float _shootAngleThreshold = 30f; // Example value, adjust as needed
    public void RunFieldOfViewSweep(/*Action<bool, bool> onFOVResult, bool addFallbackPoints = false*/)
    {

        bool seen = false;
        bool inShootAngle = false;

        int detectedCount = RunDetectionPhase(_aiTraceComponent, _fovOrigin, _proximityDetectionResults, _proximityRadius, _targetMask);
        
       
       
        if (detectedCount == 0)
        {
            _onFOVResultCallback?.Invoke(seen, inShootAngle);
            if (_fieldOfViewStatus != FieldOfViewFrequencyStatus.Normal)
                _fieldOfViewStatus = FieldOfViewFrequencyStatus.Normal;
        }

        if (_fieldOfViewStatus != FieldOfViewFrequencyStatus.Heightened)
            _fieldOfViewStatus = FieldOfViewFrequencyStatus.Heightened;

        for (int i = 0; i < detectedCount; i++)
        {
            int hitCount;
            
            RunEvaluationPhase(_proximityDetectionResults[i], out hitCount, _addFallbackPoints);

            if (hitCount == 0) { continue; }

           // SetTargetingPhaseParams(ref _fovPhaseParams);

            if (RunTargetingPhase(hitCount))
            {
                // UpdateFOVResults(true);
                // bool facingTarget = this.TargetWithinShootingRange(_aiTraceComponent, _fovLocation, _detectionPhaseResults[i].ClosestPointOnBounds(_fovLocation.position), _shootAngleThreshold * 0.5f, _shootAngleThreshold * 1.25f);
                // SetFacingtarget(facingTarget);
                seen = true;
                inShootAngle = TargetWithinShootingRange(_aiTraceComponent, _fovOrigin, _proximityDetectionResults[i].ClosestPointOnBounds(_fovOrigin.position), _horizontalShootAngle, _verticalShootAngle);
                _onFOVResultCallback?.Invoke(seen, inShootAngle);
                return;
                //return true;
            }

        }
        _onFOVResultCallback?.Invoke(seen, inShootAngle);
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
