using Oculus.Interaction.HandGrab;
using ProjectRemaster.Combat;
using UnityEngine;
using UnityEngine.Events;


public class USETEST : MonoBehaviour//, IHandGrabUseDelegate
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

    //
    // HUD internals
    TextMesh _hud;
    float _dbgTarget01;
    bool _dbgPressed;

    [Header("Debug HUD (in-VR)")]
    public bool showHud = true;              // toggle HUD on/off
    public Transform hudFollow;              // XR camera / HMD; if null uses Camera.main
    public Vector3 hudLocalOffset = new Vector3(0f, -0.08f, 0.45f);

    private void Awake()
    {
        if (!visual) visual = this.transform;
        startLocal = visual.localPosition;
        if (localAxis.sqrMagnitude < 1e-6f) localAxis = Vector3.right;
        localAxis.Normalize();
    }

    public void SetHandGrabUseInteractable(HandGrabUseInteractable hgu = null)
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
            if(IsInitialUse) IsInitialUse = false;
            else onPressed?.Invoke(); 
        }
        if (!nextPressed && wasPressed && (current01 <= releaseThreshold)) onReleased?.Invoke();
        wasPressed = nextPressed;

        // HUD
        _dbgTarget01 = target01;
        _dbgPressed = nextPressed;
        UpdateHud(target01);


    }

    void UpdateHud(float useVal)
    {
        if (!showHud)
        {
            if (_hud) _hud.gameObject.SetActive(false);
            return;
        }

        if (!_hud)
        {
            var go = new GameObject("TriggerDebugHUD");
            _hud = go.AddComponent<TextMesh>();
            _hud.fontSize = 38;
            _hud.characterSize = 0.0055f; // readable in VR
            _hud.anchor = TextAnchor.UpperLeft;
            _hud.color = Color.white;
        }
        _hud.gameObject.SetActive(true);

        var follow = hudFollow ? hudFollow : (Camera.main ? Camera.main.transform : null);
        if (follow)
        {
            _hud.transform.position = follow.TransformPoint(hudLocalOffset);
            _hud.transform.rotation = Quaternion.LookRotation(follow.forward, follow.up);
        }

        _hud.text =
            $"Use:{useVal:0.00}\n" +
            $"target:{_dbgTarget01:0.00}\n" +
            $"current:{current01:0.00}\n" +
            $"pressed:{_dbgPressed}\n" +
            $"thr:{pressThreshold:0.00}";
    }

}
