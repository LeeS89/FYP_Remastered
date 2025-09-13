using System;
using UnityEngine;

public class ProjectileCollisionComponent : ComponentEvents
{
    protected ProjectileEventManager _projectileEventManager;
    [Header("Damage Data")]
    [SerializeField] protected DamageData _damageData;
    public float BaseDamage { get; protected set; }
    public DamageType DType { get; protected set; }
    public float DamageOverTime { get; protected set; }
    public float DOTDuration { get; protected set; }
    [SerializeField] protected float _statusEffectChancePercentage;
 
   
 
    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _projectileEventManager = eventManager as ProjectileEventManager;
      
        InitializeDamageType();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager) => _projectileEventManager = null;


    protected virtual void InitializeDamageType()
    {
        if (_damageData == null) { return; }
        DType = _damageData.damageType;
        BaseDamage = _damageData.baseDamage;
        DamageOverTime = _damageData.damageOverTime;
        DOTDuration = _damageData.duration;
    }

   


    protected virtual void OnCollisionEnter(Collision collision)
    {
        CheckForDamageable(collision);
        _projectileEventManager.Collision(collision);
        _projectileEventManager.Expired();
    }


    protected virtual void CheckForDamageable(Collision other)
    {
        if (BaseDamage <= 0f) { return; }

        if (ComponentRegistry.TryGet<IDamageable>(other.gameObject, out var damageable))
        {
            damageable.NotifyDamage(BaseDamage, DType, _statusEffectChancePercentage, DamageOverTime, DOTDuration);
        }
        else 
        {
            var rb = other.collider.attachedRigidbody;
            if (rb && ComponentRegistry.TryGet<IDamageable>(rb.gameObject, out var dam))
                dam.NotifyDamage(BaseDamage, DType, _statusEffectChancePercentage, DamageOverTime, DOTDuration);
        }
    }

}
