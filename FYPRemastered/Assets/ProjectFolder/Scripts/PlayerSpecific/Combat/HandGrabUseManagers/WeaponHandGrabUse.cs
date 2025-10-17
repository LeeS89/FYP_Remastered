using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.Events;

public class WeaponHandGrabUse : MonoBehaviour
{
    [Range(0f, 1f)]
    public float pressThreshold = 0.9f;

    [Range(0f, 1f)]
    public float releaseThreshold = 0.7f;

    // private bool _wasFired = false;

    public HandGrabUseInteractable currentUse;
    public Transform visual;
    public Vector3 localAxis = Vector3.right;
    public float travel = 0.012f;
    public float pressLerp = 20f;
    public float releaseLerp = 16f;

    public UnityEvent onPressed;
    public UnityEvent onReleased;

    float current01;
    Vector3 startLocal;
    bool wasPressed;

    public float damped;

    // Prevents accidental firing on initial Grab
    public bool IsInitialUse { get; private set; }

  
    private void Awake()
    {
        if (!visual) visual = this.transform;
        startLocal = visual.localPosition;
        if (localAxis.sqrMagnitude < 1e-6f) localAxis = Vector3.right;
        localAxis.Normalize();
    }

    public void SetHandGrabUseInteractable(HandGrabUseInteractable hgu)
    {
        IsInitialUse = true;
        currentUse = hgu;
    }

    public void ResetHandGrabUseInteractable() => currentUse = null;


    private void Update()
    {
        /*if (!_weapon || _weapon is not IRanged rw) return;
        if (!rw.Equipped) return;*/
        if (currentUse == null) return;

        float target01 = currentUse ? Mathf.Clamp01(currentUse.UseProgress) : 0f;
        float rate = (target01 > current01) ? pressLerp : releaseLerp;
        current01 = Mathf.MoveTowards(current01, target01, rate * Time.deltaTime);
        visual.localPosition = startLocal + localAxis * (current01 * travel);

        bool nextPressed = wasPressed ? (current01 > releaseThreshold) : (current01 >= pressThreshold);
        // bool pressed = current01 >= pressThreshold;
        if (nextPressed && !wasPressed)
        {
            if (IsInitialUse) IsInitialUse = false;
            else onPressed?.Invoke();
        }
        if (!nextPressed && wasPressed && (current01 <= releaseThreshold)) onReleased?.Invoke();
        wasPressed = nextPressed;
      
    }

}
