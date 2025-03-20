using UnityEngine;

public class BulletVFX : ComponentEvents
{
    [SerializeField]
    private ParticleManager _particleManager;
    public ParticleSystem _particle;
    private PoolManager _poolManager;
    private BulletEventManager _bulletEventManager;

    //public MoonSceneManager _manager;
    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        if (_eventManager == null) { return; }
        _bulletEventManager = (BulletEventManager)_eventManager;
        
        _particleManager = ParticleManager.instance;
        _bulletEventManager.OnBulletParticlePlay += PlayBulletParticle;
        _bulletEventManager.OnBulletParticleStop += StopBulletParticle;
        _bulletEventManager.OnSpawnHitParticle += SpawnHitParticle;

        BaseSceneManager._instance.GetImpactParticlePool(ref _poolManager);
       
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        if (_eventManager == null) { return; }

        _bulletEventManager.Expired();
        _particleManager = null;
        _bulletEventManager.OnBulletParticlePlay -= PlayBulletParticle;
        _bulletEventManager.OnBulletParticleStop -= StopBulletParticle;
        _bulletEventManager.OnSpawnHitParticle -= SpawnHitParticle;
        base.UnRegisterLocalEvents(eventManager);
        _bulletEventManager = null;

        
    }

   

    private void SpawnHitParticle(Vector3 pos, Quaternion rot)
    { 
        _poolManager.GetParticle(pos, rot);  
    }

    private void PlayBulletParticle(BulletBase bullet/*, BulletType bulletType*/)
    {
        _particleManager.AddBullet(bullet/*, bulletType*/);
    }

    private void StopBulletParticle(BulletBase bullet/*, BulletType bulletType*/)
    {
        _particleManager.Removebullet(bullet);
    }

    /* private void OnDestroy()
     {
         BulletBase bullet = GetComponentInParent<BulletBase>();
         _particleManager.Removebullet(bullet);
     }*/
}
