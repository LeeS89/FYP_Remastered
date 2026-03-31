using Oculus.Interaction.Input;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : WeaponManagerBase
{
    //  protected Weapon _equippedWeapon;

    private PlayerEventManager _pEventManager;


    private Dictionary<Handedness, IEquippable> _equippableSlots = new(2)
    {
        { Handedness.Left,  null },
        { Handedness.Right, null },
    };
    
    public Transform Target { get; protected set; } = null;

    public override void RegisterLocalEvents(EventManagerObsolete eventManager)
    {
       // _pEventManager = eventManager as PlayerEventManager;
      //  base.RegisterLocalEvents(_pEventManager);


    }

    public override void UnRegisterLocalEvents(EventManagerObsolete eventManager)
    {
        base.UnRegisterLocalEvents(eventManager);
        _pEventManager = null;
    }

    
    public override EquipResult EquipWeapon(IEquippable equippable, Handedness hand)
    {
        if (equippable == null) return EquipResult.EquipIsNull;

        foreach (var kv in _equippableSlots) 
            if (kv.Value == equippable) return EquipResult.AlreadyEquipped;

        if (_equippableSlots[hand] != null) return EquipResult.SlotOccupied;

        _equippableSlots[hand] = equippable;
      
        return base.EquipWeapon(equippable);
        // Update weapon UI here if applicable
        // Optionally, parent the weapon to a specific transform (e.g., hand) on derived NPC class
    }

    protected override void UnEquipWeapon(IEquippable equippable, Handedness hand)
    {
        if (equippable == null) return;

        if (_equippableSlots[hand] == null || _equippableSlots[hand] != equippable) return;

        _equippableSlots[hand] = null;
        base.UnEquipWeapon(equippable);

        // Update weapon UI here if applicable
    }

   // public virtual void TryUseWeapon() { } // Should be protected, but needs to be public for testing

   // public virtual void StopUsingWeapon() { } // Should be protected, but needs to be public for testing

    public override void OnEquippableCue(EquippableCue signal, IEquippable equippable)
    {
        switch (signal)
        {
            case EquippableCue.Ready:
                if (equippable is IRanged rw) rw.Fire();
                break;
            case EquippableCue.ClipEmpty:
                break;
            case EquippableCue.Empty:
                break;
            default:
                break;
        }


    }

    #region Kept for optional changes
    private int FindFreeSlot(IEquippable[] slots)
    {
        for(int i = 0; i < slots.Length; i++)
            if (slots[i] == null) return i;
        return -1;
    }

    private bool IndexOf(IEquippable[] slots, IEquippable w)
    {
        for (int i = 0;i < slots.Length;i++)
            if (slots[i] == w) return true;

        return false;
    }

    public override void Init(IPlaceholderService services, PlayerEventManager manager)
    {
       // throw new System.NotImplementedException();
    }

    public override void Unload()
    {
       // throw new System.NotImplementedException();
    }

    // private IEquippable[] _slots = new IEquippable[2];
    #endregion
}
