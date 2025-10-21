using System;
using UnityEngine;

namespace ProjectRemaster.Combat
{
    public abstract class Weapon : MonoBehaviour, IWeapon
    {
        [SerializeField] protected Animator _anim;
       // public EventManager EventManager { get; protected set; }
        protected IWeaponOwner _owner;
      
        public Transform Target { get; protected set; } = null;

        public bool Equipped { get; protected set; } = false;

        // public bool WeaponReady { get; protected set; } = false;

        public virtual void Equip(IWeaponOwner owner)
        {
           // EventManager = eventManager;
            _owner = owner;
            Equipped = true;
        }

        public virtual void UnEquip()
        {
            Equipped = false;
         //   EventManager = null;
            _owner = null;
            Target = null;
        }

        public virtual void ResetAnimator() { }

        protected virtual void PlayAnimations() { }
    }
}