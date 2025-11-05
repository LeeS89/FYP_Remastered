using UnityEngine;
using System;

public class NPCFieldOfViewHandler
{
    private IFieldOfViewOwner _owner;
    private FOVParameters _params;
    private AlertPhase _currentAlertPhase = AlertPhase.Idle;
    private float _fovSweepFrequency;
    private float _nextCheckTime = 0f;
    private Vector3[] _evaluationHitPoints;
    private Collider[] _proximityDetectionResults;
    private AITraceComponentNew _traceComponent;


    public NPCFieldOfViewHandler(IFieldOfViewOwner owner, FOVParameters fovParams, AITraceComponentNew traceComp)
    {
        if(fovParams == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("FieldOfViewManager: FOV Params is null, adding default.");
#endif
            fovParams = new FOVParameters();
        }

        if(traceComp == null) traceComp = new AITraceComponentNew();

        _owner = owner;
        _traceComponent = traceComp;
        _params = fovParams;
        _evaluationHitPoints = new Vector3[_params.maxFovTargets];
        _proximityDetectionResults = new Collider[_params.maxFovTargets];
        _nextCheckTime = Time.time + GetCheckFrequency(_currentAlertPhase);
        _fovSweepFrequency = GetCheckFrequency(_currentAlertPhase);
    }


    public void Tick()
    {
        if (Time.time >= _nextCheckTime)
        {
            _nextCheckTime = Time.time + _fovSweepFrequency;
            RunFOVSweep();
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

    public void OverrideFOVFrequency(AlertPhase phase)
    {
        if (_currentAlertPhase == phase) return;
        _currentAlertPhase = phase;
        _fovSweepFrequency = GetCheckFrequency(_currentAlertPhase);
    }

    private void TryChangeFOVFrequency(AlertPhase phase)
    {
        if (phase <= _currentAlertPhase) return;
        _currentAlertPhase = phase;
        _fovSweepFrequency = GetCheckFrequency(_currentAlertPhase);

    }

    public void RunFOVSweep()
    {
        FOVResult result = FOVResult.TargetOutsideSweepRadius;
        bool inShootAngle = false;
        LayerMask targetMask = _params?.TargetMask() ?? default;

        int detectedCount = RunDetectionPhase(_traceComponent, _params.fovOrigin, _proximityDetectionResults, _params.fovRadius, targetMask);

        if (detectedCount == 0)
        {
            _owner?.FieldOfViewSweepResult(result, inShootAngle);
            TryChangeFOVFrequency(AlertPhase.Idle);
            return;
        }

        TryChangeFOVFrequency(AlertPhase.Heightened);

        for(int i = 0; i < detectedCount; i++)
        {
            int hitCount;
            RunEvaluationPhase(_proximityDetectionResults[i], out hitCount, _params.addTargetFallbackPoints, targetMask);

            if (hitCount == 0) continue;
            result = RunTargetingPhase(hitCount, _params.blockingMask, targetMask);

            if(result == FOVResult.TargetSeen)
            {
                inShootAngle = _params.useSeparateVerticleAngle == false ? true :
                    TargetWithinAimThreshold(_traceComponent, _params.fovOrigin, _proximityDetectionResults[i].ClosestPointOnBounds(_params.fovOrigin.position), _params.halfHorizontalShootAngle);

                _owner?.FieldOfViewSweepResult(result, inShootAngle);
                return;
            }
            
        }
        _owner?.FieldOfViewSweepResult(FOVResult.TargetNotSeen, false);
    }

    public bool TargetWithinAimThreshold(AITraceComponentNew traceComp, Transform origin, Vector3 targetPosition, float halfAngle, bool useLocalUp = true)
    {
        return traceComp.IsWithinYaw(
            origin,
            targetPosition,
            halfAngle,
            useLocalUp
            );
    }

    private FOVResult RunTargetingPhase(int targetCount, LayerMask blockingMask, LayerMask targetMask)
    {
        for (int i = 0; i < targetCount; i++)
        {
            if (!_traceComponent.HasLineOfSight(
                _params.fovOrigin,
                _evaluationHitPoints[i],
                blockingMask,
                targetMask,
                _params.FOVTarget.Transform,
                _params.ownerOrigin
                // _fallbackFOVOrigin // Remove this later
                ))
            {
                continue;
            }
            return FOVResult.TargetSeen;
        }


        return FOVResult.TargetNotSeen;
    }

    private void RunEvaluationPhase(Collider targetCollider, out int hitCount, bool addFallbackPoints, LayerMask targetMask)
    {
        Vector3 closest = targetCollider.ClosestPointOnBounds(_params.fovOrigin.position);
        if (!_traceComponent.IsWithinAngle(_params.fovOrigin, closest, _params.fovHalfAngle, _params.useSeparateVerticleAngle, _params.verticalFovHalfAngle))
        {
            hitCount = 0;
            return;
        }
       
        Vector3 waistPos = _params.ownerOrigin.TransformPoint(0f, _params.waistHeightOffset, 0f);
        Vector3 eyePos = _params.ownerOrigin.TransformPoint(0f, _params.eyeHeightOffset, 0f);
        Vector3 center = (waistPos + eyePos) * 0.5f;
        Vector3 direction = TargetingUtility.GetDirectionToTarget(closest, center);
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
        targetMask,
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

    private void AddFallbackPoints(Collider target, Vector3[] hitPoints, ref int startIndex)
    {
        if (target == null) { return; }

        hitPoints[startIndex++] = target.bounds.center + Vector3.up * target.bounds.extents.y;
        hitPoints[startIndex++] = target.bounds.center - Vector3.right * target.bounds.extents.x;
        hitPoints[startIndex++] = target.bounds.center + Vector3.right * target.bounds.extents.x;
    }

    public int RunDetectionPhase(AITraceComponentNew traceComp = null, Transform origin = null, Collider[] results = null, float radius = 0.5f, LayerMask targetMask = default)
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
}




public enum FOVResult
{
    TargetOutsideSweepRadius,
    TargetInsideSweepRadius,
    TargetSeen,
    TargetNotSeen
}













public class AITraceComponentNew
{
    private RaycastHit[] _hitBuffer = new RaycastHit[10];


    public int CheckTargetWithinCombatRange(Vector3 traceLocation, Collider[] hitResults, float sphereRadius = 0.2f, LayerMask traceLayer = default)
    {

        //Vector3 start = location.position - location.forward * (capsuleHeight / 2f);  // Bottom of capsule
        //Vector3 end = location.position + location.forward * (capsuleHeight / 2f);    // Top of capsule


        return Physics.OverlapSphereNonAlloc(traceLocation, sphereRadius, hitResults, traceLayer);
        //hitResults = _overlapResults;
        //return hits;

        /*
                for (int i = 0; i < hitResults.Length; i++)
                {
                    hitResults[i] = null;
                }
                // Clear the results if no objects were found
                return 0;*/

    }

    public bool IsTargetWithinRange(Vector3 position, float radius, int layerMask, bool debug = false, float debugDuration = 0f)
    {
        if (debug)
        {

            DebugExtension.DebugWireSphere(position, Color.blue, radius, debugDuration);

        }

        /* if (foundObject)
         {

             return Physics.OverlapSphereNonAlloc(traceLocation.position, sphereRadius, hitResults, traceLayer);
             //hitResults = _overlapResults;
             //return hits;

         }*/
        return Physics.CheckSphere(position, radius, layerMask);
    }

    public int CheckTargetProximity(Transform traceLocation, Collider[] hitResults, float sphereRadius = 0.2f, LayerMask traceLayer = default, bool debug = false)
    {

        //Vector3 start = location.position - location.forward * (capsuleHeight / 2f);  // Bottom of capsule
        //Vector3 end = location.position + location.forward * (capsuleHeight / 2f);    // Top of capsule


        bool foundObject = IsTargetWithinRange(traceLocation.position, sphereRadius, traceLayer);//Physics.CheckSphere(traceLocation.position, sphereRadius, traceLayer);

        Color debugColor = foundObject ? Color.green : Color.red; // Green if detected, red if not

        if (debug)
        {

            DebugExtension.DebugWireSphere(traceLocation.position, debugColor, sphereRadius);

        }

        if (foundObject)
        {

            return Physics.OverlapSphereNonAlloc(traceLocation.position, sphereRadius, hitResults, traceLayer);
            //hitResults = _overlapResults;
            //return hits;

        }

        for (int i = 0; i < hitResults.Length; i++)
        {
            hitResults[i] = null;
        }
        // Clear the results if no objects were found
        return 0;

    }


    public bool IsWithinView(Transform from, Vector3 targetPosition, float horizontalThreshold, float verticalThreshold)
    {

        Vector3 to = targetPosition - from.position;


        Vector3 up = from.up.normalized;
        Vector3 fwdYaw = Vector3.ProjectOnPlane(from.forward, Vector3.up).normalized;
        if (fwdYaw.sqrMagnitude < 1e-6f)
            fwdYaw = Vector3.ProjectOnPlane(from.forward, up).normalized;

        Quaternion basis = Quaternion.LookRotation(fwdYaw, up);
        Vector3 local = Quaternion.Inverse(basis) * to; // no normalize

        float h = Mathf.Abs(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg); // yaw
        float v = Mathf.Abs(Mathf.Atan2(local.y, local.z) * Mathf.Rad2Deg); // pitch

        return h <= horizontalThreshold && v <= verticalThreshold;



        /*
                Vector3 localDir = from.InverseTransformDirection(targetPosition - from.position).normalized;

                float horizontalAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
                float verticalAngle = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

                return Mathf.Abs(horizontalAngle) <= horizontalThreshold && Mathf.Abs(verticalAngle) <= verticalThreshold;
        */
        /* Vector3 directionToTarget = (targetPosition - from.position).normalized;

         // Horizontal (XZ-plane)
         Vector3 flatForward = new Vector3(from.forward.x, 0f, from.forward.z).normalized;
         Vector3 flatDirection = new Vector3(directionToTarget.x, 0f, directionToTarget.z).normalized;
         float horizontalAngle = Vector3.Angle(flatForward, flatDirection);

         // Vertical (Y-axis offset relative to flat distance)
         float dx = targetPosition.x - from.position.x;
         float dz = targetPosition.z - from.position.z;
         float yOffset = targetPosition.y - from.position.y;

         float verticalAngle = Mathf.Atan2(yOffset, Mathf.Sqrt(dx * dx + dz * dz)) * Mathf.Rad2Deg;

         return horizontalAngle <= horizontalThreshold && Mathf.Abs(verticalAngle) <= verticalThreshold;*/
    }

    public bool IsWithinAngle(Transform from, Vector3 to, float halfangle, bool separateVertical = false, float halfVertical = 0f)
    {
        Vector3 toVec = to - from.position;
        if (toVec.sqrMagnitude < 1e-8f) return true;

        if (!separateVertical)
        {
            float cosHalf = Mathf.Cos(halfangle * Mathf.Deg2Rad);
            return Vector3.Dot(from.forward, toVec.normalized) >= cosHalf;
        }

        if (Vector3.Dot(from.forward, toVec) <= 0f) return false;

        Vector3 up = from.up;

        Vector3 fwdYaw = Vector3.ProjectOnPlane(from.forward, up);
        Vector3 dirYaw = Vector3.ProjectOnPlane(toVec, up);

        bool horizontalOk;
        if (fwdYaw.sqrMagnitude < 1e-8f || dirYaw.sqrMagnitude < 1e-8f) horizontalOk = true;
        else horizontalOk = Vector3.Angle(fwdYaw, dirYaw) <= halfangle;

        Vector3 dirN = toVec.normalized;
        Vector3 dirHoriz = Vector3.ProjectOnPlane(dirN, up);
        float vAngle = (dirHoriz.sqrMagnitude < 1e-8f) ? 90f : Vector3.Angle(dirN, dirHoriz);
        bool verticalOk = vAngle <= halfVertical;

        return horizontalOk && verticalOk;

        /*
                Vector3 pos = from.position;
                Vector3 dir = (to - pos).normalized;
                float cosHalf = Mathf.Cos(halfangle * Mathf.Deg2Rad);

                return Vector3.Dot(from.forward, dir) >= cosHalf;*/

    }

    public bool IsWithinYaw(Transform from, Vector3 target, float halfYawDeg, bool useLocalUp = true)
    {
        Vector3 up = useLocalUp ? from.up : Vector3.up;

        Vector3 to = target - from.position;
        if (to.sqrMagnitude < 1e-8f) return true;

        Vector3 fwdXZ = Vector3.ProjectOnPlane(from.forward, up).normalized;
        Vector3 toXZ = Vector3.ProjectOnPlane(to, up).normalized;

        if (fwdXZ.sqrMagnitude < 1e-8f || toXZ.sqrMagnitude < 1e-8f) return true;

        float cosHalf = Mathf.Cos(halfYawDeg * Mathf.Deg2Rad);
        return Vector3.Dot(fwdXZ, toXZ) >= cosHalf;
    }





    public int EvaluateViewCone(Vector3 start, Vector3 end, float radius, Vector3 direction, float maxDistance, LayerMask targetMask, Vector3[] hitPoints)
    {

        int hitCount = Physics.CapsuleCastNonAlloc(start, end, radius, direction, _hitBuffer, maxDistance, targetMask);

        int processedCount = Mathf.Min(hitCount, hitPoints.Length);

        for (int i = 0; i < processedCount; i++)
        {
            hitPoints[i] = _hitBuffer[i].point;
        }

        return processedCount;

        /*
                RaycastHit[] hits = Physics.CapsuleCastAll(start, end, radius, direction.normalized, maxDistance, targetMask);


                 int hitCount = 0;


                foreach (var hit in hits)
                {

                    if (hitCount < hitPoints.Length)
                    {
                        hitPoints[hitCount++] = hit.point;
                    }

                }

                return hitCount;*/
    }

    /*  public bool HasLineOfSight(
         Transform from,
         Vector3 target,
         LayerMask blockingMask,
         LayerMask targetMask,
         bool debug = false
         )
      {


          if (Physics.Linecast(from.position, target, out RaycastHit hit, blockingMask | targetMask))
          {
              if (((1 << hit.collider.gameObject.layer) & targetMask) != 0)
              {
                  Debug.DrawLine(from.position, target, Color.green);
                  Debug.LogError($"Hit target: {hit.collider.gameObject.name}");

                  return true;
              }
              else if(((1 << hit.collider.gameObject.layer) & blockingMask) != 0)
              {
                  if (debug)
                  {
                      Debug.DrawLine(from.position, target, Color.red, 25f);
                      Debug.LogError($"Hit target blocking mask: {hit.collider.gameObject.name}");
                      return false;
                  }
              }
          }


          return false;
      }*/

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fallbackFrom">In cases where the Linecast hits the calling object first
    /// we use a fallback point to fire the linecast from</param>
    /// <param name="debug"></param>
    /// <returns></returns>
    public bool HasLineOfSight(
       Transform from,
       Vector3 target,
       LayerMask blockingMask,
       LayerMask targetMask,
       Transform targetTransform,
       Transform ownerTransform,
       Transform fallbackFrom = null,
       bool debug = false
       )
    {
        if (from == null || targetTransform == null || ownerTransform == null) return false;
        
        RaycastHit hitInfo;
        bool targetWasHit;

        CheckHit(from, target, out hitInfo, out targetWasHit, blockingMask);

        if (!targetWasHit) { return false; }

        

        if (fallbackFrom != null)
        {
            if (hitInfo.transform.root == ownerTransform)
            {
                targetWasHit = false;
                CheckHit(fallbackFrom, target, out hitInfo, out targetWasHit, blockingMask);
                if (!targetWasHit) { return false; }

                if (((1 << hitInfo.collider.gameObject.layer) & targetMask) != 0)
                    return true;

            }
        }

        var t = hitInfo.transform;
        if (((1 << hitInfo.collider.gameObject.layer) & targetMask) != 0
            && (t == targetTransform) || t.IsChildOf(targetTransform))
            return true;


        return false;
    }

    private void CheckHit(
       Transform from,
       Vector3 target,
       out RaycastHit hit,
       out bool hitTarget,
       LayerMask blockingMask
       )
    {

        if (Physics.Linecast(from.position, target, out hit, blockingMask))
            hitTarget = true;
        else
            hitTarget = false;
    }

}

