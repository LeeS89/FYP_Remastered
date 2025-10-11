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
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(eventManager);
        _eventManager.OnUnEquipped -= EquipWeapon;
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

    public virtual void TryUseWeapon()
    {
        if (_equippedItem == null) return;

        if(_equippedItem is IRanged rw) rw.TryFire(FireRate.SingleAutomatic);
    }

    public virtual void StopUsingWeapon()
    {
        if (_equippedItem == null) return;
        if(_equippedItem is IRanged rw) rw.OnInterupted();
    }
}
