using UnityEngine;
using UnityEngine.UIElements;

public class Bullet : BulletBase
{

    protected override void OnExpired()
    {
        _isAlive = false;
        _eventManager.ParticleStop(this, _bulletType);
        _objectPoolManager.ReleaseGameObject(_cachedRoot);
    }

    public override void Initializebullet()
    {
        base.Initializebullet();
       
    }

    public override void Freeze()
    {
        _isAlive = false;
    }

    public override void UnFreeze()
    {
        _isAlive = true;
    }
}
