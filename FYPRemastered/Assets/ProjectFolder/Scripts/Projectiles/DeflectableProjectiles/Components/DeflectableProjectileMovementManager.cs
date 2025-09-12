using UnityEngine;

public sealed class DeflectableProjectileMovementManager : ProjectileMovementManagerBase
{
    public bool DeflectionProcessed { get; private set; } = false; // Delete when changing to trigger
    public float DeflectSpeed { get; set; }

    public bool IsFrozen { get; private set; } = false;



    public DeflectableProjectileMovementManager(
        ProjectileEventManager eventManager, Rigidbody rb, 
        float initialSpeed, MovementType moveType, 
        float deflectSpeed, float acceleration = 0f, float maxSpeed = 0f,
        bool useGravity = false) : base(eventManager, rb, initialSpeed, moveType, acceleration, maxSpeed, useGravity) 
    {
        _eventManager.OnDeflected += Deflected;
        _eventManager.OnFreeze += ResetRigidBody;
        DeflectSpeed = deflectSpeed;
    }

    

    private void Deflected(ProjectileKickType type)
    {
        Debug.LogError("Deflect called");
        Vector3 directionTotarget = _eventManager.GetDirectionToTarget();

        Quaternion newRotation = Quaternion.LookRotation(directionTotarget);


        Speed = DeflectSpeed;
        _rb.MoveRotation(newRotation);

        _rb.AddForce(directionTotarget * Speed, ForceMode.VelocityChange);
        DeflectionProcessed = true;
        IsFrozen = false;
    }

    protected override void Launch()
    {
        IsFrozen = false;
        base.Launch();
    }



    public override void FixedTick()
    {
        
       base.FixedTick();
        
        /*if (!DeflectionProcessed || _rb.isKinematic) { return; } // Take out when changing to trigger

        if (_rb.linearVelocity.magnitude < Speed * 0.95f)
        {
            _rb.linearVelocity = _rb.transform.forward * Speed;
        }*/
    }

    protected override void ResetRigidBody()
    {
        //IsFrozen = true;
        DeflectionProcessed = false;
        base.ResetRigidBody();
    }

    public override void OnInstanceDestroyed()
    {
        _eventManager.OnFreeze -= ResetRigidBody;
        _eventManager.OnDeflected -= Deflected;
        base.OnInstanceDestroyed();
    }
}
