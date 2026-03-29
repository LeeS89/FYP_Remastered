using Npc.API;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FovRunner : ITickable
{
    private IFovDeps _deps;
    private float _nextSweepTime = 0f;
    private Collider[] _proximityDetectionResults;
    private RaycastHit[] _hitBuffer = new RaycastHit[5];
    private List<Vector3> _samplePoints = new(5);
    public bool RunMeleeProximityCheck { get; set; } = false;

    IFovNotifications OnNotify;
  //  public Notification OnFOVSweepComplete { get; private set; }


    public FovRunner(IFovDeps deps, IFovNotifications onNotify/*Notification onSweepComplete*/)
    {
        if (deps == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("FieldOfViewManager: FOV Params is null, adding default.");
#endif
            deps = new FovData();
        }

        _deps = deps;
        _proximityDetectionResults = new Collider[_deps.MaxTargets()];
        float sweepTime = _deps.GetSweepFrequency();
        _nextSweepTime = Time.time + sweepTime;
        OnNotify = onNotify;
    }


    public void Dispose()
    {
        _deps = null;
        _proximityDetectionResults = null;
        _hitBuffer = null;
        _samplePoints = null;
       // OnFOVSweepComplete = null;
    }

    private void SendResult(FOVResult result)
    {
        //if (!_running) return;
       
        OnNotify?.FovUpdate(result);
       /* var n = NpcNotification.FovNotifications.FOVUpdate(result);
        OnFOVSweepComplete?.Invoke(n);*/
    }

    Action<LayerMask, LayerMask, LayerMask> onMasks;

    private void RunFOVSweep()
    {
        if (TargetIsNull()) return;

        FOVResult currentResult = FOVResult.TargetNotSeen;
        bool inShootAngle = false;
        LayerMask targetMask = _deps.Target.LayerMask;//_params?.TargetMask() ?? default;

        int detectedCount = CheckTargetProximity(_deps.SweepOrigin(), _proximityDetectionResults, _deps.FovRadius(), targetMask, true);

        if (detectedCount == 0)
        {
            SendResult(FOVResult.TargetNotSeen);
            _deps.SetTargetProximityStatus(targetInsideRadius: false);
            return;
        }

        // Alters frequency of sweeps depending on target proximity
        _deps.SetTargetProximityStatus(targetInsideRadius: true);
        
        for (int i = 0; i < detectedCount; i++)
        {
            FOVResult newResult = RunEvaluationPhase(_proximityDetectionResults[i], targetMask);

            // Only proceed if the new result has higher priority than the current result. This allows us to skip expensive checks if we've already determined a high-priority result (like TargetSeenAndWithinShootingAngles)
            if (!HasHigherPriorityResult(newResult, currentResult)) continue;
            currentResult = newResult;

            // Only process actionable results once a clear LOS is established, since results like TargetSeenAndWithinShootingAngles are not meaningful if the target is not actually visible
            if (currentResult < FOVResult.ClearFov) continue;
            
            // DO Melee Sweep here => If hit, return early, else perform shooting angle check


            inShootAngle = !_deps.UseShootingAngleRestriction() ? true :
                TargetWithinAimThreshold(_deps.SweepOrigin(), _proximityDetectionResults[i].ClosestPointOnBounds(_deps.SweepOrigin().position), _deps.HalfShootAngle());

            currentResult = inShootAngle == true ? FOVResult.TargetSeenAndWithinShootingAngles : FOVResult.ClearFov;
            SendResult(currentResult);
            return;

        }
        SendResult(currentResult);
    }

    private int CheckTargetProximity(Transform traceLocation, Collider[] hitResults, float sphereRadius = 0.2f, LayerMask traceLayer = default, bool debug = false)
    {
        int numHits = Physics.OverlapSphereNonAlloc(traceLocation.position, sphereRadius, hitResults, traceLayer);

        if (debug)
        {
            Color debugColor = numHits > 0 ? Color.green : Color.red;
            DebugExtension.DebugWireSphere(traceLocation.position, debugColor, sphereRadius);
        }

        return numHits;
    }


    private bool HasHigherPriorityResult(FOVResult newResult, FOVResult currentResult) => newResult > currentResult;


    private FOVResult RunEvaluationPhase(Collider targetCollider, LayerMask targetMask)
    {
        _samplePoints.Clear();
        FOVResult result = FOVResult.TargetNotSeen;

        targetCollider.GetSamplePoints(_samplePoints);
     
        foreach (var p in _samplePoints)
        {
            if (this.IsWithinAngle(_deps.SweepOrigin(), p, _deps.FovHalfAngle()))
            {
                bool TargetIsWorldBlocked = !CheckHit(_deps.SweepOrigin(), p, _deps.WorldMask(), _deps.Target.LayerMask);
                if (TargetIsWorldBlocked) continue;

                FOVResult r;
                if (!HasValidViewOfTarget(_deps.SweepOrigin(), p, _deps.BlockingMask(), _deps.OwnerOrigin(), out r, _hitBuffer)) continue;

                if (r == FOVResult.ClearFov) return r;
                else if (r > result) result = r;

            }
        }
   
        return result;

    }

    private bool HasValidViewOfTarget(Transform from, Vector3 target, LayerMask blockingMask, Transform ownerOrigin, out FOVResult result, RaycastHit[] buffer)
    {
        if (from == null || buffer == null) { result = FOVResult.TargetNotSeen; return false; }
       
        int hits = GetTargetsInView(from, target, blockingMask, buffer);

        for (int i = 0; i < hits; i++)
        {
            var hit = buffer[i];
            if (hit.transform.IsChildOf(ownerOrigin)) continue;
            else
            {
                result = FOVResult.PartialFov;
                return true;
            }    
        }
        result = FOVResult.ClearFov;
        return true;
    }

    public int GetTargetsInView(
      Transform from,
      Vector3 target,
      LayerMask blockingMask,
      RaycastHit[] hitBuffer,
      bool debug = false
      )
    {
        Vector3 direction = (target - from.position);
        float dist = direction.magnitude;
        direction /= dist;

        return Physics.RaycastNonAlloc(from.position, direction, hitBuffer, dist, blockingMask);
      
    }
   

    private bool CheckHit(
      Transform from,
      Vector3? target,
      LayerMask blockingMask,
      LayerMask targetMask,
      bool debug = false
      )
    {
        if (from == null || target == null || target == Vector3.zero) { return false; }

        LayerMask losMask = blockingMask | targetMask;

        RaycastHit hitInfo;
        Vector3 direction = (target.Value - from.position);
        float dist = direction.magnitude;
        direction /= dist;

        if (Physics.Linecast(from.position, target.Value, out hitInfo, losMask))
        {

            var t = hitInfo.transform;
            if (((1 << hitInfo.collider.gameObject.layer) & targetMask) != 0)
            {
                /*if (debug)*/
                Debug.DrawLine(from.position, hitInfo.point, Color.green, 0.1f);
                return true;
            }
        }

        /*if(debug)*/
        Debug.DrawLine(from.position, hitInfo.point, Color.red, 0.1f);
        return false;
    }




    private bool TargetWithinAimThreshold(Transform origin, Vector3 targetPosition, float halfAngle, bool useLocalUp = true)
    {
        return this.IsWithinYaw(
            origin,
            targetPosition,
            halfAngle,
            useLocalUp
            );
    }


    private bool TargetIsNull() => _deps == null || _deps.Target == null;

    public void Tick(float dt)
    {
        if (TargetIsNull())
        {
#if UNITY_EDITOR
             Debug.LogError("Must Provide a Target for the FOV Sweep");
#endif
            return;
        }
        if (Time.time >= _nextSweepTime)
        {
            _deps.DebugFrequency(); // Obsolete, just for testing
            _nextSweepTime = Time.time + _deps.GetSweepFrequency();
            RunFOVSweep();
        }
    }

   

    public void LateTick(float dt) { }

  
}


















internal static class FovRunnerExtension
{
   

    public static bool IsTargetWithinRange(this FovRunner handler, Vector3 position, float radius, int layerMask, bool debug = false, float debugDuration = 0f)
    {
        if (debug)
            DebugExtension.DebugWireSphere(position, Color.blue, radius, debugDuration);

        return Physics.CheckSphere(position, radius, layerMask);
    }

  

    public static bool IsWithinView(this FovRunner handler, Transform from, Vector3 targetPosition, float horizontalThreshold, float verticalThreshold)
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

    public static bool IsWithinAngle(this FovRunner handler, Transform from, Vector3 to, float halfAngle)
    {
        Vector3 toVec = to - from.position;
        if (toVec.sqrMagnitude < 1e-8f) return true;

        float cosHalf = Mathf.Cos(halfAngle * Mathf.Deg2Rad);
        return Vector3.Dot(from.forward, toVec.normalized) >= cosHalf;
    }

    public static bool IsWithinAngle(this FovRunner handler, Transform from, Vector3 to, float halfangle, bool separateVertical = false, float halfVertical = 0f)
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

    public static bool IsWithinYaw(this FovRunner handler, Transform from, Vector3 target, float halfYawDeg, bool useLocalUp = true)
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





    public static int EvaluateViewCone(this FovRunner handler, Vector3 start, Vector3 end, float radius, Vector3 direction, float maxDistance, LayerMask targetMask, Vector3[] hitPoints, RaycastHit[] _hitBuffer)
    {

        int hitCount = Physics.CapsuleCastNonAlloc(start, end, radius, direction, _hitBuffer, maxDistance, targetMask);

        int processedCount = Mathf.Min(hitCount, hitPoints.Length);

        for (int i = 0; i < processedCount; i++)
        {
            hitPoints[i] = _hitBuffer[i].point;
        }

        return processedCount;

    }


    public static void GetSamplePoints(this Collider col, List<Vector3> points, float inset = 0.9f)
    {
        if (col == null || points == null) return;

        points.Clear();
        var t = col.transform;

        if(col is BoxCollider box)
        {
            Vector3 center = t.TransformPoint(box.center);
            Vector3 half = (box.size * 0.5f) * inset;

            Vector3 right = t.right * half.x;
            Vector3 up = t.up * half.y;

            points.Add(center);
            points.Add(center + up);
            points.Add(center + right);
            points.Add(center - right);

        }else if(col is CapsuleCollider cap)
        {
            Vector3 center = t.TransformPoint(cap.center);

            float radius = cap.radius * inset;
            float halfHeight = Mathf.Max(cap.height * 0.5f - cap.radius, 0f);

            Vector3 axis = cap.direction == 0 ? t.right
                : cap.direction == 1 ? t.up : t.forward;

            Vector3 topSphere = center + axis * halfHeight;
            Vector3 bottomSphere = center - axis * halfHeight;

            points.Add(center);
            points.Add(topSphere + axis * radius);
            points.Add(center + t.right * radius);
            points.Add(center - t.right * radius);
        }
     

    }

}
