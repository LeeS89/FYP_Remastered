using System;
using UnityEngine;

public abstract class Weapon : MonoBehaviour, IWeapon
{
    protected EventManager _eventManager;
    protected IWeaponOwner _owner;
    public Transform Target { get; protected set; } = null;

    public bool Equipped { get; protected set; } = false;

   // public bool WeaponReady { get; protected set; } = false;

    public virtual void Equip(EventManager eventManager, IWeaponOwner owner = null)
    {
        _eventManager = eventManager;
        _owner = owner;
        Equipped = true;
    }
    
    public virtual void UnEquip()
    {
        Equipped = false;
        _eventManager = null;
        _owner = null;
        Target = null;
    }

   
}
