using System.Collections.Generic;
using UnityEngine;

public class StatHandler : MonoBehaviour, IComponentEvents, IDamageable
{
    [SerializeField] private List<StatEntry> _statEntries;
    private Dictionary<StatType, float> _stats = new Dictionary<StatType, float>();
    private StatsComponent _statsComponent;

    public void RegisterEvents(EventManager eventManager)
    {
        foreach (var entry in _statEntries)
        {
            _stats[entry.statType] = entry.value;
        }

        _statsComponent = new StatsComponent(_stats);

        float hlth = _statsComponent.Gethealth();
        Debug.LogError("Health is: "+hlth);
    }

    public void TakeDamage(float baseDamage, DamageType dType = DamageType.None, float statusEffectChancePercentage = 0, float damageOverTime = 0, float duration = 0)
    {
        throw new System.NotImplementedException();
    }

    public void UnRegisterEvents(EventManager eventManager)
    {
        throw new System.NotImplementedException();
    }
}
