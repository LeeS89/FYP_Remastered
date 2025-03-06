using UnityEngine;

public class BulletVFX : MonoBehaviour, IBulletEvents
{
    [SerializeField]
    private ParticleManager _particleManager;

    public void RegisterEvents(BulletEventManager eventManager)
    {
        if(eventManager == null) { return; }

        _particleManager = ParticleManager.instance;
        eventManager.OnParticlePlay += PlayBulletParticle;
    }

    private void PlayBulletParticle(BulletBase bullet, BulletType bulletType)
    {
        _particleManager.AddBullet(bullet, bulletType);
    }
}
