using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UnityEngine.EventSystems.EventTrigger;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class StatsHandler
{
 //   private List<StatEntry> _stats;
    private StatEntry[] _entriesToModify = new StatEntry[4];

    private Dictionary<StatType, float> _statStore;

    public StatsHandler(Dictionary<StatType, float> sharedstats/*List<StatEntry> sharedStats*/)
    {
        _statStore = sharedstats;
       // _stats = sharedstats;

        if (!HasStat(StatType.MaxHealth))
        {
            StatEntry entry = new StatEntry();
            entry.statType = StatType.MaxHealth;
            entry.value = 100;
            _statStore.Add(entry.statType, entry.value);
            //_stats.Add(entry);
        }

        if (!HasStat(StatType.Health))
        {
            StatEntry entry = new StatEntry();  
            entry.statType = StatType.Health;
            entry.value = 0;
            _statStore.Add(entry.statType, entry.value);
            //_stats.Add(entry);
        }
        SetStat(StatType.Health, GetStat(StatType.MaxHealth));
    }

    private void SetStat(StatType type, float maxAmount)
    {
        if (_statStore.TryGetValue(type, out var amount))
        {
            amount = maxAmount;
            _statStore[type] = amount;
        }
       /* for (int i = 0; i < _stats.Count; i++)
        {
            var entry = _stats[i];
            if (entry.statType == type)
            {
                entry.value = maxAmount;
            }
        }*/
       
    }

    private bool HasStat(StatType statType)
    {
        return _statStore.ContainsKey(statType);
      //  return _stats.Exists(s => s.statType == statType);
    }

    public float ModifyStat(StatType stat, float amount/*, out float remaining*/)
    {

        if(_statStore.TryGetValue(stat, out var value))
        {
            value += amount;

            if (stat == StatType.Health) value = Mathf.Clamp(value, 0, GetMaxStatValue(stat));
            _statStore[stat] = value;
            return value;
        }

       /* for (int i = 0; i < _stats.Count; i++)
        {
            var entry = _stats[i];
            if (entry.statType == stat)
            {
                entry.value += amount;

                if (stat == StatType.Health) entry.value = Mathf.Clamp(entry.value, 0, GetMaxStatValue(stat));

                return entry.value;
            }
        }*/

        return -1;
    }

    public void ModifyStat(ResourceCost cost/*Dictionary<StatEntry, float> stash*/)
    {
        if (cost.ResourceType == StatType.None || cost.Amount <= 0) return;
        //if (stash == null || stash.Count == 0) return;
        var stat = cost.ResourceType;
        var amount = cost.Amount;

        if(_statStore.TryGetValue(stat, out var value))
        {
            value += amount;
            _statStore[stat] = value;
        }


       /* foreach (var entry in stash)
        {
            StatEntry stat = entry.Key;
            float cost = entry.Value;

            stat.value += cost;
        }*/
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

    public bool HasEnoughResources(ResourceCost resources/*, Dictionary<StatType, float> stash*//*Dictionary<StatEntry, float> stash*/)
    {
       // stash.Clear();

        if (resources.ResourceType == StatType.None || resources.Amount <= 0) return true;
        // No cost for ability, return true and allow to proceed
        //  if (resources == null || resources.Length == 0) return true; 

        var stat = resources.ResourceType;
        var cost = resources.Amount;
       // StatEntry found = null;

        if (_statStore.TryGetValue(stat, out var amount))
        {
            if (amount - cost < 0) return false;

            //stash[stat] = cost;
            return true;
        }

        return false;



        /*for (int j = 0; j < _stats.Count; j++)
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

        stash[found] = cost;*/



/*
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
            
        }*/
      
       // return true;
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
        if(_statStore.TryGetValue(stat, out var amount))
        {
            return amount;
        }

       /* for (int i = 0; i < _stats.Count; i++)
        {
            var entry = _stats[i];
            if (entry.statType == stat)
            {
                return entry.value;
            }
        }*/

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
