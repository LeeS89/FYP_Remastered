using UnityEngine;

public class Throwaway : MonoBehaviour
{
    public Animator _anim;

    public bool testDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (testDead)
        {
            EnemyDied();
            testDead = false;
        }
    }
    public void EnemyDied()
    {
        _anim.SetTrigger("dead");
    }
}
