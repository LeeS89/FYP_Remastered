using UnityEngine;

public class WeaponHandlerBase
{
    protected IWeapon _equippedWeapon;
    protected RangedWeaponBase _rangedWeapon;
    protected GameObject _owner;
    protected Transform _bulletSpawnPoint;
    protected int _clipCapacity;
    protected FireRate _fireRate;

    public WeaponHandlerBase(GameObject owner)
    {
        _owner = owner;
    }

    #region Firing conditions region

    public virtual bool IsOwnerDead { get; protected set; }
    public virtual bool IsReloading { get; protected set; }

    protected virtual void ReloadStateChanged(bool isReloading) => IsReloading = isReloading;

    protected virtual void OwnerDied(bool ownerDead) => IsOwnerDead = ownerDead;

    protected virtual FireConditions GetFireState()
    {
        if (IsOwnerDead) return FireConditions.OwnerDied;
        if (IsReloading) return FireConditions.Reloading;
        return FireConditions.Ready;
    }

    protected virtual bool CanFire()
    {
        return GetFireState() == FireConditions.Ready;
    }


    protected virtual void OutOfAmmo() { }
    #endregion


    #region Fire weapon region
    public virtual void FireRangedweapon() { }

    protected virtual void SetFireRate(FireRate fireRate) { _fireRate = fireRate; }

    public virtual void TryFireRangedWeapon() { }
    #endregion

    #region Ranged weapon params
    public virtual void SetRangedAmmoType(AmmoType type)
    {
        if (_equippedWeapon is IRangedWeapon rw)
        {
            rw.SetAmmoType(type);
        }
    }

    public virtual void SetBulletSpawnPoint(Transform bulletSpawnPoint) { _bulletSpawnPoint = bulletSpawnPoint; }
    #endregion

    #region Weapon Equip region

    public virtual void EquipWeapon(WeaponType type)
    {
        _equippedWeapon?.UnEquip();
        //if (_equippedWeapon != null) { _equippedWeapon.UnEquip(); }

        switch (type)
        {
            case WeaponType.Ranged:
                _equippedWeapon = _rangedWeapon;
                break;
            default:
                Debug.LogWarning("No Weapon Type selected");
                break;
        }

        _equippedWeapon?.Equip();
    }

    // Not yet used
    public virtual void EquipWeapon(GameObject owner, EnemyEventManager eventManager, Transform bulletSpawnLocaiton, int clipCapacity, Transform target = null) { }
    #endregion

    public virtual void OnInstanceDestroyed()
    {
        _equippedWeapon?.UnEquip();
        _equippedWeapon?.OnInstanceDestroyed();
        _equippedWeapon = null;
        _rangedWeapon = null;
        _bulletSpawnPoint = null;

        _owner = null;
    }
}
