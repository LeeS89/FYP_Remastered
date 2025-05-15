using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseSceneManager : MonoBehaviour, ISceneManager
{
    public event Action OnSceneStarted;
    public event Action OnSceneEnded;

    [SerializeField] protected WaypointBlockData _waypointBlockData;
    [SerializeField] protected WaypointManager _waypointManager;
    [SerializeField] protected List<EventManager> _eventManagers;

    public static BaseSceneManager _instance {  get; private set; }

    protected virtual void Awake()
    {
        _instance = this;
    }

    public void SceneStarted()
    {
        //GameManager.Instance.SetPlayer();
        OnSceneStarted?.Invoke();
    }

    public void SceneEnded()
    {
        OnSceneEnded?.Invoke();
    }

    public virtual void SetupScene() { }

    protected virtual void LoadSceneResources() { }

    protected virtual void LoadActiveSceneEventManagers() { }


    public virtual void GetImpactParticlePool(ref PoolManager manager) { }

    public virtual void GetBulletPool(ref PoolManager manager) { }

    protected virtual void LoadWaypoints()
    {
        if (_waypointManager == null)
        {
            _waypointManager = FindFirstObjectByType<WaypointManager>();
        }
        if (_waypointManager != null)
        {
            _waypointBlockData = _waypointManager.RetreiveWaypointData();
        }

        if(_waypointBlockData != null)
        {
            foreach (var blockData in _waypointBlockData.blockDataArray)
            {
                blockData._inUse = false;
            }
        }
    }

    public virtual BlockData RequestWaypointBlock()
    {
        if(_waypointBlockData == null)
        {
            Debug.LogWarning("No Waypoints exist in the scene, please update waypoint manager");
            return null;
        }

        foreach (var blockData in _waypointBlockData.blockDataArray)
        {
            if (!blockData._inUse)
            {
                blockData._inUse = true;
                return blockData;
            }
        }
        return null; 
    }

    public virtual void ReturnWaypointBlock(BlockData bd)
    {
        if (!_waypointBlockData.blockDataArray.Contains(bd)) { return; }

        int index = Array.FindIndex(_waypointBlockData.blockDataArray, block => block == bd);

        if(index >= 0)
        {
            _waypointBlockData.blockDataArray[index]._inUse = false;
        }
    }


}
