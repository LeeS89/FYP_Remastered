using UnityEngine;

public interface IRangedWeapon : IWeapon
{
    bool CanFire();

    void Reload();

    FireConditions GetFireState();

    void Fire();

    void Beginfire(MonoBehaviour runner);
    void EndFire(MonoBehaviour runner);

    void SetFireRate(FireRate fireRate);

    void SetAmmoType(AmmoType type);

    void SetBulletSpawnPoint(Transform bulletSpawnPoint);

    void InjectFireRateOffsets(float[] offsets);

    void Initialize(EventManager eventManager, Transform spawnPoint, FireRate fireRate, int clipCapacity);
}
