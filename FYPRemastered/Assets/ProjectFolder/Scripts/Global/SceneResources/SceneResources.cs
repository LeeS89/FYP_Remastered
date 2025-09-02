using System;
using System.Threading.Tasks;
using UnityEngine;

public abstract class SceneResources
{
    public virtual Task LoadResources()
    {
        return Task.CompletedTask;
    }

    protected virtual Task LoadResourceAudio()
    {
        return Task.CompletedTask;
    }

    protected virtual Task LoadResourceParticles()
    {
        return Task.CompletedTask;
    }

    [Obsolete("Use Resourcesrequested instead")]
    protected virtual void ResourceRequested(ResourceRequest request) { }

    protected virtual void ResourcesRequested(in ResourceRequests request) { }

    [Obsolete("Use ResourcesRequested instead")]
    protected virtual void AIResourceRequested(AIDestinationRequestData request) { }


    protected virtual void ResourceReleased(ResourceRequest request) { }

    protected virtual void NotifyClassDependancies() { }

    protected virtual void InitializePools() { }

    public virtual Task UnLoadResources() => Task.CompletedTask; 
}
