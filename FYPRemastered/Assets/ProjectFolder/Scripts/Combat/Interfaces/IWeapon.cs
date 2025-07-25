using UnityEngine;

public interface IWeapon
{
    void Equip(EventManager eventManager);
    void Unequip();
    void BeginUse(MonoBehaviour runner);
    void EndUse(MonoBehaviour runner);
    void UpdateWeapon();

    void Fire();

    bool CanFire();

    void Reload();

    FireConditions GetFireState();

    // Agent only
    /// void ChangeWeapon(WeaponType type);   
    ///


    /// <summary>
    /// Player only;
    /// WeaponType GetWeaponType();
    /// 
}
