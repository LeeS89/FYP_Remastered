using UnityEngine;

public class BulletMovementComponent : MonoBehaviour, IBulletEvents
{
    private Rigidbody _rb;
    [SerializeField] private float _speed;
   
    private bool _deflectionProcessed = false;
    private BulletEventManager _eventManager;
    private Vector3 _direction;
   
    public void RegisterEvents(BulletEventManager eventManager)
    {
        _eventManager = eventManager;
        _rb = GetComponentInParent<Rigidbody>();
        if(_eventManager != null)
        {
            _eventManager.OnFired += Launch;
            _eventManager.OnDeflected += Deflected;
        }
    }

    private void OnDisable()
    {
        _speed = 0.2f;
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
