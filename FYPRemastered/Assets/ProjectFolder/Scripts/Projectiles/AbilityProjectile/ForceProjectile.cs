using UnityEngine;

[RequireComponent(typeof(BulletCollisionComponent))]
public sealed class ForceProjectile : Projectile
{
    private IPoolManager _onCollisionParticlePool;

    protected override void AttachMovementHandler()
        => _movementHandler = new ForceProjectileMovementHandler(_projectileEventManager, GetComponent<Rigidbody>(), _projectileSpeed);
}
