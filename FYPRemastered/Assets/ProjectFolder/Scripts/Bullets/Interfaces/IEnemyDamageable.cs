using UnityEngine;

public interface IEnemyDamageable : IDamageable
{
    void HandleHit(Collider hitCollider, Vector3 rawHitPoint);
}
