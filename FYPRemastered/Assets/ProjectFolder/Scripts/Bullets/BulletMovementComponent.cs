using UnityEngine;

public class BulletMovementComponent : MonoBehaviour, IComponentEvents
{
    private Rigidbody _rb;
    [SerializeField] private float _speed;
   
    private bool _deflectionProcessed = false;
    private BulletEventManager _eventManager;
    

   
    public void RegisterEvents(EventManager eventManager)
    {
        _eventManager = eventManager as BulletEventManager;
        _rb = GetComponentInParent<Rigidbody>(true);
       
        if(_eventManager != null)
        {
            _eventManager.OnFired += Launch;
            _eventManager.OnDeflected += Deflected;
        }
    }

    public void UnRegisterEvents(EventManager eventManager)
    {
        _eventManager.OnFired -= Launch;
        _eventManager.OnDeflected -= Deflected;
        _eventManager = null;

        ResetRigidBody();
        _rb = null;
    }

    private void OnDisable()
    {
        if (_rb != null)
        {
            ResetRigidBody();
        }
    }

    private void ResetRigidBody()
    {
        _speed = 5f;
        _rb.AddForce(Vector3.zero, ForceMode.VelocityChange);
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    public bool _deflect = false;
    public void Deflected(Vector3 direction, Quaternion newRotation, float newSpeed)
    {
        _speed = newSpeed;
        _rb.MoveRotation(newRotation);

        _rb.AddForce(direction * newSpeed, ForceMode.VelocityChange);
        _deflect = true;

    }

    private void Launch()
    {
        //_rb.linearVelocity = _rb.transform.forward * _speed;
        _rb.AddForce(_rb.transform.forward * _speed, ForceMode.VelocityChange);
    }

    private void FixedUpdate()
    {
        if (!_deflect) { return; }

        if (_rb.linearVelocity.magnitude < _speed * 0.95f)
        {
            _rb.linearVelocity = _rb.transform.forward * _speed;
        }
    }


    public bool HasDeflectionBeenProcessed()
    {
        return _deflectionProcessed;
    }

    
}
