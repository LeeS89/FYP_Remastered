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
                agent.EnterAlertPhase(); // Use GameManager.Instance.PlayerPosition internally instead maybe
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
    private Dictionary<ZoneId, List<INotificationListener>> _zoneAgents = new();
    private readonly HashSet<ZoneId> _alertedZones = new();

    public override async Task LoadResources()
    {
        SceneEventAggregator.Instance.OnRegisterAgentAndZone = Register;
        SceneEventAggregator.Instance.OnAlertAgentsInZone = TryAlertZone;
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
                var n = NPCNotification.ZoneAlert();
                agent.OnNotify(n);
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

    public void RegisterAgentAndZone(INotificationListener agent, ZoneId zone)
    {
        throw new NotImplementedException();
    }

    public void UnregisterAgentAndZone(INotificationListener agent, ZoneId zone)
    {
        throw new NotImplementedException();
    }

    public bool TryAlertAgentsInZone(ZoneId zone, INotificationListener listener)
    {
        return false;
        //throw new NotImplementedException();
    }
}