using System;
using UnityEngine;


public class SceneEventAggregator : MonoBehaviour
{
    public static SceneEventAggregator Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    #region Global Scene Events
    public event Action OnSceneStarted;
    public event Action OnSceneEnded;
    public event Action<ResourceRequest> OnResourceRequested;
    public event Action<ResourceRequest> OnResourceReleased;

    public void SceneStarted()
    {
        OnSceneStarted?.Invoke();
    }

    public void SceneEnded()
    {
        OnSceneEnded?.Invoke();
    }

    public void RequestResource(ResourceRequest request)
    {
        // Trigger the event when a resource request is made
        OnResourceRequested?.Invoke(request);
    }

    public void ReleaseResource(ResourceRequest request)
    {
        // Trigger the event when a resource is released
        OnResourceReleased?.Invoke(request);
    }
    #endregion


    #region AI Agent Events

    #region Player Flanking Point Events
    /// <summary>
    /// Events used with Enemy AI system to notify when the closest flanking point to player has changed.
    /// When there is an active Alert status - OnClosestPointToPlayerChanged will be invoked by the player when ever they stop moving
    /// This in turn runs the ClosestPointToPlayerJob to find the closest point to player. Once job completes,
    /// OnClosestPointToPlayerJobComplete notifies all interested parties with the index of the closest point to player.
    /// </summary>
    public event Action OnClosestPointToPlayerChanged;
    public event Action<int> OnClosestPointToPlayerJobComplete;

    public void ClosestPointToPlayerchanged() // Player will invoke this event, and the scene manager will listen to it
    {
        OnClosestPointToPlayerChanged?.Invoke();
    }

    public void ClosestPointToPlayerJobComplete(int pointIndex)
    {
        OnClosestPointToPlayerJobComplete?.Invoke(pointIndex);
    }
    #endregion Scene Events

    #region Agent Zone Registry Events
    public event Action<EnemyFSMController, int> OnAgentZoneRegistered;
    public event Action<int, EnemyFSMController> OnAlertZoneAgents;

    public void RegisterAgentAndZone(EnemyFSMController agent, int zone)
    {
        OnAgentZoneRegistered?.Invoke(agent, zone);
    }

    public void AlertZoneAgents(int zone, EnemyFSMController source)
    {
        OnAlertZoneAgents?.Invoke(zone, source);
    }
    #endregion

    #endregion
}
