using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Services.Internal
{

    public static class AddressableLoader
    {

        #region Obsolete

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


     

        #endregion



        public static async Task<AsyncOperationHandle<TAsset>?> TryLoadAssetAsync<TAsset>(string addressKey)
        {
            if (NullOrEmptyString(addressKey)) { DebugLogs.ArgNotNull(addressKey, "addressKey"); return null; }

            try
            {
                
                var handle = Addressables.LoadAssetAsync<TAsset>(addressKey);

                await handle.Task;

                if(handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(handle);
                    return null;
                }

                return handle;

            }
            catch (Exception e)
            {
                DebugLogs.Err(null, $"Asset Load operation Failed : {e}");
                return null;
            }


        }


     
       

     

        // Make extension
       
        public static async Task<TAsset> LoadMetaAsyncNew<TAsset>(string addressKey) where TAsset : class
        {
            AsyncOperationHandle<TextAsset>? handle = null;

            try
            {
                if (NullOrEmptyString(addressKey)) { DebugLogs.ArgNotNull(addressKey, "addressKey"); return null; }

                string resolvedKey = $"{addressKey}_Meta";

                handle = Addressables.LoadAssetAsync<TextAsset>(resolvedKey);
                var textAsset = await handle.Value.Task;

                if (handle.Value.Status != AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(handle.Value);
                    return null;
                }

                var asset = ExtractFromJson<TAsset>(textAsset);

                Addressables.Release(handle.Value);

                return asset;
              
            }
            catch (Exception e)
            {
                DebugLogs.Warn($"Failed to load metadata for {addressKey}: {e}");
                if(handle.HasValue && handle.Value.IsValid()) Addressables.Release(handle.Value); 
                return null;
            }
        }

        private static TAsset ExtractFromJson<TAsset>(TextAsset jsonAsset) where TAsset : class
        {
            if (jsonAsset == null) return null;
            return JsonUtility.FromJson<TAsset>(jsonAsset.text);
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


      
        private static bool NullOrEmptyString(string txt) => string.IsNullOrWhiteSpace(txt) || string.IsNullOrEmpty(txt);


    }


}