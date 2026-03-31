using UnityEngine;

public sealed class DeflectableProjectileFXObsolete : VFXBase//ProjectileFXBase
{
    /// <summary>
    /// This class manages bullet trail particles - Currently only one global instance but could be changed to a pool if needed
    /// </summary>
    [SerializeField] private ParticleManager _particleManager;

    [SerializeField] protected PoolIdSO DeflectAudioPoolId;
    public IPoolManager DeflectAudioPoolManager { get; protected set; }
    public override void RegisterLocalEvents(EventManagerObsolete eventManager)
    {
        base.RegisterLocalEvents(eventManager);

        
        _particleManager = ParticleManager.instance;
        _cmbtEventManager.OnCollision += FireCollisionParticle;
        _cmbtEventManager.OnDeflected += PlayDeflectionAudio;
        _cmbtEventManager.OnProjectileParticlePlay += PlayBulletParticle;
        _cmbtEventManager.OnProjectileParticleStop += StopBulletParticle;
      
    }


    public override void InitialzeLocalPools()
    {
        // base.InitialzeLocalPools();
        if (CollisionParticlePoolId != null && !string.IsNullOrEmpty(CollisionParticlePoolId.Id))
            this.RequestPool(CollisionParticlePoolId, PoolRequestCallback);
        if (DeflectAudioPoolId != null && !string.IsNullOrEmpty(DeflectAudioPoolId.Id)) 
            this.RequestPool(DeflectAudioPoolId, PoolRequestCallback);
    }

    public override void UnRegisterLocalEvents(EventManagerObsolete eventManager)
    {
      //  base.UnRegisterLocalEvents(eventManager);
        //_cmbtEventManager.Expired();
        _cmbtEventManager.OnCollision -= FireCollisionParticle;
        _cmbtEventManager.OnDeflected -= PlayDeflectionAudio;
        _cmbtEventManager.OnProjectileParticlePlay -= PlayBulletParticle;
        _cmbtEventManager.OnProjectileParticleStop -= StopBulletParticle;
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        _particleManager = null;
    }

    protected override void OnPoolReceived(string poolId, IPoolManager pool)
    {
        if (string.IsNullOrEmpty(poolId) || pool == null) return;
        //base.OnPoolReceived(poolId, pool);

        if (poolId == DeflectAudioPoolId.Id) DeflectAudioPoolManager = pool;
        if (poolId == CollisionParticlePoolId.Id) CollisionParticlePoolManager = pool;
    }

    private void PlayDeflectionAudio(ProjectileKickType type)
    {
        // Different SFX for different deflection types
        if (DeflectAudioPoolManager == null) return;
        var sfx = DeflectAudioPoolManager.GetFromPool(transform.position, transform.rotation) as AudioSource;
        sfx.Play();
       
    }
   

    private void PlayBulletParticle(ProjectileBase bullet/*, BulletType bulletType*/)
    {
  
        if (_particleManager == null) { return; }    

        _particleManager.AddProjectile(bullet/*, bulletType*/);
    }

    private void StopBulletParticle(ProjectileBase bullet/*, BulletType bulletType*/)
    {
 
        if (_particleManager == null) { return; }
        _particleManager.RemoveProjectile(bullet);
    }

}
