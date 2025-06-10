using System.Threading.Tasks;
using UnityEngine;

public abstract class SceneResources
{
    public virtual Task LoadResources()
    {
        // Return a completed task to ensure all code paths return a value
        return Task.CompletedTask;
    }

    protected virtual void ResourceRequested(ResourceRequest request) { }

    protected virtual void ResourceReleased(ResourceRequest request) { }
}
