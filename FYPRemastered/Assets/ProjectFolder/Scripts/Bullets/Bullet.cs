public class Bullet : BulletBase
{

    private void OnDisable()
    {
        //BulletManagers.instance.Removebullet(this);
    }

    protected override void OnExpired()
    {
        BulletManagers.instance.Removebullet(this);
        Destroy(_cachedRoot);
    }

    /*public override void Initializebullet(Vector3 position, Quaternion rotation)
    {
        base.Initializebullet(position, rotation);
    }*/

    public override void Freeze()
    {
        throw new System.NotImplementedException();
    }

    public override void UnFreeze()
    {
        throw new System.NotImplementedException();
    }
}
