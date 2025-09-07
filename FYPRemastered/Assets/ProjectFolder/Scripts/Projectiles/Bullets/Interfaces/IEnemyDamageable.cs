using UnityEngine;

public interface IEnemyDamageable : IDamageable
{
    SkinnedMeshRenderer GetSkinnedMeshRenderer();

    Vector3 GetAdjustedHitPoint(Vector3 originalPoint, Vector3 normal);
}
