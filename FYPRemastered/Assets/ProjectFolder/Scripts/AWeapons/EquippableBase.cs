using UnityEngine;

public abstract class EquippableBase : MonoBehaviour, IEquippable
{
    public bool Equipped { get; protected set; } = false;

    public EquippableType EquippableType => type;

    [SerializeField] protected EquippableType type;

    public abstract void Equip(IWeaponOwner owner = null);


    public virtual void HandleOwnerCue(EquippableCue cue) { }// Redundant


    public abstract void UnEquip();
    
}
