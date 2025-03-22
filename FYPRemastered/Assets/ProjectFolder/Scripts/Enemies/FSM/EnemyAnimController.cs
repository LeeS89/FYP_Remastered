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
}
