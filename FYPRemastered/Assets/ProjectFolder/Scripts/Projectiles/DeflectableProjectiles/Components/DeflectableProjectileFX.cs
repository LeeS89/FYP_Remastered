using UnityEngine;

public sealed class DeflectableProjectileFX : ProjectileFXBase
{
    /// <summary>
    /// This class manages bullet trail particles - Currently only one global instance but could be changed to a pool if needed
    /// </summary>
    [SerializeField] private ParticleManager _particleManager;
   
    public IPoolManager DeflectAudioPoolManager { get; private set; }
   
  

    [Header("Only for use on Deflectable Projectiles")]
    [SerializeField] private PoolIdSO DeflectAudioPoolId;
  
   

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
       
        _particleManager = ParticleManager.instance;
        _projectileEventManager.OnDeflected += PlayDeflectionAudio;
        _projectileEventManager.OnProjectileParticlePlay += PlayBulletParticle;
        _projectileEventManager.OnProjectileParticleStop += StopBulletParticle;
      
    }


    public override void InitialzeLocalPools()
    {
        base.InitialzeLocalPools();

        if (DeflectAudioPoolId == null) return;
        this.RequestPool(DeflectAudioPoolId, PoolRequestCallback);
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(eventManager);
        _projectileEventManager.OnDeflected -= PlayDeflectionAudio;
        _projectileEventManager.OnProjectileParticlePlay -= PlayBulletParticle;
        _projectileEventManager.OnProjectileParticleStop -= StopBulletParticle;
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        _particleManager = null;
    }

    protected override void OnPoolReceived(string poolId, IPoolManager pool)
    {
        if (poolId == null) return;

        base.OnPoolReceived(poolId, pool);

        if (poolId == DeflectAudioPoolId.Id) DeflectAudioPoolManager = pool;

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
