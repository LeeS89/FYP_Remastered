using Npc.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AgentZoneRegistry : SceneResourcesObsolete, IAgentAlertService
{
    private Dictionary<ZoneId, List<INotificationListener>> _zoneAgents = new(16);
    private readonly HashSet<ZoneId> _alertedZones = new();

    public override async Task LoadResources()
    {
        SceneEventAggregatorObsolete.Instance.OnRegisterAgentAndZone = Register;
        SceneEventAggregatorObsolete.Instance.OnAlertAgentsInZone = TryAlertZone;
        SceneEventAggregatorObsolete.Instance.OnUnRegisterAgentAndZone = Unregister;
        await Task.CompletedTask;
    }

    public override async Task UnLoadResources()
    {

        SceneEventAggregatorObsolete.Instance.OnRegisterAgentAndZone = null;
        SceneEventAggregatorObsolete.Instance.OnAlertAgentsInZone = null;
        SceneEventAggregatorObsolete.Instance.OnUnRegisterAgentAndZone = null;
        ClearAll();
        _zoneAgents = null;
        await Task.CompletedTask;
    }

    private void Register(INotificationListener agent, ZoneId zone)
    {
        if (!_zoneAgents.ContainsKey(zone))
            _zoneAgents[zone] = new List<INotificationListener>();

        if (!_zoneAgents[zone].Contains(agent))
        {
            _zoneAgents[zone].Add(agent);
            //Debug.LogError("Agent Registered in Zone: "+zone);
        }

    }

    public void Unregister(INotificationListener agent, ZoneId zone)
    {
        if (_zoneAgents.TryGetValue(zone, out var list))
            list.Remove(agent);
    }

    private bool TryAlertZone(ZoneId zone, INotificationListener source)
    {
        if (!_alertedZones.Add(zone)) return false; // Already alerted => Create function to reset alerted zones 

        if (!_zoneAgents.TryGetValue(zone, out var agents)) return false;

        foreach (var agent in agents)
        {
            if (agent != source)
            {
                var n = NpcNotification.AlertNotifications.ZoneAlert();
                agent.OnNotifies(n);
            }
        }
        return true;
    }

    public IReadOnlyList<INotificationListener> GetAgentsInZone(ZoneId zone)
    {
        if (_zoneAgents.TryGetValue(zone, out var agents))
            return agents;

        return Array.Empty<INotificationListener>();
    }

    public void ClearAll()
    {
        _zoneAgents.Clear();
    }

    public bool TryRegisterAgentAndZone(INotificationListener agent, ZoneId zone)
    {
        if (!_zoneAgents.ContainsKey(zone))
            _zoneAgents[zone] = new List<INotificationListener>();

        if (!_zoneAgents[zone].Contains(agent))
        {
            _zoneAgents[zone].Add(agent);
            return true;
        }
        return false;
    }

    public void UnregisterAgentAndZone(INotificationListener agent, ZoneId zone)
    {
        throw new NotImplementedException();
    }

    public bool TryAlertAgentsInZone(ZoneId zone, INotificationListener listener)
    {
        if (!_alertedZones.Add(zone)) return false;
        if (!_zoneAgents.TryGetValue(zone, out var listeners)) return false;

        foreach (var l in listeners)
            l.OnNotifies(NpcNotification.AlertNotifications.ZoneAlert());

        return true;
    }
}
