using UnityEngine;

public class BulletMovementComponent : MonoBehaviour, IBulletEvents
{
    private Rigidbody _rb;
    private bool _isMoving = false;
    [SerializeField] private float _speed;

    public void RegisterEvents(BulletEventManager eventManager)
    {
        _rb = GetComponentInParent<Rigidbody>();
        if(eventManager != null)
        {
            eventManager.OnFired += Launch;
            eventManager.OnDeflected += Deflected;
        }
    }

    private void OnDisable()
    {
        _speed = 0.2f;
    }

    private void Deflected(Quaternion newRotation, float newSpeed)
    {
        _rb.MoveRotation(newRotation);
        _speed = newSpeed;
    }

    private void Launch()
    {
        _isMoving = true;
    }

    

    private void FixedUpdate()
    {
        if (!_isMoving) { return; }
        _rb.linearVelocity = _rb.transform.forward * _speed;

    }


}
