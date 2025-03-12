using System.Collections.Generic;
using UnityEngine;

public class BaseSceneManager : MonoBehaviour, ISceneManager
{
    [SerializeField] protected List<EventManager> _eventManagers;

    public static BaseSceneManager _instance {  get; private set; }

    protected virtual void Awake()
    {
        _instance = this;
    }

    public virtual void SetupScene() { }

    protected virtual void LoadSceneResources() { }

    protected virtual void LoadSceneEventManagers() { }
    
}
