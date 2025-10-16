using UnityEngine;

[RequireComponent(typeof(DeflectableCollisionComponent))]
public sealed class ForceProjectileObsolete : ProjectileBase
{
    private IPoolManager _onCollisionParticlePool;

    protected override void AttachMovementHandler()
        => _movementHandler = new ForceProjectileMovementHandlerObsolete(_projectileEventManager, GetComponent<Rigidbody>(), _projectileSpeed);
}
