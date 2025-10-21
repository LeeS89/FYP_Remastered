using Oculus.Interaction.Input;
using ProjectRemaster.Combat;
using System.Collections.Generic;
using UnityEngine;

public sealed class NPCWeaponManager : WeaponManagerBase
{
    [SerializeField] private List<Weapon> _availableWeapons;
    private EnemyEventManager _eEventManager;
    private IEquippable _equippedItem;

    // private Dictionary<AmmoType, IEquippable> _weaponsByTypeStore = new(5);

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _eEventManager = eventManager as EnemyEventManager;
        base.RegisterLocalEvents(_eEventManager);
     
        HasUnlimitedAmmo = true;

       /* if(_eventManager is EnemyEventManager em)
        {
            em.OnReadyToFire += Ready;
            em.OnFireRangedWeapon += Fre;
        }*/

    }

    public void Fre()
    {
        if (_equippedItem is IRanged rw) rw.Fire();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(eventManager);
        _eEventManager = null;
    }
    private bool _isUsing = false;
    protected override void TryUseWeapon()
    {
        if (_equippedItem == null) return;
        if (_isUsing) return;
        _isUsing = true;
        if (_equippedItem is IRanged rw) rw.TryFire(FireRate.FullAutomatic);
    }

    protected override void CancelWeaponUse()
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
        _equippedItem.Equip(this);

        // Update weapon UI here if applicable
        // Optionally, parent the weapon to a specific transform (e.g., hand) on derived NPC class
    }

    public void Ready(Weapon wp)
    {
       /* if(_eventManager is EnemyEventManager em)
        {
            em.AnimationTriggered(AnimationAction.Shoot);
        }*/
    }

    protected override EquipResult EquipWeapon(IEquippable equippable, Handedness hand = Handedness.Left)
    {
        return base.EquipWeapon(equippable);
    }

    public override void OnEquippableSignal(EquippableSignal signal, IEquippable equippable = null)
    {
        switch (signal)
        {
            case EquippableSignal.Ready:
                if (equippable is IRanged rw) rw.Fire();
                break;
            case EquippableSignal.ClipEmpty:
                break;
            case EquippableSignal.Empty:
                break;
            default:
                break;
        }
    }
}
