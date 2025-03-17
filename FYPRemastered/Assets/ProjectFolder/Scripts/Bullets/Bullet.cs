
public class Bullet : BulletBase
{
    public bool testFreeze = false;
    protected override void OnExpired()
    {
        RemoveFromJob();
        _distanceToPlayer = float.MaxValue;
        _isAlive = false;
        _eventManager.ParticleStop(this, _bulletType);
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
        if (_isFrozen) { return; }
      
        if (_isFrozen = BulletDistanceJob.Instance.AddFrozenBullet(this))
        {
            _eventManager.Freeze();
            _timeOut = _lifespan;
            _capsuleCollider.enabled = false;
        }
       
    }

    public override void UnFreeze()
    {
        if (!_isFrozen) { return; }

        RemoveFromJob();
    }

    protected override void RemoveFromJob()
    {
        if (_isFrozen = BulletDistanceJob.Instance.RemoveFrozenBullet(this))
        {
            if (_isCulled)
            {
                Cull(false);
            }
            _isFrozen = false;
                      
            _capsuleCollider.enabled = true;
        }

    }
}
