using UnityEngine;

[RequireComponent(typeof(DeflectableCollisionComponent))]
public sealed class ForceProjectile : ProjectileBase
{
    private IPoolManager _onCollisionParticlePool;

    protected override void AttachMovementHandler()
        => _movementHandler = new ForceProjectileMovementHandler(_projectileEventManager, GetComponent<Rigidbody>(), _projectileSpeed);
}
