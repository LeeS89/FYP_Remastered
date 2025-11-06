using System.Collections.Generic;
using UnityEngine;

public class NPCFieldOfViewHandler
{
    private IFieldOfViewOwner _owner;
    private FOVParameters _params;
    private AlertPhase _currentAlertPhase = AlertPhase.Idle;
    private float _fovSweepFrequency;
    private float _nextCheckTime = 0f;
    private Vector3[] _evaluationHitPoints;
    private Collider[] _proximityDetectionResults;
    private RaycastHit[] _hitBuffer = new RaycastHit[10];
   


    public NPCFieldOfViewHandler(IFieldOfViewOwner owner, FOVParameters fovParams)
    {
        if(fovParams == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("FieldOfViewManager: FOV Params is null, adding default.");
#endif
            fovParams = new FOVParameters();
        }

        _owner = owner;
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

        int detectedCount = RunDetectionPhase(_params.fovOrigin, _proximityDetectionResults, _params.fovRadius, targetMask);

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
                inShootAngle = _params.useShootingAngleRestriction == false ? true :
                    TargetWithinAimThreshold(_params.fovOrigin, _proximityDetectionResults[i].ClosestPointOnBounds(_params.fovOrigin.position), _params.halfHorizontalShootAngle);

                _owner?.FieldOfViewSweepResult(result, inShootAngle);
                return;
            }
            
        }
        _owner?.FieldOfViewSweepResult(FOVResult.TargetNotSeen, false);
    }

    public bool TargetWithinAimThreshold(Transform origin, Vector3 targetPosition, float halfAngle, bool useLocalUp = true)
    {
        return this.IsWithinYaw(
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
            if (!this.HasLineOfSight(
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
    private List<Vector3> _samplePoints = new(5);
    private void RunEvaluationPhase(Collider targetCollider, out int hitCount, bool addFallbackPoints, LayerMask targetMask)
    {
        Vector3 closest = targetCollider.ClosestPointOnBounds(_params.fovOrigin.position);
        _samplePoints.Add(closest);
        _samplePoints.Add(targetCollider.bounds.center + Vector3.up * targetCollider.bounds.extents.y);
        _samplePoints.Add(targetCollider.bounds.center - Vector3.right * targetCollider.bounds.extents.x);
        _samplePoints.Add(targetCollider.bounds.center + Vector3.right * targetCollider.bounds.extents.x);
        int angleCount = 0;
        foreach(var p in _samplePoints)
        {
            if (this.IsWithinAngle(_params.fovOrigin, p, _params.fovHalfAngle/*, _params.useSeparateVerticleAngle, _params.verticalFovHalfAngle*/))
            {
                angleCount++;
                break;
            }
        }
        _samplePoints.Clear();

        if (angleCount == 0)
        {
            hitCount = 0;
            return;
        }
        
       

        Vector3 waistPos = _params.ownerOrigin.TransformPoint(0f, _params.waistHeightOffset, 0f);
        Vector3 eyePos = _params.ownerOrigin.TransformPoint(0f, _params.eyeHeightOffset, 0f);
        Vector3 center = (waistPos + eyePos) * 0.5f;
        Vector3 direction = TargetingUtility.GetDirectionToTarget(closest, center);
       
        hitCount = this.EvaluateViewCone(
        waistPos,
        eyePos,
        _params.evaluationCapsuleRadius,
        direction,
        _params.fovRadius,
        targetMask,
        _evaluationHitPoints,
        _hitBuffer
        );

        
        if (hitCount > 0 && addFallbackPoints)
            AddFallbackPoints(targetCollider, _evaluationHitPoints, ref hitCount);
    }

    private void AddFallbackPoints(Collider target, Vector3[] hitPoints, ref int startIndex)
    {
        if (target == null) { return; }

        hitPoints[startIndex++] = target.bounds.center + Vector3.up * target.bounds.extents.y;
        hitPoints[startIndex++] = target.bounds.center - Vector3.right * target.bounds.extents.x;
        hitPoints[startIndex++] = target.bounds.center + Vector3.right * target.bounds.extents.x;
    }

    public int RunDetectionPhase(Transform origin = null, Collider[] results = null, float radius = 0.5f, LayerMask targetMask = default)
    {
        if (origin == null || results == null) { return 0; }

        int count = this.CheckTargetProximity(
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













internal static class FOVHandlerExtension
{
    //private RaycastHit[] _hitBuffer = new RaycastHit[10];
    //public static FOVHandlerExtension Instance = new();
    //private FOVHandlerExtension() { }

    public static int CheckTargetWithinCombatRange(this NPCFieldOfViewHandler handler, Vector3 traceLocation, Collider[] hitResults, float sphereRadius = 0.2f, LayerMask traceLayer = default)
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

    public static bool IsTargetWithinRange(this NPCFieldOfViewHandler handler, Vector3 position, float radius, int layerMask, bool debug = false, float debugDuration = 0f)
    {
        if (debug)
            DebugExtension.DebugWireSphere(position, Color.blue, radius, debugDuration);

        return Physics.CheckSphere(position, radius, layerMask);
    }

    public static int CheckTargetProximity(this NPCFieldOfViewHandler handler, Transform traceLocation, Collider[] hitResults, float sphereRadius = 0.2f, LayerMask traceLayer = default, bool debug = false)
    {

        //bool foundObject = IsTargetWithinRange(traceLocation.position, sphereRadius, traceLayer);
        bool foundObject = Physics.CheckSphere(traceLocation.position, sphereRadius, traceLayer);

        Color debugColor = foundObject ? Color.green : Color.red; // Green if detected, red if not

        if (debug)
            DebugExtension.DebugWireSphere(traceLocation.position, debugColor, sphereRadius);

        if (foundObject)
            return Physics.OverlapSphereNonAlloc(traceLocation.position, sphereRadius, hitResults, traceLayer);


        for (int i = 0; i < hitResults.Length; i++)
            hitResults[i] = null;

        return 0;

    }


    public static bool IsWithinView(this NPCFieldOfViewHandler handler, Transform from, Vector3 targetPosition, float horizontalThreshold, float verticalThreshold)
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

    }

    public static bool IsWithinAngle(this NPCFieldOfViewHandler handler, Transform from, Vector3 to, float halfAngle)
    {
        Vector3 toVec = to - from.position;
        if (toVec.sqrMagnitude < 1e-8f) return true;

        float cosHalf = Mathf.Cos(halfAngle * Mathf.Deg2Rad);
        return Vector3.Dot(from.forward, toVec.normalized) >= cosHalf;
    }

    public static bool IsWithinAngle(this NPCFieldOfViewHandler handler, Transform from, Vector3 to, float halfangle, bool separateVertical = false, float halfVertical = 0f)
    {
        var toTarget = (to - from.position).normalized;
        if (Vector3.Angle(from.forward, toTarget) <= (halfangle))
        {
            return true;
        }
        return false;
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

    public static bool IsWithinYaw(this NPCFieldOfViewHandler handler, Transform from, Vector3 target, float halfYawDeg, bool useLocalUp = true)
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





    public static int EvaluateViewCone(this NPCFieldOfViewHandler handler, Vector3 start, Vector3 end, float radius, Vector3 direction, float maxDistance, LayerMask targetMask, Vector3[] hitPoints, RaycastHit[] _hitBuffer)
    {

        int hitCount = Physics.CapsuleCastNonAlloc(start, end, radius, direction, _hitBuffer, maxDistance, targetMask);

        int processedCount = Mathf.Min(hitCount, hitPoints.Length);

        for (int i = 0; i < processedCount; i++)
        {
            hitPoints[i] = _hitBuffer[i].point;
        }

        return processedCount;

    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="fallbackFrom">In cases where the Linecast hits the calling object first
    /// we use a fallback point to fire the linecast from</param>
    /// <param name="debug"></param>
    /// <returns></returns>
    public static bool HasLineOfSight(
       this NPCFieldOfViewHandler handler,
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

    private static void CheckHit(
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

