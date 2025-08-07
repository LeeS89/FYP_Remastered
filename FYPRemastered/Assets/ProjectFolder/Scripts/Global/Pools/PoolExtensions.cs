using System.Collections;
using UnityEngine;


public static class PoolExtensions
{
    private static WaitForSeconds _delayAudio;
    private static WaitForSeconds _delayParticle;
    /*public static void GetAndPlay(this PoolManager pool, Vector3 position, Quaternion rotation)
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

    }*/

    private static IEnumerator ReturnToPoolAfterDelay(PoolManager pool, GameObject obj, float delay, WaitForSeconds waitDelay)
    {
        yield return waitDelay;
       // yield return new WaitForSeconds(delay);
        pool.ReleaseObjectToPool(obj);
    }

    public static void GetAndPlay(this PoolManager pool, Vector3 position, Quaternion rotation)
    {
        var go = pool.GetFromPool(position, rotation);
        AudioSource audio = go.GetComponent<AudioSource>();
        ParticleSystem particleSystem = go.GetComponent<ParticleSystem>();

        if (audio != null)
        {
            audio.Play();
            if(_delayAudio == null) { _delayAudio = new WaitForSeconds(audio.clip.length); }
            CoroutineRunner.Instance.StartCoroutine(ReturnToPoolAfterDelay(pool, go, audio.clip.length, _delayAudio));
        }
        else if (particleSystem != null)
        {
            if(_delayParticle == null) { _delayParticle = new WaitForSeconds(particleSystem.main.duration); }
            CoroutineRunner.Instance.StartCoroutine(ReturnToPoolAfterDelay(pool, go, particleSystem.main.duration, _delayParticle));
        }

    }
}
