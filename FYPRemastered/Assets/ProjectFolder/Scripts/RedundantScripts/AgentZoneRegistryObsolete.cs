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






























/*public class AgentZoneRegistryNew : SceneResources
{
    private Dictionary<int, List<IZoneAlertListener>> _zoneAgents = new();
    private readonly HashSet<int> _alertedZones = new();

    public override async Task LoadResources()
    {
        SceneEventAggregator.Instance.OnRegisterAgentAndZone = Register;
        SceneEventAggregator.Instance.OnAlertAgentsInZone = AlertZone;
        SceneEventAggregator.Instance.OnUnRegisterAgentAndZone = Unregister;
        await Task.CompletedTask; 
    }

    public override async Task UnLoadResources()
    {

        SceneEventAggregator.Instance.OnRegisterAgentAndZone = null;
        SceneEventAggregator.Instance.OnAlertAgentsInZone = null;
        SceneEventAggregator.Instance.OnUnRegisterAgentAndZone = null;
        ClearAll();
        _zoneAgents = null;
        await Task.CompletedTask;
    }

    private void Register(IZoneAlertListener agent, int zone)
    {
        if (!_zoneAgents.ContainsKey(zone))
            _zoneAgents[zone] = new List<IZoneAlertListener>();

        if (!_zoneAgents[zone].Contains(agent))
        {
            _zoneAgents[zone].Add(agent);
          //  Debug.LogError("Agent Registered in Zone: "+zone);
        }
            
    }

    public void Unregister(IZoneAlertListener agent, int zone)
    {
        if (_zoneAgents.TryGetValue(zone, out var list))
            list.Remove(agent);
    }

    private bool AlertZone(int zone, IZoneAlertListener source)
    {
        if (!_alertedZones.Add(zone)) return false; // Already alerted => Create function to reset alerted zones 

        if (!_zoneAgents.TryGetValue(zone, out var agents)) return false;

        foreach (var agent in agents)
        {
            if (agent != source)
                agent.EnterAlertPhase();
        }
        return true;
    }

    public IReadOnlyList<IZoneAlertListener> GetAgentsInZone(int zone)
    {
        if (_zoneAgents.TryGetValue(zone, out var agents))
            return agents;

        return Array.Empty<IZoneAlertListener>();
    }

    public void ClearAll()
    {
        _zoneAgents.Clear();
    }

}*/
public class AgentZoneRegistryNew : SceneResources, IAgentAlertService
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