using System.Threading.Tasks;
using UnityEngine;

public abstract class SceneResources
{
    public abstract Task LoadResources();

    protected virtual void ResourceRequested(ResourceRequest request) { }
    
}
