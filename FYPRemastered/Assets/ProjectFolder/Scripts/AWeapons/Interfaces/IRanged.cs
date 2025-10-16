using UnityEngine;

public interface IRanged : IWeapon
{
    void ClipEmpty();
    void Reload();

    void SetFireRate(FireRate rate);

    void TryFire();

    void TryFire(FireRate rate = FireRate.SingleAutomatic, Transform target = null);

    void Fire();

    void OnInterupted();

    bool WeaponReady { get; }
}
