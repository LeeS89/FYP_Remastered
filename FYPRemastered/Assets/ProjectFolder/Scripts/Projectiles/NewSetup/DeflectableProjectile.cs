using UnityEngine;

public sealed class DeflectableProjectile : ProjectileBase, IFreezeAndDeflectable
{
    [Header("Deflection Speed")]
    [SerializeField] private float _deflectSpeed;

    public bool testFreeze = false;
    [SerializeField] private Animator _anim;
    [SerializeField] private float _cullDistance = 0.5f;


    private GameObject _componentRegistryTargetObj;
    public static readonly byte IsFrozen = 1 << 1;
    private const byte IsCulled = 1 << 2;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        _projectileEventManager.OnGetDirectionToTarget += GetDirectionToOwnerOnDeflect;
        ComponentRegistry.Register<IFreezeAndDeflectable>(_componentRegistryTargetObj, this);

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
        => _movementHandler = new DeflectableProjectileMovementManager(_projectileEventManager, GetComponent<Rigidbody>(), _projectileSpeed, _movementType, _deflectSpeed, _acceleration, _maxSpeed, _useGravity) as DeflectableProjectileMovementManager;


    private Vector3 GetDirectionToOwnerOnDeflect()
    {
        Vector3 directionTotarget = TargetingUtility.GetDirectionToTarget(Owner, gameObject, true);
        return directionTotarget;
    }

    protected override void OnExpired()
    {
        RemoveFromJob();
        _distanceToPlayer = float.MaxValue;
        

        _projectileEventManager.ParticleEnd(this/*, _bulletType*/);
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
            _testUnFreeze = false;
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
            _projectileEventManager.ParticleEnd(this/*, _bulletType*/);
            SetState(IsCulled);

        }
        else
        {
            _anim.SetTrigger("maximize");
            _projectileEventManager.ParticleBegin(this/*, _bulletType*/);
            ClearState(IsCulled);

        }
    }

    public override void LaunchPoolable(GameObject bulletOwner)
    {
        base.LaunchPoolable(bulletOwner);
        _projectileEventManager.ParticleBegin(this);

    }

    public void Freeze()
    {

        if (HasState(IsFrozen)) { return; }

        if (BulletDistanceJob.Instance.AddFrozenProjectile(this))
        {
            SetState(IsFrozen);

            
        }
      
        _projectileEventManager.Freeze();
        _timeOut = _lifespan;
    }



    protected override void RemoveFromJob()
    {
        if (BulletDistanceJob.Instance.RemoveFrozenProjectile(this))
        {
            SetState(IsFrozen);

        }
        if (!HasState(IsFrozen)) { return; }
        if (HasState(IsCulled))
        {
            Cull(false);
        }


    }

    public void Deflect(ProjectileKickType type)
    {
        if (type == ProjectileKickType.ReFire)
        {
            if (!HasState(IsFrozen)) { return; }
            RemoveFromJob();
        }
        _projectileEventManager.Deflected(type);
    }




}
