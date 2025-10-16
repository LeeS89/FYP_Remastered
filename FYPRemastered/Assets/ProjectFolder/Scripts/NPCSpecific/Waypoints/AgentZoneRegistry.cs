using System.Collections.Generic;
using System;
using UnityEngine;
using System.Threading.Tasks;

public class AgentZoneRegistry : SceneResources
{
    private Dictionary<int, List<FSMControllerBase>> _zoneAgents = new();

    public override async Task LoadResources()
    {
        SceneEventAggregator.Instance.OnAgentZoneRegistered += Register;
        SceneEventAggregator.Instance.OnAlertZoneAgents += AlertZone;
        SceneEventAggregator.Instance.OnAgentZoneUnRegistered += Unregister;
        await Task.CompletedTask; 
    }

    public override async Task UnLoadResources()
    {
        SceneEventAggregator.Instance.OnAgentZoneRegistered -= Register;
        SceneEventAggregator.Instance.OnAlertZoneAgents -= AlertZone;
        SceneEventAggregator.Instance.OnAgentZoneUnRegistered += Unregister;
        ClearAll();
        _zoneAgents = null;
        await Task.CompletedTask;
    }

    private void Register(FSMControllerBase agent, int zone)
    {
        if (!_zoneAgents.ContainsKey(zone))
            _zoneAgents[zone] = new List<FSMControllerBase>();

        if (!_zoneAgents[zone].Contains(agent))
        {
            _zoneAgents[zone].Add(agent);
            //Debug.LogWarning("Agent Registered in Zone: "+zone);
        }
            
    }

    public void Unregister(FSMControllerBase agent, int zone)
    {
        if (_zoneAgents.TryGetValue(zone, out var list))
            list.Remove(agent);
    }

    private void AlertZone(int zone, FSMControllerBase source)
    {
        if (!_zoneAgents.TryGetValue(zone, out var agents)) return;

        foreach (var agent in agents)
        {
            if (agent != source)
            {
                agent.EnterAlertPhase(); // Use GameManager.Instance.PlayerPosition internally if needed
            }
        }
    }

    public IReadOnlyList<FSMControllerBase> GetAgentsInZone(int zone)
    {
        if (_zoneAgents.TryGetValue(zone, out var agents))
            return agents;

        return Array.Empty<FSMControllerBase>();
    }

    public void ClearAll()
    {
        _zoneAgents.Clear();
    }

}