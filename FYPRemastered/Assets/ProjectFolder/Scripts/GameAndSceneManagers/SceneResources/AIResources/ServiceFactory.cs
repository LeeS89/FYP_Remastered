using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class ServiceFactory
{
    public static async Task<bool> ExistsInSceneAsync<TAsset>(
        string sceneLabel,
        string featureLabel)
    {
        var keys = new object[] { sceneLabel, featureLabel };

        var h = Addressables.LoadResourceLocationsAsync(
            keys,
            Addressables.MergeMode.Intersection,
            typeof(TAsset)
            );

        IList<IResourceLocation> locs = await h.Task;
        Addressables.Release(h);

        return locs != null && locs.Count > 0;
    }


    public static async Task<IResourceLocation?> TryGetSingleLocationAsync<TAsset>(
        string sceneLabel,
        string featureLabel)
    {
        var keys = new object[] { sceneLabel, featureLabel };

        var h = Addressables.LoadResourceLocationsAsync(
            keys,
            Addressables.MergeMode.Intersection,
            typeof(TAsset)
        );

        IList<IResourceLocation> locs = await h.Task;
        Addressables.Release(h);

        if (locs == null || locs.Count == 0) return null;
        return locs[0]; // optionally assert if Count > 1
    }


    public static async Task<TInterface?> TryCreateAsync<TAsset, TInterface, TConcrete>(
        string sceneLabel,
        string featureLabel) where TConcrete : class, TInterface, IAddressableService, new()
    {
        var loc = await TryGetSingleLocationAsync<TAsset>(sceneLabel, featureLabel);
        if (loc == null) return default;

        var svc = new TConcrete();
        await svc.InitialiseAsync(loc);
        return svc;
    }

    public static async Task<TConcrete?> TryCreateAsync<TAsset, TConcrete>(
       string sceneLabel,
       string featureLabel,
       IResourceLocation location = null)
       where TConcrete : class, IAddressableService, new()
    {
        IResourceLocation resolvedLocation = location != null ? location : await TryGetSingleLocationAsync<TAsset>(sceneLabel, featureLabel);

        
        if (resolvedLocation == null) return null;

        var svc = new TConcrete();
        await svc.InitialiseAsync(resolvedLocation);
        return svc;
    }



}


