using UnityEngine;

[RequireComponent(typeof(DeflectableCollisionComponent))]
public sealed class Bullet : Projectile, IFreezeAndDeflectable
{
    [Header("Deflection Speed")]
    [SerializeField] private float _deflectSpeed;

    public bool testFreeze = false;
    [SerializeField] private Animator _anim;
    [SerializeField] private float _cullDistance = 0.5f;

 
    private GameObject _componentRegistryTargetObj;


    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        _projectileEventManager.OnGetDirectionToTarget += GetDirectionToOwnerOnDeflect;

    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(eventManager);
        ComponentRegistry.Unregister<IFreezeAndDeflectable>(_componentRegistryTargetObj);
        _projectileEventManager.OnGetDirectionToTarget -= GetDirectionToOwnerOnDeflect;
    }

    protected override void CreateDefaultColliderIfNoneExists(GameObject target, bool exists)
    {
        _componentRegistryTargetObj = target;
        if (!exists)
        {
            _componentRegistryTargetObj.AddComponent<CapsuleCollider>();
        }
        ComponentRegistry.Register<IFreezeAndDeflectable>(_componentRegistryTargetObj, this);
    }

    protected override void AttachMovementHandler()
        => _movementHandler = new BulletMovementHandler(_projectileEventManager, GetComponent<Rigidbody>(), _projectileSpeed, _deflectSpeed);


    protected override void OnExpired()
    {
        RemoveFromJob();
        _distanceToPlayer = float.MaxValue;
        ClearState(IsActive);
      
        _projectileEventManager.ParticleStop(this/*, _bulletType*/);
        if (HasState(IsFrozen))
        {
            ClearState(IsFrozen);
   
        }
        base.OnExpired();
        
    }

    public bool _testUnFreeze = false;
    protected override void Update()
    {
        if (!HasState(IsFrozen))
        {
            base.Update();
        }

#if UNITY_EDITOR
        if (testFreeze)
        {
            Freeze();
            testFreeze = false;
        }

        if (_testUnFreeze)
        {
           // UnFreeze();
            _testUnFreeze= false;
        }
#endif
    }

    public override void SetDistanceToPlayer(float distance)
    {
        if (!HasState(IsFrozen)) { return; }
        base.SetDistanceToPlayer(distance);

        if (_distanceToPlayer <= _cullDistance)
        {
            if (!HasState(IsCulled))
            {
                Cull(true);
            }
        }
        else
        {
            if (HasState(IsCulled))
            {
                Cull();
            }
        }
    }

    private void Cull(bool cull = false)
    {
        if (cull)
        {
            _anim.SetTrigger("minimize");
            _projectileEventManager.ParticleStop(this/*, _bulletType*/);
            SetState(IsCulled);
         
        }
        else
        {
            _anim.SetTrigger("maximize");
            _projectileEventManager.ParticlePlay(this/*, _bulletType*/);
            ClearState(IsCulled);
 
        }
    }

    public override void LaunchPoolable(GameObject bulletOwner)
    {
        base.LaunchPoolable(bulletOwner);
        _projectileEventManager.ParticlePlay(this);

    }

    public void Freeze()
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
        _projectileEventManager.Freeze();
        _timeOut = _lifespan;
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

    public void Deflect(ProjectileKickType type)
    {
        if(type == ProjectileKickType.ReFire)
        {
            if (!HasState(IsFrozen)) { return; }
            RemoveFromJob();
        }
        _projectileEventManager.Deflected(type);
    }

}
