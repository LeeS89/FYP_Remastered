using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class BulletCollisionComponent : ComponentEvents, IDeflectable
{
    [Header("Deflection Speed")]
    [SerializeField] private float _deflectSpeed;

    [Header("Game Object Collider")]
    [SerializeField] private CapsuleCollider _collider;

    [Header("Layer to Ignore after deflection")]
    [SerializeField] private LayerMask _ignoreMask;

    [Header("Damage Data")]
    [SerializeField] private DamageData _damageData;
    private float _baseDamage;
    private DamageType _damageType;
    private float _damageOverTime;
    private float _dOTDuration;
    [SerializeField] private float _statusEffectChancePercentage;

    private BulletEventManager _bulletEventManager; 
    private GameObject _parentOwner;
    private GameObject _rootComponent;
    
   
   
    public GameObject ParentOwner
    {
        get => _parentOwner;
        set
        {
            if (value != null)
            {
                _parentOwner = value;
            }
        }
    }

    public GameObject RootComponent
    {
        get => _rootComponent;
        set
        {
            _rootComponent = value;
        }
    }

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        if (_eventManager == null) { return; }
        _bulletEventManager = _eventManager as BulletEventManager;

        _bulletEventManager.OnUnFreeze += Deflect;
        InitializeDamageType();
    }

    private void InitializeDamageType()
    {
        if(_damageData == null) { return; }
        _damageType = _damageData.damageType;
        _baseDamage = _damageData.baseDamage;
        _damageOverTime = _damageData.damageOverTime;
        _dOTDuration = _damageData.duration;
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _bulletEventManager.OnUnFreeze -= Deflect;
        base.UnRegisterLocalEvents(eventManager);
        _bulletEventManager = null;
    }


    private void OnDisable()
    {
        _collider.excludeLayers = 0;
    }

    public void Deflect()
    {
        Vector3 directionTotarget = TargetingUtility.GetDirectionToTarget(_parentOwner, _rootComponent, true);
        Quaternion newRotation = Quaternion.LookRotation(directionTotarget);
       
        _bulletEventManager.Deflected(directionTotarget, newRotation, _deflectSpeed);
    }


    public bool _testDeflect = false;
    private void Update()
    {
        if (_testDeflect)
        {
            Deflect();
            _testDeflect = false;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
       
        ContactPoint contact = collision.contacts[0];
        Vector3 impactPosition = contact.point;
        
        _bulletEventManager.SpawnHitParticle(impactPosition, Quaternion.identity);

        if ((_ignoreMask & (1 << collision.gameObject.layer)) != 0)
        {
            _collider.excludeLayers = _ignoreMask;
            
            return; 
        }

        CheckForDamageableInterface(collision);
        
        
        //_parentOwner = null;
        _bulletEventManager.Expired();
        
    }

    private void CheckForDamageableInterface(Collision collision)
    {
        IDamageable damageable = null;
        if (!collision.gameObject.TryGetComponent<IDamageable>(out damageable))
        {
            damageable = collision.gameObject.GetComponentInParent<IDamageable>() ??
                         collision.gameObject.GetComponentInChildren<IDamageable>();
        }

        if (damageable == null) { return; }

        
        damageable.TakeDamage(_baseDamage, _damageType, _statusEffectChancePercentage, _damageOverTime, _dOTDuration);

    }

   
}
