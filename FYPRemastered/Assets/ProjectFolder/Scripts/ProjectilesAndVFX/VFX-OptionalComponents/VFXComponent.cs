using UnityEngine;

public class VFXComponent : VFXBase
{
    [Header("Only for use on Deflectable Projectiles")]
    [SerializeField] protected PoolIdSO DeflectAudioPoolId;

    public IPoolManager DeflectAudioPoolManager { get; protected set; }

    /// <summary>
    /// This class manages bullet trail particles - Currently only one global instance but could be changed to a pool if needed
    /// </summary>
    /// Will become Obsolete
    [SerializeField] private ParticleManager _particleManager;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);

        _particleManager = ParticleManager.instance;
        _cmbtEventManager.OnCollision += FireCollisionParticle;
        _cmbtEventManager.OnDeflected += PlayDeflectionAudio;
        _cmbtEventManager.OnProjectileParticlePlay += PlayBulletParticle; // Will be obsolete
        _cmbtEventManager.OnProjectileParticleStop += StopBulletParticle; // Will be obsolete

    }


    public override void InitialzeLocalPools()
    {
        // base.InitialzeLocalPools();
        if (CollisionParticlePoolId != null && !string.IsNullOrEmpty(CollisionParticlePoolId.Id))
            this.RequestPool(CollisionParticlePoolId, PoolRequestCallback);
        if (DeflectAudioPoolId != null && !string.IsNullOrEmpty(DeflectAudioPoolId.Id))
            this.RequestPool(DeflectAudioPoolId, PoolRequestCallback);
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
         base.UnRegisterLocalEvents(_cmbtEventManager);
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

        if (DeflectAudioPoolId != null && poolId == DeflectAudioPoolId.Id) DeflectAudioPoolManager = pool;
        if (CollisionParticlePoolId != null && poolId == CollisionParticlePoolId.Id) CollisionParticlePoolManager = pool;
    }

    private void PlayDeflectionAudio(ProjectileKickType type)
    {
        // Different SFX for different deflection types
        if (DeflectAudioPoolManager == null) return;
        SpawnPooledObject(DeflectAudioPoolManager, transform.position, transform.rotation);
       // var sfx = DeflectAudioPoolManager.GetFromPool(transform.position, transform.rotation) as AudioSource;
       // sfx.Play();

    }

    // Will be obsolete
    private void PlayBulletParticle(ProjectileBase bullet/*, BulletType bulletType*/)
    {

        if (_particleManager == null) { return; }

        _particleManager.AddProjectile(bullet/*, bulletType*/);
    }

    // Will be Obsolete
    private void StopBulletParticle(ProjectileBase bullet/*, BulletType bulletType*/)
    {

        if (_particleManager == null) { return; }
        _particleManager.RemoveProjectile(bullet);
    }
}
