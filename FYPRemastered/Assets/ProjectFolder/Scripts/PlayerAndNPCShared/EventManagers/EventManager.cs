using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectRemaster.Combat;
using Oculus.Interaction.Input;

public abstract class EventManager : MonoBehaviour
{
    protected List<ComponentEvents> _cachedListeners;

    

    /// <summary>
    /// Finds all Interface components within the object hierarchy and 
    /// ensures each component binds to their relevant events
    /// </summary>
    public virtual void BindComponentsToEvents()
    {
        _cachedListeners = new List<ComponentEvents>();

        var childListeners = GetComponentsInChildren<ComponentEvents>(true);
        _cachedListeners.AddRange(childListeners);

        foreach (var listener in _cachedListeners)
        {
            listener.RegisterLocalEvents(this);
        }
    }

    public virtual void UnbindComponentsToEvents()
    {
        foreach (var listener in _cachedListeners)
        {
            listener.UnRegisterLocalEvents(this);
        }
        _cachedListeners?.Clear();
        _cachedListeners = null;
    }

    //  public event Action<AbilityResources, Transform> OnTryUseAbility;
    public event Action<AbilityTags> OnEndAbility;

    public event Action<AbilityTags, AbilityOrigins> OnTryUseAbility;
    public void TryUseAbility(AbilityTags tag, AbilityOrigins origins) => OnTryUseAbility?.Invoke(tag, origins);
    /* public delegate void TryUseAbility(in AbilityResources resources, Transform origin = null);
     public event TryUseAbility OnTryUseAbility;

     public void TryActivateAbility(in AbilityResources resources, Transform origin = null) => OnTryUseAbility?.Invoke(resources, origin);*/
    // public void TryUseAbility(in AbilityResources resources, Transform origin = null) => OnTryUseAbility?.Invoke(resources, origin);

    public void EndAbility(AbilityTags id) => OnEndAbility?.Invoke(id);

    public event Action<bool> OnDeathStatusUpdated;

    public bool _testRespawn = false;
    private void Update()
    {
        if (_testRespawn)
        {
            _testRespawn = false;
            DeathStatusUpdated(false);
        }
    }

    public void DeathStatusUpdated(bool isDead)
    {
        OnDeathStatusUpdated?.Invoke(isDead);
    }

    public event Func<ResourceCost, /*Dictionary<StatEntry, float>,*/ bool> OnCheckIfHasSufficientResources;

    public bool CheckIfHasSufficientResources(ResourceCost resource/*, Dictionary<StatEntry, float> stash*/) => OnCheckIfHasSufficientResources?.Invoke(resource/*, stash*/) ?? false;


    public event Action<ResourceCost/*Dictionary<StatEntry, float>*/> OnSpendResources;

    public void SpendResources(ResourceCost resource/*Dictionary<StatEntry, float> stash*/) => OnSpendResources?.Invoke(resource/*stash*/);

    /*public delegate bool TrySpendResource(ResourceCost resource, out float remaining);
    public event TrySpendResource OnTrySpendResource;

    public bool SpendResource(ResourceCost resource, out float remaining)
    {
        var handler = OnTrySpendResource;
        if (handler == null)
        {
            remaining = 0f;
            return false;
        }
        return handler(resource, out remaining);
    }*/
    //    public event Func<bool> OnSpendResource;

    //  public bool SpendResource() => OnSpendResource?.Invoke() ?? false;



    public event Action<float, DamageType, float, float, float> OnNotifyDamage;

    public void TakeDamage(float baseDamage, DamageType dType = DamageType.None, float statusEffectChancePercentage = 0, float damageOverTime = 0, float duration = 0)
    {
        OnNotifyDamage?.Invoke(baseDamage, dType, statusEffectChancePercentage, damageOverTime, duration);
    }

    //GunSetup(GameObject gunOwner, EventManager eventManager, Transform bulletSpawnLocaiton, int clipCapacity, Transform target = null)
    /*public event Action<GameObject, EventManager,Transform, int, Transform> OnSetupGun;

    public void SetupGun(GameObject gunOwner, EventManager eventManager, Transform bulletSpawnLocation, int clipCapacity, Transform target = null)
    {
        OnSetupGun?.Invoke(gunOwner, eventManager, bulletSpawnLocation, clipCapacity, target);
    }*/

    public event Action<FireConditions> OnTryShoot;

    public void TryShoot(FireConditions conditions)
    {
        OnTryShoot?.Invoke(conditions);
    }

    public event Action OnNotifyReload;

    public void NotifyReload() => OnNotifyReload?.Invoke();

    public event Action OnOutOfAmmo;

    public void OutOfAmmo() => OnOutOfAmmo?.Invoke();

    


    public event Action<Vector3, float, float> OnKnockbackTriggered;

    public void Knockbacktriggered(Vector3 direction, float force, float duration)
    {
        OnKnockbackTriggered?.Invoke(direction, force, duration);
    }

    // Gun Events
    public event Action OnFireWeapon;

    public void FireWeapon() => OnFireWeapon?.Invoke();






    //// NEW SETUP
    public event Action<Weapon> OnReadyToFire;

    public void ReadyToFire(Weapon wp) => OnReadyToFire?.Invoke(wp);

    public event Action OnShoot;

    public void Shoot() => OnShoot?.Invoke();

    //public event Action On
    public Func<IEquippable, Handedness, EquipResult> OnEquipped;
    public EquipResult InteractableSelected(IEquippable item, Handedness hand = Handedness.Left) => OnEquipped?.Invoke(item, hand) ?? EquipResult.Failed;

    public event Action<IEquippable, Handedness> OnUnEquipped;
    public void InteractableReleased(IEquippable item, Handedness hand = Handedness.Left) => OnUnEquipped?.Invoke(item, hand);

    public event Action OnTriggerPressed;
    public event Action OnTriggerReleased;

    public void TriggerPressed() => OnTriggerPressed?.Invoke();
    public void TriggerReleased() => OnTriggerReleased?.Invoke();

}





























public abstract class EventManagerBase : MonoBehaviour
{
    protected List<ComponentEvents> _cachedListeners;

    List<IServicable<ISceneServiceProvider, EventManagerBase>> _servicables = new();

    public void Init(ISceneServiceProvider sProvider)
    {
        var servicables = GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var mb in servicables)
        {
            if (mb is IServicable<ISceneServiceProvider, EventManagerBase> servicable)
            {
                _servicables.Add(servicable);
            }
        }

       

        foreach(var s in _servicables)
        {
            s.Load(sProvider, this);
        }
    }

    /// <summary>
    /// Finds all Interface components within the object hierarchy and 
    /// ensures each component binds to their relevant events
    /// </summary>
    public virtual void BindComponentsToEvents()
    {
        _cachedListeners = new List<ComponentEvents>();

        var childListeners = GetComponentsInChildren<ComponentEvents>(true);
        _cachedListeners.AddRange(childListeners);

        foreach (var listener in _cachedListeners)
        {
           // listener.RegisterLocalEvents(this);
        }
    }

    public virtual void UnbindComponentsToEvents()
    {
        foreach (var listener in _cachedListeners)
        {
           // listener.UnRegisterLocalEvents(this);
        }
        _cachedListeners?.Clear();
        _cachedListeners = null;
    }

    //  public event Action<AbilityResources, Transform> OnTryUseAbility;
    public event Action<AbilityTags> OnEndAbility;

    public event Action<AbilityTags, AbilityOrigins> OnTryUseAbility;
    public void TryUseAbility(AbilityTags tag, AbilityOrigins origins) => OnTryUseAbility?.Invoke(tag, origins);
    /* public delegate void TryUseAbility(in AbilityResources resources, Transform origin = null);
     public event TryUseAbility OnTryUseAbility;

     public void TryActivateAbility(in AbilityResources resources, Transform origin = null) => OnTryUseAbility?.Invoke(resources, origin);*/
    // public void TryUseAbility(in AbilityResources resources, Transform origin = null) => OnTryUseAbility?.Invoke(resources, origin);

    public void EndAbility(AbilityTags id) => OnEndAbility?.Invoke(id);

    public event Action<bool> OnDeathStatusUpdated;

    public bool _testRespawn = false;
    private void Update()
    {
        if (_testRespawn)
        {
            _testRespawn = false;
            DeathStatusUpdated(false);
        }
    }

    public void DeathStatusUpdated(bool isDead)
    {
        OnDeathStatusUpdated?.Invoke(isDead);
    }

    public event Func<ResourceCost, /*Dictionary<StatEntry, float>,*/ bool> OnCheckIfHasSufficientResources;

    public bool CheckIfHasSufficientResources(ResourceCost resource/*, Dictionary<StatEntry, float> stash*/) => OnCheckIfHasSufficientResources?.Invoke(resource/*, stash*/) ?? false;


    public event Action<ResourceCost/*Dictionary<StatEntry, float>*/> OnSpendResources;

    public void SpendResources(ResourceCost resource/*Dictionary<StatEntry, float> stash*/) => OnSpendResources?.Invoke(resource/*stash*/);

   


    public event Action<float, DamageType, float, float, float> OnNotifyDamage;

    public void TakeDamage(float baseDamage, DamageType dType = DamageType.None, float statusEffectChancePercentage = 0, float damageOverTime = 0, float duration = 0)
    {
        OnNotifyDamage?.Invoke(baseDamage, dType, statusEffectChancePercentage, damageOverTime, duration);
    }

   

    public event Action<FireConditions> OnTryShoot;

    public void TryShoot(FireConditions conditions)
    {
        OnTryShoot?.Invoke(conditions);
    }

    public event Action OnNotifyReload;

    public void NotifyReload() => OnNotifyReload?.Invoke();

    public event Action OnOutOfAmmo;

    public void OutOfAmmo() => OnOutOfAmmo?.Invoke();

    


    public event Action<Vector3, float, float> OnKnockbackTriggered;

    public void Knockbacktriggered(Vector3 direction, float force, float duration)
    {
        OnKnockbackTriggered?.Invoke(direction, force, duration);
    }

  

    //// NEW SETUP
    public event Action<Weapon> OnReadyToFire;

    public void ReadyToFire(Weapon wp) => OnReadyToFire?.Invoke(wp);

    public event Action OnShoot;

    public void Shoot() => OnShoot?.Invoke();

    //public event Action On
    public Func<IEquippable, Handedness, EquipResult> OnEquipped;
    public EquipResult InteractableSelected(IEquippable item, Handedness hand = Handedness.Left) => OnEquipped?.Invoke(item, hand) ?? EquipResult.Failed;

    public event Action<IEquippable, Handedness> OnUnEquipped;
    public void InteractableReleased(IEquippable item, Handedness hand = Handedness.Left) => OnUnEquipped?.Invoke(item, hand);

    public event Action OnTriggerPressed;
    public event Action OnTriggerReleased;

    public void TriggerPressed() => OnTriggerPressed?.Invoke();
    public void TriggerReleased() => OnTriggerReleased?.Invoke();

}

public class EVB : EventManagerBase
{
    
}