using UnityEngine;

public class SpecialDamageRelay : BaseDamageRelay, IEnemyDamageable
{
    [SerializeField] private SkinnedMeshRenderer _visualMesh;

    public Vector3 GetAdjustedHitPoint(Vector3 originalHitPoint, Vector3 hitNormal)
    {
        if (_visualMesh == null) return originalHitPoint;

        Vector3 rayOrigin = originalHitPoint - hitNormal * 0.1f;
        Ray ray = new Ray(rayOrigin, hitNormal);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 0.3f, LayerMask.GetMask("Water")))
        {
            return hitInfo.point;
        }

        return originalHitPoint;
    }

    public SkinnedMeshRenderer GetSkinnedMeshRenderer()
    {
        return _visualMesh;
    }

    public void HandleHit(Collider hitCollider, Vector3 rawHitPoint)
    {
        
    }
}
