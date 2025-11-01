using System;
using UnityEngine;

namespace ProjectRemaster.Combat
{
    public abstract class Weapon : EquippableBase, IWeapon
    {
        [SerializeField] protected Animator _anim;
       // public EventManager EventManager { get; protected set; }
        protected IWeaponOwner _owner;
      
        public Transform Target { get; protected set; } = null;

        //protected ITargetable _target;
        // public bool WeaponReady { get; protected set; } = false;

        public override void Equip(IWeaponOwner owner)
        {
           // EventManager = eventManager;
            _owner = owner;
            Equipped = true;
        }

        public override void UnEquip()
        {
            Equipped = false;
         //   EventManager = null;
            _owner = null;
            Target = null;
        }

        public virtual void ResetAnimator() { }

        protected virtual void PlayAnimations() { }

       // public virtual void HandleOwnerCue(EquippableCue cue) { }
       
    }
}