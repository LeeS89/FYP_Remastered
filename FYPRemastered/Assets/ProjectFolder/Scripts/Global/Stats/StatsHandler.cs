using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StatsHandler
{
    private List<StatEntry> _stats;

    public StatsHandler(List<StatEntry> sharedStats)
    {
        _stats = sharedStats;

        if (!HasStat(StatType.MaxHealth))
        {
            StatEntry entry = new StatEntry();
            entry.statType = StatType.MaxHealth;
            entry.value = 100;
            _stats.Add(entry);
        }

        if (!HasStat(StatType.Health))
        {
            StatEntry entry = new StatEntry();  
            entry.statType = StatType.Health;
            entry.value = 0;
            _stats.Add(entry);
        }
        ModifyStat(StatType.Health, GetStat(StatType.MaxHealth));
    }

    private bool HasStat(StatType statType)
    {
        return _stats.Exists(s => s.statType == statType);
    }

    public float ModifyStat(StatType stat, float amount)
    {
        for(int i = 0; i < _stats.Count; i++)
        {
            var entry = _stats[i];
            if (entry.statType == stat)
            {
                entry.value += amount;
                entry.value = Mathf.Clamp(entry.value, 0, GetMaxStatValue(stat));
                return entry.value;
            }
        }
        return -1;

        /*StatEntry entry = _stats.Find(s => s.statType == stat);
        if (entry != null)
        {
            entry.value += amount;
            entry.value = Mathf.Clamp(entry.value, 0, GetMaxStatValue(stat));
            return entry.value;
            
        }
        return -1;*//// CURRENT IMPLEMENTATION
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
        for (int i = 0; i < _stats.Count; i++)
        {
            var entry = _stats[i];
            if (entry.statType == stat)
            {
                return entry.value;
            }
        }

        return 0;
        /* StatEntry entry = _stats.Find(s => s.statType == stat);
         return entry != null ? entry.value : 0;*/ // CURRENT IMPLEMENTATION
    }

    /*public float Gethealth()
    {
        //PlayerPrefs
        return _stats.TryGetValue(StatType.Health, out float value) ? value : 0f;
    }*/
}
