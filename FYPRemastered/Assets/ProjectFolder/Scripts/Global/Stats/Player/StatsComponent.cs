using System.Collections.Generic;
using UnityEngine;

public class StatsComponent : ComponentEvents
{
    [SerializeField] private List<StatEntry> _stats = new List<StatEntry>();
    private StatsHandler _statsHandler;
    [SerializeField] private CharacterType _characterType;



    public override void RegisterLocalEvents(EventManager eventManager)
    {
        //base.RegisterLocalEvents(eventManager);
        _eventManager = eventManager;
        _statsHandler = new StatsHandler(_stats);

        _eventManager.OnNotifyDamage += NotifyDamage;
        _eventManager.OnTrySpendResources += TrySpendResources;
        //float hlth = _statsComponent.Gethealth();
        //Debug.LogError("Health is: "+hlth);
    }

    

    public bool _testDamage = false;
    private void Update()
    {
        if(_testDamage)
        {
            ApplyInstantDamage(15);
            _testDamage = false;
        }
    }

    public void NotifyDamage(float baseDamage, DamageType dType = DamageType.None, float statusEffectChancePercentage = 0, float damageOverTime = 0, float duration = 0)
    {
        if (HealthIsEmpty()) { return; } 

        switch (dType)
        {
            case DamageType.Normal:
                ApplyInstantDamage(baseDamage);
                break;
            case DamageType.Fire:
                ApplyElementalDamage(dType, statusEffectChancePercentage, damageOverTime, duration);
                break;
            default:
                ApplyInstantDamage(baseDamage);
                break;
        }
        
    }

    protected bool HealthIsEmpty()
    {
        return _statsHandler.GetStat(StatType.Health) == 0;
    }

    protected void ApplyElementalDamage(DamageType dType = DamageType.None, float statusEffectChancePercentage = 0, float damageOverTime = 0, float duration = 0)
    {

    }

    protected void ApplyInstantDamage(float damage)
    {
        if(_statsHandler.ModifyStat(StatType.Health, -damage) <= 0f)
        {
            GameManager.Instance.CharacterDeathStatusChanged(_characterType, null, isDead : true);
            _eventManager.DeathStatusUpdated(true);
        }
    }

    protected bool TrySpendResources(ResourceCost[] resources) => _statsHandler.HasEnoughResources(resources);


    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        _eventManager.OnNotifyDamage -= NotifyDamage;
        
        //base.UnRegisterLocalEvents(eventManager);
    }

   

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        _statsHandler = null;
        _stats.Clear();
        _stats = null;
        _eventManager = null;
    }
}
