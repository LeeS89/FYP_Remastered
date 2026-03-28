/*using Unity.XR.CoreUtils;
using UnityEngine;

public class OldSweepAheadCasts : MonoBehaviour
{
    protected override void Accelerate()
    {
        // v1 = v0 + a*dt; step = v1*dt
        float dt = Time.fixedDeltaTime;
        Vector3 v0 = _rb.linearVelocity;
        Vector3 a = _rb.transform.forward * Acceleration; // your accel
        Vector3 v1 = v0 + a * dt;
        Vector3 step = v1 * dt;

        float dist = step.magnitude;
        if (dist > 1e-6f)
        {
            Vector3 dir = step / dist;

            if (_eventManager.Sweep(dir, dist)) return;

            // SweepCapsule(_cap, dir, dist, out var hit);

            *//* if (hit.collider)
             {
                 Transform t = _cap.transform;
                 Vector3 s = t.lossyScale;
                 Vector3 sAbs = new(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));

                 int ax = _cap.direction; // 0=X,1=Y,2=Z
                 float axisScale = ax == 0 ? sAbs.x : ax == 1 ? sAbs.y : sAbs.z;
                 float radScale = ax == 0 ? Mathf.Max(sAbs.y, sAbs.z)
                                : ax == 1 ? Mathf.Max(sAbs.x, sAbs.z)
                                       : Mathf.Max(sAbs.x, sAbs.y);

                 float r = _cap.radius * radScale;
                 float halfLine = Mathf.Max(0f, (_cap.height * 0.5f - _cap.radius) * axisScale);

                 Vector3 localAxis = ax == 0 ? Vector3.right : ax == 1 ? Vector3.up : Vector3.forward;
                 Vector3 axisWS = (t.rotation * localAxis).normalized;

                 // place the *front* endcap on the surface, then back off a hair
                 float sgn = Mathf.Sign(Vector3.Dot(axisWS, dir));
                 Vector3 front = hit.point + hit.normal * r;
                 Vector3 newCenter = front - axisWS * (halfLine * sgn);

                 _rb.position = newCenter - dir * 0.005f; // tiny skin
                 _rb.linearVelocity = Vector3.zero;

                 Impact(hit);
                 gameObject.SetActive(false);
                 return;
             }*//*
        }

        // finally apply physics accel (solver runs after FixedUpdate)
        _rb.AddForce(a, ForceMode.Acceleration);


        /////////////////
        *//*var dir = _rb.transform.forward;
        _rb.AddForce(dir * Acceleration, ForceMode.Acceleration);

        if (MaxSped > 0)
        {
            float sq = _rb.linearVelocity.sqrMagnitude;
            float maxSq = MaxSped * MaxSped;
            if (sq > maxSq) _rb.linearVelocity = _rb.linearVelocity.normalized * MaxSped;
        }*//*
    }

    protected bool SweepByCollider(Vector3 direction, float maxDistance)
    {
        RaycastHit hit;
        bool result = false;
        switch (_col)
        {
            case SphereCollider sp:
                SweepSphere(sp, direction, maxDistance, out hit);
                break;
            case CapsuleCollider cap:
                result = SweepCapsule(cap, direction, maxDistance, out hit);
                break;
            case BoxCollider box:
                SweepBox(box, direction, maxDistance, out hit);
                break;
            default:
                if (Physics.Raycast(_previousPosition, direction, out hit, maxDistance, _collisionMask, QueryTriggerInteraction.Ignore))
                {
                    HandleCollision(hit);
                }
                return false;
        }
        return result;
        if (hit.collider != null) HandleCollision(hit);
    }

    protected virtual void HandleCollision(RaycastHit hit)
    {
        if (!HitAlreadyRegistered)
        {
            HitAlreadyRegistered = true;


            Debug.LogError("Sweep Detected First!");
            _projectileEventManager.Collide(hit.point, hit.normal);
            _projectileEventManager.Expired();

        }
        // Destroy(gameObject);
    }

    protected virtual void SweepSphere(SphereCollider sp, Vector3 direction, float distance, out RaycastHit hit)
    {
        hit = default;

        // World center at previous pose: pos_prev + rot_prev * (scale * localCenter)
        Vector3 lossy = _col.transform.lossyScale;
        Vector3 centerWS = _previousPosition + _previousRotation * Vector3.Scale(sp.center, lossy);

        // Radius scaled by the max axis (PhysX-conservative)
        Vector3 sAbs = new(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), Mathf.Abs(lossy.z));
        float radiusWS = sp.radius * Mathf.Max(sAbs.x, sAbs.y, sAbs.z);

        int count = Physics.SphereCastNonAlloc(
            centerWS,
            radiusWS,
            direction,
            _hitBuffer,
            distance,
            _collisionMask,
            QueryTriggerInteraction.Ignore);

        hit = GetNearestHit(count);

    }

    public bool SweepCapsule(CapsuleCollider cap, Vector3 direction, float distance, out RaycastHit hit)
    {
        hit = default;
        if (distance <= 1e-6f || !cap) return false;

        // Build capsule at CURRENT collider pose (use cap, not _col)
        Transform t = cap.transform;
        Vector3 lossy = t.lossyScale;
        Vector3 sAbs = new(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), Mathf.Abs(lossy.z));

        int ax = cap.direction; // 0=X,1=Y,2=Z
        float axisScale = ax == 0 ? sAbs.x : ax == 1 ? sAbs.y : sAbs.z;
        float radScale = ax == 0 ? Mathf.Max(sAbs.y, sAbs.z)
                      : ax == 1 ? Mathf.Max(sAbs.x, sAbs.z)
                              : Mathf.Max(sAbs.x, sAbs.y);

        float radiusWS = cap.radius * radScale;
        float halfLine = Mathf.Max(0f, (cap.height * 0.5f - cap.radius) * axisScale);

        Vector3 localAxis = ax == 0 ? Vector3.right : ax == 1 ? Vector3.up : Vector3.forward;

        Vector3 centerWS = t.position + t.rotation * Vector3.Scale(cap.center, lossy);
        Vector3 worldAxis = (t.rotation * localAxis).normalized;

        Vector3 p0 = centerWS + worldAxis * halfLine;
        Vector3 p1 = centerWS - worldAxis * halfLine;

        // (optional) gizmo caches
        _dbgP0 = p0; _dbgP1 = p1; _dbgDir = direction; _dbgDist = distance; _dbgRadius = radiusWS;
        _dbgHadHit = false;

        int count = Physics.CapsuleCastNonAlloc(
            p0, p1, radiusWS, direction, _hitBuffer, distance, _collisionMask, QueryTriggerInteraction.Ignore);

        // make sure GetNearestHit skips owner/self:
        //  - h.collider == cap
        //  - h.rigidbody == _rb (if you cache the RB)
        hit = GetNearestHit(count);
        _dbgHadHit = hit.collider != null;
        if (_dbgHadHit) _dbgHit = hit;

        if (hit.collider)
        {
            // Transform t1 = cap.transform;
            // Vector3 s = t1.lossyScale;
            // Vector3 sAbs = new(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));

            //  int ax = cap.direction; // 0=X,1=Y,2=Z
            // float axisScale = ax == 0 ? sAbs.x : ax == 1 ? sAbs.y : sAbs.z;
            *//*  float radScale = ax == 0 ? Mathf.Max(sAbs.y, sAbs.z)
                             : ax == 1 ? Mathf.Max(sAbs.x, sAbs.z)
                                    : Mathf.Max(sAbs.x, sAbs.y);*//*

            // float r = _cap.radius * radScale;
            //  float halfLine = Mathf.Max(0f, (cap.height * 0.5f - cap.radius) * axisScale);

            //  Vector3 localAxis = ax == 0 ? Vector3.right : ax == 1 ? Vector3.up : Vector3.forward;
            //  Vector3 axisWS = (t1.rotation * localAxis).normalized;

            // place the *front* endcap on the surface, then back off a hair
            float sgn = Mathf.Sign(Vector3.Dot(worldAxis, direction));
            Vector3 front = hit.point + hit.normal * radiusWS;
            Vector3 newCenter = front - worldAxis * (halfLine * sgn);

            _rb.position = newCenter - direction * 0.005f; // tiny skin
            _rb.linearVelocity = Vector3.zero;
            HandleCollision(hit);
            // Impact(hit);
            // gameObject.SetActive(false);
            // return;
        }

        return hit.collider != null;
        *//*hit = default;
        
        _dbgType = DbgType.Capsule; _dbgDir = direction; _dbgDist = distance;

        Vector3 lossy = _col.transform.lossyScale;
        Vector3 sAbs = new(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), Mathf.Abs(lossy.z));

        // Axis scales
        int ax = cap.direction; // 0=X,1=Y,2=Z
        float axisScale = ax == 0 ? sAbs.x : ax == 1 ? sAbs.y : sAbs.z;
        float radScale = ax == 0 ? Mathf.Max(sAbs.y, sAbs.z)
                      : ax == 1 ? Mathf.Max(sAbs.x, sAbs.z)
                                : Mathf.Max(sAbs.x, sAbs.y);

        _dbgCapRadius = cap.radius * radScale;
        float radiusWS = cap.radius * radScale;

        // Half line (between spherical caps), scaled along the capsule axis
        float halfLine = Mathf.Max(0f, (cap.height * 0.5f - cap.radius) * axisScale);

        // Local axis unit vector
        Vector3 localAxis = ax == 0 ? Vector3.right : ax == 1 ? Vector3.up : Vector3.forward;

        // Center at previous pose (scale center offset!)
        Vector3 centerWS = _previousPosition + _previousRotation * Vector3.Scale(cap.center, lossy);
        Vector3 worldAxis = (_previousRotation * localAxis).normalized;

        _dbgCapP0 = centerWS + worldAxis * halfLine; // start-top
        _dbgCapP1 = centerWS - worldAxis * halfLine;
        Vector3 p0 = centerWS + worldAxis * halfLine;
        Vector3 p1 = centerWS - worldAxis * halfLine;

        int count = Physics.CapsuleCastNonAlloc(
            p0,
            p1,
            radiusWS,
            direction,
            _hitBuffer,
            distance,
            _collisionMask,
            QueryTriggerInteraction.Ignore);

        hit = GetNearestHit(count);
        _dbgHadHit = hit.collider != null; if (_dbgHadHit) _dbgHit = hit;*//*

    }

    protected void SweepBox(BoxCollider box, Vector3 direction, float distance, out RaycastHit hit)
    {
        Vector3 localHalfExtents = box.size * 0.5f;
        Vector3 worldHalfExtents = Vector3.Scale(localHalfExtents, box.transform.lossyScale);

        Vector3 center = _previousRotation * box.center + _previousPosition;
        Quaternion rotation = _previousRotation;

        int hitCount = Physics.BoxCastNonAlloc(
            center,
            worldHalfExtents,
            direction,
            _hitBuffer,
            rotation,
            distance,
            _collisionMask,
            QueryTriggerInteraction.Ignore);

        hit = GetNearestHit(hitCount);

    }

    protected RaycastHit GetNearestHit(int count)
    {
        RaycastHit best = default;
        float minDistance = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = _hitBuffer[i];
            if (hit.collider != null && hit.distance < minDistance)
            {
                minDistance = hit.distance;
                best = hit;
            }
            _hitBuffer[i] = default;
        }
        return best;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!_debugDraw || _dbgDist <= 0f) return;

        // start + end endcaps
        Gizmos.color = _startColor;
        Gizmos.DrawWireSphere(_dbgP0, _dbgRadius);
        Gizmos.DrawWireSphere(_dbgP1, _dbgRadius);

        Vector3 endP0 = _dbgP0 + _dbgDir * _dbgDist;
        Vector3 endP1 = _dbgP1 + _dbgDir * _dbgDist;

        Gizmos.color = _endColor;
        Gizmos.DrawWireSphere(endP0, _dbgRadius);
        Gizmos.DrawWireSphere(endP1, _dbgRadius);

        // path guide lines
        Gizmos.color = _pathColor;
        Gizmos.DrawLine(_dbgP0, endP0);
        Gizmos.DrawLine(_dbgP1, endP1);
        Vector3 cStart = (_dbgP0 + _dbgP1) * 0.5f;
        Gizmos.DrawLine(cStart, cStart + _dbgDir * _dbgDist);

        // hit marker
        if (_dbgHadHit)
        {
            Gizmos.color = _hitColor;
            Gizmos.DrawSphere(_dbgHit.point, Mathf.Max(0.01f, _dbgRadius * 0.1f));
            Gizmos.DrawLine(_dbgHit.point, _dbgHit.point + _dbgHit.normal * (_dbgRadius * 0.75f));
        }
    }
#endif

    public void FixedTick()
    {

        if (_rb == null || _col == null || !_col.enabled || !_col.isTrigger) return;
        Vector3 currentPosition = _col.transform.position;
        Quaternion currentRotation = _col.transform.rotation;
        Vector3 movement = currentPosition - _previousPosition;
        float distance = movement.magnitude;
        if (distance > 0.01f)
        {
            Vector3 direction = movement / distance;
            // SweepByCollider(direction, distance);
        }
        _previousPosition = currentPosition;
        _previousRotation = currentRotation;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (HitAlreadyRegistered) return;
        HitAlreadyRegistered = true;
        GameObject otherObj = other.gameObject;
        Debug.LogError("Trigger Detected First!");
        Debug.LogError("Other Nqame is: " + otherObj.name);
        Vector3 refPos = _col.bounds.center;
        Vector3 point = other.ClosestPoint(refPos);
        // Vector3 normal = (point - refPos).normalized;
        CheckForDamageableInterface(otherObj*//*, null, point, normal*//*);
        _projectileEventManager.Collide(point);

        if ((_deflectorMask & (1 << otherObj.layer)) == 0)
        {
            _projectileEventManager.Expired();
        }
    }

}
*/