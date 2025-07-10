using System.Collections.Generic;
using UnityEngine;

public class StatHandler : ComponentEvents
{
    [SerializeField] private List<StatEntry> _stats = new List<StatEntry>();
    private StatsComponent _statsComponent;
    [SerializeField] private CharacterType _characterType;



    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        _statsComponent = new StatsComponent(_stats);

        eventManager.OnNotifyDamage += NotifyDamage;
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
                Debug.LogWarning("No Damage Type Received");
                break;
        }
        
    }

    protected bool HealthIsEmpty()
    {
        return _statsComponent.GetStat(StatType.Health) == 0;
    }

    protected void ApplyElementalDamage(DamageType dType = DamageType.None, float statusEffectChancePercentage = 0, float damageOverTime = 0, float duration = 0)
    {

    }

    protected void ApplyInstantDamage(float damage)
    {
        if(_statsComponent.ModifyStat(StatType.Health, -damage) == 0f)
        {
            GameManager.Instance.CharacterDied(_characterType);
            _eventManager.OwnerDied();
        }
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        eventManager.OnNotifyDamage -= NotifyDamage;
        _statsComponent = null;
        base.UnRegisterLocalEvents(eventManager);
    }

    protected override void RegisterGlobalEvents()
    {
        // Interface function - Not Implemented in class yet
    }

    protected override void UnRegisterGlobalEvents()
    {
        // Interface function - Not Implemented in class yet
    }
}
