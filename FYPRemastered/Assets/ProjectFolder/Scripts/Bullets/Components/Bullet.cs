
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Bullet : BulletBase
{
    public bool testFreeze = false;

   

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        ComponentRegistry.Register<IPoolable>(gameObject, this);
    }

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
        _objectPoolManager.Release(gameObject);

        
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

    public override void InitializePoolable(GameObject bulletOwner)
    {
        base.InitializePoolable(bulletOwner);
       
    }

    public override void Freeze()
    {
        if (HasState(IsFrozen)) { return; }
      
        if (BulletDistanceJob.Instance.AddFrozenBullet(this))
        {
            SetState(IsFrozen);
         
            //_capsuleCollider.enabled = false;
        }
       /* else
        {
            ClearState(IsFrozen);
        }

        if (!HasState(IsFrozen)) { return; }*/
        //SetState(IsFrozen);
        _bulletEventManager.Freeze();
        _timeOut = _lifespan;
    }

    public override void UnFreeze() // => Move to IDeflectable interface & Change to ReverseDirection() 
    {
        if (!HasState(IsFrozen)) { return; }

        RemoveFromJob();
        //_bulletEventManager.ReverseDirection();
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
