using System;
using UnityEngine;

public class FieldOfViewManager
{
    /// <summary>
    /// New setup of Alerting zones and FOV management
    /// 
    /// </summary>


    // NEW
    private FieldOfViewParams _params;
    private EnemyEventManager _eventManager;
    private AlertPhase _currentAlertPhase = AlertPhase.Idle;
    private float _fovSweepFrequency;
    private float _nextCheckTime = 0f;
    private Vector3[] _evaluationHitPoints;
    private Collider[] _proximityDetectionResults;
    private AITraceComponent _traceComponent;
    private float _deescalationTimer;

    public FieldOfViewManager(EnemyEventManager eventManager, FieldOfViewParams fovParams, AITraceComponent traceComponent = null)
    {
        _eventManager = eventManager;

        if(fovParams == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("FieldOfViewManager: FOV Params is null, cannot proceed.");
#else
            return;
#endif
        }
        _params = fovParams;
        _evaluationHitPoints = new Vector3[_params.maxFovTargets];
        _proximityDetectionResults = new Collider[_params.maxFovTargets];
        _traceComponent = traceComponent != null ? traceComponent : new AITraceComponent();
        _nextCheckTime = Time.time + GetCheckFrequency(_currentAlertPhase);
        _fovSweepFrequency = GetCheckFrequency(_currentAlertPhase);
    }
    // END NEW

    #region Old Code
    // Start Old
 
 
    private Transform _fallbackFOVOrigin;
  
    
    // END OLD

    public FieldOfViewManager(
       AITraceComponent traceComponent,
       EnemyEventManager eventManager,
       Action<bool, bool> onFOVResultCallback,
      // in FieldOfViewParamsObsolete fovParams,
       bool addFallbackPoints = false
      )
    {
        _eventManager = eventManager;
      //  _traceComponent = traceComponent;
        //_onFOVResultCallback = onFOVResultCallback;
      
       // _fallbackFOVOrigin = fovParams.shootOrigin;
      
    }


    #endregion


    public void Tick()
    {
       // _fovSweepFrequency = GetCheckFrequency(_currentAlertPhase);
       
        if (Time.time >= _nextCheckTime)
        {
            _nextCheckTime = Time.time + _fovSweepFrequency;
            RunFieldOfViewSweep();
        }
   
    }

    private float GetCheckFrequency(AlertPhase phase)
    {
        return phase switch
        {
            AlertPhase.Idle => _params.idleFOVCheckFrequency,
            AlertPhase.Heightened => _params.heightenedFOVCheckFrequency,
            AlertPhase.Suspicious => _params.suspiciousFOVCheckFrequency,
            AlertPhase.Alerted => _params.alertedFOVCheckFrequency,
            _ => _params.idleFOVCheckFrequency,
        };
    }

  
    private void SetCurrentPhaseAndSweepFrequency(AlertPhase phase)
    {
      //  if (phase.CompareTo(_currentAlertPhase) == 0) return;

        /* if (phase.CompareTo(_currentAlertPhase) > 0) _currentAlertPhase = phase;
         else
         {

         }*/

        if (_currentAlertPhase == phase) return;
        _currentAlertPhase = phase;
        _fovSweepFrequency = GetCheckFrequency(phase);
    }

    public void RunFieldOfViewSweep()
    {

        bool seen = false;
        bool inShootAngle = false;

        int detectedCount = RunDetectionPhase(_traceComponent, _params.fovOrigin, _proximityDetectionResults, _params.fovRadius, _params.targetMask);
        
        if (detectedCount == 0)
        {
            _eventManager.FieldOfViewCallback(seen, inShootAngle);
            SetCurrentPhaseAndSweepFrequency(AlertPhase.Idle);
            return;
        }

        SetCurrentPhaseAndSweepFrequency(AlertPhase.Heightened);

        for (int i = 0; i < detectedCount; i++)
        {
            int hitCount;

            RunEvaluationPhase(_proximityDetectionResults[i], out hitCount, _params.addTargetFallbackPoints);

            if (hitCount == 0 && CombatComponentObsolete._testFOV) { /*Debug.LogError("CapsuleCast hit nothing");*/ continue; }

            seen = RunTargetingPhase(hitCount);

            if (seen)
            {
                SetCurrentPhaseAndSweepFrequency(AlertPhase.Alerted);
                // UpdateFOVResults(true);
                // bool facingTarget = this.TargetWithinShootingRange(_aiTraceComponent, _fovLocation, _detectionPhaseResults[i].ClosestPointOnBounds(_fovLocation.position), _shootAngleThreshold * 0.5f, _shootAngleThreshold * 1.25f);
                // SetFacingtarget(facingTarget);
                seen = true;
                inShootAngle = _params.useShootingAngleRestriction == false ? true :
                 TargetWithinAimThreshold(_traceComponent, _params.fovOrigin, _proximityDetectionResults[i].ClosestPointOnBounds(_params.fovOrigin.position), _params.halfHorizontalShootAngle);
                // inShootAngle = TargetWithinShootingRange(_traceComponent, _params.fovOrigin, _proximityDetectionResults[i].ClosestPointOnBounds(_params.fovOrigin.position), _horizontalShootAngle, _verticalShootAngle);
                _eventManager.FieldOfViewCallback(seen, inShootAngle);
                //_onFOVResultCallback?.Invoke(seen, inShootAngle);
                return;

            }


        }
        _eventManager.FieldOfViewCallback(seen, inShootAngle);
       
    }



    public int RunDetectionPhase(AITraceComponent traceComp = null, Transform origin = null, Collider[] results = null, float radius = 0.5f, LayerMask targetMask = default)
    {
        if (traceComp == null || origin == null || results == null) { return 0; }

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
        if (!_traceComponent.IsWithinAngle(_params.fovOrigin, targetCollider.bounds.center, _params.fovHalfAngle, _params.useSeparateVerticleAngle, _params.verticalFovHalfAngle))
        {
            hitCount = 0;
            return;
        }
        /* if (!_traceComponent.IsWithinView(_fovOrigin, targetCollider.bounds.center, _horizontalViewAngle, _verticalViewAngle))
         {
             hitCount = 0;
             return;
         }*/
        Vector3 waistPos = _params.ownerOrigin.TransformPoint(0f, _params.waistHeightOffset, 0f);
        Vector3 eyePos = _params.ownerOrigin.TransformPoint(0f, _params.eyeHeightOffset, 0f);
        Vector3 center = (waistPos + eyePos) * 0.5f;
        Vector3 direction = TargetingUtility.GetDirectionToTarget(targetCollider.bounds.center, center);
/*
        Vector3 waistPos = _ownerOrigin.position + Vector3.up * _evaluationCapsuleStartHeight;
        Vector3 eyePos = _ownerOrigin.position + Vector3.up * _evaluationCapsuleEndHeight;
        Vector3 sweepCenter = (waistPos + eyePos) * 0.5f;
        Vector3 directionTotarget = TargetingUtility.GetDirectionToTarget(targetCollider.bounds.center, sweepCenter);
*/
        hitCount = _traceComponent.EvaluateViewCone(
        waistPos,
        eyePos,
        _params.evaluationCapsuleRadius,
        direction,
        _params.fovRadius,
        _params.targetMask,
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
            if (!_traceComponent.HasLineOfSight(
                _params.fovOrigin,
                _evaluationHitPoints[i],
                _params.blockingMask,
                _params.targetMask,
                _params.ownerOrigin
               // _fallbackFOVOrigin // Remove this later
                ))
            {
                continue;
            }
            return true;
        }


        return false;
    }

    public bool TargetWithinAimThreshold(AITraceComponent traceComp, Transform origin, Vector3 targetPosition, float halfAngle, bool useLocalUp = true)
    {
        return traceComp.IsWithinYaw(
            origin,
            targetPosition,
            halfAngle,
            useLocalUp
            );
    }

    [Obsolete("Use TargetWithinAimThreshold instead")]
    public bool TargetWithinShootingRange(AITraceComponent traceComp, Transform origin, Vector3 targetPosition, float horizontalangle, float verticalAngle)
    {
        return traceComp.IsWithinView(
            origin,
            targetPosition,
            horizontalangle,
            verticalAngle
            );
       
    }

    private void AddFallbackPoints(Collider target, Vector3[] hitPoints, ref int startIndex)
    {
        if (target == null) { return; }

        hitPoints[startIndex++] = target.bounds.center + Vector3.up * target.bounds.extents.y;
        hitPoints[startIndex++] = target.bounds.center - Vector3.right * target.bounds.extents.x;
        hitPoints[startIndex++] = target.bounds.center + Vector3.right * target.bounds.extents.x;
    }
}
