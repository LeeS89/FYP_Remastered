using UnityEngine;

public class BaseAbilities : ComponentEvents
{
    protected GunBase _gun;
    protected Transform _bulletSpawnLocation;
    protected Transform _targetTransform;
    protected GameObject _owner;
    protected PoolManager _bulletPoolManager;
    protected ResourceRequest _request;
    protected bool _ownerHasDied = false;
    protected bool _isReloading = false;
    [SerializeField] protected int _clipCapacity = 5;

    public bool IsReloading
    {
        get => _isReloading;
        set => _isReloading = value;
    }

    public Transform BulletSpawnLocation
    {
        get => _bulletSpawnLocation;
        set => _bulletSpawnLocation = value;
    }

    public Transform TargetTransform
    {
        get => _targetTransform;
        set => _targetTransform = value;
    }

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        _request = new ResourceRequest();
        _eventManager.OnOutOfAmmo += OutOfAmmo;
        _eventManager.OnReload += ReloadingGun;
        _eventManager.OnSetupGun += GunSetup;
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _eventManager.OnReload -= ReloadingGun;
        _eventManager.OnOutOfAmmo -= OutOfAmmo;
        _eventManager.OnSetupGun -= GunSetup;
        base.UnRegisterLocalEvents(eventManager);
    }

    public virtual void GunSetup(GameObject gunOwner, EventManager eventManager, Transform bulletSpawnLocaiton, int clipCapacity, Transform target = null)
    {
        _owner = gunOwner;
        BulletSpawnLocation = bulletSpawnLocaiton;
        TargetTransform = target;

        _request.ResourceType = PoolResourceType.NormalBulletPool;
        _request.poolRequestCallback = (pool) =>
        {
            _bulletPoolManager = pool;
            _gun = new GunBase(BulletSpawnLocation, TargetTransform, eventManager, _owner, _bulletPoolManager, clipCapacity);
            _request.ResourceType = PoolResourceType.None;
            Debug.LogError("Gun set up called");
        };
        SceneEventAggregator.Instance.RequestResource(_request);

       
    }

    protected virtual void ShootGun()
    {
        if(_gun == null || !CanShoot()) { return; }

        _gun.Shoot();
    }

    protected virtual FireConditions GetFireState()
    {
        if(_ownerHasDied) return FireConditions.OwnerDied;
        if(_isReloading) return FireConditions.Reloading;
        return FireConditions.Ready;
    }

    public bool _testShootStatus = false;

    protected virtual bool CanShoot()
    {   
        return GetFireState() == FireConditions.Ready;
    }

    protected virtual void OutOfAmmo() { }

    protected virtual void ReloadingGun(bool isReloading)
    {
        _isReloading = isReloading;
        if (_isReloading)
        {
            _gun.Reload();
        }
    }
    

    protected override void OnSceneComplete()
    {
        if (_gun != null)
        {
            _gun.OnInstanceDestroyed();
            _owner = null;
            _bulletSpawnLocation = null;
            _targetTransform = null;
            _gun = null;
        }
       
    }
}
