using System;
using UnityEngine;

[Tooltip("Optional Class - Used for projectile Collision VFX & SFX")]
public class ProjectileFXBase : ComponentEvents
{
    [Header("ID's of the SFX & VFX Pools to be used on Collision")]
    [SerializeField] protected PoolIdSO HitParticlePoolId;

    

    [Header("Pool References")]
    public IPoolManager CollisionParticlePoolManager { get; protected set; }


    [Header("Called when a request for a pool is completed")]
    protected Action<PoolIdSO, IPoolManager> PoolRequestCallback;

    protected ProjectileEventManager _projectileEventManager;

    

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _projectileEventManager = eventManager as ProjectileEventManager;
        PoolRequestCallback = OnPoolReceived;
        _projectileEventManager.OnCollision += SpawnHitParticle;
    }


    public override void InitialzeLocalPools()
    {
        if (HitParticlePoolId == null) return;
        this.RequestPool(HitParticlePoolId, PoolRequestCallback);
    }


    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _projectileEventManager.Expired();

        _projectileEventManager.OnCollision -= SpawnHitParticle;
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();

        _projectileEventManager = null;
        PoolRequestCallback = null;
    }

    protected virtual void OnPoolReceived(PoolIdSO poolId, IPoolManager pool)
    {
        if (poolId == null) return;

        if (poolId == HitParticlePoolId) CollisionParticlePoolManager = pool;
    }

    protected virtual void SpawnHitParticle(Collision collision)
    {
        if (CollisionParticlePoolManager == null) return;

        ContactPoint contact = collision.GetContact(0);
       
        Vector3 pos = contact.point;
       
        var hit = CollisionParticlePoolManager.GetFromPool(pos, Quaternion.identity) as ParticleSystem;
        hit.Play();

    }

   

  

}
