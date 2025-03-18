using System.Collections.Generic;
using UnityEngine;

public class StatsComponent
{
    private Dictionary<StatType, float> _stats;

    public StatsComponent(Dictionary<StatType, float> sharedStats)
    {
        _stats = sharedStats;
    }

    public float Gethealth()
    {
        //PlayerPrefs
        return _stats.TryGetValue(StatType.Health, out float value) ? value : 0f;
    }
}
