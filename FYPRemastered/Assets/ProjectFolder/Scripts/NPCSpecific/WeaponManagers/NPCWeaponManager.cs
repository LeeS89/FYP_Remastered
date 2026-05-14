using Oculus.Interaction.Input;
using ProjectRemaster.Combat;
using System.Collections.Generic;
using UnityEngine;

public sealed class NPCWeaponManager : WeaponManagerBase
{
    [SerializeField] private List<Weapon> _availableWeapons;
    private EnemyEventManager _eEventManager;
    private IEquippable _equippedWeapon;

    // private Dictionary<AmmoType, IEquippable> _weaponsByTypeStore = new(5);

    public override void RegisterLocalEvents(EventManagerObsolete eventManager)
    {
        _eEventManager = eventManager as EnemyEventManager;
        base.RegisterLocalEvents(_eEventManager);
    
        _eEventManager.OnProcessAnimCue += OnAnimationEventCallback;
        HasUnlimitedAmmo = true;

       /* if(_eventManager is EnemyEventManager em)
        {
            em.OnReadyToFire += Ready;
            em.OnFireRangedWeapon += Fre;
        }*/

    }
/*
    public void Fre()
    {
        if (_equippedWeapon is IRanged rw) rw.Fire();
    }*/

    public override void UnRegisterLocalEvents(EventManagerObsolete eventManager)
    {
        base.UnRegisterLocalEvents(eventManager);
      
        _eEventManager.OnProcessAnimCue -= OnAnimationEventCallback;
        _eEventManager = null;
    }


    private bool _isUsing = false;
    public override void TryUseWeapon()
    {
        if (_equippedWeapon == null) return;
        if (_isUsing) return;
        _isUsing = true;
        if (_equippedWeapon != null && _equippedWeapon is IRanged rw) rw.TryUse(FireRate.FullAutomatic); // Add player as target
    }

    /*private void Fire()
    {
        if (_equippedWeapon != null && _equippedWeapon is IRanged rw) rw.Fire();
    }*/



    #region Animation callback events
    private void OnAnimationEventCallback(AnimationCue cue)
    {
        if (_equippedWeapon == null) return;
        if (_equippedWeapon is IRanged rw) RangedWeaponAnimCallbacks(rw, cue);
    }

    private void RangedWeaponAnimCallbacks(IRanged ranged, AnimationCue cue)
    {
        if (ranged == null) return;
        switch (cue)
        {
            case AnimationCue.Shoot:
                ranged.Fire();
                break;
            case AnimationCue.ReloadComplete:
                ranged.Reload();
                break;
            default:
                return;
        }
    }
    #endregion


    public override void CancelWeaponUse()
    {
        if (_equippedWeapon == null) return;
        if (!_isUsing) return;
        if (_equippedWeapon is IRanged rw) rw.OnInterupted();
        _isUsing = false;
    }

    public void Equip()
    {
        //if (equippable == null) return;
        
        if (_equippedWeapon != null) _equippedWeapon.UnEquip();
        _equippedWeapon = _availableWeapons[0] as IRanged;
        if (_equippedWeapon == null) Debug.LogError("Weapon is null");
       // _equippedItem = equippable;
        _equippedWeapon.Equip(this);

        // Update weapon UI here if applicable
        // Optionally, parent the weapon to a specific transform (e.g., hand) on derived NPC class
    }

    

    public override EquipResult EquipWeapon(IEquippable equippable, Handedness hand = Handedness.Left)
    {
        if (equippable == null) return EquipResult.EquipIsNull;
        if (_equippedWeapon != null && _equippedWeapon == equippable) return EquipResult.AlreadyEquipped;

        if(_equippedWeapon != null) UnEquipWeapon(_equippedWeapon);
        _equippedWeapon = equippable;

        return base.EquipWeapon(_equippedWeapon);
    }

    #region Equiped Weapon callback cues
    public override void OnEquippableCue(EquippableCue signal, IEquippable equippable = null)
    {
        AnimationCue cueToBroadcast = AnimationCue.None;
        switch (signal)
        {
            case EquippableCue.Ready: // Is ready to fire
                if (equippable is IRanged) cueToBroadcast = AnimationCue.Shoot;
                break;
            case EquippableCue.ClipEmpty:
                if (equippable is IRanged) cueToBroadcast = AnimationCue.Reload;
                    break;
            default:
               // cueToBroadcast = AnimationCue.None;
                break;
        }
        _eEventManager.TriggerAnimation(cueToBroadcast);
    }

    public override void Init(IPlaceholderService services, PlayerEventManager manager)
    {
        throw new System.NotImplementedException();
    }

    public override void Unload()
    {
        throw new System.NotImplementedException();
    }

    protected override void Update()
    {
        throw new System.NotImplementedException();
    }
    #endregion


}
