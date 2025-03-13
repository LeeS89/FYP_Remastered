using UnityEngine;

public class BulletVFX : MonoBehaviour, IComponentEvents, IImpactVFX
{
    [SerializeField]
    private ParticleManager _particleManager;
    public ParticleSystem _particle;
    private PoolManager _poolManager;
    private BulletEventManager _eventManager;
    //public MoonSceneManager _manager;
    public void RegisterEvents(EventManager eventManager)
    {
        if(eventManager == null) { return; }
        _eventManager = (BulletEventManager)eventManager;
        _particleManager = ParticleManager.instance;
        _eventManager.OnBulletParticlePlay += PlayBulletParticle;
        _eventManager.OnBulletParticleStop += StopBulletParticle;
        _eventManager.OnSpawnHitParticle += SpawnHitParticle;

        InterfaceRegistry.Add<IImpactVFX>(this);
      
    }

    public void UnRegisterEvents(EventManager eventManager)
    {
        if (eventManager == null) { return; }

        _eventManager.Expired();
        _particleManager = null;
        _eventManager.OnBulletParticlePlay -= PlayBulletParticle;
        _eventManager.OnBulletParticleStop -= StopBulletParticle;
        _eventManager.OnSpawnHitParticle -= SpawnHitParticle;

        InterfaceRegistry.Remove<IImpactVFX>(this);
    }

    private void SpawnHitParticle(Vector3 pos, Quaternion rot)
    { 
        _poolManager.GetParticle(pos, rot);  
    }

    private void PlayBulletParticle(BulletBase bullet, BulletType bulletType)
    {
        _particleManager.AddBullet(bullet, bulletType);
    }

    private void StopBulletParticle(BulletBase bullet, BulletType bulletType)
    {
        _particleManager.Removebullet(bullet);
    }

    public void SetImpactParticlePool(PoolManager manager)
    {
        _poolManager = manager;
    }

   

    /* private void OnDestroy()
     {
         BulletBase bullet = GetComponentInParent<BulletBase>();
         _particleManager.Removebullet(bullet);
     }*/
}
