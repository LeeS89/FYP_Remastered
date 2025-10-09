using UnityEngine;

public interface IRanged : IWeapon
{
    void ClipEmpty();
    void Reload();

    void SetFireRate(FireRate rate);

    void TryFire(FireRate rate, Transform target = null);

    void Fire();

    void OnInterupted();

    bool LockedAndLoaded { get; }
}
