using UnityEngine;

public class BulletMovementComponent : MonoBehaviour, IBulletEvents
{
    private Rigidbody _rb;
    //private bool _isMoving = false;
    [SerializeField] private float _speed;

    public void RegisterEvents(BulletEventManager eventManager)
    {
        _rb = GetComponentInParent<Rigidbody>();
        if(eventManager != null)
        {
            eventManager.OnFired += Launch;
        }
    }

   /* private void Start()
    {
        Launch();
    }*/

    private void Launch()
    {
        _rb.linearVelocity = transform.forward * _speed;
    }
   
    
}
