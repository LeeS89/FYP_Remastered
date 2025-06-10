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
}
