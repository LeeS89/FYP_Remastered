using UnityEngine;

public class ProjectileCollisionComponent : ComponentEvents
{
    protected ProjectileEventManager _projectileEventManager;
    [Header("Damage Data")]
    [SerializeField] protected DamageData _damageData;
    protected float _baseDamage;
    protected DamageType _damageType;
    protected float _damageOverTime;
    protected float _dOTDuration;
    [SerializeField] protected float _statusEffectChancePercentage;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _projectileEventManager = eventManager as ProjectileEventManager;
        InitializeDamageType();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager) => _projectileEventManager = null;


    protected virtual void InitializeDamageType() { }


    protected virtual void OnCollisionEnter(Collision collision) => _projectileEventManager.Expired();

}
