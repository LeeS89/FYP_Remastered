using Oculus.Interaction.Input;
using UnityEngine;

public abstract class WeaponManagerBase : ComponentEvents, IWeaponOwner
{
    public GameObject GameObject => gameObject;

    public bool HasUnlimitedAmmo { get; protected set; } = false;

   

    // Called from the currently equipped equippable via the IWeaponOwner interface
    public abstract void OnEquippableCue(EquippableCue signal, IEquippable equippable);
   

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


    public virtual EquipResult EquipWeapon(IEquippable equippable, Handedness hand = Handedness.Left)
    {
        equippable.Equip(owner: this);
        return EquipResult.Success;
    }

    protected virtual void UnEquipWeapon(IEquippable equippable, Handedness hand = Handedness.Left) => equippable.UnEquip();

    public virtual void TryUseWeapon() { }

    public virtual void CancelWeaponUse() { }

    protected virtual void ChangeFireRate(FireRate rate) { }

   
}