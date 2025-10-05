using UnityEngine;

public interface IRanged : IWeapon
{
    void NotifyReload();
    void Reload();

    void SetFireRate(FireRate rate);

    void Fire(Transform target = null);

    void OnFireInterupted();

    void StartAutoFire();
}
