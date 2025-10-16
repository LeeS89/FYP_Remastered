using UnityEngine;
using UnityEngine.AI;

public class Throwaway : MonoBehaviour
{
    public Animator _anim;

    public bool testDead = false;
    public NavMeshAgent _agent;
    public float _speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void dResetLook()
    {
        _anim.ResetTrigger("look");
    }
    // Update is called once per frame
    /*void Update()
    {

        *//*_speed = _agent.velocity.magnitude;
        _anim.SetFloat("speed", _speed);
        if (testDead)
        {
            EnemyDied();
            testDead = false;
        }*//*
    }*/
    public void EnemyDied()
    {
        _anim.SetTrigger("dead");
    }

    public void TestReset()
    {
        //_anim.SetLayerWeight(1, 0);
    }
}
