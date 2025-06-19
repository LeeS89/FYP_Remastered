using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SceneResourceManager
{
    private List<SceneResources> _resources;
    private List<SceneResources> _resourceDependancies;

    
    public SceneResourceManager(params SceneResources[] resources)
    {
        //SceneEventAggregator.Instance.OnDependanciesAdded += AddDependencies;
        SceneEventAggregator.Instance.OnCheckDependancyExists += CheckDependanciesExist;
        SceneEventAggregator.Instance.OnDependancyAdded += AddDependancy;
        _resources = new List<SceneResources>(resources);
        _resourceDependancies = new List<SceneResources>();
    }

    
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
        if(_resourceDependancies.Count == 0 || _resourceDependancies == null) { return; }

        var loadTasks = new List<Task>();

        foreach (var resource in _resourceDependancies)
        {
            // Add each resource's load task to the list
            loadTasks.Add(resource.LoadResources());
        }
        _resources.AddRange(_resourceDependancies);
        await Task.WhenAll(loadTasks);
    }


    private bool CheckDependanciesExist(Type type)
    {
        if(_resources == null) { return false; }
        // Check if the resource type exists in the dependencies list
        return _resourceDependancies.Any(r => r.GetType() == type) || _resources.Any(s => s.GetType() == type);
    }

    private void AddDependancy(SceneResources resource)
    {
        //Debug.LogError($"Adding dependancy: {resource.GetType().Name}");
        //if (_resourceDependancies == null) { _resourceDependancies = new List<SceneResources>(); }
        // Check if the resource is already in the list
        if (_resourceDependancies.Any(r => r.GetType() == resource.GetType()))
        {
            return;  
        }
        // Add the resource instance to the dependencies list
        _resourceDependancies.Add(resource);
    }

    public void CheckAllDependancies()
    {
        if (_resources == null || _resources.Count == 0)
        {
            Debug.LogWarning("No resources to check.");
            return;
        }
        foreach (var resource in _resources)
        {
            if (resource == null)
            {
                Debug.LogError($"Resource {resource.GetType().Name} is null in resources.");
            }
            else
            {
                Debug.LogError($"Resource {resource.GetType().Name} is present in resources.");
            }
        }
    }

    /* public void AddDependencies(List<Type> dependencyTypes)
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
     }*/

    /// <summary>
    /// Custom Update method for resources that implement IUpdateableResource.
    /// </summary>
    public void UpdateResources()
    {
        
        foreach (var resource in _resources)
        {
            if (resource is IUpdateableResource updatableResource)
            {
                updatableResource.UpdateResource();
            }
        }
    }


}
