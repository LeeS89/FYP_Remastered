using ProjectRemaster.Combat;
using System.Collections.Generic;
using UnityEngine;

public sealed class NPCWeaponManager : WeaponManager
{
    [SerializeField] private List<Weapon> _availableWeapons;
    // private Dictionary<AmmoType, IEquippable> _weaponsByTypeStore = new(5);

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        _isNPC = true;
        _eventManager.OnTriggerPressed += TryUseWeapon;
        _eventManager.OnTriggerReleased += StopUsingWeapon;
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _eventManager.OnTriggerPressed -= TryUseWeapon;
        _eventManager.OnTriggerReleased -= StopUsingWeapon;
        base.UnRegisterLocalEvents(eventManager);  
    }

    protected override void TryUseWeapon()
    {
        if (_equippedItem == null) return;

        if (_equippedItem is IRanged rw) rw.TryFire(FireRate.SingleAutomatic);
    }

    protected override void StopUsingWeapon()
    {
        if (_equippedItem == null) return;
        if (_equippedItem is IRanged rw) rw.OnInterupted();
    }
}
