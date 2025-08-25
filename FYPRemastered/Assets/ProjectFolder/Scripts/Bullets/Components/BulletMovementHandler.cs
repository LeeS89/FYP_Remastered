using UnityEngine;

public class BulletMovementHandler : ProjectileMovementHandler
{
    public bool DeflectionProcessed { get; private set; } = false;
    public float DeflectSpeed { get; set; }

    public BulletMovementHandler(ProjectileEventManager eventManager, Rigidbody rb, float speed, float deflectSpeed) : base(eventManager, rb, speed) { DeflectSpeed = deflectSpeed; }

    protected override void Launch() => _rb.AddForce(_rb.transform.forward * Speed, ForceMode.VelocityChange);

    protected override void Deflected(ProjectileKickType type)
    {
        Vector3 directionTotarget = _eventManager.GetDirectionToTarget();

        Quaternion newRotation = Quaternion.LookRotation(directionTotarget);


        Speed = DeflectSpeed;
        _rb.MoveRotation(newRotation);

        _rb.AddForce(directionTotarget * Speed, ForceMode.VelocityChange);
        DeflectionProcessed = true;
    }

    public override void FixedTick()
    {
        if (!DeflectionProcessed) { return; }

        if (_rb.linearVelocity.magnitude < Speed * 0.95f)
        {
            _rb.linearVelocity = _rb.transform.forward * Speed;
        }
    }

    protected override void ResetRigidBody()
    {
        base.ResetRigidBody();
        DeflectionProcessed = false;
    }


}
