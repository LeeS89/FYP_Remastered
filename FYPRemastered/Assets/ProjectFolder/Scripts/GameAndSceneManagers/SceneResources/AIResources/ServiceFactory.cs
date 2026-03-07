using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Services.Internal
{

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


        public static async Task<IResourceLocation> TryGetSingleLocationAsync<TAsset>(
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
            string featureLabel) where TConcrete : class, TInterface, IAddressableServiceObsolete, new()
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
            await svc.InitialiseAsyncOldToKeepItRunningForNow(resolvedLocation);
            return svc;
        }


        public static async Task<(bool found, IResourceLocation location)> TryGetLocationAsync<TAsset>(
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

            if (locs == null || locs.Count == 0) return (false, null);
            return (true, locs[0]);
        }



        ///NEW
        private static async Task<TConcrete> TryLoadStateService<TAsset, TConcrete>(string scene, string featureLabel) where TConcrete : class, IAddressableService, new()
        {
            var (usedInScene, location) = await ServiceFactory.TryGetLocationAsync<TAsset>(scene, featureLabel);

            if (!usedInScene) return null;

            var svc = new TConcrete();
            bool serviceInitSuccess = await svc.TryInitialiseAsync(location);

            if (!serviceInitSuccess)
            {
                DebugLogs.LoadFail(svc, $"(The Service of {typeof(TConcrete).Name})");
                return null;
                // Call Dispose on scv and return null;
            }

            return svc;
        }








        public static async Task<SceneMetaData?> LoadMetaAsync(string sceneName)
        {
            try
            {
                string key = $"{sceneName}_Meta";

                var handle = Addressables.LoadAssetAsync<TextAsset>(key);
                var jsonAsset = await handle.Task;

                if(jsonAsset == null)
                {
                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                        return null;
                    }
                }

                var meta = JsonUtility.FromJson<SceneMetaData>(jsonAsset.text);
                return meta;
            }
            catch (Exception e)
            {
                DebugLogs.Warn($"Failed to load metadata for {sceneName}: {e}");
                return null;
            }
        }



    }


}