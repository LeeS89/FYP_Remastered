using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StatsComponent
{
    private List<StatEntry> _stats;

    public StatsComponent(List<StatEntry> sharedStats)
    {
        _stats = sharedStats;

        ModifyStat(StatType.Health, GetStat(StatType.MaxHealth));
    }

    public float ModifyStat(StatType stat, float amount)
    {
        StatEntry entry = _stats.Find(s => s.statType == stat);
        if (entry != null)
        {
            entry.value += amount;
            entry.value = Mathf.Clamp(entry.value, 0, GetMaxStatValue(stat));
            return entry.value;
            
        }
        return -1;
    }

    private float GetMaxStatValue(StatType stat)
    {
        switch (stat)
        {
            case StatType.Health:
                return GetStat(StatType.MaxHealth);
            case StatType.Force:
                return GetStat(StatType.MaxForce);
            default:
                return float.MaxValue;
        }
    }

    public float GetStat(StatType stat)
    {
        StatEntry entry = _stats.Find(s => s.statType == stat);
        return entry != null ? entry.value : 0;
    }

    /*public float Gethealth()
    {
        //PlayerPrefs
        return _stats.TryGetValue(StatType.Health, out float value) ? value : 0f;
    }*/
}
