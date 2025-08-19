using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections;
using UnityEngine;


public class Lightsaber : GrabbableObject
{
    [SerializeField] private Animator _anim;
    [SerializeField] private AudioSource _audio;
   
    [SerializeField] private AudioClip _powerOn;
    [SerializeField] private AudioClip _powerOff;
    [SerializeField] private AudioClip _hum;
    [SerializeField] private AudioClip _deflect;
    [SerializeField] private AudioClip _swing;

    public PointableUnityEventWrapper _eventWrapper;

   

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);
        if (TryGetComponent<Animator>(out Animator anim))
        {
            _anim = anim;
        }
    }

    protected override void OnPlayerDeathStatusUpdated(bool isDead)
    {
        base.OnPlayerDeathStatusUpdated(isDead);
    }

  

    public override void OnGrabbed()
    {
        _anim.SetTrigger("extend");
        PowerUp();
    }

    protected override void OnReleased()
    {
        
        _anim.SetTrigger("retract");
        PowerDown();
    }


    private void PowerUp()
    {
        
        _audio.volume = 1f;
        _audio.loop = false;
        _audio.clip = _powerOn;
        _audio.Play();

        StartCoroutine(CrossfadeToIdle(_powerOn.length, 1f, 0.1f));
    }

   

    IEnumerator CrossfadeToIdle(float delay, float fadeDuration, float targetVolume)
    {
        yield return new WaitForSeconds(delay); // Wait for powerOnSound to finish

        float startVolume = _audio.volume;
        float elapsedTime = 0f;

        // Play idle sound at zero volume
        _audio.loop = true;
        _audio.clip = _hum;
        _audio.volume = 0f;
        _audio.Play();

        // Gradually fade in the idle sound to 0.1f volume
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            _audio.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / fadeDuration);
            yield return null;
        }

        // Ensure final volume is exactly 0.1f
        _audio.volume = targetVolume;
    }

    private void PowerDown()
    {
        StopAllCoroutines();
      
        _audio.volume = 1f;
        _audio.Stop();
        _audio.loop = false;
        _audio.clip = _powerOff;
        _audio.Play();
    }
}
