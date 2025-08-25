using Unity.VisualScripting;
using UnityEngine;



public class BulletCollisionComponent : ProjectileCollisionComponent
{

    [Header("Game Object Collider")]
    [SerializeField] private CapsuleCollider _collider;

    [Header("Layer to Ignore after deflection - In order to prevent accidental double firing")]
    [SerializeField] private LayerMask _ignoreMask;

    

    protected override void InitializeDamageType()
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
        _projectileEventManager = null;
    }


    private void OnDisable()
    {
        _collider.excludeLayers = 0;
    }

  
   
    public bool _testDeflect = false;
    private void Update()
    {
        if (_testDeflect)
        {
           // Deflect();
            _testDeflect = false;
        }
    }


    protected override void OnCollisionEnter(Collision collision)
    {
        /*foreach (ContactPoint contacts in collision.contacts)
        {
            // This is the collider on the Rigidbody’s GameObject hierarchy that was hit
            Collider hitCollider = contacts.thisCollider;

            // Grab the GameObject (which could be a child)
            GameObject hitObject = hitCollider.gameObject;

            Debug.Log($"Collider on '{hitObject.name}' was hit at {contacts.point}");

            // Optionally notify a script on the child
            var handler = hitObject.GetComponent<IChildCollisionHandler>();
            if (handler != null)
                handler.OnChildCollision(collision, contacts);
        }*/

        base.OnCollisionEnter(collision);
       
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
        
        _projectileEventManager.Collision(collision);
        //_bulletEventManager.SpawnHitParticle(impactPosition, Quaternion.identity);




        _projectileEventManager.Expired();

    }

    

    private void CheckForDamageableInterface(Collision collision, ContactPoint cPoint, Vector3 hitPoint, Vector3 hitNormal)
    {
       // Vector3 spawnPoint = hitPoint;

        if (ComponentRegistry.TryGet<IDamageable>(collision.gameObject, out var damageable))
        {
            damageable.NotifyDamage(_baseDamage, _damageType, _statusEffectChancePercentage, _damageOverTime, _dOTDuration);
        }


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
