using UnityEngine;
using UnityEngine.UIElements;

public class BulletMovementComponent : ComponentEvents
{
    [Header("Deflection Speed")]
    [SerializeField] private float _deflectSpeed;

    private Rigidbody _rb;
    [SerializeField] private float _editableSpeed;
    private float _speed;
    

    private bool _deflectionProcessed = false;
    private BulletEventManager _bulletEventManager;
    public Vector3 _scale;

    public bool DeflectionProcessed
    {
        get => _deflectionProcessed;
        private set => _deflectionProcessed = value;
    }

    private void Awake()
    {
        _scale = transform.localScale;
    }


    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _bulletEventManager = eventManager as BulletEventManager;
        _rb = GetComponentInParent<Rigidbody>(true);
        _speed = _editableSpeed;
        _bulletEventManager.OnFired += Launch;
        _bulletEventManager.OnReverseDirection += ReverseDirection;
        //_bulletEventManager.OnDeflected += Deflected;
        _bulletEventManager.OnFreeze += ResetRigidBody;

    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _bulletEventManager.OnFired -= Launch;
        //_bulletEventManager.OnDeflected -= Deflected;
        _bulletEventManager.OnReverseDirection -= ReverseDirection;
        _bulletEventManager.OnFreeze -= ResetRigidBody;
        ResetRigidBody();
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        _bulletEventManager = null;
        _rb = null;
    }

    private void OnEnable()
    {
        if(transform.localScale != _scale)
        {
            transform.localScale = _scale;
        }
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
        DeflectionProcessed = false;
        _speed = _editableSpeed;
        _rb.AddForce(Vector3.zero, ForceMode.VelocityChange);
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    //public bool _deflect = false;
    public void ReverseDirection(/*Vector3 direction, Quaternion newRotation, float newSpeed*/)
    {
        Vector3 directionTotarget = _bulletEventManager.GetDirectionToTarget();

        Quaternion newRotation = Quaternion.LookRotation(directionTotarget);


        _speed = _deflectSpeed;
        _rb.MoveRotation(newRotation);

        _rb.AddForce(directionTotarget * _speed, ForceMode.VelocityChange);
        DeflectionProcessed = true;

    }

    private void Launch()
    {
        //_rb.linearVelocity = _rb.transform.forward * _speed;
        _rb.AddForce(_rb.transform.forward * _speed, ForceMode.VelocityChange);
    }

    private void FixedUpdate()
    {
        if (!DeflectionProcessed) { return; }

        if (_rb.linearVelocity.magnitude < _speed * 0.95f)
        {
            _rb.linearVelocity = _rb.transform.forward * _speed;
        }
    }

   


    /* public bool HasDeflectionBeenProcessed()
     {
         return _deflectionProcessed;
     }*/


}
