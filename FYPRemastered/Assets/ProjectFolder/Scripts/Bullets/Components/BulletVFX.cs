using UnityEngine;

public class BulletVFX : MonoBehaviour, IBulletEvents
{
    [SerializeField]
    private ParticleManager _particleManager;

    public void RegisterEvents(BulletEventManager eventManager)
    {
        if(eventManager == null) { return; }

        _particleManager = ParticleManager.instance;
        eventManager.OnBulletParticlePlay += PlayBulletParticle;
        eventManager.OnBulletParticleStop += StopBulletParticle;
        eventManager.OnSpawnHitParticle += SpawnHitParticle;
    }

   
    private void SpawnHitParticle(Vector3 pos, Quaternion rot)
    {
        ParticlePool.SpawnHitParticle(pos, rot);
    }

    private void PlayBulletParticle(BulletBase bullet, BulletType bulletType)
    {
        _particleManager.AddBullet(bullet, bulletType);
    }

    private void StopBulletParticle(BulletBase bullet, BulletType bulletType)
    {
        _particleManager.Removebullet(bullet);
    }

    /* private void OnDestroy()
     {
         BulletBase bullet = GetComponentInParent<BulletBase>();
         _particleManager.Removebullet(bullet);
     }*/
}
