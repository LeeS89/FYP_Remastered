using UnityEngine;

public class BaseSceneManager : MonoBehaviour, ISceneManager
{
    public static BaseSceneManager _instance {  get; private set; }

    protected virtual void Awake()
    {
        _instance = this;
    }

    public virtual void SetupScene()
    {
        
    }

    
}
