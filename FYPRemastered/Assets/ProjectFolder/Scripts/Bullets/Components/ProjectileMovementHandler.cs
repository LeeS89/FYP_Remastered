using UnityEngine;

public class ProjectileMovementHandler
{
    protected ProjectileEventManager _eventManager;
    protected Rigidbody _rb;
    public float Speed { get; protected set; }
    public float InitialSpeed { get; set; }

    public ProjectileMovementHandler(ProjectileEventManager eventManager, Rigidbody rb, float initialSpeed = 0f)
    {
        _eventManager = eventManager;
        _rb = rb;
        InitialSpeed = initialSpeed;
        Speed = InitialSpeed;
        _eventManager.OnLaunch += Launch;
        _eventManager.OnFreeze += ResetRigidBody;
        _eventManager.OnDeflected += Deflected;
        ResetRigidBody();
    }

    public virtual void Tick() { }

    public virtual void FixedTick() { }

    protected virtual void Launch() { }

    protected virtual void Deflected(ProjectileKickType type) { }

    protected virtual void ResetRigidBody()
    {
        if (_rb == null) return;
        Speed = InitialSpeed;
        _rb.AddForce(Vector3.zero, ForceMode.VelocityChange);
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    public virtual void OnDisable()
    {
        ResetRigidBody();
    }

    public virtual void OnInstanceDestroyed()
    {
        _eventManager.OnDeflected -= Deflected;
        _eventManager.OnLaunch -= Launch;
        _eventManager.OnFreeze -= ResetRigidBody;
        _rb = null;
        _eventManager = null;
    }
}
