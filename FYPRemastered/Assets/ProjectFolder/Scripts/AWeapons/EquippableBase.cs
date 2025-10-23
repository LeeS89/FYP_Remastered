using UnityEngine;

public abstract class EquippableBase : MonoBehaviour, IEquippable
{
    public bool Equipped { get; protected set; } = false;

    public WeaponType EquippableType => type;

    [SerializeField] protected WeaponType type;

    public abstract void Equip(IWeaponOwner owner = null);


    public virtual void HandleOwnerCue(EquippableCue cue) { }// Redundant


    public abstract void UnEquip();
    
}
