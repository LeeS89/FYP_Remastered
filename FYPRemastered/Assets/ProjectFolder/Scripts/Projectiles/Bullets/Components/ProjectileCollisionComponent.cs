using System;
using UnityEngine;

public class ProjectileCollisionComponent : ComponentEvents
{
    protected ProjectileEventManager _projectileEventManager;
    [Header("Damage Data")]
    [SerializeField] protected DamageData _damageData;
    protected float _baseDamage;
    protected DamageType _damageType;
    protected float _damageOverTime;
    protected float _dOTDuration;
    [SerializeField] protected float _statusEffectChancePercentage;
    [SerializeField] protected Collider _col;
    [SerializeField] protected Rigidbody _rb;
    protected Vector3 _previousPosition;
    protected Quaternion _previousRotation;
    [SerializeField] protected const int _maxCollisions = 4;
    protected RaycastHit[] _hitBuffer = new RaycastHit[_maxCollisions];
    [SerializeField] protected LayerMask _collisionMask;


    //Debug
    [SerializeField] bool _debugDraw = true;
    [SerializeField] Color _pathColor = new Color(0, 1, 1, 0.8f);
    [SerializeField] Color _startColor = new Color(0, 1, 0, 0.6f);
    [SerializeField] Color _endColor = new Color(1, 1, 0, 0.6f);
    [SerializeField] Color _hitColor = new Color(1, 0, 0, 0.9f);

    // debug cache
    Vector3 _dbgDir; float _dbgDist;
    bool _dbgHadHit; RaycastHit _dbgHit;
    Vector3 _dbgSphereCenter; float _dbgSphereRadius;
    Vector3 _dbgCapP0, _dbgCapP1; float _dbgCapRadius;
    Vector3 _dbgBoxCenter; Vector3 _dbgBoxHalf; Quaternion _dbgBoxRot;
    enum DbgType { None, Sphere, Capsule, Box, Ray } DbgType _dbgType;

    // end debug

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _projectileEventManager = eventManager as ProjectileEventManager;
        if(_rb == null) _rb = GetComponentInChildren<Rigidbody>();
        if (_col == null) _col = GetComponentInChildren<Collider>();

        _previousPosition = _col.transform.position;
        _previousRotation = _col.transform.rotation;

        InitializeDamageType();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager) => _projectileEventManager = null;


    protected virtual void InitializeDamageType() { }

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
            SweepByCollider(direction, distance);
        }
        _previousPosition = currentPosition;
        _previousRotation = currentRotation;
    }


    protected void SweepByCollider(Vector3 direction, float maxDistance)
    {
        RaycastHit hit;

        switch (_col)
        {
            case SphereCollider sp:
                SweepSphere(sp, direction, maxDistance, out hit);
                break;
            case CapsuleCollider cap:
                SweepCapsule(cap, direction, maxDistance, out hit);
                break;
            case BoxCollider box:
                SweepBox(box, direction, maxDistance, out hit);
                break;
            default:
                if(Physics.Raycast(_previousPosition, direction, out hit, maxDistance, _collisionMask, QueryTriggerInteraction.Ignore))
                {
                    HandleCollision(hit);
                }
                return;
        }

        if(hit.collider != null) HandleCollision(hit);
    }

    private void HandleCollision(RaycastHit hit)
    {
        if (!_hitRegistered)
        {
            _hitRegistered = true;
            Debug.LogError("Sweep Detected First!");
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

    protected void SweepCapsule(CapsuleCollider cap, Vector3 direction, float distance, out RaycastHit hit)
    {
        hit = default;

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
        _dbgHadHit = hit.collider != null; if (_dbgHadHit) _dbgHit = hit;
       
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
        for(int i = 0; i < count; i++)
        {
            RaycastHit hit = _hitBuffer[i];
            if(hit.collider != null && hit.distance < minDistance)
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
        if (!_debugDraw || _rb == null) return;

        // path
        Gizmos.color = _pathColor;
        Gizmos.DrawLine(_previousPosition, _previousPosition + _dbgDir * _dbgDist);

        switch (_dbgType)
        {
            case DbgType.Sphere:
               // DrawSphereCastGizmos(_dbgSphereCenter, _dbgSphereRadius, _dbgDir, _dbgDist);
                break;

            case DbgType.Capsule:
                DrawCapsuleCastGizmos(_dbgCapP0, _dbgCapP1, _dbgCapRadius, _dbgDir, _dbgDist);
                break;

            case DbgType.Box:
             //   DrawBoxCastGizmos(_dbgBoxCenter, _dbgBoxHalf, _dbgBoxRot, _dbgDir, _dbgDist);
                break;
        }

        if (_dbgHadHit)
        {
            Gizmos.color = _hitColor;
            Gizmos.DrawSphere(_dbgHit.point, 0.03f);
            Gizmos.DrawLine(_dbgHit.point, _dbgHit.point + _dbgHit.normal * 0.3f);
        }
    }

    void DrawCapsuleCastGizmos(Vector3 p0, Vector3 p1, float r, Vector3 dir, float dist)
    {
        Vector3 q0 = p0 + dir * dist;
        Vector3 q1 = p1 + dir * dist;

        Gizmos.color = _startColor;
        Gizmos.DrawWireSphere(p0, r); Gizmos.DrawWireSphere(p1, r);
        Gizmos.color = _endColor;
        Gizmos.DrawWireSphere(q0, r); Gizmos.DrawWireSphere(q1, r);

        // connect a few guide lines along an orthonormal basis to suggest the cylinder
        Orthonormal((p0 - p1).normalized, out var right, out var up);
        Gizmos.color = _pathColor;
        Gizmos.DrawLine(p0 + right * r, q0 + right * r);
        Gizmos.DrawLine(p1 + right * r, q1 + right * r);
        Gizmos.DrawLine(p0 + up * r, q0 + up * r);
        Gizmos.DrawLine(p1 + up * r, q1 + up * r);
    }

    void Orthonormal(Vector3 n, out Vector3 right, out Vector3 up)
    {
        n = n.normalized;
        Vector3 a = Mathf.Abs(n.y) < 0.99f ? Vector3.up : Vector3.right;
        right = Vector3.Cross(a, n).normalized;
        up = Vector3.Cross(n, right);
    }
#endif

    protected virtual void OnCollisionEnter(Collision collision) => _projectileEventManager.Expired();

    private bool _hitRegistered = false;

    private void OnEnable()
    {
        _hitRegistered = false;
    }

    protected virtual void OnTriggerEnter(Collider other) 
    {
        if (!_hitRegistered)
        {   
            _hitRegistered = true;
            Debug.LogError("Trigger Detected First!");
            _projectileEventManager.Expired();
        }
    }/*=> _projectileEventManager.Expired();*/

}
