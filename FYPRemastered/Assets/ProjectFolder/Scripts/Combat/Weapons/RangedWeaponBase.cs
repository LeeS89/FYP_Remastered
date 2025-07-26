using UnityEngine;

public abstract class RangedWeaponBase : IRangedWeapon
{
    protected Transform _bulletSpawnPoint;
   // protected EventManager _eventManager;
    protected FireRate _fireRate;
    protected ResourceRequest _request;
    protected int _clipCapacity;
    protected PoolManager _bulletPoolManager;
    protected GameObject _gunOwner;
    protected int _clipCount;



    public abstract void Reload();
    public virtual void SetAmmoType(AmmoType type)
    {
        GetPoolManager(type);
    }

    protected virtual void SetBulletPool(PoolManager poolManager)
    {
       _bulletPoolManager = poolManager;
    }

    protected virtual void GetPoolManager(AmmoType type)
    {
        if (_request != null)
        {
            switch (type)
            {
                case AmmoType.NormalGun:
                    _request.ResourceType = PoolResourceType.NormalBulletPool;
                    break;
                default:
                    break;
            }

            _request.poolRequestCallback = SetBulletPool;

            SceneEventAggregator.Instance.RequestResource(_request);
        }
    }

    public virtual bool IsEquipped { get; protected set; }

    public virtual void Beginfire(MonoBehaviour runner) { }
    public virtual bool CanFire() { return GetFireState() == FireConditions.Ready; }
    public virtual FireConditions GetFireState()
    {
        if (IsReloading) return FireConditions.Reloading;
        return FireConditions.Ready;
    }
    public virtual void EndFire(MonoBehaviour runner) { }
    public virtual void Fire() { }

    public virtual void Equip() => IsEquipped = true;
    public virtual void Unequip() => IsEquipped = false;
    public virtual void UpdateWeapon() { }




    public virtual void SetBulletSpawnPoint(Transform bulletSpawnPoint) { _bulletSpawnPoint = bulletSpawnPoint; }
    public virtual void SetFireRate(FireRate fireRate) { _fireRate = fireRate; }

    public virtual void InjectFireRateOffsets(float[] offsets) { }

    public virtual void Initialize(EventManager eventManager, Transform spawnPoint, FireRate fireRate, int clipCapacity) { }

    public virtual void OnInstanceDestroyed()
    {
        //_eventManager = null;
        _bulletSpawnPoint = null;
        _request = null;
        _gunOwner = null;
        _bulletPoolManager = null;
    }

    public virtual bool IsReloading { get; protected set; }
    protected virtual void ReloadStateChanged(bool isReloading)
    {
        IsReloading = isReloading;
    }

}
