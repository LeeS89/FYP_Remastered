
public class Bullet : BulletBase
{
    public bool testFreeze = false;
    protected override void OnExpired()
    {
        RemoveFromJob();
        _distanceToPlayer = float.MaxValue;
        ClearState(IsAlive);
        //_isAlive = false;
        _bulletEventManager.ParticleStop(this/*, _bulletType*/);
        if (HasState(IsFrozen))
        {
            ClearState(IsFrozen);
            //_isFrozen = false;
        }
        _objectPoolManager.ReleaseGameObject(_cachedRoot);

        
    }
    public bool _testUnFreeze = false;
    protected override void Update()
    {
        base.Update();

        if (testFreeze)
        {
            Freeze();
            testFreeze = false;
        }

        if (_testUnFreeze)
        {
            UnFreeze();
            _testUnFreeze= false;
        }

    }

    public override void Initializebullet()
    {
        base.Initializebullet();
       
    }

    public override void Freeze()
    {
        if (HasState(IsFrozen)) { return; }
      
        if (BulletDistanceJob.Instance.AddFrozenBullet(this))
        {
            SetState(IsFrozen);
            //_eventManager.Freeze();
           // _timeOut = _lifespan;
            //_capsuleCollider.enabled = false;
        }
        else
        {
            ClearState(IsFrozen);
        }

        if (!HasState(IsFrozen)) { return; }
        SetState(IsFrozen);
        _bulletEventManager.Freeze();
        _timeOut = _lifespan;
    }

    public override void UnFreeze()
    {
        if (!HasState(IsFrozen)) { return; }

        RemoveFromJob();
        _bulletEventManager.UnFreeze();
    }

    protected override void RemoveFromJob()
    {
        if (BulletDistanceJob.Instance.RemoveFrozenBullet(this))
        {
            SetState(IsFrozen);


            /*if (_isCulled)
            {
                Cull(false);
            }*/
            //_isFrozen = false;
                      
            //_capsuleCollider.enabled = true;
        }
        if(!HasState(IsFrozen)) { return; }
        if (HasState(IsCulled))
        {
            Cull(false);
        }


    }
}
