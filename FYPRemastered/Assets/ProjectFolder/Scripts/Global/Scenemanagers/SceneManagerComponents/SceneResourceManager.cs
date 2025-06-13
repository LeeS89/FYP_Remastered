using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SceneResourceManager
{
    private List<SceneResources> _resources;
    private List<SceneResources> _resourceDependancies;

    // Constructor remains the same
    public SceneResourceManager(params SceneResources[] resources)
    {
        SceneEventAggregator.Instance.OnDependanciesAdded += AddDependencies;
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

    public async Task LoadDependancies()
    {
        if(_resourceDependancies.Count == 0) { return; }

        var loadTasks = new List<Task>();

        foreach (var resource in _resourceDependancies)
        {
            // Add each resource's load task to the list
            loadTasks.Add(resource.LoadResources());
        }
        _resources.AddRange(_resourceDependancies);
        await Task.WhenAll(loadTasks);
    }


    public void AddDependencies(List<Type> dependencyTypes)
    {
        Debug.LogError($"Adding dependencies: {string.Join(", ", dependencyTypes.Select(t => t.Name))}");
        if (_resourceDependancies == null) { _resourceDependancies = new List<SceneResources>(); }

        foreach (var type in dependencyTypes)
        {
            // Check if the resource is already in the list
            if (_resourceDependancies.Any(r => r.GetType() == type))
            {
                continue;  // Skip if it's already added
            }

            // Create and add the resource instance dynamically if it doesn't exist
            var resource = (SceneResources)Activator.CreateInstance(type);
            _resourceDependancies.Add(resource);
        }
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
