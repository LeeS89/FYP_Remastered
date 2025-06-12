using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SceneResourceManager
{
    private List<SceneResources> _resources;

    // Constructor remains the same
    public SceneResourceManager(params SceneResources[] resources)
    {
        _resources = new List<SceneResources>(resources);
    }

    // Asynchronous method to load resources
    public async Task LoadResourcesAsync()
    {
        // Create a list of tasks for all resources
        var loadTasks = new List<Task>();

        foreach (var resource in _resources)
        {
            // Add each resource's load task to the list
            loadTasks.Add(resource.LoadResources());
        }

        // Await all tasks concurrently
        await Task.WhenAll(loadTasks);
    }

    public async Task Loaddependancies(List<SceneResources> dependancies)
    {
        var loadTasks = new List<Task>();

        foreach (var resource in dependancies)
        {
            // Add each resource's load task to the list
            loadTasks.Add(resource.LoadResources());
        }
        _resources.AddRange(dependancies);
        await Task.WhenAll(loadTasks);
    }

    public void UpdateResources()
    {
        // Iterate through each resource and call Update if it exists
        foreach (var resource in _resources)
        {
            if (resource is IUpdateableResource updatableResource)
            {
                updatableResource.UpdateResource();
            }
        }
    }
}
