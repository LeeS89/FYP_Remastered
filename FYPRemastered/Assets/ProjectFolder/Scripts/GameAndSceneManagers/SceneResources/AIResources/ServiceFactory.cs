using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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

        [Obsolete]
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
                if (NullOrEmptyString(sceneName)) { DebugLogs.ArgNotNull(sceneName, "sceneName"); return null; }

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


        public static async Task<IResourceLocation> TryGetLocationAsyncNew<TAsset>(string addressKey)
        {
            try
            {

                if (NullOrEmptyString(addressKey)) { DebugLogs.ArgNotNull(addressKey, "address key"); return null; }

                var handle = Addressables.LoadResourceLocationsAsync(addressKey, typeof(TAsset));


                var locs = await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(handle);
                    return null;
                }

                Addressables.Release(handle);

                if (locs == null || locs.Count == 0) return null;
                return locs[0];
                

            }
            catch (Exception e)
            {
                DebugLogs.LoadFail(null, $"(The Location of {typeof(TAsset).Name})");
                return null;
            }
        }

        public static async Task<TAsset> TryLoadAssetAsync<TAsset>(string addressKey) where TAsset : UnityEngine.Object
        {
            IResourceLocation resolvedLocation = await TryGetLocationAsyncNew<TAsset>(addressKey);

            if (resolvedLocation == null) return null;

            await Task.CompletedTask;//svc.InitialiseAsyncOldToKeepItRunningForNow(resolvedLocation);
            return null;//svc;
        }

        private static async Task<TAsset> CreateAsync<TAsset>(IResourceLocation location) where TAsset : UnityEngine.Object
        {
            try
            {
                if (location == null) { DebugLogs.ArgNotNull(location, "resource location"); return null; }

                var handle = Addressables.LoadAssetAsync<TAsset>(location);
                var asset = await handle.Task;

                if(handle.Status != AsyncOperationStatus.Succeeded)
                {
                    DebugLogs.LoadFail(location, $"{typeof(TAsset).Name}");
                    Addressables.Release(handle);
                    return null;
                }

                Addressables.Release(handle);
                return null;
            } 
            catch (Exception e)
            {
                return null;
            }
        }



        private static bool NullOrEmptyString(string txt) => string.IsNullOrWhiteSpace(txt) || string.IsNullOrEmpty(txt);


    }


}