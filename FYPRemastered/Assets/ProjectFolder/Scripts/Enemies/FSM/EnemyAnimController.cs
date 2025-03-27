using UnityEngine;

public class EnemyAnimController
{
    private Animator _anim;

    public EnemyAnimController(Animator anim)
    {
        _anim = anim;
    }

    public void EnemyDied()
    {
        _anim.SetTrigger("dead");
    }

    public void UpdateSpeed(float speed)
    {
        _anim.SetFloat("speed", speed);
    }

    public void LookAround()
    {
        //_anim.SetLayerWeight(1, 1);
        //_anim.SetBool("testingBool", true);
        _anim.SetTrigger("look");
        //_anim.SetTrigger("dead");

    }


    public void DeadAnimation()
    {
        _anim.SetTrigger("dead");
    }

    public void Shoot()
    {
        _anim.SetTrigger("shoot");
    }

    public void ResetLook()
    {
        _anim.ResetTrigger("look");
        //_anim.SetLayerWeight(1, 0);
    }
}
