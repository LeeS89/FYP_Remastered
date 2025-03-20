using System.Collections.Generic;
using UnityEngine;

public class StatHandler : MonoBehaviour, IComponentEvents, IDamageable
{
    [SerializeField] private List<StatEntry> _stats = new List<StatEntry>();
    private StatsComponent _statsComponent;
    [SerializeField] private CharacterType _characterType;



    public void RegisterEvents(EventManager eventManager)
    {
        _statsComponent = new StatsComponent(_stats);

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

    public void TakeDamage(float baseDamage, DamageType dType = DamageType.None, float statusEffectChancePercentage = 0, float damageOverTime = 0, float duration = 0)
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

    private bool HealthIsEmpty()
    {
        return _statsComponent.GetStat(StatType.Health) == 0;
    }

    private void ApplyElementalDamage(DamageType dType = DamageType.None, float statusEffectChancePercentage = 0, float damageOverTime = 0, float duration = 0)
    {

    }

    private void ApplyInstantDamage(float damage)
    {
        if(_statsComponent.ModifyStat(StatType.Health, -damage) == 0f)
        {
            GameManager.Instance.CharacterDied(_characterType);
        }
    }

    public void UnRegisterEvents(EventManager eventManager)
    {
        throw new System.NotImplementedException();
    }
}
