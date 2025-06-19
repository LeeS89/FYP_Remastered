using NUnit.Framework;
using System;
using System.Collections.Generic;
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
    public event Action<AIDestinationRequestData> OnAIResourceRequested;
    public event Action<ResourceRequest> OnResourceReleased;
    //public event Action<List<Type>> OnDependanciesAdded;
    public event Action<SceneResources> OnDependancyAdded;
    public event Func<Type, bool> OnCheckDependancyExists;

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
        if (request is AIDestinationRequestData aiRequest)
        {
            OnAIResourceRequested?.Invoke(aiRequest);
        }
        else
        {
            OnResourceRequested?.Invoke(request);
        }
           
    }

    public void ReleaseResource(ResourceRequest request)
    {
        
        OnResourceReleased?.Invoke(request);
    }

    /*public void AddDependancies(List<Type> resourceDependancies)
    {
        
        OnDependanciesAdded?.Invoke(resourceDependancies);
    }*/

    public void AddDependancy(SceneResources resource)
    {
        // Invoke the event with the resource
        OnDependancyAdded?.Invoke(resource);
    }

    public bool CheckDependancyExists(Type dependency)
    {
        // Invoke the event and return the result
        return OnCheckDependancyExists?.Invoke(dependency) ?? false;
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
    public event Action<int> OnClosestFlankPointToPlayerJobComplete;
    public event Action OnRunClosestPointToPlayerJob; 
    //public event Action<ResourceRequest> OnFlankPointsRequested;

    public void ClosestPointToPlayerchanged() // Player will invoke this event, and the scene manager will listen to it
    {
        OnClosestPointToPlayerChanged?.Invoke();
    }

    public void ClosestFlankPointToPlayerJobComplete(int pointIndex)
    {
        OnClosestFlankPointToPlayerJobComplete?.Invoke(pointIndex);
    }

    public void RunClosestPointToPlayerJob()
    {
        OnRunClosestPointToPlayerJob?.Invoke();
    }

    /* public void FlankPointsRequested(ResourceRequest request) // Change
     {
         OnFlankPointsRequested?.Invoke(request);
     }*/
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

    public event Action<AIDestinationRequestData> OnPathRequested;

    public void PathRequested(AIDestinationRequestData request)
    {
        OnPathRequested?.Invoke(request);
    }

    #endregion
}
