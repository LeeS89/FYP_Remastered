using UnityEngine;

public class Bullet : BulletBase
{

    protected override void OnExpired()
    {
        _eventManager.ParticleStop(this, _bulletType);
        _cachedRoot.SetActive(false);
    }

    public override void Initializebullet()
    {
        base.Initializebullet();
       
    }

    public override void Freeze()
    {
        throw new System.NotImplementedException();
    }

    public override void UnFreeze()
    {
        throw new System.NotImplementedException();
    }
}
