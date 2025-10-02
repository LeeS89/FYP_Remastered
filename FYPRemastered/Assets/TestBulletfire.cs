using System;
using System.Collections;
using UnityEngine;

public class TestBulletfire : MonoBehaviour
{
    public PoolIdSO _bulletPoolId;
    private IPoolManager _poolManager;
    private Action<string, IPoolManager> PoolRequestCallback;
    public Transform _spawnPoint;

    private void Awake()
    {
        PoolRequestCallback = OnPoolReceived;
    }

    private void Start()
    {
        StartCoroutine(StartRequest());
    }

    private void OnPoolReceived(string id, IPoolManager pool)
    {
        if (string.IsNullOrEmpty(id) || id != _bulletPoolId.Id || pool == null) return;
       // Debug.LogError("Pool successfully received");
        _poolManager = pool;
    }

    public void Fire()
    {
        GameObject obj = _poolManager?.GetFromPool(_spawnPoint.position, _spawnPoint.rotation) as GameObject;
        if (obj != null)
        {
            if(ComponentRegistry.TryGet<IPoolable>(obj, out var bullet))
            {
                bullet.LaunchPoolable();
            }
        }
    }

    IEnumerator StartRequest()
    {
        yield return new WaitForSeconds(4f);
        this.RequestPool(_bulletPoolId.Id, PoolRequestCallback);
    }
}
