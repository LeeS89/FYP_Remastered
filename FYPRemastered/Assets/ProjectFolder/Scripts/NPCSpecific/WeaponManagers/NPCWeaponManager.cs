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
        
        if(_eventManager is EnemyEventManager em)
        {
            em.OnReadyToFire += Ready;
            em.OnFireRangedWeapon += Fre;
        }

    }

    public void Fre()
    {
        if (_equippedItem is IRanged rw) rw.Fire();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _eventManager.OnTriggerPressed -= TryUseWeapon;
        _eventManager.OnTriggerReleased -= StopUsingWeapon;
        base.UnRegisterLocalEvents(eventManager);  
    }
    private bool _isUsing = false;
    public override void TryUseWeapon()
    {
        if (_equippedItem == null) return;
        if (_isUsing) return;
        _isUsing = true;
        if (_equippedItem is IRanged rw) rw.TryFire(FireRate.FullAutomatic);
    }

    public override void StopUsingWeapon()
    {
        if (_equippedItem == null) return;
        if (!_isUsing) return;
        if (_equippedItem is IRanged rw) rw.OnInterupted();
        _isUsing = false;
    }

    public void Equip()
    {
        //if (equippable == null) return;
        
        if (_equippedItem != null) _equippedItem.UnEquip();
        _equippedItem = _availableWeapons[0] as IRanged;
        if (_equippedItem == null) Debug.LogError("Weapon is null");
       // _equippedItem = equippable;
        _equippedItem.Equip(_eventManager, this);

        // Update weapon UI here if applicable
        // Optionally, parent the weapon to a specific transform (e.g., hand) on derived NPC class
    }

    public void Ready(Weapon wp)
    {
        if(_eventManager is EnemyEventManager em)
        {
            em.AnimationTriggered(AnimationAction.Shoot);
        }
    }
}
