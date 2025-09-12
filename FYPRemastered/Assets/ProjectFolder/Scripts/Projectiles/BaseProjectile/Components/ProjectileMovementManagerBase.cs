using UnityEngine;

public class ProjectileMovementManagerBase
{
    protected ProjectileEventManager _eventManager;
    protected Rigidbody _rb;
    public float Speed { get; protected set; }
    public float InitialSpeed { get; set; }

    public float Acceleration { get; set; } = 0f;

    public float MaxSped { get; protected set; } = 0f;

    public MovementType _moveType { get; set; } = MovementType.None;

    public bool UsingGravity { get; protected set; }

  

    public ProjectileMovementManagerBase(
        ProjectileEventManager eventManager, Rigidbody rb, 
        float initialSpeed, MovementType moveType = MovementType.FixedSpeedKinematic, 
        float acceleration = 0f, float maxSpeed = 0f, bool useGravity = false)
    {
        _eventManager = eventManager;
        _rb = rb;
        InitialSpeed = initialSpeed;
        Speed = InitialSpeed;
        Acceleration = acceleration;
        MaxSped = maxSpeed;
        _eventManager.OnLaunch += Launch;
        UsingGravity = useGravity;
        _moveType = moveType;
        ResetRigidBody();
   
    }

    public virtual void Tick() { }

    public virtual void FixedTick()
    {
        switch (_moveType)
        {
            case MovementType.FixedSpeedKinematic:
                Move();
                break;
            case MovementType.Acceleration:
                Accelerate();
                break;
            default:
                return;
        }
    }

    protected void ToggleGravity(bool useGravity) => _rb.useGravity = useGravity;

    protected virtual void Move()
    {
        if (!_rb.isKinematic) _rb.isKinematic = true;
        _rb.MovePosition(_rb.position + _rb.transform.forward * Speed * Time.fixedDeltaTime);
    }

    protected virtual void Accelerate()
    {
        if(_rb.isKinematic) _rb.isKinematic = false;
        var dir = _rb.transform.forward;
        _rb.AddForce(dir * Acceleration, ForceMode.Acceleration);

        if(MaxSped > 0)
        {
            float sq = _rb.linearVelocity.sqrMagnitude;
            float maxSq = MaxSped * MaxSped;
            if (sq > maxSq) _rb.linearVelocity = _rb.linearVelocity.normalized * MaxSped;
        }
    }

    protected virtual void Launch()
    {
        _rb.useGravity = UsingGravity;

        switch (_moveType)
        {
            case MovementType.Velocity or MovementType.Acceleration:
                if (_rb.isKinematic) _rb.isKinematic = false;
                _rb.AddForce(_rb.transform.forward * Speed, ForceMode.VelocityChange);
                break;
            case MovementType.Impulse:
                if (_rb.isKinematic) _rb.isKinematic = false;
                if (!_rb.useGravity) _rb.useGravity = true;
                _rb.AddForce(_rb.transform.forward * Speed, ForceMode.Impulse);
                break;
            default:
               // Move();
                break;
        }
    }


   // protected virtual void Deflected(ProjectileKickType type) { }

    protected virtual void ResetRigidBody()
    {
        if (_rb == null) return;
        if(UsingGravity) _rb.useGravity = false;
        Speed = InitialSpeed;
        _rb.AddForce(Vector3.zero, ForceMode.VelocityChange);
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    public virtual void OnDisable() => ResetRigidBody();


    public virtual void OnInstanceDestroyed()
    {
   
        _eventManager.OnLaunch -= Launch;
       
        _rb = null;
        _eventManager = null;
    }
}
