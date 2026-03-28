using System.Collections.Generic;
using System;
using UnityEngine;
using System.Threading.Tasks;
using Npc.Internal;

[Obsolete("", true)]
public class AgentZoneRegistryObsolete : SceneResources
{
    private Dictionary<int, List<FSMControllerBaseObsolete>> _zoneAgents = new();

    public override async Task LoadResources()
    {
        SceneEventAggregatorObsolete.Instance.OnAgentZoneRegistered += Register;
        SceneEventAggregatorObsolete.Instance.OnAlertZoneAgents += AlertZone;
        SceneEventAggregatorObsolete.Instance.OnAgentZoneUnRegistered += Unregister;
        await Task.CompletedTask; 
    }

    public override async Task UnLoadResources()
    {
        SceneEventAggregatorObsolete.Instance.OnAgentZoneRegistered -= Register;
        SceneEventAggregatorObsolete.Instance.OnAlertZoneAgents -= AlertZone;
        SceneEventAggregatorObsolete.Instance.OnAgentZoneUnRegistered += Unregister;
        ClearAll();
        _zoneAgents = null;
        await Task.CompletedTask;
    }

    private void Register(FSMControllerBaseObsolete agent, int zone)
    {
        if (!_zoneAgents.ContainsKey(zone))
            _zoneAgents[zone] = new List<FSMControllerBaseObsolete>();

        if (!_zoneAgents[zone].Contains(agent))
        {
            _zoneAgents[zone].Add(agent);
            //Debug.LogWarning("Agent Registered in Zone: "+zone);
        }
            
    }

    public void Unregister(FSMControllerBaseObsolete agent, int zone)
    {
        if (_zoneAgents.TryGetValue(zone, out var list))
            list.Remove(agent);
    }

    private void AlertZone(int zone, FSMControllerBaseObsolete source)
    {
        if (!_zoneAgents.TryGetValue(zone, out var agents)) return;

        foreach (var agent in agents)
        {
            if (agent != source)
            {
                agent.EnterAlertPhase(); // Use GameManager.Instance.PlayerPosition internally instead maybe
            }
        }
    }

    public IReadOnlyList<FSMControllerBaseObsolete> GetAgentsInZone(int zone)
    {
        if (_zoneAgents.TryGetValue(zone, out var agents))
            return agents;

        return Array.Empty<FSMControllerBaseObsolete>();
    }

    public void ClearAll()
    {
        _zoneAgents.Clear();
    }

}



