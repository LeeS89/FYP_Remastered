using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BulletResources : SceneResources
{
    private GameObject _normalBulletPrefab;

    public override async Task LoadResources()
    {
        try
        {
            var normalBulletHandle = Addressables.LoadAssetAsync<GameObject>("BasicBullet");

            await normalBulletHandle.Task;

            if (normalBulletHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _normalBulletPrefab = normalBulletHandle.Result;
                if (_normalBulletPrefab == null)
                {
                    Debug.LogError("Loaded normal bullet prefab is null.");
                }
            }
            else
            {
                Debug.LogError("Failed to load the normal bullet prefab from Addressables.");
            }
        }
        catch(Exception e)
        {
            Debug.LogError($"Error loading bullet resources: {e.Message}");
        }
        await Task.CompletedTask;
    }

    protected override void NotifyDependancies()
    {
            
    }
}
