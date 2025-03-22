using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFSMController : ComponentEvents
{
    private EnemyState _currentState;
    private PatrolState _patrol;
    public List<Transform> _wayPoints;
    public NavMeshAgent _agent;
    public Animator _anim;
    private EnemyAnimController _animController;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        base.RegisterLocalEvents(eventManager);

    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(eventManager);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animController = new EnemyAnimController(_anim);
        _patrol = new PatrolState(this, _wayPoints, _agent);
        _patrol.EnterState();
    }

    public bool _testDeath = false;

    // Update is called once per frame
    void Update()
    {
        if (_testDeath)
        {
            _agent.enabled = false;
            _animController.EnemyDied();
            _testDeath = false;
        }
    }
}
