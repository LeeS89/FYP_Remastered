using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public partial class PoolManager
{
    private AudioSource _audioPrefab;
    private ObjectPool<AudioSource> _audioSourcePool;

    public PoolManager(AudioSource prefab, MonoBehaviour caller, int defaultSize = 5, int maxSize = 10)
        : this(defaultSize, maxSize)
    {
        this._audioPrefab = prefab;
        this._caller = caller;
        // Initialize the AudioSource pool
        _audioSourcePool = new ObjectPool<AudioSource>(
            CreatePooledAudioSource,
            OnGetFromPool,
            OnReturnToPool,
            OnDestroyPooledObject,
            false,
            _defaultSize,
            _maxPoolSize
        );
    }

    private void PrewarmAudioPool(int count)
    {
        int prewarmCount = Mathf.Min(count, _maxPoolSize);
        List<AudioSource> tempList = new List<AudioSource>();

        
        for (int i = 0; i < prewarmCount; i++)
        {
            AudioSource obj = _audioSourcePool.Get();
            tempList.Add(obj);
        }

        foreach (AudioSource obj in tempList)
        {
            _audioSourcePool.Release(obj);
        }

        tempList.Clear();
    }

    private AudioSource CreatePooledAudioSource()
    {
        AudioSource audio = GameObject.Instantiate( _audioPrefab );
        audio.transform.root.parent = _poolContainer.transform;
        return audio; 
    }

    private void OnGetFromPool(AudioSource audioSource)
    {
        audioSource.gameObject.SetActive(true); 
    }

    private void OnReturnToPool(AudioSource audioSource)
    {
        audioSource.gameObject.SetActive(false); 
    }

    private void OnDestroyPooledObject(AudioSource audioSource)
    {
        GameObject.Destroy(audioSource.gameObject); 
    }

    public AudioSource GetAudioSource(Vector3 position, Quaternion rotation)
    {
        return GetObjectFromPool(_audioSourcePool, position, rotation); 
    }


    private AudioSource GetObjectFromPool(ObjectPool<AudioSource> pool, Vector3 position, Quaternion rotation)
    {
        AudioSource obj = pool.Get();
        obj.transform.position = position;
        _caller.StartCoroutine(ReturnToPoolAfterDelay(obj, obj.clip.length));
        return obj;
    }

    private void ReleaseAudioSource(AudioSource audioSource)
    {
        ReleaseObjectToPool(_audioSourcePool, audioSource); 
    }

    private IEnumerator ReturnToPoolAfterDelay(AudioSource audio, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReleaseAudioSource(audio);
    }
}
