using UnityEngine;

public interface IWeapon
{
    void Equip();
    void UnEquip();
   
    void UpdateWeapon();


    void OnInstanceDestroyed();

    
    // Agent only
    /// void ChangeWeapon(WeaponType type);   
    ///


    /// <summary>
    /// Player only;
    /// WeaponType GetWeaponType();
    /// 
}
