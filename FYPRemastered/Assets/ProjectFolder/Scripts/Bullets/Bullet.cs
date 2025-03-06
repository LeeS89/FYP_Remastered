using UnityEngine;

public class Bullet : BulletBase
{

    protected override void OnExpired()
    {
        ParticleManager.instance.Removebullet(this);
        
        //_cachedRoot.SetActive(false);
        Destroy(_cachedRoot);
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
