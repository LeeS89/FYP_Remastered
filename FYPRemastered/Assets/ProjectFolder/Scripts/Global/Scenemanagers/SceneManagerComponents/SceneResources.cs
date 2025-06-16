using System.Threading.Tasks;
using UnityEngine;

public abstract class SceneResources
{
    public virtual Task LoadResources()
    {
       
        return Task.CompletedTask;
    }

    protected virtual void ResourceRequested(ResourceRequest request) { }

    protected virtual void ResourceReleased(ResourceRequest request) { }

    protected virtual void NotifyDependancies() { }
}
