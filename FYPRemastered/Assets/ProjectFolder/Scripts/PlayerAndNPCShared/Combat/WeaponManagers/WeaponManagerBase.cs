using Oculus.Interaction.Input;
using UnityEngine;

public abstract class WeaponManagerBase : ComponentEvents, IWeaponOwner
{
    public GameObject GameObject => gameObject;

    public bool HasUnlimitedAmmo { get; protected set; } = false;

   

    // Called from the currently equipped equippable via the IWeaponOwner interface
    public abstract void OnEquippableSignal(EquippableSignal signal, IEquippable equippable);
   

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        eventManager.OnEquipped += EquipWeapon;
        eventManager.OnUnEquipped += UnEquipWeapon;
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(eventManager);
        eventManager.OnEquipped -= EquipWeapon;
        eventManager.OnUnEquipped -= UnEquipWeapon;
    }


    protected virtual EquipResult EquipWeapon(IEquippable equippable, Handedness hand = Handedness.Left)
    {
        equippable.Equip(owner: this);
        return EquipResult.Success;
    }

    protected virtual void UnEquipWeapon(IEquippable equippable, Handedness hand = Handedness.Left) => equippable.UnEquip();

    protected virtual void TryUseWeapon() { }

    protected virtual void CancelWeaponUse() { }

    protected virtual void ChangeFireRate(FireRate rate) { }

   
}