using System.Collections.Generic;
using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class StatsHandler
{
    private List<StatEntry> _stats;
    private StatEntry[] _entriesToModify = new StatEntry[4];

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
        SetStat(StatType.Health, GetStat(StatType.MaxHealth));
    }

    private void SetStat(StatType type, float maxAmount)
    {
        for (int i = 0; i < _stats.Count; i++)
        {
            var entry = _stats[i];
            if (entry.statType == type)
            {
                entry.value = maxAmount;
            }
        }
       
    }

    private bool HasStat(StatType statType)
    {
        return _stats.Exists(s => s.statType == statType);
    }

    public float ModifyStat(StatType stat, float amount/*, out float remaining*/)
    {
        for (int i = 0; i < _stats.Count; i++)
        {
            var entry = _stats[i];
            if (entry.statType == stat)
            {
                entry.value += amount;

                if (stat == StatType.Health) entry.value = Mathf.Clamp(entry.value, 0, GetMaxStatValue(stat));

                return entry.value;
            }
        }

        return -1;
    }

    public void ModifyStat(Dictionary<StatEntry, float> stash)
    {
        if (stash == null || stash.Count == 0) return;

        foreach (var entry in stash)
        {
            StatEntry stat = entry.Key;
            float cost = entry.Value;

            stat.value += cost;
        }
    }

   /* public bool HasEnoughResources(ResourceCost[] resources)
    {
        // No cost for ability, return true and allow to proceed
        if (resources == null || resources.Length == 0) return true;

        for (int i = 0; i < resources.Length; i++)
        {
            var stat = resources[i].ResourceType;
            var cost = resources[i].Amount;
            StatEntry found = null;

            for (int j = 0; j < _stats.Count; j++)
            {
                var entry = _stats[j];
                if (entry.statType == stat)
                {
                    found = entry;
                    break;
                }
            }
            if (found == null) return false;

            if (found.value - cost < 0) return false;

            _entriesToModify[i] = found;

        }
        ////////////// Split here into seperate check and apply functions
        for(int i = 0; i < resources.Length; i++)
        {
            var stat = _entriesToModify[i];
            var amount = resources[i].Amount;
            ModifyStat(stat, -amount);
            _entriesToModify[i] = null;
        }
        
        return true;
    }*/

    public bool HasEnoughResources(ResourceCost[] resources, Dictionary<StatEntry, float> stash)
    {
        stash.Clear();
        // No cost for ability, return true and allow to proceed
        if (resources == null || resources.Length == 0) return true; 

        for (int i = 0; i < resources.Length; i++)
        {
            var stat = resources[i].ResourceType;
            var cost = resources[i].Amount;
            StatEntry found = null;

            for (int j = 0; j < _stats.Count; j++)
            {
                var entry = _stats[j];
                if (entry.statType == stat)
                {
                    found = entry;
                    break;
                }
            }
            if (found == null) return false;

            if (found.value - cost < 0) return false;

            stash[found] = cost;
            
        }
      
        return true;
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
