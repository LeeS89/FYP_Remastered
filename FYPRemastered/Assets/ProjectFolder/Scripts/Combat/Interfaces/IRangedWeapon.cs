using UnityEngine;

public interface IRangedWeapon : IWeaponObsolete
{
  
    void Reload();

   

    void Fire(Vector3? directionOverride = null);


    void SetAmmoType(AmmoType type);

    void SetBulletSpawnPoint(Transform bulletSpawnPoint);

   

    void Initialize(EventManager eventManager, Transform spawnPoint, AmmoType type, FireRate fireRate, int clipCapacity);
}
