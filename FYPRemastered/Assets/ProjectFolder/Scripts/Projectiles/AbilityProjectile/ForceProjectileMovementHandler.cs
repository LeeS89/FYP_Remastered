using UnityEngine;

public class ForceProjectileMovementHandler : ProjectileMovementHandler
{
    public ForceProjectileMovementHandler(ProjectileEventManager eventManager, Rigidbody rb, float initialSpeed = 0, bool usesFixedSpeed = false) : base(eventManager, rb, initialSpeed, usesFixedSpeed)
    {
        Launch();
    }

    protected override void Launch()
    {
        _rb.AddForce(Vector3.forward  * 5f, ForceMode.Impulse);
    }

    public override void FixedTick()
    {
        if (UsesFixedSpeed) return;
    }
}
