using UnityEngine;

public class Lightsaber : GrabbableObject
{
    [SerializeField] private Animator _anim;

    private void Awake()
    {
        if(TryGetComponent<Animator>(out Animator anim))
        {
            _anim = anim;
        }
    }

    protected override void OnGrabbed()
    {
        _anim.SetTrigger("extend");
    }

    protected override void OnReleased()
    {
        _anim.SetTrigger("retract");
    }
}
