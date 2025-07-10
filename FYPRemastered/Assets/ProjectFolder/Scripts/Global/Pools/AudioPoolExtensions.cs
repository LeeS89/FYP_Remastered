using System.Collections;
using UnityEngine;


public static class AudioPoolExtensions
{
    public static void GetAndPlay(this PoolManager pool, Vector3 position, Quaternion rotation)
    {
        var go = pool.GetFromPool(position, rotation);
       
        if(go.TryGetComponent<AudioSource>(out AudioSource source))
        {
            source.Play();
            CoroutineRunner.Instance.StartCoroutine(ReturnToPoolAfterDelay(pool, go, source.clip.length));
        }
        else if (go.TryGetComponent<ParticleSystem>(out ParticleSystem pSystem))
        {
            CoroutineRunner.Instance.StartCoroutine(ReturnToPoolAfterDelay(pool, go, pSystem.main.duration));
        }

    }

    private static IEnumerator ReturnToPoolAfterDelay(PoolManager pool, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.ReleaseObjectToPool(obj);
    }
}
