using UnityEngine;

public abstract class RangedWeaponBase : IRangedWeapon
{
    protected Transform _bulletSpawnPoint;
   // protected EventManager _eventManager;
   // protected FireRate _fireRate;
    protected ResourceRequest _request;
    protected int _clipCapacity;
    protected PoolManager _bulletPoolManager;
    protected GameObject _gunOwner;
    protected int _clipCount;



    public abstract void Reload();

    #region Ammo Region
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
                case AmmoType.Normal:
                    _request.ResourceType = PoolResourceType.NormalBulletPool;
                    break;
                default:
                    break;
            }

            _request.poolRequestCallback = SetBulletPool;

            SceneEventAggregator.Instance.RequestResource(_request);
        }
    }

  //  protected virtual void OutOfAmmo() { }

    #endregion

    public virtual bool IsEquipped { get; protected set; } // Possibly redundant


    #region Firing Region
   
    public virtual void Fire(Vector3? directionOverride = null)
    {
        if (_clipCount > 0)
        {

            _clipCount--;

            Vector3 direction = directionOverride ?? _bulletSpawnPoint.forward; // fallback direction
            Quaternion bulletRotation = Quaternion.LookRotation(direction);
            
            GameObject obj = _bulletPoolManager.GetFromPool(_bulletSpawnPoint.position, bulletRotation);

            BulletBase bullet = obj.GetComponentInChildren<BulletBase>();

            bullet.InitializeBullet(_gunOwner);
        }

        if (_clipCount == 0)
        {
            NotifyReload();
           // OutOfAmmo();
        }
    }

    #endregion

    public virtual void Equip() => IsEquipped = true;
    public virtual void UnEquip() => IsEquipped = false;
    public virtual void UpdateWeapon() { }




    public virtual void SetBulletSpawnPoint(Transform bulletSpawnPoint) { _bulletSpawnPoint = bulletSpawnPoint; }
   


    public virtual void Initialize(EventManager eventManager, Transform spawnPoint, AmmoType type, FireRate fireRate, int clipCapacity) { }

    public virtual void OnInstanceDestroyed()
    {
        _bulletSpawnPoint = null;
        _request = null;
        _gunOwner = null;
        _bulletPoolManager = null;
    }

   

    protected virtual void NotifyReload() { }

}
