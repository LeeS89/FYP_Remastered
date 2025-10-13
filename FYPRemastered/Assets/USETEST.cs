using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class USETEST : MonoBehaviour//, IHandGrabUseDelegate
{
    [Range(0f, 1f)]
    private float pressThreshold = 0.9f;

    [Range(0f, 1f)]
    private float releaseThreshold = 0.7f;

   // private bool _wasFired = false;

    public HandGrabUseInteractable use;
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

    private void Awake()
    {
        if(!visual) visual = this.transform;
        startLocal = visual.localPosition;
        if(localAxis.sqrMagnitude < 1e-6f) localAxis = Vector3.right;
        localAxis.Normalize();
    }


    private void Update()
    {
        float target01 = use ? Mathf.Clamp01(use.UseProgress) : 0f;
        float rate = (target01 > current01) ? pressLerp : releaseLerp;
        current01 = Mathf.MoveTowards(current01, target01, rate * Time.deltaTime);
        visual.localPosition = startLocal + localAxis * (current01 * travel);

        // optional press/release events
        bool pressed = current01 >= pressThreshold;
        if (pressed && !wasPressed) onPressed?.Invoke();
        if (!pressed && wasPressed && current01 <= releaseThreshold) onReleased?.Invoke();
        wasPressed = pressed;
    }

}
