using Oculus.Interaction.HandGrab;
using UnityEngine;

public class WeaponManager : ComponentEvents, IWeaponOwner
{
  //  protected Weapon _equippedWeapon;
    protected IEquippable _equippedItem;

    public GameObject GameObject => gameObject;

    protected bool _isNPC = false;
    public bool IsNPC => _isNPC;

    public Transform Target { get; protected set; } = null;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _eventManager = eventManager;
        base.RegisterLocalEvents(eventManager);
        _eventManager.OnEquipped += EquipWeapon;
        _eventManager.OnUnEquipped += UnEquipWeapon;
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(eventManager);
        _eventManager.OnUnEquipped -= EquipWeapon;
        _eventManager.OnUnEquipped -= UnEquipWeapon;
        _eventManager = null;

    }


    protected void EquipWeapon(IEquippable equippable)
    {
        if (equippable == null) return;

        if (_equippedItem != null) _equippedItem.UnEquip();

        _equippedItem = equippable;
        _equippedItem.Equip(_eventManager, this);
     
        // Update weapon UI here if applicable
        // Optionally, parent the weapon to a specific transform (e.g., hand) on derived NPC class
    }

    protected void UnEquipWeapon(IEquippable equippable)
    {
        if (equippable == null || equippable != _equippedItem) return;
        if (_equippedItem == null) return;
        _equippedItem.UnEquip();
        _equippedItem = null;
        // Update weapon UI here if applicable
    }

    protected virtual void TryUseWeapon() { }

    protected virtual void StopUsingWeapon() { }
    
}
