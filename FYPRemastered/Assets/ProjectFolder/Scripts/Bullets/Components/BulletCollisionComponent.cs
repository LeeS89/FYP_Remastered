using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class BulletCollisionComponent : ComponentEvents, IDeflectable
{
   /* [Header("Deflection Speed")]
    [SerializeField] private float _deflectSpeed;*/

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
    

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        if (eventManager == null) { return; }
        _bulletEventManager = eventManager as BulletEventManager;

        //_bulletEventManager.OnReverseDirection += Deflect;
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


    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        _bulletEventManager = null;
    }


    private void OnDisable()
    {
        _collider.excludeLayers = 0;
    }

    public void Deflect()
    {
        _bulletEventManager.Deflected();
        FireBack();
       
        /* Vector3 directionTotarget = _bulletEventManager.GetDirectionToTarget();

         Quaternion newRotation = Quaternion.LookRotation(directionTotarget);

         _bulletEventManager.Deflected(directionTotarget, newRotation, _deflectSpeed);*/
    }

    public void FireBack()
    {
        /*Vector3 directionTotarget = _bulletEventManager.GetDirectionToTarget();

        Quaternion newRotation = Quaternion.LookRotation(directionTotarget);*/
        _bulletEventManager.ReverseDirection();
        //_bulletEventManager.Deflected(directionTotarget, newRotation, _deflectSpeed);
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
        
        ContactPoint contact = collision.GetContact(0);
         //collision.GetContact(0, out contact);
        // ContactPoint contact = collision.contacts[0];
        Vector3 impactPosition = contact.point;
        Vector3 hitNormal = contact.normal;

        // _bulletEventManager.SpawnHitParticle(impactPosition, Quaternion.identity);

        if ((_ignoreMask & (1 << collision.gameObject.layer)) != 0)
        {
            _collider.excludeLayers = _ignoreMask;

            return;
        }

        CheckForDamageableInterface(collision, contact, impactPosition, hitNormal);
        
        _bulletEventManager.Collision(collision);
        //_bulletEventManager.SpawnHitParticle(impactPosition, Quaternion.identity);




        _bulletEventManager.Expired();

    }

    private void CheckForDamageableInterface(Collision collision, ContactPoint cPoint, Vector3 hitPoint, Vector3 hitNormal)
    {
        Vector3 spawnPoint = hitPoint;

       /* if(ComponentRegistry.TryGetDamageable(collision.gameObject, out var damageable))
        {
            damageable.NotifyDamage(_baseDamage, _damageType, _statusEffectChancePercentage, _damageOverTime, _dOTDuration);
        }*/

        
      /*  if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
           *//* if(damageable is IEnemyDamageable enemyDamageable)
            {
                spawnPoint = enemyDamageable.GetAdjustedHitPoint(hitPoint, hitNormal);
            }*//*

            damageable.NotifyDamage(_baseDamage, _damageType, _statusEffectChancePercentage, _damageOverTime, _dOTDuration);
            *//* damageable = collision.gameObject.GetComponentInParent<IDamageable>() ??
                          collision.gameObject.GetComponentInChildren<IDamageable>();*//*

           // _bulletEventManager.SpawnHitParticle(spawnPoint, Quaternion.identity);
            //return true;
        }*/
       

        //if (damageable == null) { return; }

       // return false;
        

        //return;

        /*if (damageable is IEnemyDamageable enemy)
        {
            Vector3 impactPoint = collision.contacts[0].point;
            Collider hitCollider = collision.collider;
            enemy.HandleHit(hitCollider, impactPoint);
        }*/

    }

   
}
