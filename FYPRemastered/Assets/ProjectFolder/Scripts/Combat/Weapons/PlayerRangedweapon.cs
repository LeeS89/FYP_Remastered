using UnityEngine;

public abstract class PlayerRangedweapon : IRangedWeapon
{
    public virtual void Beginfire(MonoBehaviour runner) { }
    

    public virtual bool CanFire()
    {
        throw new System.NotImplementedException();
    }

    public virtual void EndFire(MonoBehaviour runner) { }
    

    public virtual void Equip(EventManager eventManager)
    {
        throw new System.NotImplementedException();
    }

    public void Equip()
    {
        throw new System.NotImplementedException();
    }

    public void Fire()
    {
        throw new System.NotImplementedException();
    }

    public FireConditions GetFireState()
    {
        throw new System.NotImplementedException();
    }

    public void Initialize(EventManager eventManager, Transform spawnPoint, FireRate fireRate, int clipCapacity)
    {
        throw new System.NotImplementedException();
    }

    public void InjectFireRateOffsets(float[] offsets)
    {
        throw new System.NotImplementedException();
    }

    public void OnInstanceDestroyed()
    {
        throw new System.NotImplementedException();
    }

    public void Reload()
    {
        throw new System.NotImplementedException();
    }

    public void SetAmmoType(AmmoType type)
    {
        throw new System.NotImplementedException();
    }

    public void SetBulletSpawnPoint(Transform bulletSpawnPoint)
    {
        throw new System.NotImplementedException();
    }

    public void SetFireRate(FireRate fireRate)
    {
        throw new System.NotImplementedException();
    }

    public void Unequip()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateWeapon()
    {
        throw new System.NotImplementedException();
    }
}
