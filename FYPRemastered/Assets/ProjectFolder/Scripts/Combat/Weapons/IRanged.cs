using UnityEngine;

public interface IRanged : IWeapon
{
    void NotifyReload();
    void Reload();

    void SetFireRate(FireRate rate);

    void TryFire(Transform target = null);

    void Fire(Transform target = null);

    void OnFireInterupted();

    void StartAutoFire();

    void EndAutoFire();

    void Equip(EventManager eventManager, GameObject owner, FireRate fRate);

}
